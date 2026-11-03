using Haven.Application.Tags;
using Haven.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Haven.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public sealed class TagsController(TagApplicationService tagService) : Controller
{
    public async Task<IActionResult> Index(
        string? search,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await tagService.GetAdminPageAsync(
                search, page, 20, cancellationToken);
            return View(new AdminTagPageViewModel
            {
                Page = result,
                Search = search
            });
        }
        catch (ArgumentOutOfRangeException)
        {
            return BadRequest();
        }
    }

    [HttpGet]
    public IActionResult Create() => View(new TagFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        TagFormViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await tagService.CreateAsync(model.Name, model.Description, cancellationToken);
            TempData["SuccessMessage"] = "Tag đã được tạo.";
            return RedirectToAction(nameof(Index));
        }
        catch (DuplicateTagException exception)
        {
            ModelState.AddModelError(nameof(model.Name), exception.Message);
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(
        int id,
        CancellationToken cancellationToken)
    {
        var tag = await tagService.GetByIdAsync(id, cancellationToken);
        return tag is null
            ? NotFound()
            : View(new TagFormViewModel
            {
                Name = tag.Name,
                Description = tag.Description,
                IsActive = tag.IsActive
            });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        TagFormViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var tag = await tagService.UpdateAsync(
                id,
                model.Name,
                model.Description,
                model.IsActive,
                cancellationToken);
            if (tag is null)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = model.IsActive
                ? "Tag đã được cập nhật và kích hoạt."
                : "Tag đã được vô hiệu hóa.";
            return RedirectToAction(nameof(Index));
        }
        catch (DuplicateTagException exception)
        {
            ModelState.AddModelError(nameof(model.Name), exception.Message);
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
        }

        return View(model);
    }
}
