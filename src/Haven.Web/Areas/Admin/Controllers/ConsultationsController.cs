using Haven.Application.Consultations;
using Haven.Domain.Consultations;
using Haven.Web.ViewModels.Admin;
using Haven.Web.ViewModels.Consultations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Haven.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public sealed class ConsultationsController : Controller
{
    private readonly ConsultationApplicationService _consultationService;

    public ConsultationsController(
        ConsultationApplicationService consultationService)
    {
        _consultationService = consultationService;
    }

    public async Task<IActionResult> Index(
        [FromQuery] AdminConsultationFilterViewModel filter,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Filter"] = filter;
            return View(new Haven.Application.Consultations.ConsultationPageResult(
                [], Math.Max(1, filter.Page), 20, 0, 0));
        }

        try
        {
            if (filter.FromDate.HasValue != filter.ToDate.HasValue)
            {
                ModelState.AddModelError(
                    nameof(filter.FromDate),
                    "Cần chọn cả ngày bắt đầu và ngày kết thúc.");
                ViewData["Filter"] = filter;
                return View(new Haven.Application.Consultations.ConsultationPageResult(
                    [], filter.Page, 20, 0, 0));
            }

            if (filter.FromDate is { } fromDate &&
                filter.ToDate is { } toDate &&
                toDate < fromDate)
            {
                ModelState.AddModelError(nameof(filter.ToDate), "Ngày kết thúc phải từ ngày bắt đầu trở đi.");
                ViewData["Filter"] = filter;
                return View(new Haven.Application.Consultations.ConsultationPageResult(
                    [], filter.Page, 20, 0, 0));
            }

            var fromUtc = filter.FromDate?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            DateTime? toUtcExclusive = filter.ToDate is { } endDate && endDate < DateOnly.MaxValue
                ? endDate.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
                : null;
            ViewData["Filter"] = filter;
            return View(await _consultationService.SearchForAdminAsync(
                new Haven.Application.Consultations.AdminConsultationCriteria(
                    filter.Search,
                    filter.Status,
                    fromUtc.HasValue ? new DateTimeOffset(fromUtc.Value) : null,
                    toUtcExclusive.HasValue ? new DateTimeOffset(toUtcExclusive.Value) : null,
                    filter.Page,
                    20),
                cancellationToken));
        }
        catch (ArgumentOutOfRangeException)
        {
            ModelState.AddModelError(string.Empty, "Các bộ lọc hoặc số trang không hợp lệ.");
            ViewData["Filter"] = filter;
            return View(new Haven.Application.Consultations.ConsultationPageResult(
                [], Math.Max(1, filter.Page), 20, 0, 0));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _consultationService.GetForAdminAsync(
            id,
            cancellationToken);
        return result is null ? NotFound() : View(result);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(
        int id,
        UpdateConsultationStatusViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        try
        {
            var result = await _consultationService.ChangeStatusAsync(
                id,
                model.Status,
                cancellationToken);
            if (result is null)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Trạng thái yêu cầu tư vấn đã được cập nhật.";
        }
        catch (ArgumentException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
