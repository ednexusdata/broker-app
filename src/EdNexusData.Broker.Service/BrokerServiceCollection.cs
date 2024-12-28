using Microsoft.Extensions.DependencyInjection;
using EdNexusData.Broker.Service.Resolvers;
using EdNexusData.Broker.Service.Lookup;
using EdNexusData.Broker.Service.Serializers;
using EdNexusData.Broker.Service.Jobs;
using DnsClient;
using EdNexusData.Broker.Service.Worker;
using EdNexusData.Broker.Service.Cache;
using EdNexusData.Broker.Service.Services;
using EdNexusData.Broker.Common.Configuration;
using EdNexusData.Broker.Common.Connector;

namespace EdNexusData.Broker.Service;

public static class BrokerServiceCollection
{
    /*
    public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
    {
        return services;
    }
    */

    public static IServiceCollection AddBrokerServices(this IServiceCollection services)
    {
        // Loaders
        services.AddSingleton<ConnectorLoader>();
        
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
        services.AddScoped<PayloadResolver>();
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
        services.AddScoped(typeof(JobStatusService<>));

        return services;
    }

    public static IServiceCollection AddBrokerServicesForWorker(this IServiceCollection services)
    {
        // Loaders
        services.AddSingleton<ConnectorLoader>();

        // Services
        services.AddSingleton<ILookupClient, LookupClient>();
        services.AddScoped<DirectoryLookupService>();
        services.AddScoped<MessageService>();
        services.AddScoped<JobService>();
        services.AddScoped<JobStatusService>();
        
        // Resolvers
        services.AddScoped<IConfigurationResolver, ConfigurationResolver>();
        services.AddScoped<IPayloadResolver, PayloadResolver>();
        services.AddScoped<PayloadResolver>();
        services.AddScoped<ConnectorResolver>();
        services.AddScoped<FocusEducationOrganizationResolver>();
        services.AddScoped<DistrictEducationOrganizationResolver>();
        services.AddScoped<PayloadJobResolver>();
        services.AddScoped<BrokerResolver>();
        services.AddScoped<RequestResolver>();
        
        // Jobs
        services.AddScoped<RequestingJob>();
        services.AddScoped<TransmittingJob>();
        services.AddScoped<PayloadLoaderJob>();
        services.AddScoped<PrepareMappingJob>();
        services.AddScoped<ImportMappingJob>();

        // Worker
        services.AddScoped(typeof(JobStatusService<>));
        
        return services;
    }

    public static IServiceCollection AddConnectorDependencies(this IServiceCollection services)
    {
        Activator.CreateInstance<ConnectorLoader>();
        
        var types = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(s => s.GetExportedTypes())
                        .Where(p => p.GetInterface(nameof(IConnectorServiceCollection)) is not null);
        
        foreach(var type in types)
        {   
            var myMethod = type.GetMethod("AddDependencies");
            myMethod!.Invoke(null, new object[] { services });
        }

        return services;
    }
}

// Exists to not conflict with generic IPayloadResolver type
internal interface IPayloadResolver
{
}