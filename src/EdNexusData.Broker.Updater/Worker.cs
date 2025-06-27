using System.Diagnostics;


namespace EdNexusData.Broker.Updater;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            foreach (var process in Process.GetProcessesByName("dotnet"))
            {
                try
                {
                    string envVars = GetEnvironmentVariables(process.Id);
                    if (envVars.Contains("ASPNETCORE_ENVIRONMENT") || envVars.Contains("DOTNET_ENVIRONMENT"))
                    {
                        Console.WriteLine($"Process {process.Id} {process.ProcessName}");
                        process.Kill();
                        Console.WriteLine($"Process {process.Id} terminated.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accessing process {process.Id}: {ex.Message}");
                }
            }
            
            await Task.Delay(5000, stoppingToken);
        }
    }

    static string GetEnvironmentVariables(int processId)
    {
        string path = $"/proc/{processId}/environ";
        if (File.Exists(path))
        {
            return File.ReadAllText(path);
        }
        return string.Empty;
    }
}
