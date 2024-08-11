using Microsoft.Extensions.DependencyInjection;

namespace EdNexusData.Broker.Data;

public static class BrokerDataServiceCollection //: IConnectorServiceCollection
{
    /*
    public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
    {
        return services;
    }
    */

    public static IServiceCollection AddBrokerDataServices(this IServiceCollection services)
    {
        // Services
        services.AddScoped<SeederService>();

        return services;
    }

    public static IServiceCollection AddBrokerDataServicesForWorker(this IServiceCollection services)
    {
        
        return services;
    }
}