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
using Microsoft.Extensions.Configuration;
using EdNexusData.Broker.Core.Emails;

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
        services.AddSingleton<TypeResolver>();

        // Services
        services.AddScoped<StudentLookupService>();
        services.AddScoped<StudentService>();
        services.AddScoped<PayloadJobService>();
        services.AddScoped<DirectoryLookupService>();
        services.AddScoped<ManifestService>();
        services.AddScoped<MappingLookupService>();
        services.AddScoped<JobService>();
        services.AddScoped<MessageService>();
        services.AddScoped<RequestService>();
        services.AddScoped<PayloadContentService>();
        services.AddScoped<EducationOrganizationContactService>();
        services.AddScoped<ReceiveMessageService>();
        services.AddScoped<ConnectorService>();
        services.AddScoped(typeof(JobStatusService<>));
        services.AddScoped<DbConnectionService>();
        services.AddScoped<MappingRecordValidatorService>();
        services.AddScoped<SettingsService>();

        // Wrappers
        services.AddSingleton<INowWrapper, NowWrapper>();

        return services;
    }

    public static IServiceCollection AddBrokerServicesForWorker(this IServiceCollection services, 
        Microsoft.Extensions.Configuration.IConfiguration config)
    {
        // Loaders
        services.AddSingleton<ConnectorLoader>();

        // Services
        services.AddSingleton<ILookupClient, LookupClient>();
        services.AddScoped<DirectoryLookupService>();
        services.AddScoped<MessageService>();
        services.AddScoped<RequestService>();
        services.AddScoped<PayloadContentService>();
        services.AddScoped<JobService>();
        services.AddScoped<EducationOrganizationContactService>();
        services.AddScoped<DbConnectionService>();
        services.AddScoped<SettingsService>();
        
        // Resolvers
        services.AddScoped<IConfigurationResolver, ConfigurationResolver>();
        services.AddScoped<IPayloadResolver, PayloadResolver>();
        services.AddScoped<PayloadResolver>();
        services.AddScoped<ConnectorResolver>();
        services.AddScoped<FocusEducationOrganizationResolver>();
        services.AddScoped<DistrictEducationOrganizationResolver>();
        services.AddScoped<PayloadJobResolver>();
        services.AddScoped<PayloadContentActionJobResolver>();
        services.AddScoped<BrokerResolver>();
        services.AddSingleton<TypeResolver>();
        
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

        services.AddFluentEmail(config.GetValue<string>("SmtpClientOptions:From"))
            .AddRazorRenderer(typeof(EmailRoot))
            .AddMailKitSender();
                    
        return services;
    }

    public static IServiceCollection AddConnectorServicesToDefaultProvider(this IServiceCollection services)
    {
        var connectorLoader = Activator.CreateInstance<ConnectorLoader>();
        _ = connectorLoader ?? throw new InvalidOperationException("Loader could not be instantiated.");

        // var types = AppDomain.CurrentDomain.GetAssemblies()
        //                 .SelectMany(s => s.GetExportedTypes())
        //                 .Where(p => p.GetInterface(nameof(IConnectorServiceCollection)) is not null);

        services.AddSingleton(connectorLoader);

        var types = TypeResolver.ResolveConnectorInterface(connectorLoader, nameof(IConnectorServiceCollection));
        if (types is not null && types.Count > 0)
        {
            foreach (var type in types)
            {
                // Call AddDependencies on each service collection
                var myMethod = type?.GetMethod("AddDependencies");
                myMethod!.Invoke(null, [services]);
            }
        }

        return services;
    }

}


// Exists to not conflict with generic IPayloadResolver type
internal interface IPayloadResolver
{
}