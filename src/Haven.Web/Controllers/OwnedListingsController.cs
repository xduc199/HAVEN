using System.Security.Claims;
using Haven.Application.Listings;
using Haven.Domain.Properties;
using Haven.Web.Models.Listings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Haven.Web.Controllers;

[ApiController]
[Authorize]
[Route("api/my/listings")]
public sealed class OwnedListingsController : ControllerBase
{
    private const string OwnedListingRouteName = "GetOwnedListing";

    private readonly ListingApplicationService _listingService;

    public OwnedListingsController(ListingApplicationService listingService)
    {
        _listingService = listingService;
    }

    [HttpPost]
    [ProducesResponseType<OwnedListingDetails>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OwnedListingDetails>> Create(
        ListingWriteRequest request,
        CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null)
        {
            return Unauthorized();
        }

        try
        {
            var listing = await _listingService.CreateAsync(
                ownerId,
                new CreateListingCommand(
                    request.PropertyTypeId,
                    request.Title,
                    request.Description,
                    request.Purpose,
                    request.PriceAmount,
                    request.AreaSqm,
                    request.Province,
                    request.District,
                    request.AddressLine,
                    request.Bedrooms,
                    request.Bathrooms),
                cancellationToken);

            var response = OwnedListingDetails.From(listing);
            return CreatedAtRoute(
                OwnedListingRouteName,
                new { id = listing.Id },
                response);
        }
        catch (ArgumentException exception)
        {
            return RequestValidationProblem(exception.Message);
        }
    }

    [HttpGet]
    [ProducesResponseType<OwnedListingPage>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OwnedListingPage>> GetMine(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null)
        {
            return Unauthorized();
        }

        try
        {
            return Ok(await _listingService.GetOwnedPageAsync(
                ownerId,
                page,
                pageSize,
                cancellationToken));
        }
        catch (ArgumentOutOfRangeException exception)
        {
            ModelState.AddModelError("query", exception.Message);
            return ValidationProblem(ModelState);
        }
    }

    [HttpGet("{id:int}", Name = OwnedListingRouteName)]
    [ProducesResponseType<OwnedListingDetails>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OwnedListingDetails>> GetOwned(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        var ownerId = GetOwnerId();
        if (ownerId is null)
        {
            return Unauthorized();
        }

        var listing = await _listingService.GetOwnedAsync(
            ownerId,
            id,
            cancellationToken);

        return listing is null
            ? NotFound()
            : Ok(OwnedListingDetails.From(listing));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<OwnedListingDetails>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OwnedListingDetails>> Update(
        int id,
        ListingWriteRequest request,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        var ownerId = GetOwnerId();
        if (ownerId is null)
        {
            return Unauthorized();
        }

        try
        {
            var listing = await _listingService.UpdateAsync(
                ownerId,
                id,
                new UpdateListingCommand(
                    request.PropertyTypeId,
                    request.Title,
                    request.Description,
                    request.Purpose,
                    request.PriceAmount,
                    request.AreaSqm,
                    request.Province,
                    request.District,
                    request.AddressLine,
                    request.Bedrooms,
                    request.Bathrooms),
                cancellationToken);

            return listing is null
                ? NotFound()
                : Ok(OwnedListingDetails.From(listing));
        }
        catch (ArgumentException exception)
        {
            return RequestValidationProblem(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return StateConflict(exception.Message);
        }
    }

    [HttpPost("{id:int}/submit")]
    [ProducesResponseType<OwnedListingDetails>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OwnedListingDetails>> SubmitForReview(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        var ownerId = GetOwnerId();
        if (ownerId is null)
        {
            return Unauthorized();
        }

        try
        {
            var listing = await _listingService.SubmitForReviewAsync(
                ownerId,
                id,
                cancellationToken);

            return listing is null
                ? NotFound()
                : Ok(OwnedListingDetails.From(listing));
        }
        catch (InvalidOperationException exception)
        {
            return StateConflict(exception.Message);
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        var ownerId = GetOwnerId();
        if (ownerId is null)
        {
            return Unauthorized();
        }

        try
        {
            var deleted = await _listingService.DeleteOwnedAsync(
                ownerId,
                id,
                cancellationToken);
            return deleted ? NoContent() : NotFound();
        }
        catch (InvalidOperationException exception)
        {
            return StateConflict(exception.Message);
        }
    }

    private string? GetOwnerId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier);

    private ActionResult RequestValidationProblem(string message)
    {
        ModelState.AddModelError("request", message);
        return ValidationProblem(ModelState);
    }

    private ConflictObjectResult StateConflict(string message) =>
        Conflict(new ProblemDetails
        {
            Title = "The listing cannot be changed in its current state.",
            Detail = message,
            Status = StatusCodes.Status409Conflict
        });
}
