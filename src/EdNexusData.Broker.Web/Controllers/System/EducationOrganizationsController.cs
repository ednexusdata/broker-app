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
using EdNexusData.Broker.Core.Specifications;
using EdNexusData.Broker.Common.EducationOrganizations;
using EdNexusData.Broker.Web.Constants.Claims;

namespace EdNexusData.Broker.Web.Controllers;

[Authorize(Policy = CustomClaimType.SystemAdministrator)]
public class EducationOrganizationsController : AuthenticatedController<EducationOrganizationsController>
{
    private readonly IRepository<Core.EducationOrganization> _educationOrganizationRepository;
    private readonly IReadRepository<Core.EducationOrganizationDomain> _educationOrganizationDomainRepository;
    private readonly EducationOrganizationHelper _educationOrganizationHelper;
    private readonly FocusHelper focusHelper;
    private readonly ICurrentUser _currentUser;

    public EducationOrganizationsController(
        IRepository<Core.EducationOrganization> educationOrganizationRepository,
        IReadRepository<Core.EducationOrganizationDomain> educationOrganizationDomainRepository,
        EducationOrganizationHelper educationOrganizationHelper,
        FocusHelper focusHelper,
        ICurrentUser currentUser)
    {
        _educationOrganizationRepository = educationOrganizationRepository;
        _educationOrganizationDomainRepository = educationOrganizationDomainRepository;
        _educationOrganizationHelper = educationOrganizationHelper;
        this.focusHelper = focusHelper;
        _currentUser = currentUser;
    }

    private static List<string> ParseDomains(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) { return new List<string>(); }

        return input
            .Split(new[] { '\r', '\n', ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(d => d.Trim())
            .Where(d => d.Length > 0)
            .Distinct()
            .ToList();
    }

    // EfRepository only stamps CreatedAt/CreatedBy on the EducationOrganization passed to
    // AddAsync/UpdateAsync, not on child Domains rows reachable through the navigation
    // property, so those have to be set explicitly here.
    private List<Core.EducationOrganizationDomain> BuildDomains(List<string> domains)
    {
        var now = DateTimeOffset.UtcNow;
        var userId = _currentUser.AuthenticatedUserId();

        return domains.Select(d => new Core.EducationOrganizationDomain
        {
            Domain = d,
            CreatedAt = now,
            CreatedBy = userId
        }).ToList();
    }

    // Diffs against the already-tracked Domains collection instead of replacing it wholesale,
    // so domains that are unchanged keep their row (Id, CreatedAt, CreatedBy) rather than being
    // deleted and re-inserted on every save.
    private void ReconcileDomains(Core.EducationOrganization organization, List<string> domains)
    {
        var existingDomains = organization.Domains ??= new List<Core.EducationOrganizationDomain>();

        var domainsToRemove = existingDomains.Where(d => !domains.Contains(d.Domain)).ToList();
        foreach (var domain in domainsToRemove)
        {
            existingDomains.Remove(domain);
        }

        var domainsToAdd = domains.Where(d => !existingDomains.Any(existing => existing.Domain == d)).ToList();
        foreach (var domain in BuildDomains(domainsToAdd))
        {
            existingDomains.Add(domain);
        }
    }

    public async Task<IActionResult> Index(
      EducationOrganizationRequestModel model,
      CancellationToken cancellationToken)
    {
        RefreshSession();

        var searchExpressions = model.BuildSearchExpressions();

        var focusedEdOrgs = await focusHelper.GetFocusedEdOrgs(true);
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

        var domains = ParseDomains(data.DomainsInput);

        if (domains.Count > 0)
        {
            var conflictingDomains = await _educationOrganizationDomainRepository.ListAsync(new EducationOrganizationDomainsByValuesSpec(domains));

            if (conflictingDomains.Count > 0)
            {
                TempData[VoiceTone.Critical] = $"Organization not created. Domain(s) already in use: {string.Join(", ", conflictingDomains.Select(d => d.Domain))}.";
                return RedirectToAction(nameof(Create));
            }
        }

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
            Domains = BuildDomains(domains),
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
            .WithIncludeEntities(builder => builder
                .Include(educationOrganization => educationOrganization.Domains))
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
                DomainsInput = string.Join('\n', organization.Domains?.Select(d => d.Domain) ?? Enumerable.Empty<string>()),
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
            .WithIncludeEntities(builder => builder
                .Include(educationOrganization => educationOrganization.Domains))
            .Build();

        var organizations = await _educationOrganizationRepository.ListAsync(specification,CancellationToken.None);

        var organization = organizations.FirstOrDefault();

        if (organization is null) { throw new ArgumentException("Not a valid organization."); }

        if (!ModelState.IsValid) { TempData[VoiceTone.Critical] = "Organization not updated."; return View("Edit"); }

        var domains = ParseDomains(data.DomainsInput);

        if (domains.Count > 0)
        {
            var conflictingDomains = (await _educationOrganizationDomainRepository.ListAsync(new EducationOrganizationDomainsByValuesSpec(domains)))
                .Where(d => d.EducationOrganizationId != organization.Id)
                .ToList();

            if (conflictingDomains.Count > 0)
            {
                TempData[VoiceTone.Critical] = $"Organization not updated. Domain(s) already in use: {string.Join(", ", conflictingDomains.Select(d => d.Domain))}.";
                return View("Edit");
            }
        }

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
        ReconcileDomains(organization, domains);
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
