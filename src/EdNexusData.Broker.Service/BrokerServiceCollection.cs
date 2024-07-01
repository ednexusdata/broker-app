using Microsoft.Extensions.DependencyInjection;
using EdNexusData.Broker.Connector.Resolvers;
using EdNexusData.Broker.Service.Resolvers;
using EdNexusData.Broker.Service.Lookup;
using EdNexusData.Broker.Service.Serializers;
using EdNexusData.Broker.Service.Jobs;
using DnsClient;
using EdNexusData.Broker.Service.Worker;
using EdNexusData.Broker.Service.Cache;

namespace EdNexusData.Broker.Service;

public static class BrokerServiceCollection //: IConnectorServiceCollection
{
    /*
    public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
    {
        return services;
    }
    */

    public static IServiceCollection AddBrokerServices(this IServiceCollection services)
    {
        // Caches
        services.AddSingleton<MappingLookupCache>();
        
        // Other Services
        services.AddSingleton<ILookupClient, LookupClient>();
        

        // Seralizers
        services.AddScoped<ConfigurationSerializer>();
        services.AddScoped<IncomingPayloadSerializer>();
        services.AddScoped<OutgoingPayloadSerializer>();
        
        // Resolvers
        services.AddScoped<IConfigurationResolver, ConfigurationResolver>();
        services.AddScoped<IPayloadResolver, PayloadResolver>();
        services.AddScoped<FocusEducationOrganizationResolver>();
        services.AddScoped<DistrictEducationOrganizationResolver>();
        services.AddScoped<StudentLookupResolver>();
        services.AddScoped<StudentResolver>();
        services.AddScoped<MappingLookupResolver>();
        services.AddScoped<AuthenticationProviderResolver>();

        // Services
        services.AddScoped<StudentLookupService>();
        services.AddScoped<StudentService>();
        services.AddScoped<PayloadJobService>();
        services.AddScoped<DirectoryLookupService>();
        services.AddScoped<ManifestService>();
        services.AddScoped<MappingLookupService>();
        services.AddScoped<JobService>();

        return services;
    }

    public static IServiceCollection AddBrokerServicesForWorker(this IServiceCollection services)
    {
        // Services
        services.AddSingleton<ILookupClient, LookupClient>();
        services.AddScoped<DirectoryLookupService>();
        services.AddScoped<MessageService>();
        services.AddScoped<JobService>();
        
        // Resolvers
        services.AddScoped<IConfigurationResolver, ConfigurationResolver>();
        services.AddScoped<IPayloadResolver, PayloadResolver>();
        services.AddScoped<PayloadResolver>();
        services.AddScoped<ConnectorResolver>();
        services.AddScoped<FocusEducationOrganizationResolver>();
        services.AddScoped<DistrictEducationOrganizationResolver>();
        services.AddScoped<PayloadJobResolver>();
        
        // Jobs
        services.AddScoped<SendRequestJob>();
        services.AddScoped<PayloadLoaderJob>();
        services.AddScoped<PrepareMappingJob>();
        services.AddScoped<ImportMappingJob>();

        // Worker
        services.AddScoped(typeof(JobStatusService<>));
        
        return services;
    }
}