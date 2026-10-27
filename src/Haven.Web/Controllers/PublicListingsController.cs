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
