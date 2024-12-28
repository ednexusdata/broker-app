using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EdNexusData.Broker.Core;

namespace EdNexusData.Broker.Data.IntegrationTests.Fixtures;

public static class BrokerDbFixture 
{
    public static ServiceProvider? Services;

    public static async Task SeedDbContext()
    {
        await SeedUser();
        await SeedEducationOrganizations();
    }

    private static async Task SeedUser()
    {
        if (Services is null) { return; }
        
        // Get Needed Services
        var userManagerService = Services.GetRequiredService<UserManager<IdentityUser<Guid>>>();
        var userRepository = Services.GetRequiredService<IRepository<User>>();
        
        // Create User through user manager
        var identityUser = new IdentityUser<Guid> { UserName = "test@email.com", Email = "test@email.com" }; 
        await userManagerService.CreateAsync(identityUser);

        // Create user
        var user = new User()
        {
            Id = identityUser.Id,
            FirstName = "Test",
            LastName = "User",
            IsSuperAdmin = true,
            AllEducationOrganizations = PermissionType.None
        };

        await userRepository.AddAsync(user);
    }

    private static async Task SeedEducationOrganizations()
    {
        if (Services is null) { return; }

        // Get needed services
        var educationOrganizationRepository = Services.GetRequiredService<IRepository<EducationOrganization>>();

        var districtGuid = Guid.NewGuid();

        // Create new district
        var district = new EducationOrganization()
        {
            Id = districtGuid,
            Name = "Oregon School District",
            Number = "1000",
            EducationOrganizationType = EducationOrganizationType.District
        };

        await educationOrganizationRepository.AddAsync(district);

        // Create new school
        var school = new EducationOrganization()
        {
            Id = Guid.NewGuid(),
            ParentOrganizationId = districtGuid,
            Name = "Yamhill High School",
            Number = "1001",
            EducationOrganizationType = EducationOrganizationType.School
        };

        await educationOrganizationRepository.AddAsync(school);
    }
}