using EdNexusData.Broker.Domain;
using System.Text.Json;
using EdNexusData.Broker.Service.Worker;
using EdNexusData.Broker.Service.Resolvers;
using Ardalis.GuardClauses;
using EdNexusData.Broker.Domain.Worker;
using EdNexusData.Broker.Domain.Specifications;
using EdNexusData.Broker.Service.Services;
using System.ComponentModel;
using EdNexusData.Broker.Core.Jobs;
using EdNexusData.Broker.Core.PayloadContents;

namespace EdNexusData.Broker.Service.Jobs;

[Description("Load Payload")]
public class PayloadLoaderJob : IJob
{
    private readonly PayloadResolver _payloadResolver;
    private readonly PayloadJobResolver _payloadJobResolver;
    private readonly JobStatusService<PayloadLoaderJob> _jobStatusService;
    private readonly IRepository<Request> _requestRepository;
    private readonly IRepository<Domain.PayloadContent> _payloadContentRepository;
    private readonly FocusEducationOrganizationResolver _focusEducationOrganizationResolver;
    private readonly JobStatusService _jobStatusServiceForLoader;
    private readonly JobService jobService;

    public PayloadLoaderJob(
            PayloadResolver payloadResolver,
            PayloadJobResolver payloadJobResolver,
            JobStatusService<PayloadLoaderJob> jobStatusService,
            IRepository<Request> requestRepository,
            IRepository<Domain.PayloadContent> payloadContentRepository,
            FocusEducationOrganizationResolver focusEducationOrganizationResolver,
            JobStatusService jobStatusServiceForLoader,
            JobService jobService)
    {
        _payloadResolver = payloadResolver;
        _payloadJobResolver = payloadJobResolver;
        _jobStatusService = jobStatusService;
        _requestRepository = requestRepository;
        _payloadContentRepository = payloadContentRepository;
        _focusEducationOrganizationResolver = focusEducationOrganizationResolver;
        _jobStatusServiceForLoader = jobStatusServiceForLoader;
        this.jobService = jobService;
    }
    
