using EdNexusData.Broker.Core.Services;
using Environment = EdNexusData.Broker.Core.Environment;

namespace EdNexusData.Broker.Web;

public class WebEnvironment : Environment
{
    public WebEnvironment(
        IHostEnvironment hostEnvironment,
        IServiceScopeFactory serviceScopeFactory) : base(serviceScopeFactory)
    {
        ApplicationName = ApplicationName.EdNexusDataBrokerWeb;
        EnvironmentName = hostEnvironment.EnvironmentName;
    }
}