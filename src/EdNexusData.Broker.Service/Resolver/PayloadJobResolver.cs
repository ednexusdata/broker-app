using EdNexusData.Broker.Connector.Payload;
using EdNexusData.Broker.Domain;
using EdNexusData.Broker.Domain.Specifications;
using EdNexusData.Broker.SharedKernel;
using EdNexusData.Broker.Connector.Resolvers;
using Ardalis.GuardClauses;
using EdNexusData.Broker.Connector.PayloadContents;
using EdNexusData.Broker.Connector.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EdNexusData.Broker.Service.Resolvers;

public class PayloadJobResolver //: IPayloadResolver
{
    private readonly IRepository<EducationOrganizationPayloadSettings> _edOrgPayloadSettings;
    private readonly FocusEducationOrganizationResolver _focusEdOrg;
    private readonly DistrictEducationOrganizationResolver _districtEdOrg;
    private readonly IServiceProvider _serviceProvider;

    public PayloadJobResolver(
        IRepository<EducationOrganizationPayloadSettings> edOrgPayloadSettings, 
        FocusEducationOrganizationResolver focusEdOrg, 
        DistrictEducationOrganizationResolver districtEdOrg,
        IServiceProvider serviceProvider
    )
    {
        _edOrgPayloadSettings = edOrgPayloadSettings;
        _focusEdOrg = focusEdOrg;
        _districtEdOrg = districtEdOrg;
        _serviceProvider = serviceProvider;
    }

    public IPayloadContentJob Resolve(string payloadContentType)
    {
        Guard.Against.Null(payloadContentType);

        var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetExportedTypes())
                .Where(p => p.FullName == payloadContentType);

        // Locate the payload content service in connector
        var resolvedPayloadContentType = types.FirstOrDefault();

        Guard.Against.Null(resolvedPayloadContentType, "", "Could not get payload content type");

        // Locate the attribute to determien the job to run
        var jobPayloadContentType = ((JobAttribute)resolvedPayloadContentType.GetCustomAttributes(false).Where(x => x.GetType() == typeof(JobAttribute)).FirstOrDefault()!).JobType;
  
        var payloadContentJob = ActivatorUtilities.CreateInstance(_serviceProvider, jobPayloadContentType);
        
        return (IPayloadContentJob)payloadContentJob;
    }
}