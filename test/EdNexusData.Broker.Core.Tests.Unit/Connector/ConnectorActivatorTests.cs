using System.Runtime.CompilerServices;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using EdNexusData.Broker.Common.Configuration;
using EdNexusData.Broker.Core.Tests.Unit.TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EdNexusData.Broker.Core.Tests.Unit.Connector;

[Collection(ConnectorCollection.Name)]
public class ConnectorActivatorTests
{
    private const string Name = TestConnectorApp.ConnectorName;

    [Fact]
    public void Refuses_to_be_resolved_from_the_root_container()
    {
        using var app = TestConnectorApp.Create();

        var error = Assert.ThrowsAny<Exception>(() => app.Services.GetService<ConnectorActivator>());
        Assert.Contains("must be resolved from a request or job scope", error.ToString());
    }

    [Fact]
    public void Falls_back_to_ActivatorUtilities_when_no_activator_is_registered()
    {
        var provider = new Microsoft.Extensions.DependencyInjection.ServiceCollection()
            .AddScoped<IConfigurationResolver, FakeConfigurationResolver>()
            .BuildServiceProvider();

        var created = ConnectorActivator.CreateInstance(provider, typeof(NeedsConfigurationResolver));

        Assert.IsType<FakeConfigurationResolver>(((NeedsConfigurationResolver)created).Resolver);
    }

    [Fact]
    public void Creates_app_types_from_the_given_provider()
    {
        using var app = TestConnectorApp.Create();
        using var scope = app.Services.CreateScope();

        var created = (NeedsConfigurationResolver)ConnectorActivator.CreateInstance(scope.ServiceProvider, typeof(NeedsConfigurationResolver));

        Assert.Same(scope.ServiceProvider.GetRequiredService<IConfigurationResolver>(), created.Resolver);
    }

    [Fact]
    public void Connector_code_shares_the_requests_scoped_app_services()
    {
        using var app = TestConnectorApp.Create();
        using var scope = app.Services.CreateScope();

        // Like MappingController setting CurrentRecordEducationOrganizationId before calling connector code.
        var requestResolver = scope.ServiceProvider.GetRequiredService<IConfigurationResolver>();
        requestResolver.CurrentRecordEducationOrganizationId = Guid.NewGuid();

        var job = CreateJob(app, scope);

        Assert.Same(requestResolver, Get(Get(job, "Scoped"), "ConfigurationResolver"));
    }

    [Fact]
    public void Connector_scoped_services_are_shared_within_a_request_but_not_across_requests()
    {
        using var app = TestConnectorApp.Create();
        using var first = app.Services.CreateScope();
        using var second = app.Services.CreateScope();

        var a = CreateJob(app, first);
        var b = CreateJob(app, first);
        var c = CreateJob(app, second);

        Assert.Same(Get(a, "Scoped"), Get(b, "Scoped"));
        Assert.NotSame(Get(a, "Scoped"), Get(c, "Scoped"));
    }

    [Fact]
    public void Connector_singletons_are_shared_across_requests()
    {
        using var app = TestConnectorApp.Create();
        using var first = app.Services.CreateScope();
        using var second = app.Services.CreateScope();

        var a = CreateJob(app, first);
        var b = CreateJob(app, second);

        Assert.Same(Get(a, "Singleton"), Get(b, "Singleton"));
    }

    [Fact]
    public void Resolves_connector_open_generics_and_app_generics_over_connector_types()
    {
        using var app = TestConnectorApp.Create();
        using var scope = app.Services.CreateScope();

        var job = CreateJob(app, scope);

        Assert.NotNull(Get(job, "Generic"));
        Assert.IsAssignableFrom<ILogger>(Get(job, "Logger"));
        Assert.IsAssignableFrom<ILogger>(Get(Get(job, "Singleton"), "Logger"));
    }

    [Fact]
    public void Connector_services_can_be_resolved_through_the_connector_service_provider()
    {
        using var app = TestConnectorApp.Create();
        using var scope = app.Services.CreateScope();
        var activator = scope.ServiceProvider.GetRequiredService<ConnectorActivator>();

        var connectorServices = activator.GetServiceProvider(app.Runtime);

        Assert.NotNull(connectorServices.GetService(app.ConnectorType("ScopedService")));
        // Connector code that creates further types gets the same activator back.
        Assert.Same(activator, connectorServices.GetService<ConnectorActivator>());
        // One connector scope per request: later calls wrap the same scope.
        Assert.Same(
            ((AutofacServiceProvider)connectorServices).LifetimeScope,
            ((AutofacServiceProvider)activator.GetServiceProvider(app.Runtime)).LifetimeScope);
    }

