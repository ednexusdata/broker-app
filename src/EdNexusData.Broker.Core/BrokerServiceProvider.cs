using EdNexusData.Broker.Common.Connector;
using EdNexusData.Broker.Core.Resolvers;
using Microsoft.Extensions.DependencyInjection;

namespace EdNexusData.Broker.Core;

public class BrokerServiceProvider : IServiceProvider
{
    private readonly IServiceProvider _primary;
    private readonly IServiceProvider _fallback;

    public BrokerServiceProvider(IServiceProvider primary, IServiceProvider fallback)
    {
        _primary = primary;
        _fallback = fallback;
    }

    public object GetService(Type serviceType)
    {
        Console.WriteLine($"Attempting to resolve service of type: {serviceType.FullName}");

        if (_primary.GetService(serviceType) is not null)
        {
            Console.WriteLine($"Service of type {serviceType.FullName} found in primary provider.");
            return _primary.GetService(serviceType)!;
        } else if (_fallback.GetService(serviceType) is not null)
        {
            Console.WriteLine($"Service of type {serviceType.FullName} found in fallback provider.");
            return _fallback.GetService(serviceType)!;
        }

        throw new InvalidOperationException($"Service of type {serviceType.FullName} not found in either provider.");
    }

    public static void AddConnectorDependencies(IServiceProvider defaultServiceProvider)
    {
        // var connectorLoader = Activator.CreateInstance<ConnectorLoader>();
        // _ = connectorLoader ?? throw new InvalidOperationException("ConnectorLoader could not be instantiated.");

        // var types = AppDomain.CurrentDomain.GetAssemblies()
        //                 .SelectMany(s => s.GetExportedTypes())
        //                 .Where(p => p.GetInterface(nameof(IConnectorServiceCollection)) is not null);
        
        var connectorLoader = defaultServiceProvider.GetRequiredService<ConnectorLoader>();

        var types = TypeResolver.ResolveConnectorInterface(connectorLoader, nameof(IConnectorServiceCollection));
        if (types is not null && types.Count > 0)
        {
            foreach (var type in types)
            {
                // Create service collection instance
                var serviceCollection = new ServiceCollection();

                // Call AddDependencies on each service collection
                var myMethod = type?.GetMethod("AddDependencies");
                myMethod!.Invoke(null, [serviceCollection]);

                // Build the service provider and register it
                var connectorServiceProvider = serviceCollection.BuildServiceProvider();
                var brokerServiceProvider = new BrokerServiceProvider(connectorServiceProvider, defaultServiceProvider);

                serviceCollection.AddSingleton<BrokerServiceProvider>(brokerServiceProvider);

                connectorLoader.ConnectorServiceProviders.Add(type!.Assembly.GetName().Name!, brokerServiceProvider);
            }
        }
    }
}
