using EdNexusData.Broker.Core.StudentLookup;
using EdNexusData.Broker.Domain;
using EdNexusData.Broker.Core.Payloads;
using EdNexusData.Broker.Service.Resolvers;
using EdNexusData.Broker.Core;

namespace EdNexusData.Broker.Service.Lookup;

public class StudentLookupService
{
    private readonly PayloadResolver _payloadResolver;
    private readonly StudentLookupResolver _studentLookupResolver;
    private readonly FocusEducationOrganizationResolver _focusEducationOrganizationResolver;
    private readonly ConnectorLoader _connectorLoader;
    
    public StudentLookupService(ConnectorLoader connectorLoader, 
        PayloadResolver payloadResolver, 
        StudentLookupResolver studentLookupResolver,
        FocusEducationOrganizationResolver focusEducationOrganizationResolver)
    {
        _connectorLoader = connectorLoader;
        _payloadResolver = payloadResolver;
        _studentLookupResolver = studentLookupResolver;
        _focusEducationOrganizationResolver = focusEducationOrganizationResolver;
    }

    public async Task<List<StudentLookupResult>?> SearchAsync(PayloadDirection payloadDirection, string searchParameter)
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

        var connectorStudentLookupService = _studentLookupResolver.Resolve(typeConnectorToUse);

        // Prepare parameters
        var searchStudent = new Student()
        {
            StudentNumber = searchParameter,
            FirstName = searchParameter,
            LastName = searchParameter,
            MiddleName = searchParameter
        };

        // Pass search parameters to connector for processing
        var results = await connectorStudentLookupService.SearchAsync(searchStudent.ToContract());

        return results;
    }
}
