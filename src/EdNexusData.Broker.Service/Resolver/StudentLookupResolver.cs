using Microsoft.Extensions.DependencyInjection;
using EdNexusData.Broker.Core.StudentLookup;
using Ardalis.GuardClauses;

namespace EdNexusData.Broker.Service.Resolvers;

public class StudentLookupResolver
{
    private readonly IServiceProvider _serviceProvider;
    
    public StudentLookupResolver(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IStudentLookupService Resolve(Type TConnector)
    {
        var assembly = TConnector.Assembly.GetExportedTypes();
        // Locate the student lookup service in connector
        var studentLookupServiceType = assembly
            .Where(x => x.GetInterface("IStudentLookupService") != null
                     && x.IsAbstract == false)
            .FirstOrDefault();

        Guard.Against.Null(studentLookupServiceType, "", "Could not get student lookup type");
        
        var connectorStudentLookupService = 
            ActivatorUtilities.CreateInstance(_serviceProvider, studentLookupServiceType);
        
        return (IStudentLookupService)connectorStudentLookupService;
    }
}