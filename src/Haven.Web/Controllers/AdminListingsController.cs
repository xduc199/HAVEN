using Haven.Application.Listings;
using Haven.Domain.Properties;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Haven.Web.Controllers;

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/admin/listings")]
public sealed class AdminListingsController : ControllerBase
{
    private readonly ListingApplicationService _listingService;

    public AdminListingsController(ListingApplicationService listingService)
    {
        _listingService = listingService;
    }

    [HttpGet]
    [ProducesResponseType<ModerationListingPage>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ModerationListingPage>> GetAll(
        [FromQuery] ListingStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery, StringLength(200)] string? search = null,
        [FromQuery] int? propertyTypeId = null,
        [FromQuery] int? tagId = null,
        [FromQuery] DateOnly? fromDate = null,
        [FromQuery] DateOnly? toDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (fromDate.HasValue != toDate.HasValue ||
                fromDate is { } from && toDate is { } to && to < from)
            {
                ModelState.AddModelError(
                    "dateRange",
                    "Provide both dates and ensure the end date is not before the start date.");
                return ValidationProblem(ModelState);
            }

            var fromUtc = fromDate?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            DateTime? toUtcExclusive = toDate is { } endDate && endDate < DateOnly.MaxValue
                ? endDate.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
                : null;
            return Ok(await _listingService.SearchForAdminAsync(
                new AdminListingCriteria(
                    search,
                    status,
                    propertyTypeId,
                    tagId,
                    fromUtc.HasValue ? new DateTimeOffset(fromUtc.Value) : null,
                    toUtcExclusive.HasValue ? new DateTimeOffset(toUtcExclusive.Value) : null,
                    page,
                    pageSize),
                cancellationToken));
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError("query", exception.Message);
            return ValidationProblem(ModelState);
        }
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<ModerationListingDetails>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ModerationListingDetails>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        var listing = await _listingService.GetForAdminAsync(
            id,
            cancellationToken);
        return listing is null ? NotFound() : Ok(listing);
    }
}
