using System.Security.Claims;
using Haven.Domain.Privacy;
using Haven.Infrastructure.Context;
using Haven.Web.ViewModels.Privacy;
using Haven.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Haven.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public sealed class PrivacyRequestsController(ApplicationDbContext db, TimeProvider clock) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var items = await db.PrivacyRequests.AsNoTracking().OrderByDescending(item => item.CreatedAtUtc).Take(100)
            .Join(db.Users.AsNoTracking(), request => request.UserId, user => user.Id,
                (request, user) => new AdminPrivacyRequestListItem(request.Id, user.DisplayName, user.Email ?? string.Empty,
                    request.Type, request.ListingId, request.Message, request.Status, request.CreatedAtUtc))
            .ToListAsync(cancellationToken);
        return View(items);
    }

    [HttpPost]
    public async Task<IActionResult> Update(UpdatePrivacyRequestViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) { TempData["ErrorMessage"] = "Thông tin cập nhật chưa hợp lệ."; return RedirectToAction(nameof(Index)); }
        var request = await db.PrivacyRequests.SingleOrDefaultAsync(item => item.Id == model.Id, cancellationToken);
        if (request is null) return NotFound();
        try
        {
            request.UpdateStatus(model.Status, model.Resolution, clock.GetUtcNow());
            if (request.Type == PrivacyRequestType.ConsentWithdrawal && model.Status == PrivacyRequestStatus.Completed)
            {
                db.PolicyConsents.Add(PolicyConsent.Record(request.UserId, "Privacy", PolicyVersions.Privacy, PolicyConsentAction.Withdrawn, "PrivacyRequest", clock.GetUtcNow()));
            }
            await db.SaveChangesAsync(cancellationToken);
            TempData["SuccessMessage"] = "Đã cập nhật trạng thái yêu cầu.";
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            TempData["ErrorMessage"] = exception.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}
