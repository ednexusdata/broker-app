using Microsoft.AspNetCore.Mvc.Rendering;
using EdNexusData.Broker.Domain;
using EdNexusData.Broker.Domain.Specifications;
using EdNexusData.Broker.SharedKernel;

namespace EdNexusData.Broker.Web.Helpers;

public class EducationOrganizationHelper
{
    private IRepository<EducationOrganization> _repo { get; set; }
    public EducationOrganizationHelper(IRepository<EducationOrganization> repo)
    {
        _repo = repo;
    }
    
    public async Task<IEnumerable<SelectListItem>> GetDistrictsOrganizationsSelectList(Guid? focusedOrganizationId = null)
    {
        var selectListItems = new List<SelectListItem>();

        var organizations = await _repo.ListAsync(new OrganizationByTypeSpec(EducationOrganizationType.District));
        organizations = organizations.OrderBy(x => x.Name).ToList();

        foreach(var organization in organizations)
        {
            if (focusedOrganizationId is null || (focusedOrganizationId is not null && organization.Id != focusedOrganizationId))
            {
                selectListItems.Add(new SelectListItem() {
                    Text = organization.Name,
                    Value = organization.Id.ToString()
                });
            }
        }

        return selectListItems;
    }

    public async Task<IEnumerable<SelectListItem>> GetOrganizationsSelectList(List<EducationOrganization>? edOrgsToRemove)
    {
        var selectListItems = new List<SelectListItem>();

        var organizations = await _repo.ListAsync();
        organizations = organizations.OrderBy(x => x.ParentOrganization?.Name).ThenBy(x => x.Name).ToList();

        foreach(var organization in organizations)
        {
            if (edOrgsToRemove is not null && !edOrgsToRemove.Contains(organization))
            {
                selectListItems.Add(new SelectListItem() {
                    Text = (organization.EducationOrganizationType == EducationOrganizationType.District) 
                        ? organization.Name 
                        : $"{organization.ParentOrganization?.Name} / {organization.Name}",
                    Value = organization.Id.ToString()
                });
            }
        }
        
        selectListItems = selectListItems.OrderBy(x => x.Text).ToList();

        return selectListItems;
    }
}