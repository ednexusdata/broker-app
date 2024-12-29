using Microsoft.AspNetCore.Identity;

namespace EdNexusData.Broker.Core.Services;

public class EducationOrganizationContactService
{
    private readonly UserManager<IdentityUser<Guid>> userManager;
    private readonly IReadRepository<User> userRepository;

    public EducationOrganizationContactService(
        UserManager<IdentityUser<Guid>> userManager, 
        IReadRepository<User> userRepository
    )
    {
        this.userManager = userManager;
        this.userRepository = userRepository;
    }

    public async Task<EducationOrganizationContact> FromUser(Guid fromUserId)
    {
        var user = await userRepository.GetByIdAsync(fromUserId);
        var userIdentity = await userManager.FindByIdAsync(fromUserId.ToString());

        _ = user ?? throw new NullReferenceException($"Unable to locate user {fromUserId}");
        _ = userIdentity ?? throw new NullReferenceException($"Unable to locate identity user {fromUserId}");

        return new EducationOrganizationContact()
        {
            Name = user.Name,
            Email = userIdentity?.Email?.ToLower()
        };
    }
}