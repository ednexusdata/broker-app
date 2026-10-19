using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using EdNexusData.Broker.Common.Authentication;
using EdNexusData.Broker.Common.Configuration;
using EdNexusData.Broker.Common.Connector;
using EdNexusData.Broker.Common.Payloads;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EdNexusData.Broker.Core;

/// <summary>
/// Loads connectors from the connectors folder, each into its own collectible load context, and keeps the
/// index of the types they provide. Connectors can be loaded, replaced and unloaded while the app runs:
/// readers always see an immutable catalog, which is swapped atomically on change.
/// </summary>
public class ConnectorLoader
{
    private readonly object _gate = new();
    private readonly ILogger<ConnectorLoader> _logger;
    private ConnectorCatalog _catalog = ConnectorCatalog.Empty;
    private ImmutableList<ConnectorRuntime> _retiring = ImmutableList<ConnectorRuntime>.Empty;
    private IServiceCollection? _appServices;
    private IReadOnlyList<ServiceDescriptor>? _appOpenGenerics;

    public static ConnectorLoader? Instance { get; private set; }

    public string ConnectorsPath { get; }

    /// <summary>Connector versions currently in service.</summary>
    public IReadOnlyList<ConnectorRuntime> Runtimes => Catalog.Runtimes;

    public IReadOnlyList<Type> Connectors => Catalog.Connectors;
    public IReadOnlyList<Type> Authenticators => Catalog.Authenticators;
    public IReadOnlyList<Type> Payloads { get; }
    public IReadOnlyList<Type> PayloadJobs => Catalog.PayloadJobs;
    public IReadOnlyList<Type> PayloadContentActions => Catalog.PayloadContentActions;
    public IReadOnlyDictionary<string, Type> PayloadContentActionByTransformer => Catalog.PayloadContentActionByTransformer;
    public IReadOnlyDictionary<string, Type> Transformers => Catalog.Transformers;
    public IReadOnlyDictionary<Type, Type> Importers => Catalog.Importers;

    /// <summary>Connector assemblies keyed by assembly name.</summary>
    public IReadOnlyDictionary<string, Assembly> Assemblies => Catalog.Assemblies;

    /// <summary>Load contexts of the connectors in service, keyed by load context name.</summary>
    public IReadOnlyDictionary<string, ConnectorLoadContext> ConnectorLoadContexts => Catalog.LoadContexts;

    private ConnectorCatalog Catalog => Volatile.Read(ref _catalog);

    public ConnectorLoader() : this(Path.Combine(AppContext.BaseDirectory, "connectors"))
    {
    }

    public ConnectorLoader(string connectorsPath)
    {
        _logger = LoggerFactory.Create(config =>
        {
            config.AddConsole();
        }).CreateLogger<ConnectorLoader>();

        ConnectorsPath = connectorsPath;
        Payloads = LoadPayloads();

        if (!Directory.Exists(ConnectorsPath))
        {
            Directory.CreateDirectory(ConnectorsPath);
            _logger.LogInformation("Directory created: {Path}", ConnectorsPath);
        }

        var runtimes = new List<ConnectorRuntime>();
        foreach (var connectorDirectory in Directory.GetDirectories(ConnectorsPath))
        {
            var runtime = LoadNewestWorkingVersion(connectorDirectory);
            if (runtime is not null) runtimes.Add(runtime);
        }

        if (runtimes.Count == 0)
        {
            _logger.LogInformation("No connectors loaded from {Path}", ConnectorsPath);
        }

        _catalog = ConnectorCatalog.Create(runtimes);
        Instance = this;
    }

    /// <summary>
    /// The app's service registrations, so connector containers can build the app's open generics
    /// (ILogger&lt;T&gt;, IRepository&lt;T&gt;, ...) over connector types. Read when the first connector scope
    /// is created, after the app has finished registering services.
    /// </summary>
    public void UseAppServices(IServiceCollection services)
    {
        _appServices = services;
    }

    internal IReadOnlyList<ServiceDescriptor> AppOpenGenerics => _appOpenGenerics ??=
        _appServices?.Where(d => d.ServiceType.IsGenericTypeDefinition && !d.IsKeyedService && d.ImplementationType is not null).ToList()
        ?? [];

