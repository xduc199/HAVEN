using Haven.Application.Listings;
using Haven.Application.Tags;
using Haven.Domain.Properties;
using Haven.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Haven.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public sealed class ListingsController : Controller
{
    private readonly ListingApplicationService _listingService;
    private readonly TagApplicationService _tagService;
    private readonly ListingImageApplicationService _imageService;
    private readonly ListingVideoApplicationService _videoService;

    public ListingsController(
        ListingApplicationService listingService,
        TagApplicationService tagService,
        ListingImageApplicationService imageService,
        ListingVideoApplicationService videoService)
    {
        _listingService = listingService;
        _tagService = tagService;
        _imageService = imageService;
        _videoService = videoService;
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
        ViewData["ListingImages"] = await _imageService.GetImagesForAdminAsync(id, cancellationToken);
        ViewData["ListingVideos"] = await _videoService.GetVideosForAdminAsync(id, cancellationToken);
        return View(listing);
    }

    [HttpPost]
    [RequestSizeLimit(105_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 105_000_000)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadImages(int id, List<IFormFile> images, CancellationToken cancellationToken)
    {
        if (images.Count == 0)
        {
            TempData["ErrorMessage"] = "Chọn ít nhất một ảnh để tải lên.";
            return RedirectToAction(nameof(Details), new { id });
        }
        try
        {
            await _imageService.UploadAsync("admin", id, images.Select(file => new ListingImageUpload(file.OpenReadStream(), file.Length, file.FileName, file.ContentType)).ToArray(), isAdmin: true, cancellationToken: cancellationToken);
            TempData["SuccessMessage"] = "Ảnh đã được tải lên.";
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (Exception exception) when (exception is ArgumentException or IOException or DbUpdateException) { TempData["ErrorMessage"] = exception is DbUpdateException ? "Không thể lưu thông tin ảnh. Vui lòng thử lại." : exception.Message; }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteImage(int id, int imageId, CancellationToken cancellationToken)
    {
        if (!await _imageService.DeleteAsync("admin", id, imageId, isAdmin: true, cancellationToken)) return NotFound();
        TempData["SuccessMessage"] = "Ảnh đã được xóa.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [RequestSizeLimit(315_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 315_000_000)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadVideos(int id, List<IFormFile> videos, CancellationToken cancellationToken)
    {
        if (videos.Count == 0) TempData["ErrorMessage"] = "Chọn ít nhất một video để tải lên.";
        else
        {
            try
            {
                await _videoService.UploadAsync("admin", id, videos.Select(file => new ListingVideoUpload(file.OpenReadStream(), file.Length, file.FileName, file.ContentType)).ToArray(), isAdmin: true, cancellationToken: cancellationToken);
                TempData["SuccessMessage"] = "Video đã được tải lên.";
            }
            catch (KeyNotFoundException) { return NotFound(); }
            catch (Exception exception) when (exception is ArgumentException or IOException or DbUpdateException) { TempData["ErrorMessage"] = exception is DbUpdateException ? "Không thể lưu thông tin video. Vui lòng thử lại." : exception.Message; }
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteVideo(int id, int videoId, CancellationToken cancellationToken)
    {
        if (!await _videoService.DeleteAsync("admin", id, videoId, isAdmin: true, cancellationToken)) return NotFound();
        TempData["SuccessMessage"] = "Video đã được xóa.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetPrimaryImage(int id, int imageId, CancellationToken cancellationToken)
    {
        if (!await _imageService.SetPrimaryAsync("admin", id, imageId, isAdmin: true, cancellationToken)) return NotFound();
        TempData["SuccessMessage"] = "Đã cập nhật ảnh đại diện.";
        return RedirectToAction(nameof(Details), new { id });
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
