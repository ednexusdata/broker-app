using EdNexusData.Broker.Core.Specifications;
using EdNexusData.Broker.Common.EducationOrganizations;

namespace EdNexusData.Broker.Core.Tests.Unit;

public class OrganizationByTypeSpecTests
{
    private readonly List<EducationOrganization> sampleEducationOrganizations;
    
    public OrganizationByTypeSpecTests()
    { 
        sampleEducationOrganizations = new List<EducationOrganization>
        {
            new EducationOrganization { Id = Guid.NewGuid(), Name = "Entity1", EducationOrganizationType = EducationOrganizationType.District },
            new EducationOrganization { Id = Guid.NewGuid(), Name = "Entity2", EducationOrganizationType = EducationOrganizationType.District },
            new EducationOrganization { Id = Guid.NewGuid(), Name = "Entity3", EducationOrganizationType = EducationOrganizationType.School }
        };
    }
    
    [Fact]
    public void OrganizationByTypeSpec_EducationOrganizations_DistrictsOnly()
    {
        var result = new OrganizationByTypeSpec(EducationOrganizationType.District).Evaluate(sampleEducationOrganizations);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Equal("Entity1", result.First().Name);
    }
}