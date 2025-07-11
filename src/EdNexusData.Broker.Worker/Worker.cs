using EdNexusData.Broker.Core.Worker;
using Ardalis.GuardClauses;
using EdNexusData.Broker.Core;
using EdNexusData.Broker.Core.Specifications;
using EdNexusData.Broker.Common.Jobs;

namespace EdNexusData.Broker.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);

            using (var scoped = _serviceProvider.CreateScope())
            {
                var dbConnectionService = (DbConnectionService)scoped.ServiceProvider.GetService(typeof(DbConnectionService))!;
                await dbConnectionService.ThrowIfDatabaseConnectionNotUpAsync();
                
                var _jobsRepository = (IRepository<Job>)scoped.ServiceProvider.GetService(typeof(IRepository<Job>))!;
                var _jobStatusService = (JobStatusService<Worker>)scoped.ServiceProvider.GetService(typeof(JobStatusService<Worker>))!;

                var jobRecord = await _jobsRepository.FirstOrDefaultAsync(new JobsWaitingToProcess());

                if (jobRecord is not null)
                {
                    jobRecord.WorkerState = "Begin Job Run";
                    jobRecord.WorkerInstance = System.Environment.MachineName;
                    jobRecord.JobStatus = JobStatus.Running;
                    jobRecord.StartDateTime = DateTime.UtcNow;

                    await _jobsRepository.UpdateAsync(jobRecord);

                    _logger.LogInformation("Captured {jobRecordId}.", jobRecord.Id);

                    // Run job
                    try
                    {
                        _logger.LogInformation("Resolving job type for {jobRecordId}.", jobRecord.Id);

                        // Get job type
                        var jobType = AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(s => s.GetExportedTypes())
                            .Where(p => p.FullName == jobRecord.JobType!).FirstOrDefault();

                        Guard.Against.Null(jobType, "jobType", $"Unable to find job type: {jobRecord.JobType!}");

                        _logger.LogInformation("Resolved job type for {jobRecordId} to {jobType}.", jobRecord.Id, jobType.FullName);

                        // Instantiate and cast as IJob
                        var job = (IJob)ActivatorUtilities.CreateInstance(scoped.ServiceProvider, jobType);
                        
                        await _jobStatusService.UpdateJobStatus(jobRecord, JobStatus.Running, "Begin running.");
                        _logger.LogInformation("Begin running {jobRecordId}.", jobRecord.Id);

                        await job.ProcessAsync(jobRecord);
                        await _jobStatusService.UpdateJobStatus(jobRecord, JobStatus.Complete, "Complete.");
                        _logger.LogInformation("{jobRecordId} completed.", jobRecord.Id);

                    } catch (Exception e)
                    {
                        using (var exScope = _serviceProvider.CreateScope())
                        {
                            var exJobStatusService = (JobStatusService<Worker>)exScope.ServiceProvider.GetService(typeof(JobStatusService<Worker>))!;

                            var messageToSave = e.Message + "\n\n" + e.StackTrace?.ToString();
                            
                            if (e.InnerException is not null)
                            {
                                messageToSave += "\n\n=========================\n\n" + e.InnerException.Message + "\n\n" + e.InnerException.StackTrace?.ToString();
                            }
                            
                            await exJobStatusService.UpdateJobStatus(jobRecord, JobStatus.Failed, messageToSave);
                            _logger.LogInformation("{jobRecordId} failed.", jobRecord.Id);
                        }
                    }
                }
            }
        }
    }
}
