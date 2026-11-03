using Haven.Application.PropertyTypes;
using Haven.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Haven.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public sealed class PropertyTypesController : Controller
{
    private readonly PropertyTypeApplicationService _propertyTypeService;

    public PropertyTypesController(PropertyTypeApplicationService propertyTypeService)
    {
        _propertyTypeService = propertyTypeService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await _propertyTypeService.GetAllAsync(cancellationToken));

    [HttpGet]
    public IActionResult Create() => View(new PropertyTypeFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        PropertyTypeFormViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _propertyTypeService.CreateAsync(
                model.Name,
                model.SortOrder,
                model.IsActive,
                cancellationToken);
            TempData["SuccessMessage"] = "Loại bất động sản đã được tạo.";
            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
        }
        catch (DuplicatePropertyTypeNameException exception)
        {
            ModelState.AddModelError(nameof(model.Name), exception.Message);
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(
        int id,
        CancellationToken cancellationToken)
    {
        var type = (await _propertyTypeService.GetAllAsync(cancellationToken))
            .FirstOrDefault(candidate => candidate.Id == id);
        return type is null
            ? NotFound()
            : View(new PropertyTypeFormViewModel
            {
                Name = type.Name,
                SortOrder = type.SortOrder,
                IsActive = type.IsActive
            });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        PropertyTypeFormViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await _propertyTypeService.UpdateAsync(
                id,
                model.Name,
                model.SortOrder,
                model.IsActive,
                cancellationToken);
            if (result is null)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Loại bất động sản đã được cập nhật.";
            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
        }
        catch (DuplicatePropertyTypeNameException exception)
        {
            ModelState.AddModelError(nameof(model.Name), exception.Message);
        }

        return View(model);
    }
}
