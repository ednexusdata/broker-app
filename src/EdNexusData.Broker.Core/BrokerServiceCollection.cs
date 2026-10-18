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
using QuestPDF.Infrastructure;
using EdNexusData.Broker.Core.Reports;
using EdNexusData.Broker.Core.Updaters;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EdNexusData.Broker.Core;

public static class BrokerServiceCollection
{
    public static IServiceCollection AddReportingServices(this IServiceCollection services, LicenseType questPdfLicense = LicenseType.Community)
    {
        // Set the license here using the value passed by the consumer
        QuestPDF.Settings.License = questPdfLicense;

        // Register your internal PDF services
        // services.AddScoped<IPdfGenerator, MyInternalGenerator>();
        
        return services;
    }

    public static IServiceCollection AddBrokerServices(this IServiceCollection services)
    {
        // Loaders
        
        // Caches
        services.AddSingleton<MappingLookupCache>();
        
        // Other Services
        services.AddSingleton<ILookupClient>(_ => new LookupClient());

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
        services.AddScoped<PayloadContentActionJobService>();
        services.AddScoped<ActivityLogService>();

        // Wrappers
        services.AddSingleton<INowWrapper, NowWrapper>();

        // Reports
        services.AddScoped<ProofOfRequestReport>();

        return services;
    }

    public static IServiceCollection AddBrokerServicesForWorker(this IServiceCollection services, 
        Microsoft.Extensions.Configuration.IConfiguration config)
    {
        // Loaders

        // Services
        services.AddSingleton<ILookupClient>(_ => new LookupClient());
        services.AddScoped<DirectoryLookupService>();
        services.AddScoped<MessageService>();
        services.AddScoped<RequestService>();
        services.AddScoped<PayloadContentService>();
        services.AddScoped<JobService>();
        services.AddScoped<EducationOrganizationContactService>();
        services.AddScoped<DbConnectionService>();
        services.AddScoped<SettingsService>();
        services.AddScoped<PayloadContentActionJobService>();
        services.AddScoped<RetentionReminderService>();
        services.AddScoped<ProofOfRequestReport>();
        services.AddScoped<ActivityLogService>();

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
        services.AddScoped<RequestCleanupJob>();
        services.AddScoped<RequestRetentionReminderJob>();
        services.AddScoped<RequestRetentionReminderEmailJob>();

        // Worker
        services.AddScoped(typeof(JobStatusService<>));

        // Wrappers
        services.AddSingleton<INowWrapper, NowWrapper>();

        services.AddFluentEmail(config.GetValue<string>("SmtpClientOptions:From"))
            .AddRazorRenderer(typeof(EmailRoot))
            .AddMailKitSender();
                    
        return services;
    }

    /// <summary>
    /// Loads the connectors and registers the loader. Connector services are not added to the app container:
    /// each connector gets its own Autofac container (see <see cref="ConnectorActivator"/>) so it can be unloaded.
    /// </summary>
    public static IServiceCollection AddConnectorServices(this IServiceCollection services)
    {
        var connectorLoader = new ConnectorLoader();
        connectorLoader.UseAppServices(services);
        services.AddSingleton(connectorLoader);
        services.AddScoped<ConnectorActivator>();

        // Updates from the connector store (off unless ConnectorUpdates:Enabled is true)
        services.TryAddScoped<ConnectorService>();
        services.AddScoped<ConnectorUpdater>();
        services.AddSingleton<ConnectorPackageInstaller>();
        services.AddHostedService<ConnectorUpdateService>();

        return services;
    }

}


// Exists to not conflict with generic IPayloadResolver type
internal interface IPayloadResolver
{
}