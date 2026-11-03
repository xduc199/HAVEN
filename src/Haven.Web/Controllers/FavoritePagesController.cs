using System.Security.Claims;
using Haven.Application.Favorites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Haven.Web.Controllers;

[Authorize]
[Route("favorites")]
public sealed class FavoritePagesController : Controller
{
    private readonly FavoriteApplicationService _favoriteService;

    public FavoritePagesController(FavoriteApplicationService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(
        [FromQuery] int page = 1,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return View(await _favoriteService.GetForUserAsync(
                GetUserId(),
                page,
                20,
                cancellationToken));
        }
        catch (ArgumentOutOfRangeException)
        {
            return BadRequest();
        }
    }

    [HttpPost("{listingId:int}/add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(
        int listingId,
        string? returnUrl,
        CancellationToken cancellationToken)
    {
        var result = await _favoriteService.AddAsync(
            GetUserId(),
            listingId,
            cancellationToken);

        TempData[result == AddFavoriteResult.Added
            ? "SuccessMessage"
            : "ErrorMessage"] = result switch
        {
            AddFavoriteResult.Added => "Đã thêm vào danh sách yêu thích.",
            AddFavoriteResult.AlreadyExists => "Tin đã có trong danh sách yêu thích.",
            _ => "Tin không còn được công khai để thêm vào yêu thích."
        };
        return SafeRedirect(returnUrl, listingId);
    }

    [HttpPost("{listingId:int}/remove")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(
        int listingId,
        string? returnUrl,
        CancellationToken cancellationToken)
    {
        var removed = await _favoriteService.RemoveAsync(
            GetUserId(),
            listingId,
            cancellationToken);
        TempData[removed ? "SuccessMessage" : "ErrorMessage"] = removed
            ? "Đã xóa khỏi danh sách yêu thích."
            : "Không tìm thấy tin trong danh sách yêu thích.";
        return SafeRedirect(returnUrl, listingId);
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("The authenticated user has no identifier.");

    private IActionResult SafeRedirect(string? returnUrl, int listingId) =>
        !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction(
                "Details",
                "Properties",
                new { id = listingId });
}
