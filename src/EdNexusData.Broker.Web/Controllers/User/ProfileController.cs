using Ardalis.GuardClauses;
using EdNexusData.Broker.Web.Constants.DesignSystems;
using EdNexusData.Broker.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace EdNexusData.Broker.Web.Controllers;

[Authorize]
public class ProfileController : AuthenticatedController<ProfileController>
{
    private readonly IRepository<User> userRepository;
    private readonly CurrentUserHelper currentUserHelper;

    public ProfileController(IRepository<User> userRepository, CurrentUserHelper currentUserHelper)
    {
        this.userRepository = userRepository;
        this.currentUserHelper = currentUserHelper;
    }

    public async Task<IActionResult> Index()
    {
        var user = await userRepository.GetByIdAsync(currentUserHelper.CurrentUserId()!.Value);

        Guard.Against.Null(user, "user", "Unable to find user.");

        user.TimeZone = user.TimeZone.IsNullOrEmpty() ? ((TimeZoneInfo.Local.Id == "Etc/UTC") ? TimeZoneInfo.Utc.Id : TimeZoneInfo.Local.Id) : user.TimeZone;
        return View(user);
    }

    [HttpPut]
    public async Task<IActionResult> Update(User user)
    {
        var userRepo = await userRepository.GetByIdAsync(currentUserHelper.CurrentUserId()!.Value);

        Guard.Against.Null(userRepo, "userRepo", "Unable to find user.");

        userRepo.TimeZone = user.TimeZone.IsNullOrEmpty() ? ((TimeZoneInfo.Local.Id == "Etc/UTC") ? TimeZoneInfo.Utc.Id : TimeZoneInfo.Local.Id) : user.TimeZone;

        await userRepository.UpdateAsync(userRepo);

        TempData[VoiceTone.Positive] = $"Updated profile.";
        return RedirectToAction("Index");
    }
}