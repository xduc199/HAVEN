using System.Security.Claims;
using Haven.Application.Consultations;
using Haven.Application.Listings;
using Haven.Domain.Consultations;
using Haven.Infrastructure.Identity;
using Haven.Web.ViewModels.Consultations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Haven.Web.Controllers;

[Authorize]
[Route("consultations")]
public sealed class ConsultationPagesController : Controller
{
    private readonly ConsultationApplicationService _consultationService;
    private readonly ListingApplicationService _listingService;
    private readonly UserManager<HavenUser> _userManager;

    public ConsultationPagesController(
        ConsultationApplicationService consultationService,
        ListingApplicationService listingService,
        UserManager<HavenUser> userManager)
    {
        _consultationService = consultationService;
        _listingService = listingService;
        _userManager = userManager;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(
        [FromQuery] int page = 1,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return View(await _consultationService.GetForRequesterAsync(
                GetRequesterId(),
                page,
                20,
                cancellationToken));
        }
        catch (ArgumentOutOfRangeException)
        {
            return BadRequest();
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(
        int id,
        CancellationToken cancellationToken)
    {
        var consultation = await _consultationService.GetForRequesterAsync(
            GetRequesterId(),
            id,
            cancellationToken);
        return consultation is null ? NotFound() : View(consultation);
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create(
        [FromQuery] int? listingId,
        CancellationToken cancellationToken)
    {
        string? listingTitle = null;
        if (listingId is { } id)
        {
            var listing = await _listingService.GetPublicDetailsAsync(
                id,
                cancellationToken);
            if (listing is null)
            {
                return NotFound();
            }

            listingTitle = listing.Title;
        }

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        return View(new CreateConsultationViewModel
        {
            ContactName = user.DisplayName,
            Email = user.Email ?? string.Empty,
            ListingId = listingId,
            ListingTitle = listingTitle
        });
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateConsultationViewModel model,
        CancellationToken cancellationToken)
    {
        if (model.ListingId is { } listingId)
        {
            var listing = await _listingService.GetPublicDetailsAsync(
                listingId,
                cancellationToken);
            if (listing is null)
            {
                ModelState.AddModelError(
                    nameof(model.ListingId),
                    "Tin không còn được công khai.");
            }
            else
            {
                model.ListingTitle = listing.Title;
            }
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await _consultationService.CreateAsync(
                GetRequesterId(),
                new CreateConsultationCommand(
                    model.ContactName,
                    model.Email,
                    model.PhoneNumber,
                    model.ListingId,
                    model.Message),
                cancellationToken);
            TempData["SuccessMessage"] = "Yêu cầu tư vấn đã được gửi.";
            return RedirectToAction(nameof(Details), new { id = result.Id });
        }
        catch (ConsultationListingNotFoundException)
        {
            ModelState.AddModelError(
                nameof(model.ListingId),
                "Tin không còn được công khai.");
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
        }

        return View(model);
    }

    private string GetRequesterId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("The authenticated user has no identifier.");
}
