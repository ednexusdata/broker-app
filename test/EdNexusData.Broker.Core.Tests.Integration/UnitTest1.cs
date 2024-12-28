using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using EdNexusData.Broker.Core.Tests.Integration.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace EdNexusData.Broker.Core.Tests.Integration;

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
        var dbcontext = _services.Services!.GetService<DbContext>();

        Assert.NotNull(dbcontext);
    }
}