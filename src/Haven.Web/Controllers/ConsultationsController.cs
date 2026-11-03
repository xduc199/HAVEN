using System.Security.Claims;
using Haven.Application.Consultations;
using Haven.Web.Models.Consultations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Haven.Web.Controllers;

[ApiController]
[Authorize]
[Route("api/consultations")]
public sealed class ConsultationsController : ControllerBase
{
    private readonly ConsultationApplicationService _consultationService;

    public ConsultationsController(
        ConsultationApplicationService consultationService)
    {
        _consultationService = consultationService;
    }

    [HttpPost]
    [ProducesResponseType<ConsultationDetails>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConsultationDetails>> Create(
        CreateConsultationRequest request,
        CancellationToken cancellationToken)
    {
        var requesterId = GetRequesterId();
        if (requesterId is null)
        {
            return Unauthorized();
        }

        try
        {
            var result = await _consultationService.CreateAsync(
                requesterId,
                new CreateConsultationCommand(
                    request.ContactName,
                    request.Email,
                    request.PhoneNumber,
                    request.ListingId,
                    request.Message),
                cancellationToken);
            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                result);
        }
        catch (ConsultationListingNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError("request", exception.Message);
            return ValidationProblem(ModelState);
        }
    }

    [HttpGet]
    [ProducesResponseType<ConsultationPageResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ConsultationPageResult>> GetMine(
        [FromQuery] ConsultationListRequest request,
        CancellationToken cancellationToken)
    {
        var requesterId = GetRequesterId();
        if (requesterId is null)
        {
            return Unauthorized();
        }

        try
        {
            return Ok(await _consultationService.GetForRequesterAsync(
                requesterId,
                request.Page,
                request.PageSize,
                cancellationToken));
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError("query", exception.Message);
            return ValidationProblem(ModelState);
        }
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<ConsultationDetails>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConsultationDetails>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var requesterId = GetRequesterId();
        if (requesterId is null)
        {
            return Unauthorized();
        }

        if (id <= 0)
        {
            return BadRequest();
        }

        var result = await _consultationService.GetForRequesterAsync(
            requesterId,
            id,
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    private string? GetRequesterId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier);
}
