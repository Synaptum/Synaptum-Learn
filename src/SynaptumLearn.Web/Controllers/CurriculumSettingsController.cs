
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SynaptumLearn.Application.Schools.GetCurriculumAccessMode;
using SynaptumLearn.Application.Schools.UpdateCurriculumAccessMode;
using SynaptumLearn.Web.Models.CurriculumSettings;

namespace SynaptumLearn.Web.Controllers;

[Authorize]
public sealed class CurriculumSettingsController : Controller
{
    private readonly ISender _sender;

    public CurriculumSettingsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        try
        {
            var settings = await _sender.Send(
                new GetCurriculumAccessModeQuery(),
                cancellationToken);

            return View(new CurriculumSettingsViewModel
            {
                AccessMode = settings.AccessMode,
                CurrentTerm = settings.CurrentTerm
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(
        CurriculumSettingsViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _sender.Send(
                new UpdateCurriculumAccessModeCommand(
                    model.AccessMode,
                    model.CurrentTerm),
                cancellationToken);

            TempData["SuccessMessage"] =
                "Curriculum settings updated successfully.";

            return RedirectToAction(nameof(Index));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}
