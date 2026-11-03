using Haven.Application.Consultations;
using Haven.Web.Models.Consultations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Haven.Web.Controllers;

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/admin/consultations")]
public sealed class AdminConsultationsController : ControllerBase
{
    private readonly ConsultationApplicationService _consultationService;

    public AdminConsultationsController(
        ConsultationApplicationService consultationService)
    {
        _consultationService = consultationService;
    }

    [HttpGet]
    [ProducesResponseType<ConsultationPageResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ConsultationPageResult>> GetAll(
        [FromQuery] ConsultationListRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (request.FromDate.HasValue != request.ToDate.HasValue ||
                request.FromDate is { } fromDate &&
                request.ToDate is { } toDate &&
                toDate < fromDate)
            {
                ModelState.AddModelError(
                    "dateRange",
                    "Provide both dates and ensure the end date is not before the start date.");
                return ValidationProblem(ModelState);
            }

            var fromUtc = request.FromDate?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            DateTime? toUtcExclusive = request.ToDate is { } endDate && endDate < DateOnly.MaxValue
                ? endDate.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
                : null;
            return Ok(await _consultationService.SearchForAdminAsync(
                new Haven.Application.Consultations.AdminConsultationCriteria(
                    request.Search,
                    request.Status,
                    fromUtc.HasValue ? new DateTimeOffset(fromUtc.Value) : null,
                    toUtcExclusive.HasValue ? new DateTimeOffset(toUtcExclusive.Value) : null,
                    request.Page,
                    request.PageSize),
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
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConsultationDetails>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        var result = await _consultationService.GetForAdminAsync(
            id,
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("{id:int}/status")]
    [ProducesResponseType<ConsultationDetails>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConsultationDetails>> UpdateStatus(
        int id,
        UpdateConsultationStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        try
        {
            var result = await _consultationService.ChangeStatusAsync(
                id,
                request.Status,
                cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError("status", exception.Message);
            return ValidationProblem(ModelState);
        }
    }
}
