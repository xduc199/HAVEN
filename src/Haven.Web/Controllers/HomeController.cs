using System.Diagnostics;
using Haven.Application.Listings;
using Haven.Application.PropertyTypes;
using Microsoft.AspNetCore.Mvc;
using Haven.Web.Models;
using Haven.Web.ViewModels.Properties;

namespace Haven.Web.Controllers;

public class HomeController : Controller
{
    private readonly ListingApplicationService _listingService;
    private readonly PropertyTypeApplicationService _propertyTypeService;

    public HomeController(
        ListingApplicationService listingService,
        PropertyTypeApplicationService propertyTypeService)
    {
        _listingService = listingService;
        _propertyTypeService = propertyTypeService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var types = await _propertyTypeService.GetActiveAsync(cancellationToken);
        var results = await _listingService.SearchPublicAsync(
            new ListingSearchCriteria(
                null, null, null, null, null, null, null, null, null,
                null, ListingSearchSort.Newest, 1, 6),
            cancellationToken);
        var options = types
            .Select(type => new PropertyTypeOptionViewModel(type.Id, type.Name))
            .ToArray();
        var names = options.ToDictionary(type => type.Id, type => type.Name);
        var cards = results.Items.Select(item => new PropertyCardViewModel(
            item.Id,
            item.Title,
            item.Purpose,
            item.PriceAmount,
            item.AreaSqm,
            names.GetValueOrDefault(item.PropertyTypeId, "Bất động sản"),
            item.Province,
            item.District,
            item.Bedrooms)).ToArray();

        return View(new HomePageViewModel(cards, options));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Contact() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    public IActionResult MuaBan() =>
        RedirectToAction("Index", "Properties", new { purpose = "Sale" });

    public IActionResult ChoThue() =>
        RedirectToAction("Index", "Properties", new { purpose = "Rent" });

    public IActionResult DuAn() => View();

}
