using Haven.Infrastructure.Identity;
using Haven.Infrastructure.Context;
using Haven.Domain.Privacy;
using Haven.Web.Services;
using Haven.Web.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Haven.Web.Controllers;

[Route("account")]
public sealed class AccountController : Controller
{
    private readonly UserManager<HavenUser> _userManager;
    private readonly SignInManager<HavenUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<AccountController> _logger;
    private readonly ApplicationDbContext _db;
    private readonly TimeProvider _clock;
    private readonly AvatarImageStorage _avatarStorage;

    public AccountController(
        UserManager<HavenUser> userManager,
        SignInManager<HavenUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<AccountController> logger,
        ApplicationDbContext db,
        TimeProvider clock,
        AvatarImageStorage avatarStorage)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _logger = logger;
        _db = db;
        _clock = clock;
        _avatarStorage = avatarStorage;
    }

    [AllowAnonymous]
    [HttpGet("register")]
    public IActionResult Register() => View(new RegisterViewModel());

    [AllowAnonymous]
    [HttpPost("register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!model.AcceptedTerms)
            ModelState.AddModelError(nameof(model.AcceptedTerms), "Bạn cần đồng ý với Điều khoản sử dụng.");
        if (!model.ConsentedPersonalDataProcessing)
            ModelState.AddModelError(nameof(model.ConsentedPersonalDataProcessing), "Bạn cần xác nhận đồng ý xử lý dữ liệu cá nhân để tạo tài khoản.");
        if (model.TermsVersion != PolicyVersions.Terms || model.PrivacyVersion != PolicyVersions.Privacy)
            ModelState.AddModelError(string.Empty, "Phiên bản chính sách đã thay đổi. Hãy tải lại trang đăng ký.");
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (!await EnsureUserRoleAsync())
        {
            ModelState.AddModelError(
                string.Empty,
                "Đăng ký hiện chưa khả dụng. Vui lòng thử lại sau.");
            return View(model);
        }

        var email = model.Email.Trim();
        var user = new HavenUser
        {
            UserName = email,
            Email = email,
            DisplayName = model.DisplayName.Trim()
        };
        await using var registrationTransaction = _db.Database.IsRelational()
            ? await _db.Database.BeginTransactionAsync()
            : null;
        var createResult = await _userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            AddIdentityErrors(createResult);
            return View(model);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, "User");
        if (!roleResult.Succeeded)
        {
            var rollbackSucceeded = true;
            if (registrationTransaction is not null)
                await registrationTransaction.RollbackAsync();
            else
                rollbackSucceeded = (await _userManager.DeleteAsync(user)).Succeeded;
            _logger.LogError(
                "Could not assign the User role to a registered account. Rollback succeeded: {RollbackSucceeded}. Codes: {ErrorCodes}",
                rollbackSucceeded,
                string.Join(", ", roleResult.Errors.Select(error => error.Code)));
            ModelState.AddModelError(
                string.Empty,
                "Không thể hoàn tất đăng ký. Vui lòng thử lại sau.");
            return View(model);
        }

        try
        {
            var now = _clock.GetUtcNow();
            _db.PolicyConsents.AddRange(
                PolicyConsent.Record(user.Id, "Terms", PolicyVersions.Terms, PolicyConsentAction.Granted, "Registration", now),
                PolicyConsent.Record(user.Id, "Privacy", PolicyVersions.Privacy, PolicyConsentAction.Granted, "Registration", now));
            await _db.SaveChangesAsync();
        }
        catch (Exception exception)
        {
            var rollbackSucceeded = true;
            if (registrationTransaction is not null)
                await registrationTransaction.RollbackAsync();
            else
            {
                foreach (var entry in _db.ChangeTracker.Entries<PolicyConsent>().Where(entry => entry.State == EntityState.Added).ToArray())
                    entry.State = EntityState.Detached;
                rollbackSucceeded = (await _userManager.DeleteAsync(user)).Succeeded;
            }
            _logger.LogError(exception, "Could not record required policy consent. Account rollback succeeded: {RollbackSucceeded}", rollbackSucceeded);
            ModelState.AddModelError(string.Empty, "Không thể hoàn tất đăng ký. Vui lòng thử lại sau.");
            return View(model);
        }

        if (registrationTransaction is not null)
            await registrationTransaction.CommitAsync();

        await _signInManager.SignInAsync(user, isPersistent: false);
        TempData["SuccessMessage"] = "Tài khoản đã được tạo.";
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    [HttpGet("login")]
    public IActionResult Login(string? returnUrl = null) =>
        View(new LoginViewModel { ReturnUrl = returnUrl });

    [AllowAnonymous]
    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email.Trim());
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                _logger.LogWarning("A sign-in attempt was rejected for a locked account.");
            }

            ModelState.AddModelError(
                string.Empty,
                result.IsLockedOut
                    ? "Tài khoản tạm thời bị khóa. Vui lòng thử lại sau."
                    : "Email hoặc mật khẩu không đúng.");
            return View(model);
        }

        TempData["SuccessMessage"] = "Đăng nhập thành công.";
        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            return Redirect("/Admin");
        }

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) &&
            Url.IsLocalUrl(model.ReturnUrl) &&
            !TargetsAdminArea(model.ReturnUrl))
        {
            return LocalRedirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        TempData["SuccessMessage"] = "Bạn đã đăng xuất.";
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            await _signInManager.SignOutAsync();
            return Challenge();
        }

        return View(new ProfileViewModel
        {
            DisplayName = user.DisplayName,
            Email = user.Email ?? string.Empty,
            AvatarPath = user.AvatarPath
        });
    }

    [Authorize]
    [HttpPost("profile/avatar")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UpdateAvatar(IFormFile? avatar, CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            await _signInManager.SignOutAsync();
            return Challenge();
        }
        if (avatar is null)
        {
            TempData["ErrorMessage"] = "Chọn ảnh đại diện trước khi lưu.";
            return RedirectToAction(nameof(Profile));
        }

        try
        {
            var newPath = await _avatarStorage.SaveAsync(avatar, cancellationToken);
            var oldPath = user.AvatarPath;
            user.AvatarPath = newPath;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                await _avatarStorage.DeleteAsync(newPath);
                AddIdentityErrors(result);
                TempData["ErrorMessage"] = "Không thể cập nhật ảnh đại diện. Vui lòng thử lại.";
                return RedirectToAction(nameof(Profile));
            }

            await _signInManager.RefreshSignInAsync(user);
            await _avatarStorage.DeleteAsync(oldPath);
            TempData["SuccessMessage"] = "Ảnh đại diện đã được cập nhật.";
        }
        catch (InvalidDataException exception)
        {
            _logger.LogInformation("Avatar upload was rejected: {Reason}", exception.Message);
            TempData["ErrorMessage"] = exception.Message;
        }
        return RedirectToAction(nameof(Profile));
    }

    [Authorize]
    [HttpPost("profile")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        model.Email = User.Identity?.Name ?? model.Email;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            await _signInManager.SignOutAsync();
            return Challenge();
        }

        user.DisplayName = model.DisplayName.Trim();
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            return View(new ProfileViewModel
            {
                DisplayName = model.DisplayName,
                Email = user.Email ?? string.Empty,
                AvatarPath = user.AvatarPath
            });
        }

        await _signInManager.RefreshSignInAsync(user);
        TempData["SuccessMessage"] = "Hồ sơ đã được cập nhật.";
        return RedirectToAction(nameof(Profile));
    }

    [Authorize]
    [HttpGet("change-password")]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    [Authorize]
    [HttpPost("change-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            await _signInManager.SignOutAsync();
            return Challenge();
        }

        var result = await _userManager.ChangePasswordAsync(
            user,
            model.CurrentPassword,
            model.NewPassword);
        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            return View(model);
        }

        await _signInManager.RefreshSignInAsync(user);
        TempData["SuccessMessage"] = "Mật khẩu đã được đổi.";
        return RedirectToAction(nameof(Profile));
    }

    [AllowAnonymous]
    [HttpGet("access-denied")]
    public IActionResult AccessDenied() => View();

    private async Task<bool> EnsureUserRoleAsync()
    {
        if (await _roleManager.RoleExistsAsync("User"))
        {
            return true;
        }

        var result = await _roleManager.CreateAsync(new IdentityRole("User"));
        if (result.Succeeded || await _roleManager.RoleExistsAsync("User"))
        {
            return true;
        }

        _logger.LogError(
            "Could not ensure the User role. Identity error codes: {ErrorCodes}",
            string.Join(", ", result.Errors.Select(error => error.Code)));
        return false;
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

    private static bool TargetsAdminArea(string returnUrl)
    {
        var path = returnUrl.Split('?', '#')[0];
        path = Uri.UnescapeDataString(path).Replace('\\', '/');
        if (path.StartsWith("~/", StringComparison.Ordinal))
        {
            path = path[1..];
        }

        var firstSegment = path.TrimStart('/').Split('/', 2)[0];
        return string.Equals(firstSegment, "Admin", StringComparison.OrdinalIgnoreCase);
    }
}
