using Haven.Application.PropertyTypes;
using Haven.Web.Models.PropertyTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Haven.Web.Controllers;

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/admin/property-types")]
public sealed class AdminPropertyTypesController : ControllerBase
{
    private readonly PropertyTypeApplicationService _propertyTypeService;

    public AdminPropertyTypesController(
        PropertyTypeApplicationService propertyTypeService)
    {
        _propertyTypeService = propertyTypeService;
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PropertyTypeResponse>>(
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<PropertyTypeResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var propertyTypes = await _propertyTypeService.GetAllAsync(
            cancellationToken);
        return Ok(propertyTypes.Select(PropertyTypeResponse.From).ToArray());
    }

    [HttpPost]
    [ProducesResponseType<PropertyTypeResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PropertyTypeResponse>> Create(
        PropertyTypeWriteRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var propertyType = await _propertyTypeService.CreateAsync(
                request.Name,
                request.SortOrder,
                request.IsActive,
                cancellationToken);
            var response = PropertyTypeResponse.From(propertyType);
            return CreatedAtAction(
                nameof(GetAll),
                routeValues: null,
                value: response);
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError("request", exception.Message);
            return ValidationProblem(ModelState);
        }
        catch (DuplicatePropertyTypeNameException exception)
        {
            return Conflict(new ProblemDetails
            {
                Title = "A property type with this name already exists.",
                Detail = exception.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<PropertyTypeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PropertyTypeResponse>> Update(
        int id,
        PropertyTypeWriteRequest request,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        try
        {
            var propertyType = await _propertyTypeService.UpdateAsync(
                id,
                request.Name,
                request.SortOrder,
                request.IsActive,
                cancellationToken);
            return propertyType is null
                ? NotFound()
                : Ok(PropertyTypeResponse.From(propertyType));
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError("request", exception.Message);
            return ValidationProblem(ModelState);
        }
        catch (DuplicatePropertyTypeNameException exception)
        {
            return Conflict(new ProblemDetails
            {
                Title = "A property type with this name already exists.",
                Detail = exception.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }
}
