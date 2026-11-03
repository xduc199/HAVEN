using Haven.Application.Dashboard;
using Haven.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Haven.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public sealed class ReportsController(
    AdminDashboardApplicationService dashboardService,
    TimeProvider timeProvider) : Controller
{
    public async Task<IActionResult> Index(
        [FromQuery] AdminAnalyticsFilterViewModel filter,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        try
        {
            var range = filter.Resolve(today);
            ViewData["Filter"] = filter;
            return View(await dashboardService.GetAnalyticsAsync(
                range.FromDate,
                range.ToDate,
                cancellationToken));
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(nameof(filter.FromDate), exception.Message);
            filter.Period = "custom";
            ViewData["Filter"] = filter;
            return View(await dashboardService.GetAnalyticsAsync(
                today.AddDays(-6),
                today,
                cancellationToken));
        }
    }
}