    public async Task ProcessAsync(Job jobInstance)
    {
        Guard.Against.Null(jobInstance.ReferenceGuid, "referenceGuid", $"Unable to find request Id {jobInstance.ReferenceGuid}");
        
        var request = await _requestRepository.FirstOrDefaultAsync(new RequestByIdwithEdOrgs(jobInstance.ReferenceGuid.Value));
        
        Guard.Against.Null(request, "request", $"Unable to find request id {jobInstance.ReferenceGuid}");

        await _jobStatusService.UpdateRequestStatus(jobInstance, request, RequestStatus.Loading, "Begin outgoing jobs loading for: {0}", request.Payload);

        await _jobStatusService.UpdateRequestStatus(jobInstance, request, RequestStatus.Loading, "Begin fetching payload contents for: {0}", request.EducationOrganization?.ParentOrganizationId);

        // Get outgoing payload settings
        var outgoingPayloadSettings = await _payloadResolver.FetchOutgoingPayloadSettingsAsync(request.Payload, request.EducationOrganization!.ParentOrganizationId!.Value);
        var outgoingPayloadContents = outgoingPayloadSettings.PayloadContents;

        if (outgoingPayloadContents is null || outgoingPayloadContents.Count <= 0)
        {
            await _jobStatusService.UpdateRequestStatus(jobInstance, request, RequestStatus.Loading, "No payload contents");
            return;
        }

        // Determine which jobs to execute based on outgoing payload config
        foreach(var outgoingPayloadContent in outgoingPayloadContents)
        {
            // Set the ed org
            _focusEducationOrganizationResolver.EducationOrganizationId = request.EducationOrganization!.ParentOrganizationId!.Value;

            // Resolve job to execute
            await _jobStatusService.UpdateRequestStatus(jobInstance, request, RequestStatus.Loading, "Resolving job to execute for payload content type: {0}", outgoingPayloadContent.PayloadContentType);
            var jobToExecute = _payloadJobResolver.Resolve(outgoingPayloadContent.PayloadContentType);
            await _jobStatusService.UpdateRequestStatus(jobInstance, request, RequestStatus.Loading, "Resolved job to execute: {0}", jobToExecute.GetType().FullName);

            // Attach job status info
            jobToExecute.JobStatusService = new JobStatusServiceProxy(jobToExecute, jobInstance, request);

            object? result = null;

            try
            {
                if (jobToExecute is DelayedPayloadJob)
                {
                    DelayedPayloadJob delayedJobToExecute = (DelayedPayloadJob)jobToExecute!;
                    
                    var startResult = await delayedJobToExecute.StartAsync(request.Student?.Student?.StudentNumber!, (outgoingPayloadContent.Settings is not null) ? JsonDocument.Parse(outgoingPayloadContent.Settings) : null);
                    
                    if (startResult == DelayedJobStatus.Finish)
                    {
                        result = await delayedJobToExecute.FinishAsync();
                    }
                    else
                    {
                        var continueLooping = true;
                        DelayedJobStatus? continueResult = null;
                        while (continueLooping)
                        {
                            await Task.Delay(5000);
                            continueResult = await delayedJobToExecute.ContinueAsync();
                            if (continueResult != DelayedJobStatus.Continue)
                                continueLooping = false;
                        }

                        if (continueResult is not null && continueResult == DelayedJobStatus.Finish)
                        {
                            result = await delayedJobToExecute.FinishAsync();
                        }
                    }
                }
                else
                {
                    // Execute the job
                    result = await jobToExecute.ExecuteAsync(request.Student?.Student?.StudentNumber!, (outgoingPayloadContent.Settings is not null) ? JsonDocument.Parse(outgoingPayloadContent.Settings) : null);
                }
            }
            catch (Exception e)
            {
                await _jobStatusService.UpdateRequestStatus(jobInstance, request, RequestStatus.Loaded, "Errored with: {0}.", e.Message);
                throw;
            }
            

            await _jobStatusService.UpdateRequestStatus(jobInstance, request, RequestStatus.Loading, "Received result: {0}", (result is null) ? "No data" : result.GetType().FullName);
            
            // check if there is a result and if it is of type DataPayloadContent
            if (result is not null && result.GetType().IsAssignableTo(typeof(DataPayloadContent)))
            {
                var payloadContentResult = (DataPayloadContent)result;

                Guard.Against.Null(payloadContentResult, "payloadContentResult", "Unable to cast result to DataPayloadContent type.");
                
                var payloadContentTypeType = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(s => s.GetExportedTypes())
                        .Where(p => p.FullName == outgoingPayloadContent.PayloadContentType).FirstOrDefault();

                // Save the result
                var payloadContent = new Domain.PayloadContent()
                {
                    RequestId = request.Id,
                    JsonContent = JsonSerializer.SerializeToDocument(result), // JsonDocument.Parse(result.Content),
                    ContentType = payloadContentResult.Schema.ContentType,
                    FileName =  $"{result?.GetType().Name}.json"
                };
                await _payloadContentRepository.AddAsync(payloadContent);
                await _jobStatusService.UpdateRequestStatus(jobInstance, request, RequestStatus.Loading, "Saved data payload content: {0}", jobToExecute.GetType().FullName);
            }

            // check if there is a result and if it is of type DataPayloadContent
            if (result is not null && result.GetType().IsAssignableTo(typeof(DocumentPayloadContent)))
            {
                var payloadContentResult = (DocumentPayloadContent)result;

                Guard.Against.Null(payloadContentResult, "payloadContentResult", "Unable to cast result to DocumentPayloadContent type.");

                // Save the result
                var payloadContent = new Domain.PayloadContent()
                {
                    RequestId = request.Id,
                    BlobContent = payloadContentResult.Content,
                    ContentType = payloadContentResult.ContentType,
                    FileName =  payloadContentResult.FileName
                };
                await _payloadContentRepository.AddAsync(payloadContent);
                await _jobStatusService.UpdateRequestStatus(jobInstance, request, RequestStatus.Loading, "Saved document payload content: {0}", jobToExecute.GetType().FullName);
            }
        }

        await _jobStatusService.UpdateRequestStatus(jobInstance, request, RequestStatus.Loaded, "Finished updating request.");

        // Queue job to send update
        var jobData = new MessageContents { RequestId = request.Id, RequestStatus = RequestStatus.Loaded, MessageText = "Updated request status to loaded." };
        var job = await jobService.CreateJobAsync(typeof(SendMessageJob), typeof(Request), request.Id, null, JsonSerializer.SerializeToDocument(jobData));
    }
}