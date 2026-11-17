using Haven.Infrastructure.Identity;
using Haven.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Haven.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public sealed class UsersController(UserManager<HavenUser> userManager) : Controller
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
        var now = DateTimeOffset.UtcNow;
        var rows = new List<AdminUserRowViewModel>(users.Count);
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            rows.Add(new AdminUserRowViewModel(user.Id, user.DisplayName, user.Email ?? string.Empty,
                roles.Count == 0 ? "Chưa có role" : string.Join(", ", roles),
                user.LockoutEnd.HasValue && user.LockoutEnd.Value > now,
                roles.Contains("Admin", StringComparer.Ordinal)));
        }

        return View(new AdminUserPageViewModel(rows, filter.Search, page, PageSize, totalCount));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetLockout(string id, bool locked)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null) return NotFound();
        if (string.Equals(user.Id, userManager.GetUserId(User), StringComparison.Ordinal))
        {
            TempData["ErrorMessage"] = "Bạn không thể khóa tài khoản đang sử dụng.";
            return RedirectToAction(nameof(Index));
        }

        if (await userManager.IsInRoleAsync(user, "Admin"))
        {
            TempData["ErrorMessage"] = "Không thể khóa tài khoản Admin từ màn hình này.";
            return RedirectToAction(nameof(Index));
        }

        var result = await userManager.SetLockoutEnabledAsync(user, true);
        if (result.Succeeded)
        {
            result = await userManager.SetLockoutEndDateAsync(user,
                locked ? DateTimeOffset.MaxValue : null);
        }

        if (result.Succeeded && !locked)
        {
            result = await userManager.ResetAccessFailedCountAsync(user);
        }

        if (result.Succeeded)
        {
            result = await userManager.UpdateSecurityStampAsync(user);
        }

        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Succeeded
            ? locked ? "Tài khoản đã được khóa. Phiên đăng nhập hiện tại sẽ bị thu hồi trong tối đa một phút." : "Tài khoản đã được mở khóa."
            : "Không thể cập nhật trạng thái tài khoản.";
        return RedirectToAction(nameof(Index));
    }
}
