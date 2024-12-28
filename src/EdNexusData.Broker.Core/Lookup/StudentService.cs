using EdNexusData.Broker.Core.Resolvers;
using EdNexusData.Broker.Common.Students;
using EdNexusData.Broker.Common.Payloads;

namespace EdNexusData.Broker.Core.Lookup;

public class StudentService
{
    private readonly PayloadResolver _payloadResolver;
    private readonly StudentResolver _studentResolver;
    private readonly FocusEducationOrganizationResolver _focusEducationOrganizationResolver;
    private readonly ConnectorLoader _connectorLoader;
    
    public StudentService(ConnectorLoader connectorLoader, 
                    PayloadResolver payloadResolver, 
                    StudentResolver studentResolver, 
                    FocusEducationOrganizationResolver focusEducationOrganizationResolver)
    {
        _connectorLoader = connectorLoader;
        _payloadResolver = payloadResolver;
        _studentResolver = studentResolver;
        _focusEducationOrganizationResolver = focusEducationOrganizationResolver;
    }

    public async Task<IStudent?> FetchAsync(PayloadDirection payloadDirection, Core.Student studentToFetch)
    {
        string studentLookupConnector = default!;

        if (payloadDirection == PayloadDirection.Incoming)
        {
            var payloadSettings = await _payloadResolver.FetchIncomingPayloadSettingsAsync<StudentCumulativeRecordPayload>((await _focusEducationOrganizationResolver.Resolve()).Id);

            if (payloadSettings.StudentInformationSystem is null)
            {
                throw new ArgumentNullException("Student Information System missing on incoming payload settings.");
            }

            studentLookupConnector = payloadSettings.StudentInformationSystem;
        }

        if (payloadDirection == PayloadDirection.Outgoing)
        {
            var payloadSettings = await _payloadResolver.FetchOutgoingPayloadSettingsAsync<StudentCumulativeRecordPayload>((await _focusEducationOrganizationResolver.Resolve()).Id);

            if (payloadSettings.StudentLookupConnector is null)
            {
                throw new ArgumentNullException("Student Lookup Connector missing on outgoing payload settings.");
            }
            studentLookupConnector = payloadSettings.StudentLookupConnector;
        }

        if (studentLookupConnector == default)
        {
            throw new ArgumentNullException("Unable to find connector to use for student lookup.");
        }

        Type typeConnectorToUse = _connectorLoader.GetConnector(studentLookupConnector)!;

        var connectorStudentService = _studentResolver.Resolve(typeConnectorToUse);

        return await connectorStudentService.FetchAsync(studentToFetch.ToCommon());
    }
}
