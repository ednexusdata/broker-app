using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EdNexusData.Broker.Web.Helpers;
using EdNexusData.Broker.Web.Constants.DesignSystems;
using EdNexusData.Broker.Web.Models.OutgoingRequests;
using EdNexusData.Broker.Web.Models.Paginations;
using Ardalis.Specification;
using EdNexusData.Broker.Web.ViewModels.EducationOrganizations;
using System.Linq.Expressions;
using EdNexusData.Broker.Web.Extensions.States;
using EdNexusData.Broker.Web.Specifications;
using EdNexusData.Broker.Common.EducationOrganizations;

namespace EdNexusData.Broker.Web.Controllers;

[Authorize(Policy = "SuperAdmin")]
public class EducationOrganizationsController : AuthenticatedController<EducationOrganizationsController>
{
    private readonly IRepository<Core.EducationOrganization> _educationOrganizationRepository;
    private readonly EducationOrganizationHelper _educationOrganizationHelper;
    private readonly FocusHelper focusHelper;

    public EducationOrganizationsController(
        IRepository<Core.EducationOrganization> educationOrganizationRepository,
        EducationOrganizationHelper educationOrganizationHelper,
        FocusHelper focusHelper)
    {
        _educationOrganizationRepository = educationOrganizationRepository;
        _educationOrganizationHelper = educationOrganizationHelper;
        this.focusHelper = focusHelper;
    }

    public async Task<IActionResult> Index(
      EducationOrganizationRequestModel model,
      CancellationToken cancellationToken)
    {
        RefreshSession();

        var searchExpressions = model.BuildSearchExpressions();

        var focusedEdOrgs = await focusHelper.GetFocusedEdOrgs();
        searchExpressions.Add(x => focusedEdOrgs.Contains(x));

        var sortExpressions = model.BuildSortExpressions();

        var specificationPre = new SearchableWithPaginationSpecification<Core.EducationOrganization>.Builder(model.Page, model.Size)
            .WithAscending(model.IsAscending)
            .WithSortExpressions(sortExpressions)
            .WithSearchExpressions(searchExpressions)
            .WithIncludeEntities(builder => builder
                .Include(educationOrganization => educationOrganization.ParentOrganization))
            ;
       var specification = specificationPre.Build();

        var totalItems = await _educationOrganizationRepository.CountAsync(
            specification,
            cancellationToken);

        var educationOrganizations = await _educationOrganizationRepository.ListAsync(
            specification,
            cancellationToken);

        var educationOrganizationViewModels = educationOrganizations
            .Select(educationOrganization => new EducationOrganizationRequestViewModel(educationOrganization));

        var result = new PaginatedViewModel<EducationOrganizationRequestViewModel>(
            educationOrganizationViewModels,
            totalItems,
            model.Page,
            model.Size,
            model.SortBy,
            model.SortDir,
            model.SearchBy);

        return View(result);
    }
    
