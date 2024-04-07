// using System.Net.Http.Headers;
// using EdNexusData.Broker.Domain;
// using EdNexusData.Broker.SharedKernel;
// using src.Models.Assessments;
// using src.Models.Courses;
// using src.Models.Grades;
// using src.Models.ProgramAssociations;
// using src.Models.SectionAssociations;
// using src.Models.Students;
// using src.Services.Shared;
// using src.Services.Tokens;

// namespace src.Services.Students;
// public class StudentService : IStudentService
// {
//     private readonly IHttpClientFactory _httpClientFactory;
//     private readonly IClientService _clientService;
//     private readonly ITokenService _tokenService;
//     private readonly IRepository<EducationOrganizationPayloadSettings> _educationOrganizationPayloadSettings;
//     private readonly IRepository<EducationOrganizationConnectorSettings> _educationOrganizationConnectorSettings;

//     public StudentService(
//         IHttpClientFactory httpClientFactory,
//         ITokenService tokenService,
//         IRepository<EducationOrganizationPayloadSettings> educationOrganizationPayloadSettings,
//         IRepository<EducationOrganizationConnectorSettings> educationOrganizationConnectorSettings,
//         IClientService clientService)
//     {
//         _httpClientFactory = httpClientFactory;
//         _tokenService = tokenService;
//         _educationOrganizationPayloadSettings = educationOrganizationPayloadSettings;
//         _educationOrganizationConnectorSettings = educationOrganizationConnectorSettings;
//         _clientService = clientService;
//     }

//     public async Task<IEnumerable<StudentResponse>> GetAllAsync(StudentRequest request)
//     {
//         try
//         {
//             var accessToken = await _tokenService.GetAccessTokenAsync();
//             var educationOrganizationPayloadSettings = await _educationOrganizationPayloadSettings.ListAsync();

//             var educationOrganizationPayloadSetting = educationOrganizationPayloadSettings.FirstOrDefault(
//                 educationOrganizationPayload => educationOrganizationPayload.Payload == "StudentCumulativeRecord"
//             );
//             var studentEndpoint = ""; //educationOrganizationPayloadSetting?.Settings?.RootElement.GetProperty("Students").GetString();

//             var studentUri = new Uri(studentEndpoint);
//             var baseAddress = studentUri.GetLeftPart(UriPartial.Authority);

//             using var httpClient = _httpClientFactory.CreateClient();
//             httpClient.BaseAddress = new Uri(baseAddress);
//             var requestUrl = $"{studentEndpoint}?{ToQueryString(request)}";
//             httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
//             var studentResponse = await httpClient.GetAsync(requestUrl);

//             return await studentResponse.Content.ReadFromJsonAsync<IEnumerable<StudentResponse>>()
//                 ?? Enumerable.Empty<StudentResponse>();
//         }
//         catch (Exception ex)
//         {
//             throw new Exception("Failed to obtain students.", ex);
//         }
//     }
//     /*
//     public async Task<StudentAggregateResponse> GetById(string id)
//     {
//         try
//         {
//             var accessToken = await _tokenService.GetAccessTokenAsync();
//             var educationOrganizationPayloadSettings = await _educationOrganizationPayloadSettings.ListAsync();
//             var educationOrganizationPayloadSetting = educationOrganizationPayloadSettings.FirstOrDefault(
//                 educationOrganizationPayload => educationOrganizationPayload.Payload == "StudentCumulativeRecord"
//             ) ?? throw new Exception("Education organization payload setting not found.");

//             var rootElement = ""; //educationOrganizationPayloadSetting.Settings?.RootElement ?? throw new ArgumentNullException();

//             var studentParamUrl = $"?studentUniqueId={id}";

//             var taskDictionary = new Dictionary<string, Task>();
//             var studentAggregateResponse = new StudentAggregateResponse();

//             var assessmentResponse = _clientService.GetApiResponseAsync<AssessmentResponse>(
//                 $"{rootElement.GetProperty("Assessments").GetString()!}{studentParamUrl}",
//                 accessToken);

//             var programAssociationResponse = _clientService.GetApiResponseAsync<ProgramAssociationResponse>(
//                 $"{rootElement.GetProperty("ProgramAssociations").GetString()!}{studentParamUrl}",
//                 accessToken);

//             var sectionAssociationResponse = _clientService.GetApiResponseAsync<SectionAssociationResponse>(
//                 $"{rootElement.GetProperty("SectionAssociations").GetString()!}{studentParamUrl}",
//                 accessToken);

//             var courseTranscriptResponse = _clientService.GetApiResponseAsync<CourseTranscriptResponse>(
//                 $"{rootElement.GetProperty("CourseTranscripts").GetString()!}{studentParamUrl}",
//                 accessToken);

//             var gradesResponse = _clientService.GetApiResponseAsync<GradeResponse>(
//                 $"{rootElement.GetProperty("Grades").GetString()!}{studentParamUrl}",
//                 accessToken);

//             await Task.WhenAll(
//                 assessmentResponse,
//                 programAssociationResponse,
//                 sectionAssociationResponse,
//                 courseTranscriptResponse,
//                 gradesResponse);

//             return new StudentAggregateResponse(){
//                 Assessments = assessmentResponse.Result,
//                 ProgramAssociations = programAssociationResponse.Result,
//                 SectionAssociations = sectionAssociationResponse.Result,
//                 CourseTranscripts = courseTranscriptResponse.Result,
//                 Grades = gradesResponse.Result
//             };
//         }
//         catch (Exception ex)
//         {
//             throw new Exception("Failed to obtain access token.", ex);
//         }
//     }
//     */

//     private static string ToQueryString(object requestParams)
//     {
//         var properties = from property in requestParams.GetType().GetProperties()
//                         where property.GetValue(requestParams, null) != null
//                         select $"{property.Name}={property.GetValue(requestParams, null)}";

//         return string.Join("&", properties.ToArray());
//     }
// }
