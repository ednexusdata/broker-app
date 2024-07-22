using System.Reflection;
using System.Text.Json;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using EdNexusData.Broker.Domain;
using EdNexusData.Broker.Domain.Specifications;
using EdNexusData.Broker.SharedKernel;
using EdNexusData.Broker.Web.Constants.DesignSystems;
using EdNexusData.Broker.Web.Extensions.Genders;
using EdNexusData.Broker.Web.Extensions.States;
using EdNexusData.Broker.Web.ViewModels.Mappings;
using EdNexusData.Broker.Web.ViewModels.Requests;
using EdNexusData.Broker.Service.Lookup;
using static EdNexusData.Broker.Web.Constants.Claims.CustomClaimType;
using EdNexusData.Broker.Service;
using EdNexusData.Broker.Service.Jobs;

namespace EdNexusData.Broker.Web.Controllers;

[Authorize(Policy = TransferIncomingRecords)]
public class MappingController : AuthenticatedController<MappingController>
{
    private readonly IReadRepository<EducationOrganization> _educationOrganizationRepository;
    private readonly IRepository<Request> _incomingRequestRepository;
    private readonly IRepository<Mapping> _mappingRepository;
    private readonly IRepository<PayloadContent> _payloadContentRepository;
    private readonly IRepository<Domain.Action> _actionRepository;
    private readonly JobService _jobService;
    private readonly IServiceProvider _serviceProvider;

    public MappingController(
        IReadRepository<EducationOrganization> educationOrganizationRepository,
        IRepository<Request> incomingRequestRepository,
        IRepository<Mapping> mappingRepository,
        IRepository<PayloadContent> payloadContentRepository,
        IRepository<Domain.Action> actionRepository,
        JobService jobService,
        IServiceProvider serviceProvider)
    {
        _educationOrganizationRepository = educationOrganizationRepository;
        _incomingRequestRepository = incomingRequestRepository;
        _mappingRepository = mappingRepository;
        _payloadContentRepository = payloadContentRepository;
        _jobService = jobService;
        _actionRepository = actionRepository;
        _serviceProvider = serviceProvider;
    }

    public async Task<IActionResult> Index(Guid id)
    {
        var mapping = await _mappingRepository.FirstOrDefaultAsync(new MappingByActionId(id));
        if (mapping is null) // Mapping needs preparing
        {
            var action = await _actionRepository.GetByIdAsync(id);
            return View(action);
        }

        return RedirectToAction(nameof(Edit), new { id = mapping.Id });
    }

    [HttpPost]
    public async Task<IActionResult> Prepare(Guid id)
    {
        var action = await _actionRepository.FirstOrDefaultAsync(new ActionWithPayloadContent(id));

        Guard.Against.Null(action, "action", "Invalid action");
        Guard.Against.Null(action.PayloadContent, "action.PayloadContent", "Invalid payload content to action");

        // Prepare mapping
        await _jobService.CreateJobAsync(typeof(PrepareMappingJob), typeof(Domain.Action), action.Id);

        TempData[VoiceTone.Positive] = $"Preparing mapping for {action.PayloadContent!.FileName} ({action.PayloadContent!.Id}).";
        return RedirectToAction(nameof(Index), new { id = id });
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var mapping = await _mappingRepository.FirstOrDefaultAsync(new MappingWithPayloadContent(id));
        if (mapping is null) return NotFound();

        var action = await _actionRepository.FirstOrDefaultAsync(new ActionWithPayloadContent(mapping.ActionId.Value));

        Type mappingType = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => p.FullName == mapping.MappingType).FirstOrDefault()!;

        Type mappingCollectionType = typeof(List<>).MakeGenericType([mappingType]);

        Guard.Against.Null(action, "mapping.Action", "Action missing");
        Guard.Against.Null(action.PayloadContent, "mapping.Action.PayloadContent", "Payload content missing");
        
        var viewModel = new MappingViewModel()
        {
            MappingId = mapping.Id,
            MappingSourceRecords = JsonConvert.DeserializeObject(mapping.JsonSourceMapping.ToJsonString()!, mappingCollectionType)!,
            MappingDestinationRecords = JsonConvert.DeserializeObject(mapping.JsonDestinationMapping.ToJsonString()!, mappingCollectionType)!,
            Mapping = mapping,
            MappingLookupService = _serviceProvider.GetService<MappingLookupService>(),
            RequestId = action.PayloadContent.RequestId
        };

        viewModel.SetProperties(mapping.MappingType!);

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

        // Persist the data
        mapping.JsonDestinationMapping = mappedForm.ToJsonDocument();

        await _mappingRepository.UpdateAsync(mapping);
        
        TempData[VoiceTone.Positive] = $"Updated mapping ({mapping.Id}).";
        return RedirectToAction(nameof(Edit), new { id = mappingId });
    }

    [HttpPut]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(Guid id)
    {
        var incomingRequest = await _incomingRequestRepository.FirstOrDefaultAsync(new RequestByIdwithEdOrgs(id));

        Guard.Against.Null(incomingRequest);

        incomingRequest.RequestStatus = RequestStatus.WaitingToImport;

        await _incomingRequestRepository.UpdateAsync(incomingRequest);

        TempData[VoiceTone.Positive] = $"Request waiting to import ({incomingRequest.Id}).";
        return RedirectToAction(nameof(Index), new { id = id });
    }
}