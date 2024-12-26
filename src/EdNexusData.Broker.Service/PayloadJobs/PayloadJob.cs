// using System.Text.Json;
// using EdNexusData.Broker.Core.Jobs;

// namespace EdNexusData.Broker.Service;

// public abstract class PayloadJob
// {
//     public static bool AllowConfiguration = false;
    
//     public static bool AllowMultiple = false;

//     public IJobStatusService? JobStatusService;
    
//     public abstract Task<object?> ExecuteAsync(string studentUniqueId, JsonDocument? configuration);
// }