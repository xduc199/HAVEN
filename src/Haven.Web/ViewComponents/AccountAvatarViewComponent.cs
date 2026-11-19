using Haven.Infrastructure.Identity;
using Haven.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Haven.Web.ViewComponents;

public sealed class AccountAvatarViewComponent(UserManager<HavenUser> userManager) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var user = await userManager.GetUserAsync(HttpContext.User);
        if (user is null) return Content(string.Empty);
        return View(new AvatarViewModel(user.AvatarPath, user.DisplayName, "small"));
    }
}
