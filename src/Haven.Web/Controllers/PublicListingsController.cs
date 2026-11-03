using Haven.Application.Listings;
using Haven.Web.Models.Listings;
using Microsoft.AspNetCore.Mvc;

namespace Haven.Web.Controllers;

[ApiController]
[Route("api/listings")]
public sealed class PublicListingsController : ControllerBase
{
    private readonly ListingApplicationService _listingService;

    public PublicListingsController(ListingApplicationService listingService)
    {
        _listingService = listingService;
    }

    [HttpGet]
    [ProducesResponseType<PublicListingSearchResults>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PublicListingSearchResults>> Search(
        [FromQuery] PublicListingSearchRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var results = await _listingService.SearchPublicAsync(
                new ListingSearchCriteria(
                    request.Keyword,
                    request.PropertyTypeId,
                    request.Purpose,
                    request.Province,
                    request.District,
                    request.MinimumPrice,
                    request.MaximumPrice,
                    request.MinimumAreaSqm,
                    request.MaximumAreaSqm,
                    request.Bedrooms,
                    request.Sort,
                    request.Page,
                    request.PageSize),
                cancellationToken);

            return Ok(results);
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError("query", exception.Message);
            return ValidationProblem(ModelState);
        }
    }

    [HttpGet("compare")]
    [ProducesResponseType<IReadOnlyList<PublicListingDetails>>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<PublicListingDetails>>> Compare(
        [FromQuery] ListingCompareRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var listings = await _listingService.ComparePublicAsync(
                request.Ids,
                cancellationToken);

            return listings is null ? NotFound() : Ok(listings);
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError("ids", exception.Message);
            return ValidationProblem(ModelState);
        }
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<PublicListingDetails>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicListingDetails>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        var listing = await _listingService.GetPublicDetailsAsync(
            id,
            cancellationToken);

        return listing is null ? NotFound() : Ok(listing);
    }
}
