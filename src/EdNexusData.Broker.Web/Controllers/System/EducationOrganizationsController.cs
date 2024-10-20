using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EdNexusData.Broker.SharedKernel;
using EdNexusData.Broker.Domain;
using EdNexusData.Broker.Web.Helpers;
using EdNexusData.Broker.Web.Constants.DesignSystems;
using EdNexusData.Broker.Web.Models.OutgoingRequests;
using EdNexusData.Broker.Web.Models.Paginations;
using Ardalis.Specification;
using EdNexusData.Broker.Web.ViewModels.EducationOrganizations;
using System.Linq.Expressions;
using EdNexusData.Broker.Web.Extensions.States;
using EdNexusData.Broker.Web.Specifications;

namespace EdNexusData.Broker.Web.Controllers;

[Authorize(Policy = "SuperAdmin")]
public class EducationOrganizationsController : AuthenticatedController<EducationOrganizationsController>
{
    private readonly IRepository<EducationOrganization> _educationOrganizationRepository;
    private readonly EducationOrganizationHelper _educationOrganizationHelper;
    public EducationOrganizationsController(
        IHttpContextAccessor httpContextAccessor,
        IRepository<EducationOrganization> educationOrganizationRepository,
        EducationOrganizationHelper educationOrganizationHelper)
    {
        _educationOrganizationRepository = educationOrganizationRepository;
        _educationOrganizationHelper = educationOrganizationHelper;
    }

    public async Task<IActionResult> Index(
      EducationOrganizationRequestModel model,
      CancellationToken cancellationToken)
    {
        RefreshSession();

        var searchExpressions = model.BuildSearchExpressions();

        var sortExpressions = model.BuildSortExpressions();

        var specificationPre = new SearchableWithPaginationSpecification<EducationOrganization>.Builder(model.Page, model.Size)
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

        var sortExpressions = model.BuildSortExpressions();

        var specification = new SearchableWithPaginationSpecification<EducationOrganization>.Builder(model.Page, model.Size)
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
        var educationOrganizations = await _educationOrganizationHelper.GetDistrictsOrganizationsSelectList();
        var viewModel = new CreateEducationOrganizationRequestViewModel
        {
            EducationOrganizations = educationOrganizations,
            States = States.GetSelectList()
        };

        return View(viewModel);
    }

    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> Create(CreateEducationOrganizationRequestViewModel data)
    {
        if (!ModelState.IsValid) { TempData[VoiceTone.Critical] = "Organization not created."; return RedirectToAction(nameof(Create)); }
        
        var organization = new EducationOrganization()
        {
            Id = Guid.NewGuid(),
            ParentOrganizationId = data.EducationOrganizationType == EducationOrganizationType.School ? data.ParentOrganizationId : null,
            Name = data.Name,
            Number = data.Number,
            EducationOrganizationType = data.EducationOrganizationType,
            Address = new Address()
            {
                StreetNumberName = data.StreetNumberName,
                City = data.City,
                PostalCode = data.PostalCode,
                StateAbbreviation = data.StateAbbreviation
            },
            Domain = data.Domain,
            Contacts = new List<EducationOrganizationContact>()
            {
                new EducationOrganizationContact {
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
       Expression<Func<EducationOrganization, bool>> focusOrganizationExpression = request =>
            request.Id == Id;

        var specification = new SearchableWithPaginationSpecification<EducationOrganization>.Builder(1, -1)
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
                Name = organization.Name!,
                EducationOrganizationType = organization.EducationOrganizationType,
                Number = organization.Number!,
                StreetNumberName = organization.Address?.StreetNumberName!,
                City = organization.Address?.City,
                StateAbbreviation = organization.Address?.StateAbbreviation,
                PostalCode = organization.Address?.PostalCode,
                States = States.GetSelectList(),
                Domain = organization.Domain,
                ContactName = organization.Contacts?.First().Name,
                ContactJobTitle = organization.Contacts?.First().JobTitle,
                ContacEmail = organization.Contacts?.First().Email,
                ContactPhone = organization.Contacts?.First().Phone
            };
        }

        organizationViewModel.EducationOrganizations = await _educationOrganizationHelper.GetDistrictsOrganizationsSelectList(Id);

        return View(organizationViewModel);
    }

    [ValidateAntiForgeryToken]
    [HttpPatch]
    public async Task<IActionResult> Update(CreateEducationOrganizationRequestViewModel data)
    {
        if (!data.EducationOrganizationId.HasValue) { throw new ArgumentNullException("EducationOrganizationId required."); }
        
        // Get existing organization
       Expression<Func<EducationOrganization, bool>> focusOrganizationExpression = request =>
            request.Id == data.EducationOrganizationId;

        var specification = new SearchableWithPaginationSpecification<EducationOrganization>.Builder(1, -1)
            .WithSearchExpression(focusOrganizationExpression)
            .Build();

        var organizations = await _educationOrganizationRepository.ListAsync(specification,CancellationToken.None);

        var organization = organizations.FirstOrDefault();

        if (organization is null) { throw new ArgumentException("Not a valid organization."); }

        if (!ModelState.IsValid) { TempData[VoiceTone.Critical] = "Organization not updated."; return View("Edit"); }

        // Prepare organization object
        organization.Name = data.Name;
        if (data.EducationOrganizationType == EducationOrganizationType.School)
        {
            organization.ParentOrganizationId = data.ParentOrganizationId;
        }
        else
        {
            organization.ParentOrganizationId = null;
        }
        organization.Number = data.Number;
        organization.EducationOrganizationType = data.EducationOrganizationType;
        organization.Address = new Address()
        {
            StreetNumberName = data.StreetNumberName,
            City = data.City,
            PostalCode = data.PostalCode,
            StateAbbreviation = data.StateAbbreviation
        };
        organization.Domain = data.Domain;

        organization.Contacts = new List<EducationOrganizationContact>()
            {
                new EducationOrganizationContact {
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
