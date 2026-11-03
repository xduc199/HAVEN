using Haven.Application.Tags;
using Haven.Web.Models.Tags;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Haven.Web.Controllers;

[ApiController]
[Route("api/tags")]
public sealed class TagsController(
    TagApplicationService tagService) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType<TagPageResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<TagPageResponse>> GetPublic(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await tagService.GetPublicPageAsync(page, pageSize, cancellationToken);
            return Ok(ToResponse(result));
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return BadRequest(new ProblemDetails { Title = exception.Message });
        }
    }

    [AllowAnonymous]
    [HttpGet("{slug}/listings")]
    [ProducesResponseType<PublicTagListingPageResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicTagListingPageResponse>> GetApprovedListings(
        string slug,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var tag = await tagService.GetBySlugAsync(slug, cancellationToken);
        if (tag is null)
        {
            return NotFound();
        }

        try
        {
            var result = await tagService.GetListingsAsync(
                tag.Id, page, pageSize, cancellationToken);
            return Ok(new PublicTagListingPageResponse(
                result.Items.Select(item => new PublicTagListingResponse(
                    item.Id,
                    item.PropertyTypeId,
                    item.Title,
                    item.Purpose,
                    item.PriceAmount,
                    item.AreaSqm,
                    item.Province,
                    item.District,
                    item.Bedrooms,
                    item.CreatedAtUtc)).ToArray(),
                result.Page,
                result.PageSize,
                result.TotalCount,
                result.TotalPages));
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return BadRequest(new ProblemDetails { Title = exception.Message });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = exception.Message });
        }
    }

    private static TagPageResponse ToResponse(TagPage page) =>
        new(page.Items.Select(TagResponse.From).ToArray(),
            page.Page, page.PageSize, page.TotalCount, page.TotalPages);
}
