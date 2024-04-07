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

namespace EdNexusData.Broker.Web.Controllers;

[Authorize(Policy = TransferIncomingRecords)]
public class MappingController : AuthenticatedController<MappingController>
{
    private readonly IReadRepository<EducationOrganization> _educationOrganizationRepository;
    private readonly IRepository<Request> _incomingRequestRepository;
    private readonly IRepository<Mapping> _mappingRepository;
    private readonly IServiceProvider _serviceProvider;

    public MappingController(
        IReadRepository<EducationOrganization> educationOrganizationRepository,
        IRepository<Request> incomingRequestRepository,
        IRepository<Mapping> mappingRepository,
        IServiceProvider serviceProvider)
    {
        _educationOrganizationRepository = educationOrganizationRepository;
        _incomingRequestRepository = incomingRequestRepository;
        _mappingRepository = mappingRepository;
        _serviceProvider = serviceProvider;
    }

    public async Task<IActionResult> Index(Guid id)
    {
        var incomingRequest = await _incomingRequestRepository.GetByIdAsync(id);
        if (incomingRequest is null) return NotFound();

        var mappings = await _mappingRepository.ListAsync(new MappingByRequestId(incomingRequest.Id));

        if (mappings is not null && mappings.Count > 0)
        {
            return RedirectToAction(nameof(Edit), new { id = mappings.FirstOrDefault()!.Id } );
        }

        return View(incomingRequest);
    }

    [HttpPost]
    public async Task<IActionResult> Prepare(Guid id)
    {
        var incomingRequest = await _incomingRequestRepository.FirstOrDefaultAsync(new RequestByIdwithEdOrgs(id));

        Guard.Against.Null(incomingRequest);

        incomingRequest.RequestStatus = RequestStatus.WaitingToPrepare;

        await _incomingRequestRepository.UpdateAsync(incomingRequest);

        TempData[VoiceTone.Positive] = $"Preparing mapping for request ({incomingRequest.Id}).";
        return RedirectToAction(nameof(Index), new { id = id });
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var mapping = await _mappingRepository.GetByIdAsync(id);
        if (mapping is null) return NotFound();

        var incomingRequest = await _incomingRequestRepository.GetByIdAsync(mapping.RequestId!.Value);
        if (incomingRequest is null) return NotFound();

        var mappings = await _mappingRepository.ListAsync(new MappingByRequestId(mapping.RequestId!.Value));

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