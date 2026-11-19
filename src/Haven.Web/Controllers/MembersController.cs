using Haven.Application.Listings;
using Haven.Infrastructure.Identity;
using Haven.Web.ViewModels;
using Haven.Web.ViewModels.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Haven.Web.Controllers;

[AllowAnonymous]
[Route("members")]
public sealed class MembersController(UserManager<HavenUser> userManager, ListingApplicationService listingService, ListingImageApplicationService imageService) : Controller
{
    private const int PageSize = 12;

    [HttpGet("{id}", Name = "PublicMemberProfile")]
    public async Task<IActionResult> Profile(string id, int page = 1, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id) || id.Length > 450 || page < 1) return NotFound();
        var user = await userManager.Users.Where(candidate => candidate.Id == id)
            .Select(candidate => new { candidate.Id, candidate.DisplayName, candidate.AvatarPath })
            .SingleOrDefaultAsync(cancellationToken);
        if (user is null) return NotFound();

        var result = await listingService.GetApprovedForOwnerAsync(user.Id, page, PageSize, cancellationToken);
        var actualPage = result.TotalCount == 0 ? 1 : Math.Min(page, (int)((result.TotalCount + PageSize - 1) / PageSize));
        if (actualPage != page)
            result = await listingService.GetApprovedForOwnerAsync(user.Id, actualPage, PageSize, cancellationToken);
        ViewData["Title"] = user.DisplayName;
        var cards = new List<PropertyCardViewModel>(result.Listings.Count);
        foreach (var listing in result.Listings)
        {
            var images = await imageService.GetPublicImagesAsync(listing.Id, cancellationToken);
            cards.Add(new PropertyCardViewModel(
                listing.Id, listing.Title, listing.Purpose, listing.PriceAmount, listing.AreaSqm,
                listing.PropertyTypeName, listing.Province, listing.District, listing.Bedrooms,
                images.FirstOrDefault()?.Id));
        }
        return View(new PublicProfileViewModel(user.DisplayName, user.AvatarPath, cards, actualPage, PageSize, result.TotalCount));
    }
}
