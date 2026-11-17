using Haven.Application.Listings;
using Haven.Application.Locations;
using Haven.Application.PropertyTypes;
using Haven.Domain.Properties;
using Haven.Web.ViewModels.Listings;
using Haven.Web.ViewModels.Properties;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Haven.Web.Controllers;

[Authorize]
[Route("profile/listings")]
public sealed class MyListingsController : Controller
{
    private readonly ListingApplicationService _listingService;
    private readonly PropertyTypeApplicationService _propertyTypeService;
    private readonly ListingImageApplicationService _imageService;
    private readonly ListingVideoApplicationService _videoService;
    private readonly AdministrativeLocationApplicationService _locations;

    public MyListingsController(
        ListingApplicationService listingService,
        PropertyTypeApplicationService propertyTypeService,
        ListingImageApplicationService imageService,
        ListingVideoApplicationService videoService,
        AdministrativeLocationApplicationService locations)
    {
        _listingService = listingService;
        _propertyTypeService = propertyTypeService;
        _imageService = imageService;
        _videoService = videoService;
        _locations = locations;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(
        [FromQuery] int page = 1,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return View(await _listingService.GetOwnedPageAsync(
                GetOwnerId(),
                page,
                20,
                cancellationToken));
        }
        catch (ArgumentOutOfRangeException)
        {
            return BadRequest();
        }
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken) =>
        View("Form", new ListingFormViewModel
        {
            PropertyTypes = await GetPropertyTypesAsync(cancellationToken),
            Provinces = await _locations.GetProvincesAsync(cancellationToken)
        });

