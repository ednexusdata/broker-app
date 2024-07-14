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
    private readonly JobService _jobService;
    private readonly IServiceProvider _serviceProvider;

    public MappingController(
        IReadRepository<EducationOrganization> educationOrganizationRepository,
        IRepository<Request> incomingRequestRepository,
        IRepository<Mapping> mappingRepository,
        IRepository<PayloadContent> payloadContentRepository,
        JobService jobService,
        IServiceProvider serviceProvider)
    {
        _educationOrganizationRepository = educationOrganizationRepository;
        _incomingRequestRepository = incomingRequestRepository;
        _mappingRepository = mappingRepository;
        _payloadContentRepository = payloadContentRepository;
        _jobService = jobService;
        _serviceProvider = serviceProvider;
    }

    public async Task<IActionResult> Index(Guid id)
    {
        var mapping = await _mappingRepository.FirstOrDefaultAsync(new MappingByPayloadContentId(id));
        if (mapping is null) // Mapping needs preparing
        {
            var payloadContent = await _payloadContentRepository.GetByIdAsync(id);
            return View(payloadContent);
        }

        return RedirectToAction(nameof(Edit), new { id = mapping.Id });
    }

    [HttpPost]
    public async Task<IActionResult> Prepare(Guid id)
    {
        var payloadContent = await _payloadContentRepository.GetByIdAsync(id);

        Guard.Against.Null(payloadContent, "payloadContent", "Invalid payload content");

        // Prepare mapping
        await _jobService.CreateJobAsync(typeof(PrepareMappingJob), typeof(PayloadContent), payloadContent.Id);

        TempData[VoiceTone.Positive] = $"Preparing mapping for {payloadContent.FileName} ({payloadContent.Id}).";
        return RedirectToAction(nameof(Index), new { id = id });
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var mapping = await _mappingRepository.FirstOrDefaultAsync(new MappingWithPayloadContent(id));
        if (mapping is null) return NotFound();

        var mappings = await _mappingRepository.ListAsync(new MappingByRequestId(mapping.PayloadContent!.RequestId));

        Type mappingType = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => p.FullName == mapping.MappingType).FirstOrDefault()!;

        Type mappingCollectionType = typeof(List<>).MakeGenericType([mappingType]);
        
        var viewModel = new MappingViewModel()
        {
            MappingId = mapping.Id,
            RequestMappings = mappings,
            MappingSourceRecords = JsonConvert.DeserializeObject(mapping.SourceMapping.ToJsonString()!, mappingCollectionType)!,
            MappingDestinationRecords = JsonConvert.DeserializeObject(mapping.DestinationMapping.ToJsonString()!, mappingCollectionType)!,
            Mapping = mapping,
            MappingLookupService = _serviceProvider.GetService<MappingLookupService>()
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
        mapping.DestinationMapping = mappedForm.ToJsonDocument();

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