using Autofac;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace EdNexusData.Broker.Core;

/// <summary>
/// Creates connector types inside their connector's own DI scope, so they can use the connector's
/// registrations as well as the app's, and so the connector can be unloaded once the work using it ends.
/// Scoped: one per request or job. Each connector version touched gets one child scope, which holds a
/// lease on that version until this activator is disposed with the request or job scope.
/// </summary>
public sealed class ConnectorActivator : IAsyncDisposable, IDisposable
{
    private readonly ILifetimeScope _requestScope;
    private readonly ConnectorLoader _connectorLoader;
    private readonly Dictionary<ConnectorRuntime, ILifetimeScope> _connectorScopes = new();
    private readonly object _gate = new();
    private bool _disposed;

    public ConnectorActivator(ILifetimeScope requestScope, ConnectorLoader connectorLoader)
    {
        if (requestScope.Tag == LifetimeScope.RootTag)
        {
            throw new InvalidOperationException(
                $"{nameof(ConnectorActivator)} must be resolved from a request or job scope, not the root container, or connectors can never be unloaded.");
        }

        _requestScope = requestScope;
        _connectorLoader = connectorLoader;
    }

    /// <summary>
    /// Drop-in replacement for <see cref="ActivatorUtilities.CreateInstance(IServiceProvider, Type, object[])"/>:
    /// connector types are created in their connector's scope, everything else from <paramref name="provider"/>.
    /// </summary>
    public static object CreateInstance(IServiceProvider provider, Type type, params object[] parameters)
    {
        var activator = provider.GetService<ConnectorActivator>();
        return activator is null
            ? ActivatorUtilities.CreateInstance(provider, type, parameters)
            : activator.CreateInstance(type, provider, parameters);
    }

    public object CreateInstance(Type type, IServiceProvider fallbackProvider, params object[] parameters)
    {
        var runtime = _connectorLoader.FindRuntime(type);
        var provider = runtime is null ? fallbackProvider : GetServiceProvider(runtime);
        return ActivatorUtilities.CreateInstance(provider, type, parameters);
    }

    /// <summary>The service provider for <paramref name="runtime"/> within the current request or job.</summary>
    public IServiceProvider GetServiceProvider(ConnectorRuntime runtime)
    {
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (!_connectorScopes.TryGetValue(runtime, out var scope))
            {
                if (!runtime.TryAcquireLease())
                {
                    throw new InvalidOperationException($"Connector {runtime.Name} {runtime.Version} has been unloaded.");
                }

                try
                {
                    var appRootScope = ((ISharingLifetimeScope)_requestScope).RootLifetimeScope;
                    scope = runtime.BeginRequestScope(_requestScope, appRootScope, _connectorLoader.AppOpenGenerics, this);
                }
                catch
                {
                    runtime.ReleaseLease();
                    throw;
                }

                _connectorScopes.Add(runtime, scope);
            }

            return scope.Resolve<IServiceProvider>();
        }
    }

    public void Dispose() => DisposeAsync().AsTask().GetAwaiter().GetResult();

    public async ValueTask DisposeAsync()
    {
        KeyValuePair<ConnectorRuntime, ILifetimeScope>[] scopes;
        lock (_gate)
        {
            if (_disposed) return;
            _disposed = true;
            scopes = _connectorScopes.ToArray();
            _connectorScopes.Clear();
        }

        foreach (var (runtime, scope) in scopes)
        {
            try
            {
                await scope.DisposeAsync();
            }
            finally
            {
                runtime.ReleaseLease();
            }
        }
    }
}
