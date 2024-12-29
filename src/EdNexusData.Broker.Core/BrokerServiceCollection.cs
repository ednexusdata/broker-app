using Microsoft.Extensions.DependencyInjection;
using EdNexusData.Broker.Core.Resolvers;
using EdNexusData.Broker.Core.Lookup;
using EdNexusData.Broker.Core.Serializers;
using EdNexusData.Broker.Core.Jobs;
using DnsClient;
using EdNexusData.Broker.Core.Worker;
using EdNexusData.Broker.Core.Cache;
using EdNexusData.Broker.Core.Services;
using EdNexusData.Broker.Common.Configuration;
using EdNexusData.Broker.Common.Connector;
using EdNexusData.Broker.Core.Interfaces;

namespace EdNexusData.Broker.Core;

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
        services.AddScoped<EducationOrganizationContactService>();
        services.AddScoped(typeof(JobStatusService<>));

        // Wrappers
        services.AddSingleton<INowWrapper, NowWrapper>();

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
        services.AddScoped<EducationOrganizationContactService>();
        
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

        // Wrappers
        services.AddSingleton<INowWrapper, NowWrapper>();
        
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