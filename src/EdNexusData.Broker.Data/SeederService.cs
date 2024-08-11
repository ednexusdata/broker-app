using EdNexusData.Broker.Data.Seeds.Populated;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EdNexusData.Broker.Data;

public class SeederService
{
    private readonly IConfiguration _configuration;
    private readonly BrokerDbContext _brokerDbContext;
    private readonly ILogger<SeederService> _logger;

    public SeederService(IConfiguration configuration, BrokerDbContext brokerDbContext, ILogger<SeederService> logger)
    {
        _configuration = configuration;
        _brokerDbContext = brokerDbContext;
        _logger = logger;
    }

    public async Task Invoke()
    {
        // Check configuration for seeder
        var seederConfig = _configuration.GetValue<string>("DatabaseSeed");

        _logger.LogInformation($"Retrieved config value: {seederConfig}.");

        if (string.IsNullOrEmpty(seederConfig))
            return;
        
        // See if all seeds already applied
        var exists = await _brokerDbContext.Seeds!.Where(x => x.SeedId == "20240810163800_InitialSeed").FirstOrDefaultAsync();

        if (exists is null)
        {
            // Apply Seeder
            var seeder = new InitialSeed(_brokerDbContext, _logger);
            await seeder.SeedAsync();

            var seed = new Seed()
            {
                SeedName = "Populated",
                SeedId = "20240810163800_InitialSeed"
            };
            _brokerDbContext.Seeds?.Add(seed);
            await _brokerDbContext.SaveChangesAsync();
        }
        

        _logger.LogInformation($"Seeder completed.");
    }
}