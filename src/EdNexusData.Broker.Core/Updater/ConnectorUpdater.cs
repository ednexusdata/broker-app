using EdNexusData.Broker.Core.Models;
using EdNexusData.Broker.Core.Services;
using Microsoft.Extensions.Logging;

namespace EdNexusData.Broker.Core.Updaters;

/// <summary>
/// Brings the connectors this process runs in line with the connector references saved from the store page:
/// a reference with Version "*" follows the store's latest release, any other version is pinned.
/// New versions are installed next to the running one and swapped in live; work already using the old
/// version finishes on it first.
/// </summary>
public class ConnectorUpdater
{
    private readonly ConnectorService connectorService;
    private readonly ConnectorLoader connectorLoader;
    private readonly ConnectorPackageInstaller installer;
    private readonly ILogger<ConnectorUpdater> logger;

    public ConnectorUpdater(
        ConnectorService connectorService,
        ConnectorLoader connectorLoader,
        ConnectorPackageInstaller installer,
        ILogger<ConnectorUpdater> logger
    )
    {
        this.connectorService = connectorService;
        this.connectorLoader = connectorLoader;
        this.installer = installer;
        this.logger = logger;
    }

    /// <summary>Checks the store and installs or updates every connector reference that needs it.</summary>
    public async Task<IReadOnlyList<ConnectorUpdateResult>> Synchronize(CancellationToken cancellationToken = default)
    {
        var plans = await Analyze(cancellationToken);
        var results = new List<ConnectorUpdateResult>();

        foreach (var plan in plans)
        {
            if (plan.Action == ConnectorUpdateAction.None)
            {
                results.Add(new ConnectorUpdateResult(plan, Succeeded: true, plan.Reason));
                continue;
            }

            string? versionDirectory = null;
            try
            {
                versionDirectory = await installer.InstallAsync(plan.Name, plan.TargetVersion!, plan.Package!, cancellationToken);

                if (plan.Action == ConnectorUpdateAction.Install)
                {
                    connectorLoader.LoadConnector(Path.GetDirectoryName(versionDirectory)!);
                }
                else
                {
                    // Returns once the new version is in service; the old one unloads when its work finishes.
                    var retirement = connectorLoader.ReplaceConnectorAsync(plan.Name, versionDirectory);
                    _ = retirement.ContinueWith(
                        t => logger.LogError(t.Exception, "Retiring the old version of connector {Name} failed.", plan.Name),
                        CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
                }

                logger.LogInformation("Connector {Name} {Action}: {From} -> {To}.", plan.Name, plan.Action, plan.InstalledVersion?.ToString() ?? "not installed", plan.TargetVersion);
                results.Add(new ConnectorUpdateResult(plan, Succeeded: true, $"{plan.Action} to {plan.TargetVersion}"));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Connector {Name} {Action} to {Version} failed; the running version (if any) stays in service.", plan.Name, plan.Action, plan.TargetVersion);

                // A version that will not load must not stay on disk: the next start would try it first.
                if (versionDirectory is not null) RemoveVersionThatDoesNotLoad(versionDirectory);

                // Network trouble is retried at the next check; a bad or incompatible package is not.
                if (ex is not (HttpRequestException or TimeoutException))
                {
                    installer.RememberFailure(plan.Name, plan.TargetVersion!, plan.Package!, ex.Message);
                }

                results.Add(new ConnectorUpdateResult(plan, Succeeded: false, ex.Message));
            }
        }

        return results;
    }

    /// <summary>What each connector reference needs, without changing anything.</summary>
    public async Task<IReadOnlyList<ConnectorUpdatePlan>> Analyze(CancellationToken cancellationToken = default)
    {
        var storeList = await connectorService.GetStoreConnectors() ?? [];
        var connectorReferences = await connectorService.GetConnectorReferences();
        var plans = new List<ConnectorUpdatePlan>();

        foreach (var connectorReference in connectorReferences)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var name = connectorReference.Reference;
            var runtime = connectorLoader.Runtimes.FirstOrDefault(r => r.Name == name);
            var installed = runtime is null ? null : InstalledVersion(runtime);
            ConnectorUpdatePlan Skip(string reason) => new(name, ConnectorUpdateAction.None, installed, null, null, reason);

            var storeConnector = storeList.FirstOrDefault(x => x.ReferenceName == name);
            if (storeConnector is null)
            {
                plans.Add(Skip("Not listed in the connector store."));
                continue;
            }

            var release = storeConnector.FindRelease(connectorReference.Version);
            var target = release?.ToSystemVersion();
            if (release is null || target is null)
            {
                plans.Add(Skip($"The store has no release matching version '{connectorReference.Version}'."));
                continue;
            }

            var followsLatest = string.IsNullOrWhiteSpace(connectorReference.Version) || connectorReference.Version == "*";
            if (installed is not null && (installed == target || (followsLatest && installed > target)))
            {
                plans.Add(Skip($"Up to date ({installed})."));
                continue;
            }

            var package = release.PackageFor(ConnectorPackageInstaller.CurrentPlatforms);
            if (package is null)
            {
                plans.Add(Skip($"Release {target} has no installable package for {string.Join("/", ConnectorPackageInstaller.CurrentPlatforms)} (a package needs an https URL and a SHA-256)."));
                continue;
            }

            if (installer.PreviousFailure(name, target, package) is string failure)
            {
                plans.Add(Skip($"Release {target} failed to install earlier and is not retried until the store lists a different package: {failure}"));
                continue;
            }

            var action = runtime is null ? ConnectorUpdateAction.Install : ConnectorUpdateAction.Update;
            plans.Add(new ConnectorUpdatePlan(name, action, installed, target, package, null));
        }

        return plans;
    }

    private void RemoveVersionThatDoesNotLoad(string versionDirectory)
    {
        var inUse = connectorLoader.Runtimes.Any(r => string.Equals(
            Path.TrimEndingDirectorySeparator(r.Directory), Path.TrimEndingDirectorySeparator(versionDirectory), StringComparison.Ordinal));
        if (inUse || !Directory.Exists(versionDirectory)) return;

        try
        {
            Directory.Delete(versionDirectory, recursive: true);
            logger.LogInformation("Removed {Directory}, which could not be loaded.", versionDirectory);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            logger.LogWarning(ex, "Could not remove {Directory}; on restart older versions are tried if it still fails to load.", versionDirectory);
        }
    }

    /// <summary>
    /// The installed version: the version folder it was loaded from (connectors/&lt;name&gt;/&lt;version&gt;/), which is
    /// what the store calls it, otherwise its assembly version.
    /// </summary>
    public static Version? InstalledVersion(ConnectorRuntime runtime)
    {
        var parent = Path.GetFileName(Path.GetDirectoryName(Path.TrimEndingDirectorySeparator(runtime.Directory)));
        if (parent == runtime.Name && StoreConnectorRelease.ParseVersion(Path.GetFileName(runtime.Directory)) is Version folderVersion)
        {
            return folderVersion;
        }

        return runtime.Version is null ? null : StoreConnectorRelease.Normalize(runtime.Version);
    }
}

public enum ConnectorUpdateAction
{
    None,
    Install,
    Update,
}

/// <param name="Reason">Why nothing will be done, when <see cref="Action"/> is None.</param>
public record ConnectorUpdatePlan(
    string Name,
    ConnectorUpdateAction Action,
    Version? InstalledVersion,
    Version? TargetVersion,
    StoreConnectorPackage? Package,
    string? Reason);

public record ConnectorUpdateResult(ConnectorUpdatePlan Plan, bool Succeeded, string? Message);
