// using System.Text.Json;
// using EdNexusData.Broker.Common.Jobs;

// namespace EdNexusData.Broker.Core;

// public abstract class PayloadJob
// {
//     public static bool AllowConfiguration = false;
    
//     public static bool AllowMultiple = false;

//     public IJobStatusService? JobStatusService;
    
//     public abstract Task<object?> ExecuteAsync(string studentUniqueId, JsonDocument? configuration);
// }