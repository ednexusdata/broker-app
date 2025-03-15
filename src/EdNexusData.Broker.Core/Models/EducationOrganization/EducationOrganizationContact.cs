// Copyright: 2024 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

namespace EdNexusData.Broker.Core;

public class EducationOrganizationContact
{
    public string? Name { get; set; }
    public string? JobTitle { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }

    public Common.EducationOrganizations.EducationOrganizationContact ToCommon()
    {
        return new Common.EducationOrganizations.EducationOrganizationContact()
        {
            Name = Name,
            JobTitle = JobTitle,
            Phone = Phone,
            Email = Email
        };
    }
}