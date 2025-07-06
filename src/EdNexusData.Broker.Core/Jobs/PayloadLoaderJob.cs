using System.Text.Json;
using EdNexusData.Broker.Core.Worker;
using EdNexusData.Broker.Core.Resolvers;
using EdNexusData.Broker.Core.Services;
using System.ComponentModel;
using EdNexusData.Broker.Common.Jobs;
using EdNexusData.Broker.Common.PayloadContents;
using EdNexusData.Broker.Core.Interfaces;
using EdNexusData.Broker.Core.Messages;

namespace EdNexusData.Broker.Core.Jobs;

[Description("Load Payload")]
public class PayloadLoaderJob : IJob
{
    private readonly PayloadResolver _payloadResolver;
    private readonly PayloadJobResolver _payloadJobResolver;
    private readonly JobStatusService<PayloadLoaderJob> jobStatusService;
    private readonly PayloadContentService payloadContentService;
    private readonly FocusEducationOrganizationResolver focusEducationOrganizationResolver;
    private readonly RequestService requestService;
    private readonly JobService jobService;
    private readonly EducationOrganizationContactService educationOrganizationContactService;
    private readonly INowWrapper nowWrapper;

    public PayloadLoaderJob(
        PayloadResolver payloadResolver,
        PayloadJobResolver payloadJobResolver,
        JobStatusService<PayloadLoaderJob> jobStatusService,
        PayloadContentService payloadContentService,
        FocusEducationOrganizationResolver focusEducationOrganizationResolver,
        RequestService requestService,
        JobService jobService,
        EducationOrganizationContactService educationOrganizationContactService,
        INowWrapper nowWrapper
    )
    {
        _payloadResolver = payloadResolver;
        _payloadJobResolver = payloadJobResolver;
        this.jobStatusService = jobStatusService;
        this.payloadContentService = payloadContentService;
        this.focusEducationOrganizationResolver = focusEducationOrganizationResolver;
        this.requestService = requestService;
        this.jobService = jobService;
        this.educationOrganizationContactService = educationOrganizationContactService;
        this.nowWrapper = nowWrapper;
    }
    
