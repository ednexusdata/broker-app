using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EdNexusData.Broker.Domain;
using EdNexusData.Broker.SharedKernel;

namespace EdNexusData.Broker.Service.IntegrationTests.Fixtures;

public static class BrokerDbFixture 
{
    public static ServiceProvider? Services;

    public static async Task SeedDbContext()
    {
        await SeedUser();
        await SeedEducationOrganizations();
    }

    private static async Task SeedUser()
    {
        if (Services is null) { return; }
        
        // Get Needed Services
        var userManagerService = Services.GetRequiredService<UserManager<IdentityUser<Guid>>>();
        var userRepository = Services.GetRequiredService<IRepository<User>>();
        
        // Create User through user manager
        var identityUser = new IdentityUser<Guid> { UserName = "test@email.com", Email = "test@email.com" }; 
        await userManagerService.CreateAsync(identityUser);

        // Create user
        var user = new User()
        {
            Id = identityUser.Id,
            FirstName = "TestInt",
            LastName = "User",
            IsSuperAdmin = true,
            AllEducationOrganizations = PermissionType.None
        };

        await userRepository.AddAsync(user);
    }

    private static async Task SeedEducationOrganizations()
    {
        if (Services is null) { return; }

        // Get needed services
        var educationOrganizationRepository = Services.GetRequiredService<IRepository<EducationOrganization>>();
        var requestsRepository = Services.GetRequiredService<IRepository<Request>>();
        var messageRepository = Services.GetRequiredService<IRepository<Message>>();
        var payloadRepository = Services.GetRequiredService<IRepository<PayloadContent>>();

        var districtGuid = Guid.NewGuid();

        // Create new district
        var district = new EducationOrganization()
        {
            Id = districtGuid,
            Name = "Oregon School District",
            Number = "1000",
            EducationOrganizationType = EducationOrganizationType.District
        };

        await educationOrganizationRepository.AddAsync(district);

        // Create new school
        var school = new EducationOrganization()
        {
            Id = Guid.NewGuid(),
            ParentOrganizationId = districtGuid,
            Name = "Yamhill High School",
            Number = "1001",
            EducationOrganizationType = EducationOrganizationType.School
        };

        await educationOrganizationRepository.AddAsync(school);

        string studentJsonString =
@"{
    ""OregonNexus.Connector.Edupoint.Synergy.Student"": {
        ""SisNumber"": ""456789"",
        ""StudentGuid"": ""5d3957d3-aee4-45c4-bf5b-4a6fc65ca68a""
    }
}";

        // Create incoming request
        var requestGuid = Guid.NewGuid();

        var incomingRequest = new Request()
        {
            Id = requestGuid,
            EducationOrganizationId = school.Id,
            InitialRequestSentDate = DateTime.UtcNow.AddDays(-1),
            Student = JsonDocument.Parse(studentJsonString)!,
            RequestStatus = RequestStatus.Received
        };

        await requestsRepository.AddAsync(incomingRequest);

        // Create message with contents
        var messageGuid = Guid.NewGuid();

        var message = new Message()
        {
            Id = messageGuid,
            RequestId = requestGuid,
            MessageTimestamp = DateTime.UtcNow,
            RequestResponse = RequestResponse.Response
        };

        await messageRepository.AddAsync(message);