    public Type? GetConnector(string connectorType)
    {
        return Connectors.FirstOrDefault(x => x.FullName == connectorType);
    }

    public Type? GetAuthenticator(string authenticationType)
    {
        return Authenticators.FirstOrDefault(x => x.GetInterface(nameof(IAuthenticationProvider)) is not null && x.FullName == authenticationType);
    }

    public List<Type>? GetConfigurations(Assembly assembly)
    {
        return assembly.GetExportedTypes().Where(x => x.GetInterface(nameof(IConfiguration)) is not null).ToList();
    }

    public List<Type>? GetPayloads(Assembly assembly)
    {
        return assembly.GetExportedTypes().Where(x => x.GetInterface(nameof(Payload)) is not null && x.IsAbstract == false).ToList();
    }

    public IReadOnlyList<Type>? GetPayloadJobs()
    {
        return PayloadJobs;
    }

    public IReadOnlyList<Type>? GetPayloadContentActions()
    {
        return PayloadContentActions;
    }

    /// <summary>
    /// Finds a type by full name in the app itself or in a connector in service. Connector versions that are
    /// waiting to unload are never returned, even though .NET still lists their assemblies until they are collected.
    /// </summary>
    public Type? ResolveType(string fullName)
    {
        foreach (var assembly in AssemblyLoadContext.Default.Assemblies)
        {
            var type = assembly.GetType(fullName, throwOnError: false);
            if (type is not null) return type;
        }

        foreach (var runtime in Runtimes)
        {
            foreach (var assembly in runtime.Assemblies)
            {
                var type = assembly.GetType(fullName, throwOnError: false);
                if (type is not null) return type;
            }
        }

        return null;
    }

    /// <summary>The connector version that <paramref name="type"/> belongs to, or null for app types.</summary>
    public ConnectorRuntime? FindRuntime(Type type)
    {
        ConnectorRuntime? found = null;

        foreach (var assembly in ConnectorRuntime.AssembliesOf(type))
        {
            if (AssemblyLoadContext.GetLoadContext(assembly) is not ConnectorLoadContext context) continue;

            var runtime = FindRuntime(context)
                ?? throw new InvalidOperationException($"Type {type} belongs to a connector that is no longer loaded.");

            if (found is not null && found != runtime)
            {
                throw new InvalidOperationException($"Type {type} combines types from connectors {found.Name} and {runtime.Name}.");
            }

            found = runtime;
        }

        return found;
    }

    /// <summary>The connector version owning <paramref name="assembly"/>, including versions waiting to unload.</summary>
    public ConnectorRuntime? FindRuntime(Assembly assembly)
    {
        return AssemblyLoadContext.GetLoadContext(assembly) is ConnectorLoadContext context ? FindRuntime(context) : null;
    }

    private ConnectorRuntime? FindRuntime(ConnectorLoadContext context)
    {
        return Catalog.Runtimes.FirstOrDefault(r => r.LoadContext == context)
            ?? Volatile.Read(ref _retiring).FirstOrDefault(r => r.LoadContext == context);
    }

    /// <summary>Loads a connector that is not loaded yet from <paramref name="connectorDirectory"/> (connectors/&lt;name&gt;).</summary>
    public ConnectorRuntime LoadConnector(string connectorDirectory)
    {
        var name = Path.GetFileName(Path.TrimEndingDirectorySeparator(connectorDirectory));
        var activeDirectory = ResolveActiveDirectory(connectorDirectory)
            ?? throw new InvalidOperationException($"No connector assemblies found in {connectorDirectory}.");

        var runtime = ConnectorRuntime.Load(name, activeDirectory, _logger);
        Swap(name, runtime, mustExist: false);
        return runtime;
    }

    /// <summary>
    /// Loads <paramref name="versionDirectory"/> as the new version of connector <paramref name="name"/> and puts it
    /// in service, then retires the old version: new work uses the new version immediately, and the old one is
    /// unloaded once the requests and jobs already using it finish. The returned task completes at that point.
    /// If the new version fails to load, the old one stays in service.
    /// </summary>
    public Task<ConnectorUnloadResult> ReplaceConnectorAsync(string name, string versionDirectory)
    {
        var runtime = ConnectorRuntime.Load(name, versionDirectory, _logger);
        var old = Swap(name, runtime, mustExist: true)!;
        return RetireAsync(old);
    }

