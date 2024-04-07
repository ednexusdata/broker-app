using Microsoft.Extensions.DependencyInjection;
using EdNexusData.Broker.Connector.Student;
using Ardalis.GuardClauses;

namespace EdNexusData.Broker.Service.Resolvers;

public class StudentResolver
{
    private readonly IServiceProvider _serviceProvider;
    
    public StudentResolver(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IStudentService Resolve(Type TConnector)
    {
        var assembly = TConnector.Assembly.GetExportedTypes();
        // Locate the student lookup service in connector
        var studentLookupServiceType = assembly
            .Where(x => x.GetInterface(nameof(IStudent)) is not null 
                     && x.IsAbstract == false)
            .FirstOrDefault();

        Guard.Against.Null(studentLookupServiceType, "", "Could not get student type");
        
        var connectorStudentLookupService = (IStudentService)
            ActivatorUtilities.CreateInstance(_serviceProvider, studentLookupServiceType);
        
        return connectorStudentLookupService;
    }
}