using System.Reflection;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using EdNexusData.Broker.Web.Constants.DesignSystems;
using EdNexusData.Broker.Web.ViewModels.Mappings;
using EdNexusData.Broker.Core.Lookup;
using static EdNexusData.Broker.Web.Constants.Claims.CustomClaimType;
using EdNexusData.Broker.Core.Jobs;
using EdNexusData.Broker.Web.Helpers;
using EdNexusData.Broker.Core.Services;

namespace EdNexusData.Broker.Web.Controllers;

[Authorize(Policy = TransferIncomingRecords)]
public class MappingController : AuthenticatedController<MappingController>
{
    private readonly IReadRepository<EducationOrganization> _educationOrganizationRepository;
    private readonly IRepository<Request> _incomingRequestRepository;
    private readonly IRepository<Mapping> _mappingRepository;
    private readonly IRepository<PayloadContent> _payloadContentRepository;
    private readonly IRepository<PayloadContentAction> _actionRepository;
    private readonly JobService _jobService;
    private readonly IServiceProvider _serviceProvider;
    private readonly CurrentUserHelper currentUserHelper;
    private readonly MappingRecordValidatorService mappingRecordValidatorService;

  public MappingController(
        IReadRepository<EducationOrganization> educationOrganizationRepository,
        IRepository<Request> incomingRequestRepository,
        IRepository<Mapping> mappingRepository,
        IRepository<PayloadContent> payloadContentRepository,
        IRepository<PayloadContentAction> actionRepository,
        JobService jobService,
        IServiceProvider serviceProvider,
        CurrentUserHelper currentUserHelper,
        MappingRecordValidatorService mappingRecordValidatorService)
    {
        _educationOrganizationRepository = educationOrganizationRepository;
        _incomingRequestRepository = incomingRequestRepository;
        _mappingRepository = mappingRepository;
        _payloadContentRepository = payloadContentRepository;
        _jobService = jobService;
        _actionRepository = actionRepository;
        _serviceProvider = serviceProvider;
        this.currentUserHelper = currentUserHelper;
        this.mappingRecordValidatorService = mappingRecordValidatorService;
  }

    public async Task<IActionResult> Index(Guid id, Guid? jobId)
    {
        var mapping = await _mappingRepository.FirstOrDefaultAsync(new MappingByActionId(id));
        if (mapping is null) // Mapping needs preparing
        {
            var action = await _actionRepository.GetByIdAsync(id);
            ViewBag.JobId = jobId;
            return View(action);
        }

        return RedirectToAction(nameof(Edit), new { id = mapping.Id });
    }

    [HttpPost]
    public async Task<IActionResult> Prepare(Guid id)
    {
        var action = await _actionRepository.FirstOrDefaultAsync(new PayloadContentActionWithPayloadContent(id));

        Guard.Against.Null(action, "action", "Invalid action");
        Guard.Against.Null(action.PayloadContent, "action.PayloadContent", "Invalid payload content to action");

        // Prepare mapping
        var createdJob = await _jobService.CreateJobAsync(typeof(PrepareMappingJob), typeof(PayloadContentAction), action.Id, currentUserHelper.CurrentUserId()!.Value);

        action.PayloadContentActionStatus = PayloadContentActionStatus.WaitingToPrepare;
        await _actionRepository.UpdateAsync(action);

        TempData[VoiceTone.Positive] = $"Preparing mapping for {action.PayloadContent!.FileName} ({action.PayloadContent!.Id}).";
        return RedirectToAction(nameof(Index), new { id = id, jobId = createdJob.Id });
    }

    public async Task<IActionResult> Edit(Guid id, Guid? jobId)
    {
        var mapping = await _mappingRepository.FirstOrDefaultAsync(new MappingWithPayloadContent(id));
        if (mapping is null) return NotFound();

        if (mapping.PayloadContentAction!.PayloadContentActionStatus.NotIn(PayloadContentActionStatus.Prepared, PayloadContentActionStatus.Imported))
        {
            ViewBag.JobId = jobId;
        }

        Type mappingType = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => p.FullName == mapping.MappingType).FirstOrDefault()!;

        Type mappingCollectionType = typeof(List<>).MakeGenericType([mappingType]);

        Guard.Against.Null(mapping.PayloadContentAction, "mapping.Action", "Action missing");
        Guard.Against.Null(mapping.PayloadContentAction.PayloadContent, "mapping.Action.PayloadContent", "Payload content missing");
        
        var viewModel = new MappingViewModel()
        {
            MappingId = mapping.Id,
            MappingSourceRecords = JsonConvert.DeserializeObject(mapping.JsonSourceMapping.ToJsonString()!, mappingCollectionType)!,
            MappingDestinationRecords = JsonConvert.DeserializeObject(mapping.JsonDestinationMapping.ToJsonString()!, mappingCollectionType)!,
            
            Mapping = mapping,
            MappingLookupService = _serviceProvider.GetService<MappingLookupService>(),
            RequestId = mapping.PayloadContentAction.PayloadContent.RequestId
        };

        viewModel.SetProperties(mapping.MappingType!);

        viewModel.IsValid = mappingRecordValidatorService.IsValidRecords(viewModel.MappingDestinationRecords);

