// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com
namespace EdNexusData.Broker.Core;

public class RequestAddress
{
    public District? District { get; set; }
    public School? School { get; set; }
    public EducationOrganizationContact? Sender { get; set; }

    public string? BrokerAddress { get; set; }

    public Common.Requests.RequestAddress ToCommon()
    {
        return new Common.Requests.RequestAddress()
        {
            District = District?.ToCommon(),
            School = School?.ToCommon(),
            Sender = Sender?.ToCommon(),
            BrokerAddress = BrokerAddress
        };
    }
}