    /// <summary>
    /// Takes connector <paramref name="name"/> out of service and unloads it once the work using it finishes.
    /// </summary>
    public Task<ConnectorUnloadResult> UnloadConnectorAsync(string name)
    {
        var old = Swap(name, replacement: null, mustExist: true)!;
        return RetireAsync(old);
    }

    /// <summary>
    /// Loads the newest version of a connector that loads, so one bad version folder (say an incompatible
    /// update) does not stop the app from starting; older versions and then the unversioned folder are tried.
    /// </summary>
    private ConnectorRuntime? LoadNewestWorkingVersion(string connectorDirectory)
    {
        var name = Path.GetFileName(connectorDirectory);
        var candidates = CandidateDirectories(connectorDirectory);
        if (candidates.Count == 0)
        {
            _logger.LogWarning("No connector assemblies found in {Directory}", connectorDirectory);
            return null;
        }

        foreach (var candidate in candidates)
        {
            try
            {
                return ConnectorRuntime.Load(name, candidate, _logger);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not load connector {Name} from {Directory}; trying an older version if there is one.", name, candidate);
            }
        }

        _logger.LogError("No version of connector {Name} could be loaded.", name);
        return null;
    }

    /// <summary>
    /// Folders a connector could be loaded from, preferred first: version subfolders from highest to lowest,
    /// then the connector folder itself when it contains DLLs (the original unversioned layout).
    /// </summary>
    public static IReadOnlyList<string> CandidateDirectories(string connectorDirectory)
    {
        var candidates = Directory.GetDirectories(connectorDirectory)
            .Select(d => (Path: d, Version: Version.TryParse(Path.GetFileName(d), out var v) ? v : null))
            .Where(d => d.Version is not null && Directory.EnumerateFiles(d.Path, "*.dll").Any())
            .OrderByDescending(d => d.Version)
            .Select(d => d.Path)
            .ToList();

        if (Directory.EnumerateFiles(connectorDirectory, "*.dll").Any()) candidates.Add(connectorDirectory);
        return candidates;
    }

    /// <summary>
    /// Picks the folder to load for a connector: its highest version subfolder (connectors/&lt;name&gt;/&lt;version&gt;/,
    /// which is where updates are installed), otherwise the connector folder itself when it contains DLLs
    /// (the original unversioned layout).
    /// </summary>
    public static string? ResolveActiveDirectory(string connectorDirectory) =>
        CandidateDirectories(connectorDirectory).FirstOrDefault();

    /// <summary>Replaces the runtime named <paramref name="name"/> in the catalog and returns the old one.</summary>
    private ConnectorRuntime? Swap(string name, ConnectorRuntime? replacement, bool mustExist)
    {
        try
        {
            lock (_gate)
            {
                var catalog = _catalog;
                var old = catalog.Runtimes.FirstOrDefault(r => string.Equals(r.Name, name, StringComparison.Ordinal));

                if (old is null && mustExist) throw new InvalidOperationException($"Connector {name} is not loaded.");
                if (old is not null && !mustExist) throw new InvalidOperationException($"Connector {name} is already loaded.");

                var runtimes = catalog.Runtimes.Where(r => r != old).ToList();
                if (replacement is not null) runtimes.Add(replacement);

                // Building the catalog throws on conflicting registrations, before anything is swapped.
                var updated = ConnectorCatalog.Create(runtimes);

                if (old is not null) _retiring = _retiring.Add(old);
                Volatile.Write(ref _catalog, updated);
                return old;
            }
        }
        catch
        {
            replacement?.ShutDown();
            throw;
        }
    }

