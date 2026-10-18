using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EdNexusData.Broker.Core.Updaters;

/// <summary>
/// Checks the connector store on a schedule and updates connectors in this process (see <see cref="ConnectorUpdater"/>).
/// Every Web and Worker process runs its own check, so each converges on the saved connector references by itself;
/// processes sharing a connectors folder reuse a version another one already installed.
/// Off unless ConnectorUpdates:Enabled is true, because it downloads and runs code.
/// </summary>
public class ConnectorUpdateService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConnectorUpdateService> _logger;

    public ConnectorUpdateService(IServiceScopeFactory scopeFactory, IConfiguration configuration, ILogger<ConnectorUpdateService> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public bool Enabled => _configuration.GetValue("ConnectorUpdates:Enabled", false);

    private TimeSpan Interval => TimeSpan.FromMinutes(Math.Max(1, _configuration.GetValue("ConnectorUpdates:IntervalMinutes", 60)));

    private TimeSpan InitialDelay => TimeSpan.FromSeconds(Math.Max(0, _configuration.GetValue("ConnectorUpdates:InitialDelaySeconds", 30)));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!Enabled)
        {
            _logger.LogInformation("Connector updates are disabled (ConnectorUpdates:Enabled).");
            return;
        }

        _logger.LogInformation("Checking for connector updates every {Interval}.", Interval);

        try
        {
            await Task.Delay(InitialDelay, stoppingToken);
            while (!stoppingToken.IsCancellationRequested)
            {
                await CheckOnceAsync(stoppingToken);
                await Task.Delay(Interval, stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    /// <summary>Runs one update check. Failures are logged and retried at the next interval.</summary>
    public async Task<IReadOnlyList<ConnectorUpdateResult>> CheckOnceAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var updater = scope.ServiceProvider.GetRequiredService<ConnectorUpdater>();
            var results = await updater.Synchronize(cancellationToken);

            foreach (var result in results.Where(r => r.Plan.Action == ConnectorUpdateAction.None))
            {
                // Up to date is the normal case; anything else means a reference cannot be followed.
                var level = result.Plan.InstalledVersion is not null && result.Plan.Reason!.StartsWith("Up to date")
                    ? LogLevel.Debug
                    : LogLevel.Information;
                _logger.Log(level, "Connector {Name} not updated: {Message}", result.Plan.Name, result.Message);
            }

            return results;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Checking for connector updates failed; retrying in {Interval}.", Interval);
            return [];
        }
    }
}
