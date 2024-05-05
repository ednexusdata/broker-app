using EdNexusData.Broker.Connector.StudentLookup;
using EdNexusData.Broker.Connector.Resolvers;
using EdNexusData.Broker.Domain;
using EdNexusData.Broker.Connector.Payloads;
using EdNexusData.Broker.Service.Resolvers;
using EdNexusData.Broker.Connector;
using Ardalis.GuardClauses;
using EdNexusData.Broker.Connector.Student;

namespace EdNexusData.Broker.Service.Lookup;

public class StudentService
{
    private readonly IPayloadResolver _payloadResolver;
    private readonly StudentResolver _studentResolver;
    private readonly FocusEducationOrganizationResolver _focusEducationOrganizationResolver;
    private readonly ConnectorLoader _connectorLoader;
    
    public StudentService(ConnectorLoader connectorLoader, 
                    IPayloadResolver payloadResolver, 
                    StudentResolver studentResolver, 
                    FocusEducationOrganizationResolver focusEducationOrganizationResolver)
    {
        _connectorLoader = connectorLoader;
        _payloadResolver = payloadResolver;
        _studentResolver = studentResolver;
        _focusEducationOrganizationResolver = focusEducationOrganizationResolver;
    }

    public async Task<IStudent?> FetchAsync(PayloadDirection payloadDirection, Student studentToFetch)
    {
        string studentLookupConnector = default!;

        if (payloadDirection == PayloadDirection.Incoming)
        {
            var payloadSettings = await _payloadResolver.FetchIncomingPayloadSettingsAsync<StudentCumulativeRecord>((await _focusEducationOrganizationResolver.Resolve()).Id);

            if (payloadSettings.StudentInformationSystem is null)
            {
                throw new ArgumentNullException("Student Information System missing on incoming payload settings.");
            }

            studentLookupConnector = payloadSettings.StudentInformationSystem;
        }

        if (payloadDirection == PayloadDirection.Outgoing)
        {
            var payloadSettings = await _payloadResolver.FetchOutgoingPayloadSettingsAsync<StudentCumulativeRecord>((await _focusEducationOrganizationResolver.Resolve()).Id);

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

        return await connectorStudentService.FetchAsync(studentToFetch);
    }
}
