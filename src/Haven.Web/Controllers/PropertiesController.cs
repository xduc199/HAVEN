using System.Security.Claims;
using Haven.Application.Favorites;
using Haven.Application.Listings;
using Haven.Application.PropertyTypes;
using Haven.Domain.Properties;
using Haven.Web.ViewModels.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Haven.Web.Controllers;

[Route("properties")]
public sealed class PropertiesController : Controller
{
    private readonly ListingApplicationService _listingService;
    private readonly PropertyTypeApplicationService _propertyTypeService;
    private readonly FavoriteApplicationService _favoriteService;

    public PropertiesController(
        ListingApplicationService listingService,
        PropertyTypeApplicationService propertyTypeService,
        FavoriteApplicationService favoriteService)
    {
        _listingService = listingService;
        _propertyTypeService = propertyTypeService;
        _favoriteService = favoriteService;
    }

    [AllowAnonymous]
    [HttpGet("/Property")]
    public IActionResult LegacyPropertyRoute(string? location = null) =>
        RedirectToAction(
            nameof(Index),
            new { province = location });

    [HttpGet("")]
    public async Task<IActionResult> Index(
        [FromQuery] PropertySearchViewModel model,
        CancellationToken cancellationToken)
    {
        model.PropertyTypes = await GetPropertyTypeOptionsAsync(cancellationToken);
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await _listingService.SearchPublicAsync(
                new ListingSearchCriteria(
                    model.Keyword,
                    model.PropertyTypeId,
                    model.Purpose,
                    model.Province,
                    model.District,
                    model.MinimumPrice,
                    model.MaximumPrice,
                    model.MinimumAreaSqm,
                    model.MaximumAreaSqm,
                    model.Bedrooms,
                    model.Sort,
                    model.Page,
                    12),
                cancellationToken);

            model.Items = MapCards(result.Items, model.PropertyTypes);
            model.TotalCount = result.TotalCount;
            model.TotalPages = result.TotalPages;
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
        }

        return View(model);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return NotFound();
        }

        var listing = await _listingService.GetPublicDetailsAsync(
            id,
            cancellationToken);
        if (listing is null)
        {
            return NotFound();
        }

        var isFavorite = false;
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is not null)
        {
            isFavorite = await _favoriteService.IsFavoriteAsync(
                userId,
                id,
                cancellationToken);
        }

        return View(new PropertyDetailsViewModel(
            listing,
            isFavorite,
            await GetPropertyTypeOptionsAsync(cancellationToken)));
    }

    [AllowAnonymous]
    [HttpGet("/compare")]
    public async Task<IActionResult> Compare(
        [FromQuery] int[] ids,
        CancellationToken cancellationToken)
    {
        var distinctIds = ids
            .Where(id => id > 0)
            .Distinct()
            .ToArray();
        if (ids.Any(id => id <= 0))
        {
            ModelState.AddModelError(
                string.Empty,
                "Listing IDs must be positive integers.");
        }

        if (distinctIds.Length != ids.Length)
        {
            ModelState.AddModelError(
                string.Empty,
                "Duplicate listings were removed from the comparison.");
        }

        if (distinctIds.Length > 3)
        {
            ModelState.AddModelError(
                string.Empty,
                "Compare up to three listings at a time.");
            distinctIds = distinctIds.Take(3).ToArray();
        }

        if (distinctIds.Length == 0)
        {
            return View(new ComparePageViewModel([], []));
        }

        var listings = await _listingService.ComparePublicAsync(
            distinctIds,
            cancellationToken);
        if (listings is null)
        {
            return NotFound();
        }

        return View(new ComparePageViewModel(listings, distinctIds));
    }

    private async Task<IReadOnlyList<PropertyTypeOptionViewModel>>
        GetPropertyTypeOptionsAsync(CancellationToken cancellationToken)
    {
        var propertyTypes = await _propertyTypeService.GetActiveAsync(
            cancellationToken);
        return propertyTypes
            .Select(type => new PropertyTypeOptionViewModel(type.Id, type.Name))
            .ToArray();
    }

    private static IReadOnlyList<PropertyCardViewModel> MapCards(
        IReadOnlyList<PublicListingSummary> items,
        IReadOnlyList<PropertyTypeOptionViewModel> propertyTypes)
    {
        var names = propertyTypes.ToDictionary(type => type.Id, type => type.Name);
        return items.Select(item => new PropertyCardViewModel(
            item.Id,
            item.Title,
            item.Purpose,
            item.PriceAmount,
            item.AreaSqm,
            names.GetValueOrDefault(item.PropertyTypeId, "Bất động sản"),
            item.Province,
            item.District,
            item.Bedrooms)).ToArray();
    }
}
