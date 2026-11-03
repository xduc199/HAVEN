using System.Security.Claims;
using Haven.Application.Listings;
using Haven.Application.PropertyTypes;
using Haven.Domain.Properties;
using Haven.Web.ViewModels.Listings;
using Haven.Web.ViewModels.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Haven.Web.Controllers;

[Authorize]
[Route("profile/listings")]
public sealed class MyListingsController : Controller
{
    private readonly ListingApplicationService _listingService;
    private readonly PropertyTypeApplicationService _propertyTypeService;

    public MyListingsController(
        ListingApplicationService listingService,
        PropertyTypeApplicationService propertyTypeService)
    {
        _listingService = listingService;
        _propertyTypeService = propertyTypeService;
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
            PropertyTypes = await GetPropertyTypesAsync(cancellationToken)
        });

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        ListingFormViewModel model,
        CancellationToken cancellationToken)
    {
        model.PropertyTypes = await GetPropertyTypesAsync(cancellationToken);
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
            TempData["SuccessMessage"] = "Tin đăng nháp đã được tạo.";
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
        return listing is null ? NotFound() : View(OwnedListingDetails.From(listing));
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
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        ListingFormViewModel model,
        CancellationToken cancellationToken)
    {
        model.PropertyTypes = await GetPropertyTypesAsync(cancellationToken);
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
            var deleted = await _listingService.DeleteOwnedAsync(
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
        CancellationToken cancellationToken) =>
        new()
        {
            PropertyTypeId = listing.PropertyTypeId,
            Title = listing.Title,
            Description = listing.Description,
            Purpose = listing.Purpose,
            PriceAmount = listing.PriceAmount,
            AreaSqm = listing.AreaSqm,
            Province = listing.Province,
            District = listing.District,
            AddressLine = listing.AddressLine,
            Bedrooms = listing.Bedrooms,
            Bathrooms = listing.Bathrooms,
            PropertyTypes = await GetPropertyTypesAsync(cancellationToken)
        };

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
            model.Bathrooms);

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
            model.Bathrooms);
}
