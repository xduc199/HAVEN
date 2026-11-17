using System.Security.Claims;
using Haven.Application.Listings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Haven.Web.Controllers;

[Route("listing-images")]
public sealed class ListingImagesController(ListingImageApplicationService imageService) : Controller
{
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
    {
        if (id <= 0) return NotFound();
        ListingImageContent? content;
        try
        {
            content = await imageService.OpenAsync(id, User.FindFirstValue(ClaimTypes.NameIdentifier), User.IsInRole("Admin"), cancellationToken);
        }
        catch (FileNotFoundException) { return NotFound(); }
        if (content is null) return NotFound();
        Response.Headers.XContentTypeOptions = "nosniff";
        Response.Headers.CacheControl = "private, no-store";
        return File(content.Content, content.ContentType, enableRangeProcessing: true);
    }
}

[Route("listing-videos")]
public sealed class ListingVideosController(ListingVideoApplicationService videoService) : Controller
{
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
    {
        if (id <= 0) return NotFound();
        ListingVideoContent? content;
        try
        {
            content = await videoService.OpenAsync(id, User.FindFirstValue(ClaimTypes.NameIdentifier), User.IsInRole("Admin"), cancellationToken);
        }
        catch (FileNotFoundException) { return NotFound(); }
        if (content is null) return NotFound();
        Response.Headers.XContentTypeOptions = "nosniff";
        Response.Headers.CacheControl = "private, no-store";
        return File(content.Content, content.ContentType, enableRangeProcessing: true);
    }
}