        return View(viewModel);
    }

    [HttpPut]
    public async Task<IActionResult> Update(Guid mappingId, IFormCollection form)
    {
        var mapping = await _mappingRepository.GetByIdAsync(mappingId);
        if (mapping is null) return NotFound();
        
        // Create a generic type of what was sent through
        Type mappingType = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => p.FullName == mapping.MappingType).FirstOrDefault()!;

        Type mappingCollectionType = typeof(List<>).MakeGenericType([mappingType]);

        var mappedForm = ActivatorUtilities.CreateInstance(_serviceProvider, mappingCollectionType);

        // Perform the model binding
        await TryUpdateModelAsync(mappedForm, mappingCollectionType, "mapping");

        // Cast mappedForm to IEnumerable
        var mappedFormEnumerable = (IEnumerable<dynamic>)mappedForm;

        ArgumentNullException.ThrowIfNull(mapping.JsonDestinationMapping);

        var originalMappedForm = (IEnumerable<dynamic>)System.Text.Json.JsonSerializer.Deserialize(mapping.JsonDestinationMapping, mappingCollectionType)!;

        // Loop through new values and update original
        foreach(var record in mappedFormEnumerable)
        {
            // Get properties
            Type recordType = record.GetType();
            var properties = recordType.GetProperties().Where(prop => prop.CanRead && prop.CanWrite);
            
            // Find corresponding object in original
            var originalRecord = originalMappedForm.Where(x => x.BrokerId == record.BrokerId).FirstOrDefault();
            if (originalRecord != null)
            {
                // Loop through properties
                foreach(var property in properties.Where(x => x.Name != "BrokerId" && x.Name != "IsValid"))
                {
                    // Update property value not null
                    if (property.GetValue(record) != null)
                    {
                        var newValue = property.GetValue(record);
                        
                        PropertyInfo originalProperty = originalRecord.GetType().GetProperty(property.Name);
                        if (originalProperty is not null)
                        {
                            originalProperty.SetValue(originalRecord, newValue);
                        }
                    }
                }
            }
            
            // TODO: Add the ability to add a record
        }

        // Validate the records
        var validatedRecords = mappingRecordValidatorService.ValidateRecords(originalMappedForm);

        // Persist the data
        mapping.JsonDestinationMapping = validatedRecords.ToJsonDocument();

        await _mappingRepository.UpdateAsync(mapping);
        
        TempData[VoiceTone.Positive] = $"Updated mapping ({mapping.Id}).";
        return RedirectToAction(nameof(Edit), new { id = mappingId });
    }

    [HttpPut]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(Guid id, Guid MappingId)
    {
        var payloadContentAction = await _actionRepository.GetByIdAsync(id);
        Guard.Against.Null(payloadContentAction, "payloadContentAction", "Unable to find payload content action.");

        // Create job
        var createdJob = await _jobService.CreateJobAsync(typeof(PayloadContentActionJob), typeof(PayloadContentAction), payloadContentAction.Id, currentUserHelper.CurrentUserId()!.Value);

        payloadContentAction.PayloadContentActionStatus = PayloadContentActionStatus.WaitingToImport;
        await _actionRepository.UpdateAsync(payloadContentAction);

        TempData[VoiceTone.Positive] = $"Mapping waiting to execute payload content action {payloadContentAction.PayloadContentActionType} ({payloadContentAction.Id}).";
        return RedirectToAction(nameof(Edit), new { id = MappingId, jobId = createdJob.Id });
    }

    public async Task<IActionResult> Detail(Guid id, Guid mappingBrokerId, int propertyCounter)
    {
        var mapping = await _mappingRepository.GetByIdAsync(id);

        ArgumentNullException.ThrowIfNull(mapping);

        Type mappingType = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => p.FullName == mapping.MappingType).FirstOrDefault()!;

        Type mappingCollectionType = typeof(List<>).MakeGenericType([mappingType]);

        var sourceMapping = (IEnumerable<dynamic>)JsonConvert.DeserializeObject(mapping.JsonSourceMapping.ToJsonString()!, mappingCollectionType)!;
        var initialMapping = (IEnumerable<dynamic>)JsonConvert.DeserializeObject(mapping.JsonInitialMapping.ToJsonString()!, mappingCollectionType)!;
        var destinationMapping = (IEnumerable<dynamic>)JsonConvert.DeserializeObject(mapping.JsonDestinationMapping.ToJsonString()!, mappingCollectionType)!;

        var sourceRecord = sourceMapping.Where(x => x.BrokerId == mappingBrokerId.ToString()).FirstOrDefault();
        var initialRecord = initialMapping.Where(x => x.BrokerId == mappingBrokerId.ToString()).FirstOrDefault();
        var destinationRecord = destinationMapping.Where(x => x.BrokerId == mappingBrokerId.ToString()).FirstOrDefault();

        var detailViewModel = new MappingDetailViewModel()
        {
            MappingBrokerId = mappingBrokerId,
            Source = sourceRecord!,
            Initial = initialRecord!,
            Destination = destinationRecord!,
            Mapping = mapping,
            MappingLookupService = _serviceProvider.GetService<MappingLookupService>(),
            PropertyCounter = propertyCounter
        };

        detailViewModel.SetProperties(mapping.MappingType!);

        return View(detailViewModel);
    }
}