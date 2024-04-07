using Microsoft.AspNetCore.Http;
using EdNexusData.Broker.Domain;
using EdNexusData.Broker.Domain.Specifications;
using EdNexusData.Broker.SharedKernel;
using Ardalis.GuardClauses;

namespace EdNexusData.Broker.Service.Resolvers;

public class FocusEducationOrganizationResolver
{
    private readonly ISession? _session;
    private readonly IServiceProvider _serviceProvider;
    private readonly IRepository<EducationOrganization> _edOrgRepo;

    public Guid? EducationOrganizationId { get; set; }
    
    public FocusEducationOrganizationResolver(IHttpContextAccessor httpContext, IRepository<EducationOrganization> edOrgRepo,  IServiceProvider serviceProvider)
    {
        _session = httpContext!.HttpContext!.Session;
        _edOrgRepo = edOrgRepo;
        _serviceProvider = serviceProvider;
    }

    public FocusEducationOrganizationResolver(IRepository<EducationOrganization> edOrgRepo,  IServiceProvider serviceProvider)
    {
        _session = null;
        _edOrgRepo = edOrgRepo;
        _serviceProvider = serviceProvider;
    }

    public async Task<EducationOrganization> Resolve(Guid? educationOrganizationId = null)
    {
        if (educationOrganizationId is null && _session is not null)
        {
            educationOrganizationId = Guid.Parse(_session.GetString("Focus.EducationOrganization.Key")!);    
        } else if (EducationOrganizationId is not null)
        {
            educationOrganizationId = EducationOrganizationId;
        }
        
        Guard.Against.Null(educationOrganizationId, "Missing Education Organization Id");

        var educationOrganization = await _edOrgRepo.FirstOrDefaultAsync(new OrganizationByIdWithParentSpec(educationOrganizationId.Value));

        Guard.Against.Null(educationOrganization, "Unable to find Education Organization Id");

        return educationOrganization;
    }
}