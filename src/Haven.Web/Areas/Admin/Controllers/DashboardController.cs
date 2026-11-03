using Haven.Application.Dashboard;
using Haven.Application.PropertyTypes;
using Haven.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Haven.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public sealed class DashboardController : Controller
{
    private readonly AdminDashboardApplicationService _dashboardService;
    private readonly PropertyTypeApplicationService _propertyTypeService;
    private readonly TimeProvider _timeProvider;

    public DashboardController(
        AdminDashboardApplicationService dashboardService,
        PropertyTypeApplicationService propertyTypeService,
        TimeProvider timeProvider)
    {
        _dashboardService = dashboardService;
        _propertyTypeService = propertyTypeService;
        _timeProvider = timeProvider;
    }

    public async Task<IActionResult> Index(
        [FromQuery] AdminAnalyticsFilterViewModel filter,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);
        ViewData["HasActivePropertyTypes"] =
            (await _propertyTypeService.GetActiveAsync(cancellationToken)).Count > 0;
        try
        {
            var range = filter.Resolve(today);
            ViewData["Filter"] = filter;
            return View(await _dashboardService.GetDashboardAsync(
                range.FromDate,
                range.ToDate,
                cancellationToken));
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(nameof(filter.FromDate), exception.Message);
            filter.Period = "custom";
            ViewData["Filter"] = filter;
            var fallback = (today.AddDays(-6), today);
            return View(await _dashboardService.GetDashboardAsync(
                fallback.Item1,
                fallback.Item2,
                cancellationToken));
        }
    }
}
