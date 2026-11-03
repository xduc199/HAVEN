using Haven.Application.Tags;
using Haven.Web.Models.Tags;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Haven.Web.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "AdminOnly")]
public sealed class AdminTagsController(TagApplicationService tagService) : ControllerBase
{
    [HttpGet("tags")]
    [ProducesResponseType<TagPageResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<TagPageResponse>> GetTags(
        [FromQuery, StringLength(80)] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await tagService.GetAdminPageAsync(
                search, page, pageSize, cancellationToken);
            return Ok(new TagPageResponse(
                result.Items.Select(TagResponse.From).ToArray(),
                result.Page,
                result.PageSize,
                result.TotalCount,
                result.TotalPages));
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return BadRequest(new ProblemDetails { Title = exception.Message });
        }
    }

    [HttpGet("tags/{id:int}")]
    [ProducesResponseType<TagResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TagResponse>> GetTag(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        var tag = await tagService.GetByIdAsync(id, cancellationToken);
        return tag is null ? NotFound() : Ok(TagResponse.From(tag));
    }

    [HttpPost("tags")]
    [ProducesResponseType<TagResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TagResponse>> CreateTag(
        CreateTagRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var tag = await tagService.CreateAsync(
                request.Name, request.Description, cancellationToken);
            var response = TagResponse.From(tag);
            return CreatedAtAction(nameof(GetTag), new { id = tag.Id }, response);
        }
        catch (DuplicateTagException exception)
        {
            return Conflict(new ProblemDetails { Title = exception.Message });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = exception.Message });
        }
    }

    [HttpPut("tags/{id:int}")]
    [ProducesResponseType<TagResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TagResponse>> UpdateTag(
        int id,
        UpdateTagRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var tag = await tagService.UpdateAsync(
                id,
                request.Name,
                request.Description,
                request.IsActive,
                cancellationToken);
            return tag is null ? NotFound() : Ok(TagResponse.From(tag));
        }
        catch (DuplicateTagException exception)
        {
            return Conflict(new ProblemDetails { Title = exception.Message });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = exception.Message });
        }
    }

    [HttpGet("listings/{listingId:int}/tags")]
    [ProducesResponseType<IReadOnlyList<TagResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TagResponse>>> GetListingTags(
        int listingId,
        CancellationToken cancellationToken)
    {
        try
        {
            var tags = await tagService.GetForListingAsync(listingId, cancellationToken);
            return Ok(tags.Select(TagResponse.From).ToArray());
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return BadRequest(new ProblemDetails { Title = exception.Message });
        }
    }

    [HttpPost("listings/{listingId:int}/tags/{tagId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AssignTag(
        int listingId,
        int tagId,
        CancellationToken cancellationToken)
    {
        try
        {
            return await tagService.AssignToListingAsync(
                listingId, tagId, cancellationToken) switch
            {
                TagAssignmentResult.Added => NoContent(),
                TagAssignmentResult.AlreadyAssigned => Conflict(),
                TagAssignmentResult.ListingNotFound => NotFound(),
                TagAssignmentResult.TagNotFound => NotFound(),
                TagAssignmentResult.TagInactive => Conflict(new ProblemDetails
                {
                    Title = "Inactive tags cannot be assigned."
                }),
                _ => Problem()
            };
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return BadRequest(new ProblemDetails { Title = exception.Message });
        }
    }

    [HttpDelete("listings/{listingId:int}/tags/{tagId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveTag(
        int listingId,
        int tagId,
        CancellationToken cancellationToken)
    {
        try
        {
            return await tagService.RemoveFromListingAsync(
                listingId, tagId, cancellationToken)
                ? NoContent()
                : NotFound();
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return BadRequest(new ProblemDetails { Title = exception.Message });
        }
    }
}
