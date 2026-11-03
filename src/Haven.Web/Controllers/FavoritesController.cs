using System.Security.Claims;
using Haven.Application.Favorites;
using Haven.Web.Models.Favorites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Haven.Web.Controllers;

[ApiController]
[Authorize]
[Route("api/favorites")]
public sealed class FavoritesController : ControllerBase
{
    private readonly FavoriteApplicationService _favoriteService;

    public FavoritesController(FavoriteApplicationService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    [HttpGet]
    [ProducesResponseType<FavoritePage>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<FavoritePage>> Get(
        [FromQuery] FavoritePageRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var page = await _favoriteService.GetForUserAsync(
                userId,
                request.Page,
                request.PageSize,
                cancellationToken);
            return Ok(page);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            ModelState.AddModelError("query", exception.Message);
            return ValidationProblem(ModelState);
        }
    }

    [HttpPost("{listingId:int}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Add(
        int listingId,
        CancellationToken cancellationToken)
    {
        if (listingId <= 0)
        {
            return BadRequest();
        }

        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _favoriteService.AddAsync(
            userId,
            listingId,
            cancellationToken);

        return result switch
        {
            AddFavoriteResult.Added => StatusCode(StatusCodes.Status201Created),
            AddFavoriteResult.AlreadyExists => Conflict(new ProblemDetails
            {
                Title = "The listing is already in favorites.",
                Status = StatusCodes.Status409Conflict
            }),
            _ => NotFound()
        };
    }

    [HttpDelete("{listingId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(
        int listingId,
        CancellationToken cancellationToken)
    {
        if (listingId <= 0)
        {
            return BadRequest();
        }

        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var removed = await _favoriteService.RemoveAsync(
            userId,
            listingId,
            cancellationToken);
        return removed ? NoContent() : NotFound();
    }

    private string? GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier);
}
