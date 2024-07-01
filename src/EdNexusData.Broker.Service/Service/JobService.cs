using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Identity;
using EdNexusData.Broker.Domain;
using EdNexusData.Broker.SharedKernel;
using EdNexusData.Broker.Domain.Worker;
using EdNexusData.Broker.Connector;
using System.Text.Json;

namespace EdNexusData.Broker.Service;

public class JobService
{
    private readonly IRepository<Job> _jobRepository;

    public JobService(IRepository<Job> jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async Task<Job> CreateJobAsync(Type jobType, Type? referenceType = null, Guid? referenceGuid = null, JsonDocument? jobParameters = null)
    {
        if (jobType.GetInterface(nameof(IJob)) == null)
        {
            throw new ArgumentException("job type does not implement IJob");
        }
            
        var jobRecord = new Job()
        {
            QueueDateTime = DateTime.UtcNow,
            JobType = jobType.FullName,
            JobParameters = jobParameters,
            ReferenceType = (referenceType is not null) ? referenceType.FullName : null,
            ReferenceGuid = (referenceGuid is not null) ? referenceGuid : null,
            JobStatus = JobStatus.Waiting
        };

        await _jobRepository.AddAsync(jobRecord);

        return jobRecord;
    }
}