    [Fact]
    public void App_container_never_registers_connector_types()
    {
        using var app = TestConnectorApp.Create();
        using (var scope = app.Services.CreateScope())
        {
            CreateJob(app, scope);
            var activator = scope.ServiceProvider.GetRequiredService<ConnectorActivator>();
            foreach (var service in app.Runtime.Services.Where(s => !s.ServiceType.ContainsGenericParameters))
            {
                activator.GetServiceProvider(app.Runtime).GetService(service.ServiceType);
            }
        }

        // A registration for a connector type in the app container keeps that connector loaded forever.
        var connectorServices = app.Container.ComponentRegistry.Registrations
            .SelectMany(r => r.Services)
            .OfType<IServiceWithType>()
            .Where(s => InvolvesCollectibleAssembly(s.ServiceType))
            .ToList();
        Assert.Empty(connectorServices);
    }

    [Fact]
    public void Disposed_activator_cannot_hand_out_connector_services()
    {
        using var app = TestConnectorApp.Create();
        var scope = app.Services.CreateScope();
        var activator = scope.ServiceProvider.GetRequiredService<ConnectorActivator>();
        scope.Dispose();

        Assert.Throws<ObjectDisposedException>(() => activator.GetServiceProvider(app.Runtime));
    }

    [Fact]
    public async Task Unloaded_versions_cannot_be_used_by_new_work()
    {
        using var app = TestConnectorApp.Create();
        var oldRuntime = app.Runtime;

        await app.Loader.UnloadConnectorAsync(Name);

        using var scope = app.Services.CreateScope();
        var activator = scope.ServiceProvider.GetRequiredService<ConnectorActivator>();
        var error = Assert.Throws<InvalidOperationException>(() => activator.GetServiceProvider(oldRuntime));
        Assert.Contains("has been unloaded", error.Message);
    }

    [Fact]
    public async Task Replace_waits_for_work_already_using_the_old_version()
    {
        using var app = TestConnectorApp.Create();
        var scope = app.Services.CreateAsyncScope();
        CreateJob(app, scope);

        var retirement = app.Loader.ReplaceConnectorAsync(Name, app.CreateNewVersionFolder());

        await Task.Delay(500);
        Assert.False(retirement.IsCompleted);

        await scope.DisposeAsync();
        var result = await retirement.WaitAsync(TimeSpan.FromSeconds(30));
        Assert.True(result.Collected);
    }

    [Fact]
    public async Task Old_version_is_released_after_its_services_were_used()
    {
        // Covers the leaks found while building this: connector singletons taking ILogger<ConnectorType>,
        // app open generics closed over connector types, and Autofac's reflection caches.
        using var app = TestConnectorApp.Create();
        await UseEverythingInTwoRequests(app);

        var result = await app.Loader.ReplaceConnectorAsync(Name, app.CreateNewVersionFolder()).WaitAsync(TimeSpan.FromSeconds(30));

        Assert.True(result.Collected);
    }

    [Fact]
    public async Task New_version_works_after_a_replace()
    {
        using var app = TestConnectorApp.Create();
        await UseEverythingInTwoRequests(app);
        await app.Loader.ReplaceConnectorAsync(Name, app.CreateNewVersionFolder()).WaitAsync(TimeSpan.FromSeconds(30));

        using var scope = app.Services.CreateScope();
        var job = CreateJob(app, scope);

        Assert.Same(scope.ServiceProvider.GetRequiredService<IConfigurationResolver>(), Get(Get(job, "Scoped"), "ConfigurationResolver"));
        Assert.Same(app.Runtime, app.Loader.FindRuntime(job.GetType()));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static object CreateJob(TestConnectorApp app, IServiceScope scope) =>
        ConnectorActivator.CreateInstance(scope.ServiceProvider, app.ConnectorType("ActivatedJob"));

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static async Task UseEverythingInTwoRequests(TestConnectorApp app)
    {
        for (var i = 0; i < 2; i++)
        {
            await using var scope = app.Services.CreateAsyncScope();
            CreateJob(app, scope);
        }
    }

    private static object? Get(object? instance, string property) =>
        instance!.GetType().GetProperty(property)!.GetValue(instance);

    private static bool InvolvesCollectibleAssembly(Type type)
    {
        if (type.HasElementType) return InvolvesCollectibleAssembly(type.GetElementType()!);
        return type.Assembly.IsCollectible
            || (type.IsConstructedGenericType && type.GetGenericArguments().Any(InvolvesCollectibleAssembly));
    }

    public class NeedsConfigurationResolver
    {
        public NeedsConfigurationResolver(IConfigurationResolver resolver)
        {
            Resolver = resolver;
        }

        public IConfigurationResolver Resolver { get; }
    }
}
