// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

namespace EdNexusData.Broker.Core;

public class User : BaseEntity, IAggregateRoot
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public bool IsSuperAdmin { get; set; } = false;
    public PermissionType AllEducationOrganizations { get; set; } = PermissionType.None;

    public string? TimeZone { get; set; }

    public List<UserRole>? UserRoles { get; set; }

    public string Name {
        get {
            return FirstLastName;
        }
    }

    public string FirstLastName { 
        get {
            return $"{FirstName} {LastName}";
        }
    }

    public string LastFirstName { 
        get {
            return $"{LastName}, {FirstName}";
        }
    }
}
