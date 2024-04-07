using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EdNexusData.Broker.Domain;
using EdNexusData.Broker.Domain.Specifications;
using EdNexusData.Broker.SharedKernel;
using EdNexusData.Broker.Web;

namespace EdNexusData.Broker.Controllers.Api;

[AllowAnonymous]
[ApiController]
[Route("api/v1/directory")]
public class DirectoryController : Controller
{
    private readonly IReadRepository<EducationOrganization> _educationOrganizations;

    public DirectoryController(IReadRepository<EducationOrganization> educationOrganizations)
    {
        _educationOrganizations = educationOrganizations;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return Ok();
    }

    [HttpGet]
    [Route("search")]
    public async Task<IActionResult> Search([FromQuery] string domain)
    {
        try
        {
            var districtEdOrg = await _educationOrganizations.FirstOrDefaultAsync(new OrganizationByDomainSpec(domain));

            if (districtEdOrg is null)
            {
                throw new NoDistrictForDomainException();
            }
            
            var district = new District()
            {
                Id = districtEdOrg.Id,
                Name = districtEdOrg.Name,
                Number = districtEdOrg.Number,
                Domain = districtEdOrg.Domain,
                Address = (districtEdOrg.Address != null) ? districtEdOrg.Address : new Address(),
                Schools = new List<School>()
            };

            if (districtEdOrg.EducationOrganizations?.Count > 0)
            {
                foreach(var schoolEdOrg in districtEdOrg.EducationOrganizations)
                {
                    district.Schools.Add(new School()
                    {
                        Id = schoolEdOrg.Id,
                        Name = schoolEdOrg.Name,
                        Number = schoolEdOrg.Number,
                        Address = (schoolEdOrg.Address != null) ? schoolEdOrg.Address : new Address(),
                        Contacts = (schoolEdOrg.Contacts != null) ? schoolEdOrg.Contacts : new List<EducationOrganizationContact>()
                    });
                }
            }

            // var jsonResults = JsonSerializer.Serialize(results, new JsonSerializerOptions()
            // {
            //     ReferenceHandler = ReferenceHandler.IgnoreCycles,
            //     WriteIndented = true
            // });

            return Ok(district);
        }
        catch (NoDistrictForDomainException)
        {
            var message = $"No district found for domain: {domain}";
            return NotFound(message.ToJsonDocument());
        }
        catch(Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, $"{ex.Message}\n\n{ex.StackTrace}");
        }
    }

}

public class NoDistrictForDomainException : Exception {}