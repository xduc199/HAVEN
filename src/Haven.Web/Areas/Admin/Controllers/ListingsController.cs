using Haven.Application.Listings;
using Haven.Application.Tags;
using Haven.Domain.Properties;
using Haven.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Haven.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public sealed class ListingsController : Controller
{
    private readonly ListingApplicationService _listingService;
    private readonly TagApplicationService _tagService;

    public ListingsController(
        ListingApplicationService listingService,
        TagApplicationService tagService)
    {
        _listingService = listingService;
        _tagService = tagService;
    }

    public async Task<IActionResult> Index(
        [FromQuery] AdminListingFilterViewModel filter,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Filter"] = filter;
            await SetListingFiltersAsync(cancellationToken);
            return View(new Haven.Application.Listings.ModerationListingPage(
                [], Math.Max(1, filter.Page), 20, 0, 0));
        }

        try
        {
            ViewData["Filter"] = filter;
            if (filter.FromDate.HasValue != filter.ToDate.HasValue)
            {
                ModelState.AddModelError(
                    nameof(filter.FromDate),
                    "Cần chọn cả ngày bắt đầu và ngày kết thúc.");
                await SetListingFiltersAsync(cancellationToken);
                return View(new Haven.Application.Listings.ModerationListingPage(
                    [], filter.Page, 20, 0, 0));
            }

            if (filter.FromDate is { } fromDate &&
                filter.ToDate is { } toDate &&
                toDate < fromDate)
            {
                ModelState.AddModelError(nameof(filter.ToDate), "Ngày kết thúc phải từ ngày bắt đầu trở đi.");
                await SetListingFiltersAsync(cancellationToken);
                return View(new Haven.Application.Listings.ModerationListingPage(
                    [], filter.Page, 20, 0, 0));
            }

            var fromUtc = filter.FromDate?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            DateTime? toUtcExclusive = filter.ToDate is { } endDate && endDate < DateOnly.MaxValue
                ? endDate.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
                : null;
            var page = await _listingService.SearchForAdminAsync(
                new AdminListingCriteria(
                    filter.Search,
                    filter.Status,
                    filter.PropertyTypeId,
                    filter.TagId,
                    fromUtc.HasValue ? new DateTimeOffset(fromUtc.Value) : null,
                    toUtcExclusive.HasValue ? new DateTimeOffset(toUtcExclusive.Value) : null,
                    filter.Page,
                    20),
                cancellationToken);
            await SetListingFiltersAsync(cancellationToken);
            return View(page);
        }
        catch (ArgumentOutOfRangeException)
        {
            ModelState.AddModelError(string.Empty, "Các bộ lọc hoặc số trang không hợp lệ.");
            ViewData["Filter"] = filter;
            await SetListingFiltersAsync(cancellationToken);
            return View(new Haven.Application.Listings.ModerationListingPage(
                [], Math.Max(1, filter.Page), 20, 0, 0));
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            ViewData["Filter"] = filter;
            await SetListingFiltersAsync(cancellationToken);
            return View(new Haven.Application.Listings.ModerationListingPage(
                [], filter.Page, 20, 0, 0));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(
        int id,
        CancellationToken cancellationToken)
    {
        var listing = await _listingService.GetForAdminAsync(id, cancellationToken);
        if (listing is null)
        {
            return NotFound();
        }

        ViewData["ListingTags"] = await _tagService.GetForListingAsync(id, cancellationToken);
        ViewData["AvailableTags"] = (await _tagService.GetAdminPageAsync(
            null, 1, 100, cancellationToken)).Items
            .Where(tag => tag.IsActive)
            .ToArray();
        return View(listing);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignTag(
        int id,
        ListingTagAssignmentViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Chọn một tag hợp lệ.";
            return RedirectToAction(nameof(Details), new { id });
        }

        try
        {
            var result = await _tagService.AssignToListingAsync(
                id, model.TagId, cancellationToken);
            TempData[result == TagAssignmentResult.Added ? "SuccessMessage" : "ErrorMessage"] =
                result switch
                {
                    TagAssignmentResult.Added => "Tag đã được gắn vào listing.",
                    TagAssignmentResult.AlreadyAssigned => "Tag này đã được gắn.",
                    TagAssignmentResult.ListingNotFound => "Không tìm thấy listing.",
                    TagAssignmentResult.TagNotFound => "Không tìm thấy tag.",
                    TagAssignmentResult.TagInactive => "Không thể gắn tag đã vô hiệu hóa.",
                    _ => "Không thể gắn tag."
                };
        }
        catch (ArgumentOutOfRangeException)
        {
            TempData["ErrorMessage"] = "Listing hoặc tag không hợp lệ.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveTag(
        int id,
        int tagId,
        CancellationToken cancellationToken)
    {
        try
        {
            TempData[await _tagService.RemoveFromListingAsync(id, tagId, cancellationToken)
                ? "SuccessMessage"
                : "ErrorMessage"] = "Cập nhật tag của listing hoàn tất.";
        }
        catch (ArgumentOutOfRangeException)
        {
            TempData["ErrorMessage"] = "Listing hoặc tag không hợp lệ.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var listing = await _listingService.ApproveAsync(id, cancellationToken);
            if (listing is null)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Tin đăng đã được duyệt.";
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Reject(
        int id,
        CancellationToken cancellationToken)
    {
        var listing = await _listingService.GetForAdminAsync(id, cancellationToken);
        if (listing is null)
        {
            return NotFound();
        }

        if (listing.Status != ListingStatus.Pending)
        {
            TempData["ErrorMessage"] = "Chỉ tin đang chờ duyệt mới có thể bị từ chối.";
            return RedirectToAction(nameof(Details), new { id });
        }

        ViewData["ListingTitle"] = listing.Title;
        return View(new RejectListingViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(
        int id,
        RejectListingViewModel model,
        CancellationToken cancellationToken)
    {
        var listing = await _listingService.GetForAdminAsync(id, cancellationToken);
        if (listing is null)
        {
            return NotFound();
        }

        ViewData["ListingTitle"] = listing.Title;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await _listingService.RejectAsync(
                id,
                model.Reason,
                cancellationToken);
            if (result is null)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Tin đăng đã bị từ chối.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(nameof(model.Reason), exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Hide(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var listing = await _listingService.HideAsync(id, cancellationToken);
            if (listing is null)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Tin đăng đã được ẩn khỏi kết quả công khai.";
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task SetListingFiltersAsync(CancellationToken cancellationToken)
    {
        ViewData["PropertyTypes"] = await _listingService.GetActivePropertyTypesAsync(cancellationToken);
        ViewData["Tags"] = (await _tagService.GetAdminPageAsync(
            null, 1, 100, cancellationToken)).Items;
    }
}
