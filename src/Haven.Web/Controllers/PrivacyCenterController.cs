using System.Security.Claims;
using Haven.Domain.Privacy;
using Haven.Infrastructure.Context;
using Haven.Infrastructure.Identity;
using Haven.Web.ViewModels.Privacy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Haven.Web.Controllers;

[Authorize]
[Route("Account/Privacy")]
public sealed class PrivacyCenterController(
    ApplicationDbContext db,
    UserManager<HavenUser> userManager,
    TimeProvider clock) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return Challenge();
        var consents = await db.PolicyConsents.AsNoTracking().Where(item => item.UserId == userId)
            .OrderByDescending(item => item.OccurredAtUtc)
            .Select(item => new ConsentHistoryItem(item.DocumentKey, item.DocumentVersion, item.Action, item.OccurredAtUtc))
            .ToListAsync(cancellationToken);
        var requests = await db.PrivacyRequests.AsNoTracking().Where(item => item.UserId == userId)
            .OrderByDescending(item => item.CreatedAtUtc)
            .Select(item => new PrivacyRequestListItem(item.Id, item.Type, item.ListingId, item.Message, item.Status, item.Resolution, item.CreatedAtUtc))
            .ToListAsync(cancellationToken);
        return View(new PrivacyCenterViewModel(user.Email ?? string.Empty, user.DisplayName, consents, requests));
    }

    [HttpPost("requests")]
    public async Task<IActionResult> CreateRequest(CreatePrivacyRequestViewModel model, CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(model.Type)) ModelState.AddModelError(nameof(model.Type), "Loại yêu cầu không hợp lệ.");
        if (model.Type == PrivacyRequestType.Complaint && model.ListingId is null)
            ModelState.AddModelError(nameof(model.ListingId), "Hãy nhập mã tin đăng cần phản ánh.");
        if (model.Type != PrivacyRequestType.Complaint) model.ListingId = null;
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Vui lòng kiểm tra loại yêu cầu và nội dung đã nhập.";
            return RedirectToAction(nameof(Index));
        }

        if (model.ListingId is int listingId && !await db.Listings.AnyAsync(item => item.Id == listingId, cancellationToken))
        {
            TempData["ErrorMessage"] = "Không tìm thấy tin đăng cần phản ánh.";
            return RedirectToAction(nameof(Index));
        }
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        db.PrivacyRequests.Add(PrivacyRequest.Create(userId, model.Type, model.Message, model.ListingId, clock.GetUtcNow()));
        await db.SaveChangesAsync(cancellationToken);
        TempData["SuccessMessage"] = "Yêu cầu đã được gửi riêng tư để HAVEN xem xét.";
        return RedirectToAction(nameof(Index));
    }
}
