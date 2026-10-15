using MediatR;
using Microsoft.AspNetCore.Mvc;
using SynaptumLearn.Application.Curriculum.GradeSubjects.ListGradeSubjects;
using Microsoft.AspNetCore.Mvc.Rendering;
using SynaptumLearn.Application.Curriculum.Grades.ListGrades;
using SynaptumLearn.Application.Curriculum.Subjects.ListSubjects;
using SynaptumLearn.Web.Models.GradeSubjects;
using SynaptumLearn.Application.Curriculum.GradeSubjects.AssignSubjectToGrade;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using SynaptumLearn.Application.Common.Security;

namespace SynaptumLearn.Web.Controllers;

[Authorize(Roles = PlatformRoles.PlatformAdmin)]
public sealed class GradeSubjectsController : Controller
{
    private readonly ISender _sender;

    public GradeSubjectsController(ISender sender)
    {
        _sender = sender;
    }

    #region Index
    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var assignments = await _sender.Send(
            new ListGradeSubjectsQuery(),
            cancellationToken);

        return View(assignments);
    }
    #endregion

    #region Create
    [HttpGet]
    public async Task<IActionResult> Create(
    CancellationToken cancellationToken)
    {
        var model = new AssignSubjectToGradeViewModel();

        await PopulateDropdownsAsync(
            model,
            cancellationToken);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
    AssignSubjectToGradeViewModel model,
    CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(model, cancellationToken);
            return View(model);
        }

        try
        {
            await _sender.Send(
                new AssignSubjectToGradeCommand(
                    model.GradeId!.Value,
                    model.SubjectId!.Value),
                cancellationToken);
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                var propertyName = error.PropertyName;

                // Command-level errors appear in the validation summary.
                if (propertyName != nameof(model.GradeId) &&
                    propertyName != nameof(model.SubjectId))
                {
                    propertyName = string.Empty;
                }

                ModelState.AddModelError(
                    propertyName,
                    error.ErrorMessage);
            }

            await PopulateDropdownsAsync(model, cancellationToken);

            return View(model);
        }

        TempData["SuccessMessage"] =
            "Subject assigned to grade successfully.";

        return RedirectToAction(nameof(Index));
    }
    #endregion

    #region Helper
    private async Task PopulateDropdownsAsync(
    AssignSubjectToGradeViewModel model,
    CancellationToken cancellationToken)
    {
        var grades = await _sender.Send(
            new ListGradesQuery(),
            cancellationToken);

        var subjects = await _sender.Send(
            new ListSubjectsQuery(),
            cancellationToken);

        model.Grades = grades
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            })
            .ToList();

        model.Subjects = subjects
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            })
            .ToList();
    }
    #endregion
}