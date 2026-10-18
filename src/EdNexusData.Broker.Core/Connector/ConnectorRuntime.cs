using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using EdNexusData.Broker.Common.Attributes;
using EdNexusData.Broker.Common.Authentication;
using EdNexusData.Broker.Common.Configuration;
using EdNexusData.Broker.Common.Connector;
using EdNexusData.Broker.Common.Jobs;
using EdNexusData.Broker.Common.Mappings;
using EdNexusData.Broker.Common.PayloadContents;
using EdNexusData.Broker.Common.PayloadContentActions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EdNexusData.Broker.Core;

/// <summary>
/// One loaded version of a connector: its collectible load context, the types it contributes,
/// the services it registers, and the Autofac container holding them.
/// Instances are immutable once loaded; replacing a connector creates a new runtime.
/// </summary>
/// <remarks>
/// The connector's container is deliberately not a child of the app container. Autofac child scopes query
/// their parents for every service they resolve, and the app container would then keep registrations for
/// connector types (for example ILogger&lt;ConnectorType&gt;), so the connector could never unload. Instead,
/// app services reach connector code only by forwarding (<see cref="ForwardAppServicesSource"/>), and app
/// generics over connector types are built inside the connector container (<see cref="AppGenericsSource"/>).
/// </remarks>
public sealed class ConnectorRuntime
{
    private readonly object _gate = new();
    private readonly TaskCompletionSource _drained = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private IContainer? _container;
    private int _leases;
    private bool _retired;
    private bool _shutDown;

    /// <summary>Connector folder name, e.g. EdNexusData.Broker.Connector.Fixture. Unique among loaded connectors.</summary>
    public string Name { get; }
    public string Directory { get; }
    public ConnectorLoadContext LoadContext { get; }
    public IReadOnlyList<Assembly> Assemblies { get; }
    public Type ConnectorType { get; }
    public Version? Version => ConnectorType.Assembly.GetName().Version;

    public IReadOnlyList<Type> Configurations { get; }
    public IReadOnlyList<Type> Authenticators { get; }
    public IReadOnlyList<Type> PayloadJobs { get; }
    public IReadOnlyList<Type> PayloadContentActions { get; }
    public IReadOnlyDictionary<string, Type> Transformers { get; }
    public IReadOnlyDictionary<Type, Type> Importers { get; }
    public IReadOnlyDictionary<string, Type> PayloadContentActionByTransformer { get; }

    /// <summary>Services the connector registers through its IConnectorServiceCollection.AddDependencies.</summary>
    public IReadOnlyList<ServiceDescriptor> Services { get; }

    public bool IsRetired { get { lock (_gate) return _retired; } }

    private ConnectorRuntime(string name, string directory, ConnectorLoadContext loadContext, ILogger logger)
    {
        Name = name;
        Directory = directory;
        LoadContext = loadContext;
        Assemblies = loadContext.Assemblies.ToList();

        var exportedTypes = Assemblies.SelectMany(GetExportedTypesSafe).ToList();

        ConnectorType = exportedTypes.FirstOrDefault(t => t.GetInterface(nameof(IConnector)) is not null)
            ?? throw new InvalidOperationException($"No IConnector implementation found in connector {name} ({directory}).");

        // Index only the connector's own assembly, matching the original loader.
        var connectorTypes = GetExportedTypesSafe(ConnectorType.Assembly).ToList();

        Configurations = connectorTypes.Where(t => t.GetInterface(nameof(IConfiguration)) is not null).ToList();
        Authenticators = connectorTypes.Where(t => t.GetInterface(nameof(IAuthenticationProvider)) is not null).ToList();
        PayloadJobs = connectorTypes.Where(t => t.IsAssignableTo(typeof(PayloadJob)) && !t.IsAbstract).ToList();
        PayloadContentActions = connectorTypes.Where(t => t.IsAssignableTo(typeof(PayloadContentActionJob)) && !t.IsAbstract).ToList();
        Transformers = IndexTransformers(connectorTypes);
        Importers = IndexImporters(connectorTypes);
        PayloadContentActionByTransformer = IndexPayloadContentActionsByTransformer(PayloadContentActions);
        Services = CollectServices(exportedTypes);

        logger.LogInformation("Connector loaded: {Name} {Version} from {Directory}", Name, Version, Directory);
    }

