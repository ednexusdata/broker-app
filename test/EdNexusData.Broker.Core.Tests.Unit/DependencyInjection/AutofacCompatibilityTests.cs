using Autofac;
using Autofac.Extensions.DependencyInjection;
using DnsClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EdNexusData.Broker.Core.Tests.Unit.DependencyInjection;

/// <summary>
/// The Web and Worker hosts use Autofac. Autofac resolves some registrations differently from Microsoft DI
/// (it refuses to choose between equally long constructors, for one), so every service the hosts register
/// must resolve under Autofac whenever it resolves under Microsoft DI.
/// </summary>
public class AutofacCompatibilityTests
{
    public static TheoryData<string> Registrations => new() { "Web", "Worker" };

    [Theory]
    [MemberData(nameof(Registrations))]
    public void Every_service_that_resolves_with_Microsoft_DI_also_resolves_with_Autofac(string host)
    {
        var services = CreateServices(host);
        using var microsoft = services.BuildServiceProvider();
        var builder = new ContainerBuilder();
        builder.Populate(services);
        using var container = builder.Build();
        var autofac = new AutofacServiceProvider(container);

        var serviceTypes = services
            .Where(d => !d.ServiceType.ContainsGenericParameters && !d.IsKeyedService)
            .Select(d => d.ServiceType)
            .Distinct()
            .ToList();

        var autofacOnlyFailures = serviceTypes
            .Where(t => TryResolve(autofac, t) is string && TryResolve(microsoft, t) is null)
            .Select(t => $"{t.FullName}: {TryResolve(autofac, t)}")
            .ToList();

        Assert.NotEmpty(serviceTypes);
        Assert.Empty(autofacOnlyFailures);
    }

    [Theory]
    [MemberData(nameof(Registrations))]
    public void LookupClient_resolves_with_Autofac(string host)
    {
        // LookupClient has several one-parameter constructors; registering it by type fails under Autofac.
        var builder = new ContainerBuilder();
        builder.Populate(CreateServices(host));
        using var container = builder.Build();

        Assert.IsType<LookupClient>(container.Resolve<ILookupClient>());
    }

    private static IServiceCollection CreateServices(string host)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["SmtpClientOptions:From"] = "broker@example.com" })
            .Build();

        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddHttpClient();

        if (host == "Web")
        {
            services.AddBrokerServices();
        }
        else
        {
            services.AddBrokerServicesForWorker(configuration);
        }

        services.AddReportingServices();
        services.AddConnectorServices();
        return services;
    }

    /// <summary>Null when the service resolves, otherwise the innermost error.</summary>
    private static string? TryResolve(IServiceProvider provider, Type serviceType)
    {
        try
        {
            using var scope = provider.CreateScope();
            scope.ServiceProvider.GetService(serviceType);
            return null;
        }
        catch (Exception ex)
        {
            while (ex.InnerException is not null) ex = ex.InnerException;
            return ex.Message;
        }
    }
}
