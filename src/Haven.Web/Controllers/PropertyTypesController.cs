using Haven.Application.PropertyTypes;
using Haven.Web.Models.PropertyTypes;
using Microsoft.AspNetCore.Mvc;

namespace Haven.Web.Controllers;

[ApiController]
[Route("api/property-types")]
public sealed class PropertyTypesController : ControllerBase
{
    private readonly PropertyTypeApplicationService _propertyTypeService;

    public PropertyTypesController(
        PropertyTypeApplicationService propertyTypeService)
    {
        _propertyTypeService = propertyTypeService;
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PropertyTypeResponse>>(
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PropertyTypeResponse>>> Get(
        CancellationToken cancellationToken)
    {
        var propertyTypes = await _propertyTypeService.GetActiveAsync(
            cancellationToken);
        return Ok(propertyTypes.Select(PropertyTypeResponse.From).ToArray());
    }
}
