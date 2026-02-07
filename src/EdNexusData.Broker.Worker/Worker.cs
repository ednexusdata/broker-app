using EdNexusData.Broker.Core.Worker;
using Ardalis.GuardClauses;
using EdNexusData.Broker.Core;
using EdNexusData.Broker.Core.Specifications;
using EdNexusData.Broker.Common.Jobs;
using Microsoft.Extensions.Caching.Memory;
using EdNexusData.Broker.Core.Services;
using EdNexusData.Broker.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace EdNexusData.Broker.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMemoryCache memoryCache;

    public Worker(
        ILogger<Worker> logger,
        IServiceProvider serviceProvider,
        IMemoryCache memoryCache)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        this.memoryCache = memoryCache;
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

                var settingsService = (SettingsService)scoped.ServiceProvider.GetService(typeof(SettingsService))!;

                // Clear cache if needed
                var clearCacheSetting = await settingsService.GetValueAsync("LastCacheClearedAt");
                if (clearCacheSetting != null)
                {
                    var dblastCacheCleared = DateTimeOffset.Parse(clearCacheSetting);
                    var lastCachedValue = memoryCache.Get<DateTimeOffset?>(CachedRepository<Setting>.LastCachedValue);
                    if (lastCachedValue is not null && dblastCacheCleared >= lastCachedValue)
                    {
                        if (memoryCache is MemoryCache memCache)
                        {
                            memCache.Compact(1.0);
                            _logger.LogInformation("Cache cleared.");
                        }
                    }
                }

                var _context = (BrokerDbContext)scoped.ServiceProvider.GetService(typeof(BrokerDbContext))!;
                Job? job = null;

                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var sql = "";

                    if (_context.IsSqlServer())
                    {
                        sql = "SELECT TOP 1 * FROM Worker_Jobs WITH (UPDLOCK, NOWAIT) WHERE JobStatus = '0' ORDER BY QueueDateTime";
                    } else if (_context.IsPostgreSql())
                    {
                        sql = "SELECT * FROM \"Worker_Jobs\" WHERE \"JobStatus\" = '0' ORDER BY \"QueueDateTime\" LIMIT 1 FOR UPDATE SKIP LOCKED";
                    } else
                    {
                        throw new Exception("Unable to detect BrokerDbContext driver.");
                    }

                    job = (await _context.WorkerJobs!
                        .FromSqlRaw(sql)
                        .ToListAsync())
                        .FirstOrDefault();

                    if (job != null)
                    {
                        // Capture the job
                        job.JobStatus = JobStatus.Running;
                        job.StartDateTime = DateTime.UtcNow;
                        _context.WorkerJobs?.Update(job);
                        await _context.SaveChangesAsync();

                        await transaction.CommitAsync();
                        _logger.LogInformation($"Processed Job ID: {job.Id}");
                    }
                    else
                    {
                        _logger.LogInformation("No available jobs to process.");
                    }
                }
                catch (Exception)
                {
                    // Rollback the transaction in case of an error
                    await transaction.RollbackAsync();
                    throw;
                }

                if (job is not null)
                {
                    await ProcessJob(scoped, job);
                }
            }
        }
    }

    private async Task ProcessJob(IServiceScope scoped, Job jobRecord)
    {
        var _jobsRepository = (IRepository<Job>)scoped.ServiceProvider.GetService(typeof(IRepository<Job>))!;
        var _jobStatusService = (JobStatusService<Worker>)scoped.ServiceProvider.GetService(typeof(JobStatusService<Worker>))!; 
        
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
                .FirstOrDefault(p => p.FullName == jobRecord.JobType!);

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
