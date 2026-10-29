using Haven.Application.Listings;
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
    [ProducesResponseType<ListingSearchResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ListingSearchResult>> Search(
        [FromQuery] ListingSearchCriteria criteria,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _listingService.SearchPublicAsync(
                criteria,
                cancellationToken);

            return Ok(result);
        }
        catch (ListingSearchValidationException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid listing search criteria",
                Detail = exception.Message
            });
        }
    }
}
