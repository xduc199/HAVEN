using Haven.Application.Listings;
using Haven.Web.Models.Listings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Haven.Web.Controllers;

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/admin/moderation")]
public sealed class AdminModerationController : ControllerBase
{
    private readonly ListingApplicationService _listingService;

    public AdminModerationController(ListingApplicationService listingService)
    {
        _listingService = listingService;
    }

    [HttpGet("pending")]
    [ProducesResponseType<ModerationListingPage>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ModerationListingPage>> GetPending(
        [FromQuery] ModerationQueueRequest request,
        CancellationToken cancellationToken)
    {
        var page = await _listingService.GetForAdminAsync(
            Haven.Domain.Properties.ListingStatus.Pending,
            request.Page,
            request.PageSize,
            cancellationToken);
        return Ok(page);
    }

    [HttpPost("{id:int}/approve")]
    [ProducesResponseType<ModerationListingDetails>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ModerationListingDetails>> Approve(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        try
        {
            var listing = await _listingService.ApproveAsync(id, cancellationToken);
            return listing is null
                ? NotFound()
                : Ok(ModerationListingDetails.From(listing));
        }
        catch (InvalidOperationException exception)
        {
            return StateConflict(exception.Message);
        }
    }

    [HttpPost("{id:int}/reject")]
    [ProducesResponseType<ModerationListingDetails>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ModerationListingDetails>> Reject(
        int id,
        ModerationRejectRequest request,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        try
        {
            var listing = await _listingService.RejectAsync(
                id,
                request.Reason,
                cancellationToken);
            return listing is null
                ? NotFound()
                : Ok(ModerationListingDetails.From(listing));
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError("reason", exception.Message);
            return ValidationProblem(ModelState);
        }
        catch (InvalidOperationException exception)
        {
            return StateConflict(exception.Message);
        }
    }

    [HttpPost("{id:int}/hide")]
    [ProducesResponseType<ModerationListingDetails>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ModerationListingDetails>> Hide(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        try
        {
            var listing = await _listingService.HideAsync(id, cancellationToken);
            return listing is null
                ? NotFound()
                : Ok(ModerationListingDetails.From(listing));
        }
        catch (InvalidOperationException exception)
        {
            return StateConflict(exception.Message);
        }
    }

    private ConflictObjectResult StateConflict(string message) =>
        Conflict(new ProblemDetails
        {
            Title = "The listing cannot be moderated in its current state.",
            Detail = message,
            Status = StatusCodes.Status409Conflict
        });
}