    /// <summary>Loads every DLL in <paramref name="directory"/> into a new collectible load context.</summary>
    public static ConnectorRuntime Load(string name, string directory, ILogger logger)
    {
        var context = new ConnectorLoadContext(directory, name);
        try
        {
            foreach (var file in System.IO.Directory.GetFiles(directory, "*.dll"))
            {
                try
                {
                    context.LoadFromAssemblyPath(file);
                }
                catch (BadImageFormatException)
                {
                    // Native or non-.NET DLL shipped alongside the connector.
                }
                catch (FileLoadException)
                {
                    // Already loaded, e.g. as a dependency of an earlier file.
                }
            }

            return new ConnectorRuntime(name, directory, context, logger);
        }
        catch
        {
            context.Unload();
            throw;
        }
    }

    /// <summary>
    /// The connector's container, holding its registrations for as long as this version is loaded.
    /// Connector singletons live here; app services are forwarded from <paramref name="appRootScope"/>.
    /// </summary>
    internal IContainer GetContainer(ILifetimeScope appRootScope, IReadOnlyList<ServiceDescriptor> appOpenGenerics)
    {
        lock (_gate)
        {
            if (_shutDown) throw new ObjectDisposedException($"Connector {Name} {Version} has been unloaded.");

            if (_container is null)
            {
                var builder = new ContainerBuilder();
                builder.RegisterSource(new ForwardAppServicesSource(appRootScope));
                builder.RegisterSource(new AppGenericsSource(appOpenGenerics));
                builder.Populate(Services);
                _container = builder.Build();
            }

            return _container;
        }
    }

    /// <summary>
    /// Begins this connector's scope for one request or job. App services resolve from <paramref name="requestScope"/>,
    /// so connector code shares the request's instances (DbContext, IConfigurationResolver, ...).
    /// </summary>
    internal ILifetimeScope BeginRequestScope(ILifetimeScope requestScope, ILifetimeScope appRootScope, IReadOnlyList<ServiceDescriptor> appOpenGenerics, ConnectorActivator activator)
    {
        return GetContainer(appRootScope, appOpenGenerics).BeginLifetimeScope(builder =>
        {
            builder.RegisterSource(new ForwardAppServicesSource(requestScope));

            // Connector code that creates further types reuses this request's connector scopes.
            builder.RegisterInstance(activator).ExternallyOwned();
        });
    }

    /// <summary>Marks the runtime as in use. Returns false once it has been shut down.</summary>
    internal bool TryAcquireLease()
    {
        lock (_gate)
        {
            if (_shutDown) return false;
            _leases++;
            return true;
        }
    }

    internal void ReleaseLease()
    {
        lock (_gate)
        {
            _leases--;
            if (_retired && _leases == 0) _drained.TrySetResult();
        }
    }

    /// <summary>Stops the runtime being handed out and returns a task that completes when no work is using it.</summary>
    internal Task RetireAsync()
    {
        lock (_gate)
        {
            _retired = true;
            if (_leases == 0) _drained.TrySetResult();
        }
        return _drained.Task;
    }

    /// <summary>
    /// Disposes the connector container (and with it the connector's singletons), clears Autofac's shared
    /// reflection cache for this load context, and unloads it. Returns a weak reference to check that it was
    /// actually collected.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal WeakReference ShutDown()
    {
        IContainer? container;
        lock (_gate)
        {
            _shutDown = true;
            container = _container;
            _container = null;
        }

        container?.Dispose();

        // Some cache entries are keyed by app generics closed over connector types (e.g. Logger<ConnectorType>),
        // whose own assembly is the app's, so the generic arguments are checked as well.
        var loadContext = LoadContext;
        bool InThisConnector(Assembly assembly) => AssemblyLoadContext.GetLoadContext(assembly) == loadContext;
        ReflectionCacheSet.Shared.Clear((member, referencedAssemblies) =>
            referencedAssemblies.Any(InThisConnector) || (member is Type type && AssembliesOf(type).Any(InThisConnector)));

        ClearFrameworkReflectionCaches();

        LoadContext.Unload();
        return new WeakReference(LoadContext);
    }

