namespace EdNexusData.Broker.Core.Specifications.Tests.Unit;

public class OrganizationByDomainSpecTests
{
    private readonly List<EducationOrganization> sampleEducationOrganizations;

    public OrganizationByDomainSpecTests()
    {
        sampleEducationOrganizations = new List<EducationOrganization>
        {
            new EducationOrganization
            {
                Id = Guid.NewGuid(),
                Name = "Entity1",
                Domains = new List<EducationOrganizationDomain>
                {
                    new EducationOrganizationDomain { Domain = "entity1.org" },
                    new EducationOrganizationDomain { Domain = "entity1-alt.org" }
                }
            },
            new EducationOrganization
            {
                Id = Guid.NewGuid(),
                Name = "Entity2",
                Domains = new List<EducationOrganizationDomain>
                {
                    new EducationOrganizationDomain { Domain = "entity2.org" }
                }
            }
        };
    }

    [Fact]
    public void OrganizationByDomainSpec_MatchesOnPrimaryDomain()
    {
        var result = new OrganizationByDomainSpec("entity1.org").Evaluate(sampleEducationOrganizations);

        Assert.Single(result);
        Assert.Equal("Entity1", result.First().Name);
    }

    [Fact]
    public void OrganizationByDomainSpec_MatchesOnSecondaryDomain()
    {
        var result = new OrganizationByDomainSpec("entity1-alt.org").Evaluate(sampleEducationOrganizations);

        Assert.Single(result);
        Assert.Equal("Entity1", result.First().Name);
    }

    [Fact]
    public void OrganizationByDomainSpec_NoMatchForUnknownDomain()
    {
        var result = new OrganizationByDomainSpec("unknown.org").Evaluate(sampleEducationOrganizations);

        Assert.Empty(result);
    }
}
