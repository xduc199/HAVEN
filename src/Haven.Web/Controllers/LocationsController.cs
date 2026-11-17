using Haven.Application.Locations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Haven.Web.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/locations")]
public sealed class LocationsController(AdministrativeLocationApplicationService locations) : ControllerBase
{
    [HttpGet("provinces")]
    public async Task<IActionResult> GetProvinces(CancellationToken cancellationToken) =>
        Ok(await locations.GetProvincesAsync(cancellationToken));

    [HttpGet("provinces/{provinceCode}/wards")]
    public async Task<IActionResult> GetWards(string provinceCode, CancellationToken cancellationToken)
    {
        try { return Ok(await locations.GetWardsAsync(provinceCode, cancellationToken)); }
        catch (ArgumentException) { return BadRequest(); }
    }
}
