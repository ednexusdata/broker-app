using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using EdNexusData.Broker.Core.Tests.Integration.Fixtures;
using Microsoft.EntityFrameworkCore;
using EdNexusData.Broker.Core.Lookup;
using System.Threading.Tasks;
using DnsClient;

namespace EdNexusData.Broker.Core.Tests.Integration;

public class DirectoryLookupServiceTests
{

    public DirectoryLookupServiceTests()
    {
    }
    
    [Fact]
    public async Task TestNotNull()
    {
        var _lookupClient = new LookupClient();
        
            var txtRecords = new List<string>()
            {
                "v=edubroker1; a=broker.ednexusdata.org",
                "v=edubroker1; env=prod; a=broker.ednexusdata.org",
                "v=edubroker1; env=production; a=broker.ednexusdata.org",
                "v=edubroker1; env=train; a=brokertrain.ednexusdata.org"
            };
            
            if (txtRecords.Count() > 0)
            {
                var brokerTXTRecord2 = txtRecords
                    .Where(x => x.IndexOf("v=edubroker", StringComparison.OrdinalIgnoreCase) >= 0).ToList();

                var brokerTXTRecord3 = txtRecords.Where(x =>  
                         x.IndexOf($"env=train", StringComparison.OrdinalIgnoreCase) >= 0);
                    
                var brokerTXTRecord4 = txtRecords.Where(x =>  
                         x.IndexOf($"env=", StringComparison.OrdinalIgnoreCase) <= 0);

                var brokerTXTRecord = txtRecords
                    .Where(x => x.IndexOf("v=edubroker", StringComparison.OrdinalIgnoreCase) >= 0)
                    .Where(x => x.IndexOf($"env=prod", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                x.IndexOf($"env=", StringComparison.OrdinalIgnoreCase) <= 0)
                    .ToList();

                Assert.NotNull(brokerTXTRecord);
            }

    }
}