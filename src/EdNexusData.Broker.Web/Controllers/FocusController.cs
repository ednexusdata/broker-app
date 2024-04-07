// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using Microsoft.AspNetCore.Mvc;
using EdNexusData.Broker.Web.Models;
using Microsoft.AspNetCore.Authorization;
using EdNexusData.Broker.Web.Constants.DesignSystems;
using static EdNexusData.Broker.Web.Constants.Sessions.SessionKey;
using EdNexusData.Broker.SharedKernel;
using EdNexusData.Broker.Domain;
using System.Linq.Expressions;
using EdNexusData.Broker.Web.Specifications;
using Ardalis.Specification;
namespace EdNexusData.Broker.Web.Controllers;

[Authorize]
public class FocusController : AuthenticatedController<FocusController>
{
    private readonly IReadRepository<EducationOrganization> _educationOrganizationRepository;

    public FocusController(IReadRepository<EducationOrganization> educationOrganizationRepository)
    {
        _educationOrganizationRepository = educationOrganizationRepository;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetFocus(FocusViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.FocusEducationOrganizationId))
        {
            TempData[VoiceTone.Critical] = "Unable to set focus.";
            return Redirect(model.ReturnUrl);
        }

        HttpContext?.Session.SetString(FocusOrganizationKey, model.FocusEducationOrganizationId);

        if (Guid.TryParse(model.FocusEducationOrganizationId, out var educationOrganizationId))
        {
            Expression<Func<EducationOrganization, bool>> focusOrganizationExpression = request => request.Id == educationOrganizationId;

            var specification = new SearchableWithPaginationSpecification<EducationOrganization>.Builder(1, -1)
                .WithSearchExpression(focusOrganizationExpression)
                .WithIncludeEntities(builder => builder
                    .Include(educationOrganization => educationOrganization.ParentOrganization))
                .Build();

            var organizations = await _educationOrganizationRepository.ListAsync(specification, CancellationToken.None);
            var organization = organizations.FirstOrDefault();

            if (organization != null)
            {
                var parentOrganizationName = organization.ParentOrganization?.Name;
                var organizationName = organization.Name;

                if (!string.IsNullOrWhiteSpace(parentOrganizationName))
                    HttpContext?.Session.SetString(FocusOrganizationDistrict, parentOrganizationName);

                HttpContext?.Session.SetString(FocusOrganizationSchool, organizationName);
            }
        }

        return Redirect(model.ReturnUrl);
    }

}
