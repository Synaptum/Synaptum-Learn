
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Application.Curriculum.GradeSubjects.ListGradeSubjects;
using SynaptumLearn.Application.Teachers.AssignTeacherSubject;
using SynaptumLearn.Application.Teachers.ListAssignableTeachers;
using SynaptumLearn.Application.Teachers.ListTeacherSubjects;
using SynaptumLearn.Application.Teachers.RemoveTeacherSubject;
using SynaptumLearn.Web.Models.TeacherSubjects;

namespace SynaptumLearn.Web.Controllers;

[Authorize]
public sealed class TeacherSubjectsController : Controller
{
    private readonly ISender _sender;
    private readonly ISchoolAdminAccessService _accessService;

    public TeacherSubjectsController(
        ISender sender,
        ISchoolAdminAccessService accessService)
    {
        _sender = sender;
        _accessService = accessService;
    }

    #region Index
    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        try
        {
            var assignments = await _sender.Send(
                new ListTeacherSubjectsQuery(),
                cancellationToken);

            return View(assignments);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
    #endregion

    #region Create
    [HttpGet]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken)
    {
        try
        {
            await _accessService.GetAuthorizedSchoolIdAsync(
                cancellationToken);

            var model = new AssignTeacherSubjectViewModel();
            await PopulateDropdownsAsync(model, cancellationToken);

            return View(model);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        AssignTeacherSubjectViewModel model,
        CancellationToken cancellationToken)
    {
        try
        {
            await _accessService.GetAuthorizedSchoolIdAsync(
                cancellationToken);

            if (ModelState.IsValid)
            {
                await _sender.Send(
                    new AssignTeacherSubjectCommand(
                        model.TeacherProfileId!.Value,
                        model.GradeSubjectId!.Value),
                    cancellationToken);

                TempData["SuccessMessage"] =
                    "Teacher assigned to subject successfully.";

                return RedirectToAction("Index", "SchoolUsers");
            }

            await PopulateDropdownsAsync(model, cancellationToken);
            return View(model);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);

            await PopulateDropdownsAsync(model, cancellationToken);
            return View(model);
        }
    }
    #endregion

    #region Remove
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(
        Guid assignmentId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _sender.Send(
                new RemoveTeacherSubjectCommand(assignmentId),
                cancellationToken);

            TempData["SuccessMessage"] =
                "Teacher assignment removed successfully.";
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
    #endregion

    #region PrivateMethod
    private async Task PopulateDropdownsAsync(
        AssignTeacherSubjectViewModel model,
        CancellationToken cancellationToken)
    {
        var teachers = await _sender.Send(
            new ListAssignableTeachersQuery(),
            cancellationToken);

        var gradeSubjects = await _sender.Send(
            new ListGradeSubjectsQuery(),
            cancellationToken);

        model.Teachers = teachers
            .Select(t => new SelectListItem
            {
                Value = t.TeacherProfileId.ToString(),
                Text = $"{t.FirstName} {t.LastName} ({t.EmployeeNumber})"
            })
            .ToList();

        model.GradeSubjects = gradeSubjects
            .Select(gs => new SelectListItem
            {
                Value = gs.Id.ToString(),
                Text = $"{gs.GradeName} - {gs.SubjectName}"
            })
            .ToList();
    }
    #endregion

}