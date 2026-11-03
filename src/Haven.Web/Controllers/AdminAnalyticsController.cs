using Haven.Application.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Haven.Web.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "AdminOnly")]
public sealed class AdminAnalyticsController(
    AdminDashboardApplicationService dashboardService) : ControllerBase
{
    [HttpGet("analytics")]
    [HttpGet("reports")]
    [ProducesResponseType<AdminAnalyticsReport>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AdminAnalyticsReport>> GetAnalytics(
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        try
        {
            if (fromDate.HasValue != toDate.HasValue)
            {
                ModelState.AddModelError(
                    "dateRange",
                    "Provide both fromDate and toDate, or omit both.");
                return ValidationProblem(ModelState);
            }

            return Ok(await dashboardService.GetAnalyticsAsync(
                fromDate,
                toDate,
                cancellationToken));
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError("dateRange", exception.Message);
            return ValidationProblem(ModelState);
        }
    }
}