    [HttpGet("search")]
    public async Task<IActionResult> Search(
      EducationOrganizationRequestModel model,
      CancellationToken cancellationToken)
    {
        RefreshSession();

        var searchExpressions = model.BuildSearchExpressions();

        var schools = await focusHelper.GetFocusedSchools();
        searchExpressions.Add(x => schools.Contains(x));

        var sortExpressions = model.BuildSortExpressions();

        var specification = new SearchableWithPaginationSpecification<Core.EducationOrganization>.Builder(model.Page, model.Size)
            .WithAscending(model.IsAscending)
            .WithSortExpressions(sortExpressions)
            .WithSearchExpressions(searchExpressions)
            .WithIncludeEntities(builder => builder
                .Include(educationOrganization => educationOrganization.ParentOrganization)
            )
            .Build();

        var totalItems = await _educationOrganizationRepository.CountAsync(
            specification,
            cancellationToken);

        var educationOrganizations = await _educationOrganizationRepository.ListAsync(
            specification,
            cancellationToken);

        var educationOrganizationViewModels = educationOrganizations
            .Select(educationOrganization => new EducationOrganizationRequestViewModel(educationOrganization));

        var result = new PaginatedViewModel<EducationOrganizationRequestViewModel>(
            educationOrganizationViewModels,
            totalItems,
            model.Page,
            model.Size,
            model.SortBy,
            model.SortDir,
            model.SearchBy);

        return Ok(result);
    }
    public async Task<IActionResult> Create()
    {
        var viewModel = new CreateEducationOrganizationRequestViewModel
        {
            DistrictEducationOrganizations = await _educationOrganizationHelper.GetDistrictsOrganizationsSelectList(),
            RegionEducationOrganizations = await _educationOrganizationHelper.GetRegionOrganizationsSelectList(),
            States = States.GetSelectList()
        };

        return View(viewModel);
    }

    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> Create(CreateEducationOrganizationRequestViewModel data)
    {
        if (!ModelState.IsValid) { TempData[VoiceTone.Critical] = "Organization not created."; return RedirectToAction(nameof(Create)); }
        
        var organization = new Core.EducationOrganization()
        {
            Id = Guid.NewGuid(),
            ParentOrganizationId = 
                data.EducationOrganizationType == EducationOrganizationType.School ? 
                data.DistrictParentOrganizationId : data.RegionParentOrganizationId,
            Name = data.Name,
            ShortName = data.ShortName,
            Number = data.Number,
            CeebCode = data.CeebCode,
            NcesCode = data.NcesCode,
            StateCode = data.StateCode,
            EducationOrganizationType = data.EducationOrganizationType,
            Address = new Core.Address()
            {
                StreetNumberName = data.StreetNumberName,
                City = data.City,
                PostalCode = data.PostalCode,
                StateAbbreviation = data.StateAbbreviation
            },
            Domain = data.Domain,
            TimeZone = data.TimeZone!,
            Contacts = new List<Core.EducationOrganizationContact>()
            {
                new Core.EducationOrganizationContact {
                    Name = data.ContactName,
                    Email = data.ContacEmail,
                    Phone = data.ContactPhone,
                    JobTitle = data.ContactJobTitle
                }
            }
        };

        await _educationOrganizationRepository.AddAsync(organization);

        TempData[VoiceTone.Positive] = $"Created organization {organization.Name} ({organization.Id}).";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Update(Guid Id)
    {
       Expression<Func<Core.EducationOrganization, bool>> focusOrganizationExpression = request =>
            request.Id == Id;

        var specification = new SearchableWithPaginationSpecification<Core.EducationOrganization>.Builder(1, -1)
            .WithSearchExpression(focusOrganizationExpression)
            .Build();

        var organizations = await _educationOrganizationRepository.ListAsync(specification,CancellationToken.None);

        var organization = organizations.FirstOrDefault();
        var organizationViewModel = new CreateEducationOrganizationRequestViewModel();

        if (organization is not null)
        {
            organizationViewModel = new CreateEducationOrganizationRequestViewModel()
            {
                EducationOrganizationId = organization.Id,
                ParentOrganizationId = organization.ParentOrganizationId,
                DistrictParentOrganizationId = (organization.EducationOrganizationType == EducationOrganizationType.School) ? organization.ParentOrganizationId : null,
                RegionParentOrganizationId = (organization.EducationOrganizationType != EducationOrganizationType.School) ? organization.ParentOrganizationId : null,
                Name = organization.Name!,
                ShortName = organization.ShortName,
                EducationOrganizationType = organization.EducationOrganizationType,
                Number = organization.Number!,
                CeebCode = organization.CeebCode!,
                NcesCode = organization.NcesCode!,
                StateCode = organization.StateCode!,
                StreetNumberName = organization.Address?.StreetNumberName!,
                City = organization.Address?.City,
                StateAbbreviation = organization.Address?.StateAbbreviation,
                PostalCode = organization.Address?.PostalCode,
                States = States.GetSelectList(),
                Domain = organization.Domain,
                TimeZone = organization.TimeZone ?? ((TimeZoneInfo.Local.Id == "Etc/UTC") ? TimeZoneInfo.Utc.Id : TimeZoneInfo.Local.Id),
                ContactName = organization.Contacts?.First().Name,
                ContactJobTitle = organization.Contacts?.First().JobTitle,
                ContacEmail = organization.Contacts?.First().Email,
                ContactPhone = organization.Contacts?.First().Phone
            };
        }

        organizationViewModel.DistrictEducationOrganizations = await _educationOrganizationHelper.GetDistrictsOrganizationsSelectList(Id);
        organizationViewModel.RegionEducationOrganizations = await _educationOrganizationHelper.GetRegionOrganizationsSelectList(Id);

        return View(organizationViewModel);
    }

    [ValidateAntiForgeryToken]
    [HttpPatch]
    public async Task<IActionResult> Update(CreateEducationOrganizationRequestViewModel data)
    {
        if (!data.EducationOrganizationId.HasValue) { throw new ArgumentNullException("EducationOrganizationId required."); }
        
        // Get existing organization
       Expression<Func<Core.EducationOrganization, bool>> focusOrganizationExpression = request =>
            request.Id == data.EducationOrganizationId;

        var specification = new SearchableWithPaginationSpecification<Core.EducationOrganization>.Builder(1, -1)
            .WithSearchExpression(focusOrganizationExpression)
            .Build();

        var organizations = await _educationOrganizationRepository.ListAsync(specification,CancellationToken.None);

        var organization = organizations.FirstOrDefault();

        if (organization is null) { throw new ArgumentException("Not a valid organization."); }

        if (!ModelState.IsValid) { TempData[VoiceTone.Critical] = "Organization not updated."; return View("Edit"); }

        // Prepare organization object
        organization.Name = data.Name;
        organization.ShortName = data.ShortName;
        organization.ParentOrganizationId = 
                data.EducationOrganizationType == EducationOrganizationType.School ? 
                data.DistrictParentOrganizationId : data.RegionParentOrganizationId;
        
        organization.Number = data.Number;
        organization.CeebCode = data.CeebCode;
        organization.NcesCode = data.NcesCode;
        organization.StateCode = data.StateCode;
        organization.Address = new Core.Address()
        {
            StreetNumberName = data.StreetNumberName,
            City = data.City,
            PostalCode = data.PostalCode,
            StateAbbreviation = data.StateAbbreviation
        };
        organization.Domain = data.Domain;
        organization.TimeZone = data.TimeZone!;

        organization.Contacts = new List<Core.EducationOrganizationContact>()
            {
                new Core.EducationOrganizationContact {
                    Name = data.ContactName,
                    Email = data.ContacEmail,
                    Phone = data.ContactPhone,
                    JobTitle = data.ContactJobTitle
                }
            };

        await _educationOrganizationRepository.UpdateAsync(organization);

        TempData[VoiceTone.Positive] = $"Updated organization {organization.Name} ({organization.Id}).";

        return RedirectToAction(nameof(Index));
    }

    [ValidateAntiForgeryToken]
    [HttpDelete]
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id is null) { throw new ArgumentNullException("Missing id for organization to delete."); }
        
        var organization = await _educationOrganizationRepository.GetByIdAsync(id.Value);

        if (organization is null) { throw new ArgumentException("Not a valid organization."); }

        await _educationOrganizationRepository.DeleteAsync(organization);

        TempData[VoiceTone.Positive] = $"Deleted organization {organization.Name} ({organization.Id}).";

        return RedirectToAction(nameof(Index));
    }
}
