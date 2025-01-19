using Environment = EdNexusData.Broker.Core.Environment;

namespace EdNexusData.Broker.Web;

public class WebEnvironment : Environment
{
    private readonly IHostEnvironment hostEnvironment;

    public WebEnvironment(IHostEnvironment hostEnvironment)
    {
        this.hostEnvironment = hostEnvironment;

        EnvironmentName = hostEnvironment.EnvironmentName;
    }
}