    private async Task<ConnectorUnloadResult> RetireAsync(ConnectorRuntime runtime)
    {
        var name = runtime.Name;
        var version = runtime.Version;
        _logger.LogInformation("Connector {Name} {Version} retired; waiting for work using it to finish.", name, version);

        await runtime.RetireAsync().ConfigureAwait(false);

        var loadContext = ShutDown(runtime);
        lock (_gate)
        {
            _retiring = _retiring.Remove(runtime);
        }
        runtime = null!; // Drop this state machine's reference so the load context can be collected.

        var collected = await WaitForCollectionAsync(loadContext).ConfigureAwait(false);
        if (collected)
        {
            _logger.LogInformation("Connector {Name} {Version} unloaded.", name, version);
        }
        else
        {
            _logger.LogWarning("Connector {Name} {Version} was shut down but is still referenced, so its memory was not released. Restart the process to release it.", name, version);
        }

        return new ConnectorUnloadResult(name, version, collected);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference ShutDown(ConnectorRuntime runtime) => runtime.ShutDown();

    private static async Task<bool> WaitForCollectionAsync(WeakReference loadContext)
    {
        for (var attempt = 0; attempt < 20 && loadContext.IsAlive; attempt++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (!loadContext.IsAlive) break;
            await Task.Delay(100).ConfigureAwait(false);
        }

        return !loadContext.IsAlive;
    }

    private List<Type> LoadPayloads()
    {
        // Payloads come from Broker.Common (default context), not connector load contexts
        var types = typeof(Payload).Assembly.GetExportedTypes()
            .Where(p => p.IsSubclassOf(typeof(Payload)) && !p.IsAbstract)
            .ToList();

        foreach (var type in types)
        {
            _logger.LogInformation("Payload loaded: {Type} from {AssemblyQualifiedName}", type.FullName, type.AssemblyQualifiedName);
        }

        return types;
    }
}

/// <summary>Outcome of unloading a connector version.</summary>
/// <param name="Collected">False when something still references the old version, so its memory is only released on restart.</param>
public record ConnectorUnloadResult(string Name, Version? Version, bool Collected);

/// <summary>Immutable index of the connectors in service.</summary>
internal sealed class ConnectorCatalog
{
    public static readonly ConnectorCatalog Empty = Create([]);

    public required IReadOnlyList<ConnectorRuntime> Runtimes { get; init; }
    public required IReadOnlyList<Type> Connectors { get; init; }
    public required IReadOnlyList<Type> Authenticators { get; init; }
    public required IReadOnlyList<Type> PayloadJobs { get; init; }
    public required IReadOnlyList<Type> PayloadContentActions { get; init; }
    public required IReadOnlyDictionary<string, Type> Transformers { get; init; }
    public required IReadOnlyDictionary<Type, Type> Importers { get; init; }
    public required IReadOnlyDictionary<string, Type> PayloadContentActionByTransformer { get; init; }
    public required IReadOnlyDictionary<string, Assembly> Assemblies { get; init; }
    public required IReadOnlyDictionary<string, ConnectorLoadContext> LoadContexts { get; init; }

    /// <summary>Merges the runtimes' indexes. Throws if two connectors register the same key.</summary>
    public static ConnectorCatalog Create(IReadOnlyList<ConnectorRuntime> runtimes)
    {
        return new ConnectorCatalog
        {
            Runtimes = runtimes.ToImmutableArray(),
            Connectors = runtimes.Select(r => r.ConnectorType).ToImmutableArray(),
            Authenticators = runtimes.SelectMany(r => r.Authenticators).ToImmutableArray(),
            PayloadJobs = runtimes.SelectMany(r => r.PayloadJobs).ToImmutableArray(),
            PayloadContentActions = runtimes.SelectMany(r => r.PayloadContentActions).ToImmutableArray(),
            Transformers = runtimes.SelectMany(r => r.Transformers).ToImmutableDictionary(),
            Importers = runtimes.SelectMany(r => r.Importers).ToImmutableDictionary(),
            PayloadContentActionByTransformer = runtimes.SelectMany(r => r.PayloadContentActionByTransformer).ToImmutableDictionary(),
            Assemblies = runtimes.ToImmutableDictionary(r => r.ConnectorType.Assembly.GetName().Name!, r => r.ConnectorType.Assembly),
            LoadContexts = runtimes.ToImmutableDictionary(r => r.LoadContext.Name!, r => r.LoadContext),
        };
    }
}