    /// <summary>
    /// Clears the reflection caches .NET libraries keep for Hot Reload (for example System.ComponentModel's
    /// TypeDescriptor caches), which would otherwise keep connector types alive. Each library registers its
    /// clearing method with [assembly: MetadataUpdateHandler]; passing null asks it to clear everything.
    /// </summary>
    private static void ClearFrameworkReflectionCaches()
    {
        foreach (var assembly in AssemblyLoadContext.Default.Assemblies)
        {
            foreach (var handler in assembly.GetCustomAttributes<System.Reflection.Metadata.MetadataUpdateHandlerAttribute>())
            {
                var clearCache = handler.HandlerType.GetMethod("ClearCache", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, [typeof(Type[])]);
                clearCache?.Invoke(null, [null]);
            }
        }
    }

    /// <summary>Assemblies <paramref name="type"/> is built from, including generic arguments and element types.</summary>
    internal static IEnumerable<Assembly> AssembliesOf(Type type)
    {
        if (type.HasElementType)
        {
            foreach (var assembly in AssembliesOf(type.GetElementType()!)) yield return assembly;
            yield break;
        }

        yield return type.Assembly;

        if (type.IsConstructedGenericType)
        {
            foreach (var argument in type.GetGenericArguments())
            {
                foreach (var assembly in AssembliesOf(argument)) yield return assembly;
            }
        }
    }

    private static IEnumerable<Type> GetExportedTypesSafe(Assembly assembly)
    {
        try
        {
            return assembly.GetExportedTypes();
        }
        catch (Exception ex) when (ex is ReflectionTypeLoadException or FileNotFoundException or FileLoadException)
        {
            return [];
        }
    }

    private List<ServiceDescriptor> CollectServices(IEnumerable<Type> exportedTypes)
    {
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

        foreach (var type in exportedTypes.Where(t => t.GetInterface(nameof(IConnectorServiceCollection)) is not null))
        {
            var addDependencies = type.GetMethod("AddDependencies", BindingFlags.Public | BindingFlags.Static)
                ?? throw new InvalidOperationException($"{type.FullName} does not define a public static AddDependencies method.");
            addDependencies.Invoke(null, [services]);
        }

        return services.ToList();
    }

    private static Dictionary<string, Type> IndexTransformers(IEnumerable<Type> types)
    {
        var transformers = new Dictionary<string, Type>();

        foreach (var transformer in types.Where(t => t.GetInterface("ITransformer`2") is not null && !t.IsAbstract))
        {
            foreach (var mapsFrom in transformer.GetCustomAttributes(false).OfType<MapsFromAttribute>())
            {
                transformers.Add($"{transformer.Assembly.GetName().Name}::{mapsFrom.Schema}::{mapsFrom.SchemaVersion}", transformer);
            }
        }

        return transformers;
    }

    private static Dictionary<Type, Type> IndexImporters(IEnumerable<Type> types)
    {
        var importers = new Dictionary<Type, Type>();

        foreach (var importer in types.Where(t => t.GetInterface("IImporter") is not null && !t.IsAbstract))
        {
            foreach (var imports in importer.GetCustomAttributes(false).OfType<ImportsAttribute>())
            {
                foreach (var importType in imports.Types)
                {
                    importers.Add(importType, importer);
                }
            }
        }

        return importers;
    }

    private static Dictionary<string, Type> IndexPayloadContentActionsByTransformer(IEnumerable<Type> payloadContentActions)
    {
        var byTransformer = new Dictionary<string, Type>();

        foreach (var payloadContentAction in payloadContentActions)
        {
            var processes = payloadContentAction.GetCustomAttribute<ProcessesTransformerAttribute>();
            if (processes is null) continue;

            foreach (var transformerType in processes.ProcessesTransformerTypes)
            {
                if (transformerType == typeof(DocumentPayloadContent))
                {
                    byTransformer.Add($"{payloadContentAction.Assembly.GetName().Name}::{transformerType.Assembly.GetName().Name}.{transformerType.Name}", payloadContentAction);
                    continue;
                }

                foreach (var mapsFrom in transformerType.GetCustomAttributes(false).OfType<MapsFromAttribute>())
                {
                    byTransformer.Add($"{transformerType.Assembly.GetName().Name}::{mapsFrom.Schema}::{mapsFrom.SchemaVersion}", payloadContentAction);
                }
            }
        }

        return byTransformer;
    }
}

