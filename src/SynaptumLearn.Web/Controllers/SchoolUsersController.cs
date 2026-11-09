
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Application.Users.CreateSchoolUser;
using SynaptumLearn.Application.Users.ListSchoolUsers;
using SynaptumLearn.Application.Curriculum.Grades.ListGrades;
using SynaptumLearn.Web.Models.SchoolUsers;

namespace SynaptumLearn.Web.Controllers;

[Authorize]
public class SchoolUsersController : Controller
{
    private readonly ISender _sender;
    private readonly ISchoolAdminAccessService _schoolAdminAccessService;

    public SchoolUsersController(
     ISender sender,
     ISchoolAdminAccessService schoolAdminAccessService)
    {
        _sender = sender;
        _schoolAdminAccessService = schoolAdminAccessService;
    }

    #region Index
    [HttpGet]
    public async Task<IActionResult> Index(
    CancellationToken cancellationToken)
    {
        try
        {
            var users = await _sender.Send(
                new ListSchoolUsersQuery(),
                cancellationToken);

            return View(users);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
    #endregion

    #region CreateTeacher
    [HttpGet]
    public async Task<IActionResult> CreateTeacher(
    CancellationToken cancellationToken)
    {
        try
        {
            await _schoolAdminAccessService
                .GetAuthorizedSchoolIdAsync(cancellationToken);

            return View();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTeacher(
        CreateTeacherViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var command = new CreateSchoolUserCommand(
                model.FirstName,
                model.LastName,
                model.Email,
                model.Password,
                SchoolUserAccountType.Teacher,
                EmployeeNumber: model.EmployeeNumber,
                IsClassTeacher: model.IsClassTeacher);

            await _sender.Send(command, cancellationToken);

            TempData["SuccessMessage"] =
                "Teacher account created successfully.";

            return RedirectToAction(nameof(CreateTeacher));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    #endregion

    #region CreateLearner
    [HttpGet]
    public async Task<IActionResult> CreateLearner(
        CancellationToken cancellationToken)
    {
        try
        {
            await _schoolAdminAccessService
                .GetAuthorizedSchoolIdAsync(cancellationToken);

            await LoadGradesAsync(cancellationToken);

            return View(new CreateLearnerViewModel());
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateLearner(
        CreateLearnerViewModel model,
        CancellationToken cancellationToken)
    {
        try
        {
            await _schoolAdminAccessService
                .GetAuthorizedSchoolIdAsync(cancellationToken);

            if (!ModelState.IsValid)
            {
                await LoadGradesAsync(cancellationToken);
                return View(model);
            }

            var command = new CreateSchoolUserCommand(
                model.FirstName,
                model.LastName,
                model.Email,
                model.Password,
                SchoolUserAccountType.Learner,
                LearnerNumber: model.LearnerNumber,
                GradeId: model.GradeId);

            await _sender.Send(command, cancellationToken);

            TempData["SuccessMessage"] =
                "Learner account created successfully.";

            return RedirectToAction(nameof(CreateLearner));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadGradesAsync(cancellationToken);
            return View(model);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    private async Task LoadGradesAsync(
        CancellationToken cancellationToken)
    {
        var grades = await _sender.Send(
            new ListGradesQuery(),
            cancellationToken);

        ViewBag.Grades = new SelectList(
            grades,
            "Id",
            "Name");
    }
    #endregion

}
