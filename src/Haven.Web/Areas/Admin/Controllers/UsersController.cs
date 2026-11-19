using Haven.Infrastructure.Identity;
using Haven.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Haven.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public sealed class UsersController(
    UserManager<HavenUser> userManager,
    ILogger<UsersController> logger,
    TimeProvider timeProvider) : Controller
{
    private const int PageSize = 20;

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] AdminUserFilterViewModel filter, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(new AdminUserPageViewModel([], filter.Search, 1, PageSize, 0));

        var query = userManager.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            query = query.Where(user => user.Email!.Contains(term) || user.DisplayName.Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));
        var page = Math.Clamp(filter.Page, 1, totalPages);
        var users = await query.OrderBy(user => user.DisplayName).ThenBy(user => user.Email)
            .Skip((page - 1) * PageSize).Take(PageSize).ToListAsync(cancellationToken);
        var now = timeProvider.GetUtcNow();
        var rows = new List<AdminUserRowViewModel>(users.Count);
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            var lockoutEnd = user.LockoutEnd;
            rows.Add(new AdminUserRowViewModel(
                user.Id,
                user.DisplayName,
                user.Email ?? string.Empty,
                roles.Count == 0 ? "Chưa có role" : string.Join(", ", roles),
                lockoutEnd == DateTimeOffset.MaxValue,
                lockoutEnd is { } end && end > now && end != DateTimeOffset.MaxValue,
                lockoutEnd is { } temporaryEnd && temporaryEnd > now && temporaryEnd != DateTimeOffset.MaxValue
                    ? temporaryEnd
                    : null,
                roles.Contains("Admin", StringComparer.Ordinal),
                user.AvatarPath));
        }

        return View(new AdminUserPageViewModel(rows, filter.Search, page, PageSize, totalCount));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetLockout(
        string? id,
        bool? locked,
        string? search,
        int page = 1)
    {
        page = Math.Max(page, 1);
        if (string.IsNullOrWhiteSpace(id) || locked is null)
        {
            TempData["ErrorMessage"] = "Yêu cầu khóa tài khoản không hợp lệ. Vui lòng thử lại.";
            return UserListRedirect(search, page);
        }

        var user = await userManager.FindByIdAsync(id);
        if (user is null) return NotFound();
        if (string.Equals(user.Id, userManager.GetUserId(User), StringComparison.Ordinal))
        {
            TempData["ErrorMessage"] = "Bạn không thể khóa tài khoản đang sử dụng.";
            return UserListRedirect(search, page);
        }

        if (await userManager.IsInRoleAsync(user, "Admin"))
        {
            TempData["ErrorMessage"] = "Không thể khóa tài khoản Admin từ màn hình này.";
            return UserListRedirect(search, page);
        }

        IdentityResult result;
        if (locked.Value)
        {
            if (user.LockoutEnd == DateTimeOffset.MaxValue)
            {
                TempData["SuccessMessage"] = "Tài khoản đã được khóa bởi quản trị viên.";
                return UserListRedirect(search, page);
            }

            var now = timeProvider.GetUtcNow();
            if (user.LockoutEnd is { } temporaryEnd && temporaryEnd > now)
            {
                TempData["ErrorMessage"] = "Tài khoản đang bị Identity khóa tạm thời do đăng nhập sai. Thao tác Admin không ghi đè thời hạn khóa này.";
                return UserListRedirect(search, page);
            }

            result = await userManager.SetLockoutEnabledAsync(user, true);
            if (result.Succeeded)
                result = await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        }
        else if (user.LockoutEnd == DateTimeOffset.MaxValue)
        {
            result = await userManager.SetLockoutEndDateAsync(user, null);
        }
        else if (user.LockoutEnd is { } temporaryEnd && temporaryEnd > timeProvider.GetUtcNow())
        {
            TempData["ErrorMessage"] = "Khóa tạm thời do đăng nhập sai sẽ tự hết hạn; thao tác Admin không thể xóa trạng thái này.";
            return UserListRedirect(search, page);
        }
        else
        {
            TempData["SuccessMessage"] = "Tài khoản hiện không bị khóa bởi quản trị viên.";
            return UserListRedirect(search, page);
        }

        if (result.Succeeded)
            result = await userManager.UpdateSecurityStampAsync(user);

        if (!result.Succeeded)
        {
            logger.LogWarning(
                "Could not {Operation} an account through Admin user management. Identity error codes: {ErrorCodes}",
                locked.Value ? "lock" : "unlock",
                string.Join(", ", result.Errors.Select(error => error.Code)));
        }

        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Succeeded
            ? locked.Value
                ? "Tài khoản đã được khóa. Các phiên đăng nhập hiện có sẽ hết hiệu lực sau lần kiểm tra bảo mật kế tiếp."
                : "Tài khoản đã được mở khóa. Bộ đếm đăng nhập sai được giữ nguyên."
            : "Không thể cập nhật trạng thái tài khoản. Vui lòng thử lại.";
        return UserListRedirect(search, page);
    }

    private IActionResult UserListRedirect(string? search, int page) =>
        RedirectToAction(nameof(Index), new { search, page });
}