        // Create payload contents
        string studentCourseHistoryJsonString =
@"[
        {
            ""id"": ""25d7ea19f55841bfbc9520482549410f"",
            ""studentUniqueId"": ""605509"",
            ""courseCode"": ""03100500"",
            ""educationOrganizationId"": 255901001,
            ""schoolYear"": 2022,
            ""courseTitle"": ""Algebra I"",
            ""attemptedCredits"": ""1"",
            ""earnedCredits"": ""0.5"",
            ""finalLetterGradeEarned"": ""F"",
            ""externalEducationOrganizationNameOfInstitution"": ""Grand Bend High School""
        },
        {
            ""id"": ""1172b84de15e461c9dc96e2261753baf"",
            ""studentUniqueId"": ""605509"",
            ""courseCode"": ""03100500"",
            ""educationOrganizationId"": 255901001,
            ""schoolYear"": 2022,
            ""courseTitle"": ""Algebra I"",
            ""attemptedCredits"": ""1"",
            ""earnedCredits"": ""0.5"",
            ""finalLetterGradeEarned"": ""F"",
            ""externalEducationOrganizationNameOfInstitution"": ""Grand Bend High School""
        },
        {
            ""id"": ""ec04af58dcd744a2854971b40f7629cc"",
            ""studentUniqueId"": ""605509"",
            ""courseCode"": ""HUMT"",
            ""educationOrganizationId"": 255901001,
            ""schoolYear"": 2022,
            ""courseTitle"": ""Humanities"",
            ""attemptedCredits"": ""1"",
            ""earnedCredits"": ""0.5"",
            ""finalLetterGradeEarned"": ""F"",
            ""externalEducationOrganizationNameOfInstitution"": ""Grand Bend High School""
        },
        {
            ""id"": ""719ee107cf6744049cbbafd8513e8e23"",
            ""studentUniqueId"": ""605509"",
            ""courseCode"": ""ART-1"",
            ""educationOrganizationId"": 255901001,
            ""schoolYear"": 2022,
            ""courseTitle"": ""Art I"",
            ""attemptedCredits"": ""1"",
            ""earnedCredits"": ""1"",
            ""finalLetterGradeEarned"": ""D"",
            ""externalEducationOrganizationNameOfInstitution"": ""Grand Bend High School""
        },
        {
            ""id"": ""d4324fe8eef147078decf4a8653c3c6f"",
            ""studentUniqueId"": ""605509"",
            ""courseCode"": ""ART-1"",
            ""educationOrganizationId"": 255901001,
            ""schoolYear"": 2022,
            ""courseTitle"": ""Art I"",
            ""attemptedCredits"": ""1"",
            ""earnedCredits"": ""1"",
            ""finalLetterGradeEarned"": ""A"",
            ""externalEducationOrganizationNameOfInstitution"": ""Grand Bend High School""
        },
        {
            ""id"": ""305fd762fe65422b8d003f4a8fe6c5a2"",
            ""studentUniqueId"": ""605509"",
            ""courseCode"": ""BIO"",
            ""educationOrganizationId"": 255901001,
            ""schoolYear"": 2022,
            ""courseTitle"": ""Biology"",
            ""attemptedCredits"": ""1"",
            ""earnedCredits"": ""1"",
            ""finalLetterGradeEarned"": ""A"",
            ""externalEducationOrganizationNameOfInstitution"": ""Grand Bend High School""
        },
        {
            ""id"": ""57cf86ad0c28469882bc896134aee1d2"",
            ""studentUniqueId"": ""605509"",
            ""courseCode"": ""BIO"",
            ""educationOrganizationId"": 255901001,
            ""schoolYear"": 2022,
            ""courseTitle"": ""Biology"",
            ""attemptedCredits"": ""1"",
            ""earnedCredits"": ""1"",
            ""finalLetterGradeEarned"": ""A"",
            ""externalEducationOrganizationNameOfInstitution"": ""Grand Bend High School""
        },
        {
            ""id"": ""4284ba4ea0a149dbb4e0593fb37f0fa7"",
            ""studentUniqueId"": ""605509"",
            ""courseCode"": ""ENG-1"",
            ""educationOrganizationId"": 255901001,
            ""schoolYear"": 2022,
            ""courseTitle"": ""English I"",
            ""attemptedCredits"": ""1"",
            ""earnedCredits"": ""1"",
            ""finalLetterGradeEarned"": ""A"",
            ""externalEducationOrganizationNameOfInstitution"": ""Grand Bend High School""
        },
        {
            ""id"": ""b898802f40034b479c905c6903a56ef0"",
            ""studentUniqueId"": ""605509"",
            ""courseCode"": ""ENG-1"",
            ""educationOrganizationId"": 255901001,
            ""schoolYear"": 2022,
            ""courseTitle"": ""English I"",
            ""attemptedCredits"": ""1"",
            ""earnedCredits"": ""1"",
            ""finalLetterGradeEarned"": ""A"",
            ""externalEducationOrganizationNameOfInstitution"": ""Grand Bend High School""
        },
        {
            ""id"": ""b925931b09c843a48aaddd6dc2febdc2"",
            ""studentUniqueId"": ""605509"",
            ""courseCode"": ""HLTH-ED"",
            ""educationOrganizationId"": 255901001,
            ""schoolYear"": 2022,
            ""courseTitle"": ""Health Education"",
            ""attemptedCredits"": ""1"",
            ""earnedCredits"": ""1"",
            ""finalLetterGradeEarned"": ""D"",
            ""externalEducationOrganizationNameOfInstitution"": ""Grand Bend High School""
        },
        {
            ""id"": ""fe54ac72136d4b1f92728809d59bd456"",
            ""studentUniqueId"": ""605509"",
            ""courseCode"": ""HLTH-ED"",
            ""educationOrganizationId"": 255901001,
            ""schoolYear"": 2022,
            ""courseTitle"": ""Health Education"",
            ""attemptedCredits"": ""1"",
            ""earnedCredits"": ""1"",
            ""finalLetterGradeEarned"": ""A"",
            ""externalEducationOrganizationNameOfInstitution"": ""Grand Bend High School""
        },
        {
            ""id"": ""7ce1bbe02b3840bbb294336fbc42951c"",
            ""studentUniqueId"": ""605509"",
            ""courseCode"": ""HUMT"",
            ""educationOrganizationId"": 255901001,
            ""schoolYear"": 2022,
            ""courseTitle"": ""Humanities"",
            ""attemptedCredits"": ""1"",
            ""earnedCredits"": ""1"",
            ""finalLetterGradeEarned"": ""A"",
            ""externalEducationOrganizationNameOfInstitution"": ""Grand Bend High School""
        },
        {
            ""id"": ""ec2406ff03264e998b48a027e3166840"",
            ""studentUniqueId"": ""605509"",
            ""courseCode"": ""WGEO"",
            ""educationOrganizationId"": 255901001,
            ""schoolYear"": 2022,
            ""courseTitle"": ""World Geography Studies"",
            ""attemptedCredits"": ""1"",
            ""earnedCredits"": ""1"",
            ""finalLetterGradeEarned"": ""C"",
            ""externalEducationOrganizationNameOfInstitution"": ""Grand Bend High School""
        },
        {
            ""id"": ""2e47979374fd431a974d0a5d961f6483"",
            ""studentUniqueId"": ""605509"",
            ""courseCode"": ""WGEO"",
            ""educationOrganizationId"": 255901001,
            ""schoolYear"": 2022,
            ""courseTitle"": ""World Geography Studies"",
            ""attemptedCredits"": ""1"",
            ""earnedCredits"": ""1"",
            ""finalLetterGradeEarned"": ""D"",
            ""externalEducationOrganizationNameOfInstitution"": ""Grand Bend High School""
        }
]";
        var stuCrsHisPayloadContent = new PayloadContent()
        {
            Id = Guid.NewGuid(),
            MessageId = messageGuid,
            ContentType = "EdFi.OdsApi.Sdk.Models.All.EdFiCourseTranscript",
            JsonContent = JsonDocument.Parse(studentCourseHistoryJsonString)
        };
        await payloadRepository.AddAsync(stuCrsHisPayloadContent);

    }
}