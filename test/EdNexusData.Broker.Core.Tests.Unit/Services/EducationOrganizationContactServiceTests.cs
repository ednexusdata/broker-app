using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ComponentModel;

namespace EdNexusData.Broker.Core.Services.Tests.Unit;

public class EducationOrganizationContactServiceTests
{
    private readonly Mock<UserManager<IdentityUser<Guid>>> userManager;
    private readonly Mock<IReadRepository<User>> userRepository;
    private readonly EducationOrganizationContactService service;
    private Guid userGuid = Guid.Parse("15d7c642-bef7-4ea9-8043-bae5277ab175");
    
    public EducationOrganizationContactServiceTests()
    { 
        var userStoreMock = new Mock<IUserStore<IdentityUser<Guid>>>(); 
        var passwordHasherMock = new Mock<IPasswordHasher<IdentityUser<Guid>>>(); 
        var userValidatorsMock = new List<IUserValidator<IdentityUser<Guid>>>().AsEnumerable(); 
        var passwordValidatorsMock = new List<IPasswordValidator<IdentityUser<Guid>>>().AsEnumerable(); 
        var keyNormalizerMock = new Mock<ILookupNormalizer>(); 
        var errorsMock = new Mock<IdentityErrorDescriber>(); 
        var servicesMock = new Mock<IServiceProvider>(); 
        var loggerMock = new Mock<ILogger<UserManager<IdentityUser<Guid>>>>();
        
        userManager = new Mock<UserManager<IdentityUser<Guid>>>(
            userStoreMock.Object, 
            new Mock<IOptions<IdentityOptions>>().Object, 
            passwordHasherMock.Object, 
            userValidatorsMock, 
            passwordValidatorsMock, 
            keyNormalizerMock.Object, 
            errorsMock.Object, 
            servicesMock.Object, 
            loggerMock.Object
        );
        userRepository = new Mock<IReadRepository<User>>();

        userManager.Setup(u => u.FindByIdAsync(
            It.IsAny<string>()
        )).ReturnsAsync(
            new IdentityUser<Guid>()
            {
                Id = userGuid,
                Email = "john.doe@fakeemail.com"
            }
        );

        userRepository.Setup(u => u.GetByIdAsync(
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new User() { Id = userGuid, LastName = "Doe", FirstName = "John" });
        
        service = new EducationOrganizationContactService(userManager.Object, userRepository.Object);
    }
    
    [Fact, Description("Ensure properly formed EducationOrganizationContact object returned")] 
    public async void FromUser_CreateEdOrgContact_JohnDoeEdOrgContact()
    {
        var result = await service.FromUser(userGuid);

        Assert.NotNull(result);
        Assert.IsType<EducationOrganizationContact>(result);
        Assert.Equal("John Doe", result.Name);
        Assert.Equal("john.doe@fakeemail.com", result.Email);
        Assert.Null(result.JobTitle);
    }
}