    public async Task ProcessAsync(Job jobInstance)
    {   
        // Step 1: Get Request
        var request = await requestService.Get(jobInstance);
        _ = request ?? throw new ArgumentNullException($"Unable to find request Id {jobInstance.ReferenceGuid}");

        await jobStatusService.UpdateRequestStatus(jobInstance, request, RequestStatus.Extracting, "Begin outgoing jobs extracting for: {0}", request.Payload);
        await jobStatusService.UpdateRequestStatus(jobInstance, request, RequestStatus.Extracting, "Begin fetching payload contents for: {0}", request.EducationOrganization?.ParentOrganizationId);

        // Step 2: Get outgoing payload settings
        var outgoingPayloadSettings = await _payloadResolver.FetchOutgoingPayloadSettingsAsync(request.Payload, request.EducationOrganization!.ParentOrganizationId!.Value);
        var outgoingPayloadContents = outgoingPayloadSettings.PayloadContents;

        if (outgoingPayloadContents is null || outgoingPayloadContents.Count <= 0)
        {
            await jobStatusService.UpdateRequestStatus(jobInstance, request, RequestStatus.Extracted, "No payload contents");
            return;
        }

        // Step 3: Determine which jobs to execute based on outgoing payload config
        foreach(var outgoingPayloadContent in outgoingPayloadContents)
        {
            // Set the ed org
            focusEducationOrganizationResolver.EducationOrganizationId = request.EducationOrganization!.ParentOrganizationId!.Value;

            // Resolve job to execute
            await jobStatusService.UpdateRequestStatus(jobInstance, request, RequestStatus.Extracting, "Resolving job to execute for payload content type: {0}", outgoingPayloadContent.PayloadContentType);
            var jobToExecute = _payloadJobResolver.Resolve(outgoingPayloadContent.PayloadContentType);
            await jobStatusService.UpdateRequestStatus(jobInstance, request, RequestStatus.Extracting, "Resolved job to execute: {0}", jobToExecute.GetType().FullName);

            // Attach job status info
            jobToExecute.JobStatusService = new JobStatusServiceProxy<PayloadLoaderJob>(jobStatusService, jobInstance, request);

            object? result = null;

            try
            {
                if (jobToExecute is DelayedPayloadJob)
                {
                    DelayedPayloadJob delayedJobToExecute = (DelayedPayloadJob)jobToExecute!;
                    
                    var startResult = await delayedJobToExecute.StartAsync(
                        request.Student?.Student?.StudentNumber!,
                        (outgoingPayloadContent.Settings is not null) ? JsonDocument.Parse(outgoingPayloadContent.Settings) : null,
                        jobToExecute.JobStatusService
                    );
                    
                    if (startResult == DelayedJobStatus.Finish)
                    {
                        result = await delayedJobToExecute.FinishAsync(jobToExecute.JobStatusService);
                    }
                    else
                    {
                        var continueLooping = true;
                        DelayedJobStatus? continueResult = null;
                        while (continueLooping)
                        {
                            await Task.Delay(5000);
                            continueResult = await delayedJobToExecute.ContinueAsync(jobToExecute.JobStatusService);
                            if (continueResult != DelayedJobStatus.Continue)
                                continueLooping = false;
                        }

                        if (continueResult is not null && continueResult == DelayedJobStatus.Finish)
                        {
                            result = await delayedJobToExecute.FinishAsync(jobToExecute.JobStatusService);
                        }
                    }
                }
                else
                {
                    // Execute the job
                    result = await jobToExecute.ExecuteAsync(
                        request.Student?.Student?.StudentNumber!,
                        (outgoingPayloadContent.Settings is not null) ? JsonDocument.Parse(outgoingPayloadContent.Settings) : null,
                        jobToExecute.JobStatusService
                    );
                }
            }
            catch (Exception e)
            {
                await jobStatusService.UpdateRequestStatus(jobInstance, request, RequestStatus.Extracted, "Errored with: {0}.", e.Message);
                throw;
            }
            

            await jobStatusService.UpdateRequestStatus(jobInstance, request, RequestStatus.Extracting, "Received result: {0}", (result is null) ? "No data" : result.GetType().FullName);
            
            // check if there is a result and if it is of type DataPayloadContent
            if (result is not null && result.GetType().IsAssignableTo(typeof(DataPayloadContent)))
            {
                var payloadContentResult = (DataPayloadContent)result;
                _ = payloadContentResult ?? throw new InvalidCastException("Unable to cast result to DataPayloadContent type.");
                
                // var payloadContentTypeType = AppDomain.CurrentDomain.GetAssemblies()
                //         .SelectMany(s => s.GetExportedTypes())
                //         .Where(p => p.FullName == outgoingPayloadContent.PayloadContentType).FirstOrDefault();

                var json = JsonSerializer.SerializeToDocument(result);

                // Save the result
                var payloadContent = await payloadContentService.AddJsonFile(
                    request.Id, 
                    json, 
                    payloadContentResult.Schema.ContentType, 
                    $"{result?.GetType().Name}.json"
                );
                await jobStatusService.UpdateRequestStatus(jobInstance, request, RequestStatus.Extracting, "Saved data payload content: {0}", jobToExecute.GetType().FullName);
            }

            if (result is not null && result.GetType().IsAssignableTo(typeof(List<DataPayloadContent>))) 
            {
                var payloadContentResults = (List<DataPayloadContent>)result;
                _ = payloadContentResults ?? throw new InvalidCastException("Unable to cast result to List<DataPayloadContent> type.");
                
                if (payloadContentResults.Count > 0)
                {
                    foreach(var payloadContentResult in payloadContentResults)
                    {
                        var json = JsonSerializer.SerializeToDocument(payloadContentResult);

                        // Save the result
                        var payloadContent = await payloadContentService.AddJsonFile(
                            request.Id, 
                            json, 
                            payloadContentResult.Schema.ContentType, 
                            $"{payloadContentResult?.GetType().Name}.json"
                        );
                    }
                }

                await jobStatusService.UpdateRequestStatus(jobInstance, request, RequestStatus.Extracting, "Saved data payload content: {0}", jobToExecute.GetType().FullName);
            }

            // check if there is a result and if it is of type DataPayloadContent
            if (result is not null && (result.GetType().IsAssignableTo(typeof(DocumentPayloadContent)) || result.GetType().IsAssignableTo(typeof(List<DocumentPayloadContent>))))
            {
                if (result is DocumentPayloadContent)
                {
                    var payloadContentResult = (DocumentPayloadContent)result;
                    _ = payloadContentResult ?? throw new NullReferenceException("Unable to cast result to DocumentPayloadContent type.");

                    await AddDocument(payloadContentResult, request, jobInstance, jobToExecute);
                }
                
                if (result is List<DocumentPayloadContent>)
                {
                    var payloadContentResults = (List<DocumentPayloadContent>)result;
                    _ = payloadContentResults ?? throw new NullReferenceException("Unable to cast result to List<DocumentPayloadContent> type.");

                    if (payloadContentResults.Count > 0)
                    {
                        foreach(var payloadContentResult in payloadContentResults)
                        {
                            await AddDocument(payloadContentResult, request, jobInstance, jobToExecute);
                        }
                    }
                }
            }
        }

        await jobStatusService.UpdateRequestStatus(jobInstance, request, RequestStatus.Extracted, "Finished updating request.");

        // Queue job to send update
        var jobData = new MessageContents { 
            Sender = await educationOrganizationContactService.FromUser(jobInstance.InitiatedUserId!.Value), 
            SenderSentTimestamp = nowWrapper.UtcNow, 
            RequestId = request?.Id, 
            RequestStatus = RequestStatus.Extracted, 
            MessageText = "Updated request status to extracted.",
            EducationOrganizationId = request?.EducationOrganizationId,
            MessageType = typeof(StatusUpdateMessage).FullName
        };
        var job = await jobService.CreateJobAsync(typeof(SendMessageJob), typeof(Request), request?.Id, jobInstance.InitiatedUserId, JsonSerializer.SerializeToDocument(jobData));
    }

    private async Task<PayloadContent?> AddDocument(DocumentPayloadContent document, Request request, Job jobInstance, PayloadJob jobToExecute)
    {
        var payloadContent = await payloadContentService.AddBlobFile(
            request.Id, 
            document.Content, 
            document.ContentType, 
            document.FileName
        );
        await jobStatusService.UpdateRequestStatus(jobInstance, request, RequestStatus.Extracting, "Saved document payload content: {0}", jobToExecute.GetType().FullName);
        return payloadContent;
    }
}