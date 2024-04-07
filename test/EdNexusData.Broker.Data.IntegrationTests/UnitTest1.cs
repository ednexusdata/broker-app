using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using EdNexusData.Broker.Data.IntegrationTests.Fixtures;

namespace EdNexusData.Broker.Data.IntegrationTests;

[Collection("BrokerWebDICollection")]
public class UnitTest1
{
    private readonly BrokerWebDIServicesFixture _services;

    public UnitTest1(BrokerWebDIServicesFixture services)
    {
        _services = services;
    }
    
    [Fact]
    public void Test1()
    {
        var dbconext = _services.Services!.GetService<BrokerDbContext>();

        Assert.NotNull(dbconext);
    }
}