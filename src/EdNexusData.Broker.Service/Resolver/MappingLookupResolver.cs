using Microsoft.Extensions.DependencyInjection;
using Ardalis.GuardClauses;
using EdNexusData.Broker.Connector;

namespace EdNexusData.Broker.Service.Resolvers;

public class MappingLookupResolver
{
    private readonly IServiceProvider _serviceProvider;
    
    public MappingLookupResolver(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IMappingLookup Resolve(Type TConnector)
    {
        var assembly = TConnector.Assembly.GetExportedTypes();
        // Locate the student lookup service in connector
        var studentLookupServiceType = assembly
            .Where(x => x.GetInterface("IMappingLookup") != null
                     && x.IsAbstract == false
                     && x.FullName == TConnector.FullName)
            .FirstOrDefault();

        Guard.Against.Null(studentLookupServiceType, "", "Could not get mapping lookup type");
        
        var connectorStudentLookupService = 
            ActivatorUtilities.CreateInstance(_serviceProvider, studentLookupServiceType);
        
        return (IMappingLookup)connectorStudentLookupService;
    }
}