/// <summary>
/// Resolves app services inside a connector container or scope from an app scope, so connector code shares
/// the app's instances (in a request: the request's DbContext, IConfigurationResolver, ...).
/// Only types without connector parts are forwarded, so the app container never sees a connector type.
/// </summary>
internal sealed class ForwardAppServicesSource : IRegistrationSource
{
    // The DI infrastructure must resolve to the connector scope itself, not the app scope.
    private static readonly HashSet<Type> ScopeInfrastructure =
    [
        typeof(ILifetimeScope),
        typeof(IComponentContext),
        typeof(IServiceProvider),
        typeof(IServiceScopeFactory),
        typeof(IServiceProviderIsService),
        typeof(IServiceProviderIsKeyedService),
        typeof(IKeyedServiceProvider),
    ];

    private readonly ILifetimeScope _appScope;

    public ForwardAppServicesSource(ILifetimeScope appScope)
    {
        _appScope = appScope;
    }

    public bool IsAdapterForIndividualComponents => false;

    public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor)
    {
        if (service is not IServiceWithType serviceWithType) return [];

        var serviceType = serviceWithType.ServiceType;
        if (ScopeInfrastructure.Contains(serviceType)) return [];

        // Never ask the app container about connector types: it would keep them in its registry and
        // the connector could never unload. Those resolve within the connector container instead.
        if (ConnectorRuntime.AssembliesOf(serviceType).Any(a => a.IsCollectible)) return [];

        if (!_appScope.ComponentRegistry.IsRegistered(service)) return [];

        var registration = RegistrationBuilder
            .ForDelegate(serviceType, (_, parameters) => _appScope.ResolveService(service, parameters))
            .As(service)
            .ExternallyOwned()
            .CreateRegistration();

        return [registration];
    }
}

/// <summary>
/// Builds the app's open-generic services closed over connector types (ILogger&lt;T&gt;, IOptions&lt;T&gt;,
/// IRepository&lt;T&gt;, ...) inside the connector container, with the lifetime the app registered them with.
/// </summary>
internal sealed class AppGenericsSource : IRegistrationSource
{
    private readonly IReadOnlyList<ServiceDescriptor> _appOpenGenerics;

    public AppGenericsSource(IReadOnlyList<ServiceDescriptor> appOpenGenerics)
    {
        _appOpenGenerics = appOpenGenerics;
    }

    public bool IsAdapterForIndividualComponents => false;

    public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor)
    {
        if (service is not TypedService typed) return [];

        var serviceType = typed.ServiceType;
        if (!serviceType.IsConstructedGenericType) return [];
        if (!ConnectorRuntime.AssembliesOf(serviceType).Any(a => a.IsCollectible)) return [];

        var definition = serviceType.GetGenericTypeDefinition();
        var descriptor = _appOpenGenerics.LastOrDefault(d => d.ServiceType == definition);
        if (descriptor?.ImplementationType is null) return [];

        Type implementationType;
        try
        {
            implementationType = descriptor.ImplementationType.MakeGenericType(serviceType.GetGenericArguments());
        }
        catch (ArgumentException)
        {
            // Generic constraints not satisfied.
            return [];
        }

        var registration = RegistrationBuilder.ForType(implementationType).As(service);
        switch (descriptor.Lifetime)
        {
            case ServiceLifetime.Singleton:
                registration.SingleInstance();
                break;
            case ServiceLifetime.Scoped:
                registration.InstancePerLifetimeScope();
                break;
            default:
                registration.InstancePerDependency();
                break;
        }

        return [registration.CreateRegistration()];
    }
}