    [HttpPost("create")]
    [RequestSizeLimit(420_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 420_000_000)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        ListingFormViewModel model,
        CancellationToken cancellationToken)
    {
        model.IsEdit = false;
        model.PropertyTypes = await GetPropertyTypesAsync(cancellationToken);
        await ApplyLocationAsync(model, null, cancellationToken);
        if (model.PropertyTypes.Count == 0)
        {
            ModelState.AddModelError(
                nameof(model.PropertyTypeId),
                "Hiện chưa có loại bất động sản đang hoạt động. Vui lòng liên hệ Admin hoặc thử lại sau.");
        }

        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        try
        {
            var listing = await _listingService.CreateAsync(
                GetOwnerId(),
                ToCommand(model),
                cancellationToken);
            if (model.Images.Count > 0)
            {
                try
                {
                    await _imageService.UploadAsync(GetOwnerId(), listing.Id, ToImageUploads(model.Images), cancellationToken: cancellationToken);
                    TempData["SuccessMessage"] = "Tin đăng nháp và ảnh đã được lưu.";
                }
                catch (Exception exception) when (exception is ArgumentException or IOException or DbUpdateException)
                {
                    TempData["ErrorMessage"] = $"Tin nháp đã được tạo nhưng ảnh chưa tải lên: {exception.Message}";
                }
            }
            if (model.Videos.Count > 0)
            {
                try
                {
                    await _videoService.UploadAsync(GetOwnerId(), listing.Id, ToVideoUploads(model.Videos), cancellationToken: cancellationToken);
                    TempData["SuccessMessage"] = "Tin đăng nháp, hình ảnh và video đã được lưu.";
                }
                catch (Exception exception) when (exception is ArgumentException or IOException or DbUpdateException)
                {
                    TempData["ErrorMessage"] = $"Tin đăng đã được tạo nhưng video chưa tải lên: {exception.Message}";
                }
            }
            else if (model.Images.Count == 0) TempData["SuccessMessage"] = "Tin đăng nháp đã được tạo.";
            return RedirectToAction(nameof(Details), new { id = listing.Id });
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View("Form", model);
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(
        int id,
        CancellationToken cancellationToken)
    {
        var listing = await _listingService.GetOwnedAsync(
            GetOwnerId(),
            id,
            cancellationToken);
        if (listing is null) return NotFound();
        var images = await _imageService.GetOwnedImagesAsync(GetOwnerId(), id, cancellationToken);
        var videos = await _videoService.GetOwnedVideosAsync(GetOwnerId(), id, cancellationToken);
        return View(new OwnedListingDetailsPage(OwnedListingDetails.From(listing), images ?? [], videos ?? []));
    }

    [HttpPost("{id:int}/videos")]
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
                await _videoService.UploadAsync(GetOwnerId(), id, ToVideoUploads(videos), cancellationToken: cancellationToken);
                TempData["SuccessMessage"] = "Video đã được tải lên.";
            }
            catch (KeyNotFoundException) { return NotFound(); }
            catch (UnauthorizedAccessException) { return NotFound(); }
            catch (ArgumentException exception) { TempData["ErrorMessage"] = exception.Message; }
            catch (InvalidOperationException exception) { TempData["ErrorMessage"] = exception.Message; }
            catch (IOException) { TempData["ErrorMessage"] = "Không thể lưu video. Vui lòng thử lại."; }
            catch (DbUpdateException) { TempData["ErrorMessage"] = "Không thể lưu thông tin video. Vui lòng thử lại."; }
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("{id:int}/videos/{videoId:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteVideo(int id, int videoId, CancellationToken cancellationToken)
    {
        try
        {
            if (!await _videoService.DeleteAsync(GetOwnerId(), id, videoId, cancellationToken: cancellationToken)) return NotFound();
            TempData["SuccessMessage"] = "Video đã được xóa.";
        }
        catch (UnauthorizedAccessException) { return NotFound(); }
        catch (InvalidOperationException exception) { TempData["ErrorMessage"] = exception.Message; }
        catch (IOException) { TempData["ErrorMessage"] = "Không thể xóa tệp video khỏi bộ nhớ."; }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("{id:int}/images")]
    [RequestSizeLimit(105_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 105_000_000)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadImages(int id, List<IFormFile> images, CancellationToken cancellationToken)
    {
        if (images.Count == 0) TempData["ErrorMessage"] = "Chọn ít nhất một ảnh để tải lên.";
        else
        {
            try
            {
                var uploads = images.Select(file => new ListingImageUpload(file.OpenReadStream(), file.Length, file.FileName, file.ContentType)).ToArray();
                await _imageService.UploadAsync(GetOwnerId(), id, uploads, cancellationToken: cancellationToken);
                TempData["SuccessMessage"] = "Ảnh đã được tải lên.";
            }
            catch (KeyNotFoundException) { return NotFound(); }
            catch (UnauthorizedAccessException) { return NotFound(); }
            catch (ArgumentException exception) { TempData["ErrorMessage"] = exception.Message; }
            catch (InvalidOperationException exception) { TempData["ErrorMessage"] = exception.Message; }
            catch (IOException) { TempData["ErrorMessage"] = "Không thể lưu ảnh. Vui lòng thử lại."; }
            catch (DbUpdateException) { TempData["ErrorMessage"] = "Không thể lưu thông tin ảnh. Vui lòng thử lại."; }
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("{id:int}/images/{imageId:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteImage(int id, int imageId, CancellationToken cancellationToken)
    {
        try
        {
            if (!await _imageService.DeleteAsync(GetOwnerId(), id, imageId, cancellationToken: cancellationToken)) return NotFound();
            TempData["SuccessMessage"] = "Ảnh đã được xóa.";
        }
        catch (UnauthorizedAccessException) { return NotFound(); }
        catch (InvalidOperationException exception) { TempData["ErrorMessage"] = exception.Message; }
        catch (IOException) { TempData["ErrorMessage"] = "Không thể xóa tệp ảnh khỏi bộ nhớ."; }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("{id:int}/images/{imageId:int}/primary")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetPrimaryImage(int id, int imageId, CancellationToken cancellationToken)
    {
        try
        {
            if (!await _imageService.SetPrimaryAsync(GetOwnerId(), id, imageId, cancellationToken: cancellationToken)) return NotFound();
            TempData["SuccessMessage"] = "Đã cập nhật ảnh đại diện.";
        }
        catch (UnauthorizedAccessException) { return NotFound(); }
        catch (InvalidOperationException exception) { TempData["ErrorMessage"] = exception.Message; }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet("{id:int}/edit")]
    public async Task<IActionResult> Edit(
        int id,
        CancellationToken cancellationToken)
    {
        var listing = await _listingService.GetOwnedAsync(
            GetOwnerId(),
            id,
            cancellationToken);
        if (listing is null)
        {
            return NotFound();
        }

        if (listing.Status is not (
            ListingStatus.Draft or
            ListingStatus.Rejected or
            ListingStatus.Approved))
        {
            TempData["ErrorMessage"] = "Tin đang chờ duyệt hoặc đã ẩn nên không thể sửa.";
            return RedirectToAction(nameof(Details), new { id });
        }

        return View("Form", await ToFormModelAsync(listing, cancellationToken));
    }

    [HttpPost("{id:int}/edit")]
    [RequestSizeLimit(105_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 105_000_000)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        ListingFormViewModel model,
        CancellationToken cancellationToken)
    {
        var existingListing = await _listingService.GetOwnedAsync(GetOwnerId(), id, cancellationToken);
        if (existingListing is null) return NotFound();
        model.IsEdit = true;
        model.CurrentStatus = existingListing.Status;
        model.PropertyTypes = await GetPropertyTypesAsync(cancellationToken);
        await ApplyLocationAsync(model, existingListing, cancellationToken);
        model.ExistingImages = await _imageService.GetOwnedImagesAsync(GetOwnerId(), id, cancellationToken) ?? [];
        model.ExistingVideos = await _videoService.GetOwnedVideosAsync(GetOwnerId(), id, cancellationToken) ?? [];
        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        try
        {
            var listing = await _listingService.UpdateAsync(
                GetOwnerId(),
                id,
                ToUpdateCommand(model),
                cancellationToken);
            if (listing is null)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Tin đăng đã được cập nhật.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View("Form", model);
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View("Form", model);
        }
    }

    [HttpPost("{id:int}/submit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var listing = await _listingService.SubmitForReviewAsync(
                GetOwnerId(),
                id,
                cancellationToken);
            if (listing is null)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Tin đã được gửi để kiểm duyệt.";
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!await _videoService.DeleteOwnedVideosAsync(GetOwnerId(), id, cancellationToken)) return NotFound();
            var deleted = await _imageService.DeleteOwnedListingAsync(
                GetOwnerId(),
                id,
                cancellationToken);
            if (!deleted)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Tin nháp đã được xóa.";
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private string GetOwnerId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("The authenticated user has no identifier.");

    private async Task<IReadOnlyList<PropertyTypeOptionViewModel>>
        GetPropertyTypesAsync(CancellationToken cancellationToken)
    {
        var types = await _propertyTypeService.GetActiveAsync(cancellationToken);
        return types.Select(type => new PropertyTypeOptionViewModel(
            type.Id,
            type.Name)).ToArray();
    }

    private async Task<ListingFormViewModel> ToFormModelAsync(
        Haven.Domain.Properties.Listing listing,
        CancellationToken cancellationToken)
    {
        var model = new ListingFormViewModel
        {
            PropertyTypeId = listing.PropertyTypeId,
            Title = listing.Title,
            Description = listing.Description,
            Purpose = listing.Purpose,
            PriceAmount = listing.PriceAmount,
            AreaSqm = listing.AreaSqm,
            Province = listing.Province,
            District = listing.District,
            ProvinceCode = listing.ProvinceCode,
            WardCode = listing.WardCode,
            AddressLine = listing.AddressLine,
            Bedrooms = listing.Bedrooms,
            Bathrooms = listing.Bathrooms,
            PropertyTypes = await GetPropertyTypesAsync(cancellationToken),
            IsEdit = true,
            CurrentStatus = listing.Status,
            ExistingImages = await _imageService.GetOwnedImagesAsync(GetOwnerId(), listing.Id, cancellationToken) ?? [],
            ExistingVideos = await _videoService.GetOwnedVideosAsync(GetOwnerId(), listing.Id, cancellationToken) ?? []
        };
        model.Provinces = await _locations.GetProvincesAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(listing.ProvinceCode)) model.Wards = await _locations.GetWardsAsync(listing.ProvinceCode, cancellationToken);
        return model;
    }

    private static CreateListingCommand ToCommand(ListingFormViewModel model) =>
        new(
            model.PropertyTypeId,
            model.Title,
            model.Description,
            model.Purpose,
            model.PriceAmount,
            model.AreaSqm,
            model.Province,
            model.District,
            model.AddressLine,
            model.Bedrooms,
            model.Bathrooms,
            model.ProvinceCode,
            model.WardCode);

    private static UpdateListingCommand ToUpdateCommand(
        ListingFormViewModel model) =>
        new(
            model.PropertyTypeId,
            model.Title,
            model.Description,
            model.Purpose,
            model.PriceAmount,
            model.AreaSqm,
            model.Province,
            model.District,
            model.AddressLine,
            model.Bedrooms,
            model.Bathrooms,
            model.ProvinceCode,
            model.WardCode);

    private async Task ApplyLocationAsync(ListingFormViewModel model, Listing? existing, CancellationToken cancellationToken)
    {
        model.Provinces = await _locations.GetProvincesAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(model.ProvinceCode) && string.IsNullOrWhiteSpace(model.WardCode) && existing is { ProvinceCode: null, WardCode: null })
        {
            model.Province = existing.Province;
            model.District = existing.District;
            return;
        }
        if (string.IsNullOrWhiteSpace(model.ProvinceCode) || string.IsNullOrWhiteSpace(model.WardCode))
        {
            if (existing is null || existing.ProvinceCode is not null) ModelState.AddModelError(nameof(model.ProvinceCode), "Hãy chọn tỉnh/thành phố và xã/phường từ danh mục.");
            model.Province = existing?.Province ?? string.Empty;
            model.District = existing?.District ?? string.Empty;
            return;
        }

        try
        {
            var ward = await _locations.GetWardAsync(model.WardCode, cancellationToken);
            if (ward is null || ward.ProvinceCode != model.ProvinceCode)
            {
                ModelState.AddModelError(nameof(model.WardCode), "Xã/phường không thuộc tỉnh/thành phố đã chọn.");
                model.Wards = await _locations.GetWardsAsync(model.ProvinceCode, cancellationToken);
                model.Province = existing?.Province ?? string.Empty;
                model.District = existing?.District ?? string.Empty;
                return;
            }
            model.Province = ward.ProvinceName;
            model.District = ward.Name;
            model.Wards = await _locations.GetWardsAsync(model.ProvinceCode, cancellationToken);
        }
        catch (ArgumentException)
        {
            ModelState.AddModelError(nameof(model.ProvinceCode), "Mã tỉnh/thành phố hoặc xã/phường không hợp lệ.");
            model.Province = existing?.Province ?? string.Empty;
            model.District = existing?.District ?? string.Empty;
        }
    }

    private static IReadOnlyList<ListingImageUpload> ToImageUploads(IEnumerable<IFormFile> files) =>
        files.Select(file => new ListingImageUpload(file.OpenReadStream(), file.Length, file.FileName, file.ContentType)).ToArray();

    private static IReadOnlyList<ListingVideoUpload> ToVideoUploads(IEnumerable<IFormFile> files) =>
        files.Select(file => new ListingVideoUpload(file.OpenReadStream(), file.Length, file.FileName, file.ContentType)).ToArray();
}
