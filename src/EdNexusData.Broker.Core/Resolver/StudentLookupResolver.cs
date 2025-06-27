using Microsoft.Extensions.DependencyInjection;
using EdNexusData.Broker.Common.StudentLookup;
using Ardalis.GuardClauses;
using System.Runtime.Loader;

namespace EdNexusData.Broker.Core.Resolvers;

public class StudentLookupResolver
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TypeResolver typeResolver;
    private readonly ConnectorLoader connectorLoader;

    public StudentLookupResolver(
        IServiceProvider serviceProvider,
        TypeResolver typeResolver,
        ConnectorLoader connectorLoader
    )
    {
        _serviceProvider = serviceProvider;
        this.typeResolver = typeResolver;
        this.connectorLoader = connectorLoader;
    }

    public IStudentLookupService Resolve(Type TConnector)
    {
        // var assembly = TConnector.Assembly.GetExportedTypes();
        // // Locate the student lookup service in connector

        // var studentLookupServiceType = assembly
        //     .Where(x => x.GetInterface("IStudentLookupService") != null
        //              && x.IsAbstract == false)
        //     .FirstOrDefault();

        var studentLookupServiceType = typeResolver.ResolveConnectorInterface(TConnector.Assembly, "IStudentLookupService")?.FirstOrDefault();
        var brokerServiceProvider = connectorLoader.ConnectorServiceProviders
            .FirstOrDefault(x => x.Key == TConnector.Assembly.GetName().Name).Value;

        Guard.Against.Null(studentLookupServiceType, "", "Could not get student lookup type");

        var connectorStudentLookupService = 
            ActivatorUtilities.CreateInstance(brokerServiceProvider, studentLookupServiceType);
        
        return (IStudentLookupService)connectorStudentLookupService;
    }
}