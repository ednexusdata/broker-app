using EdNexusData.Broker.Domain;
using Ardalis.GuardClauses;
using EdNexusData.Broker.Common.EducationOrganizations;

namespace EdNexusData.Broker.Service.Resolvers;

public class DistrictEducationOrganizationResolver
{
    private readonly IRepository<Domain.EducationOrganization> _edOrgRepo;
    private readonly IServiceProvider _serviceProvider;
    
    public DistrictEducationOrganizationResolver(IRepository<Domain.EducationOrganization> edOrgRepo, IServiceProvider serviceProvider)
    {
        _edOrgRepo = edOrgRepo;
        _serviceProvider = serviceProvider;
    }

    public Domain.EducationOrganization Resolve(Domain.EducationOrganization educationOrganization)
    {
        if (educationOrganization.EducationOrganizationType == EducationOrganizationType.District)
            return educationOrganization;
        else
        {
            Guard.Against.Null(educationOrganization.ParentOrganization, "Unable to resolve education organization to parent district.");

            return educationOrganization.ParentOrganization;
        }
    }

    public async Task<string> Resolve(string educationOrganizationId)
    {
        Guard.Against.Null(educationOrganizationId, "Missing education organization id.");

        var educationOrganization = await _edOrgRepo.GetByIdAsync(educationOrganizationId);

        Guard.Against.Null(educationOrganization);

        return Resolve(educationOrganization).Id.ToString();
    }

    public async Task<Guid> Resolve(Guid educationOrganizationId)
    {
        Guard.Against.Null(educationOrganizationId, "Missing education organization id.");

        var educationOrganization = await _edOrgRepo.GetByIdAsync(educationOrganizationId);

        Guard.Against.Null(educationOrganization);

        return Resolve(educationOrganization).Id;
    }
}