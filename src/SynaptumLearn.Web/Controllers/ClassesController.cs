
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SynaptumLearn.Application.Classes.CreateClass;
using SynaptumLearn.Application.Classes.ListClasses;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Application.Curriculum.Grades.ListGrades;
using SynaptumLearn.Application.Classes.EnrollLearner;
using SynaptumLearn.Application.Classes.ListEligibleLearners;
using SynaptumLearn.Application.Classes.ListClassLearners;
using SynaptumLearn.Application.Classes.TransferLearner;
using SynaptumLearn.Web.Models.Classes;

namespace SynaptumLearn.Web.Controllers;

[Authorize]
public sealed class ClassesController : Controller
{
    private readonly ISender _sender;
    private readonly ISchoolAdminAccessService _accessService;

    public ClassesController(
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
            var classes = await _sender.Send(
                new ListClassesQuery(),
                cancellationToken);

            return View(classes);
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

            var model = new CreateClassViewModel
            {
                AcademicYear = DateTime.Today.Year + 1
            };

            await PopulateGradesAsync(model, cancellationToken);

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
        CreateClassViewModel model,
        CancellationToken cancellationToken)
    {
        try
        {
            await _accessService.GetAuthorizedSchoolIdAsync(
                cancellationToken);

            if (ModelState.IsValid)
            {
                await _sender.Send(
                    new CreateClassCommand(
                        model.Name,
                        model.GradeId!.Value,
                        model.AcademicYear),
                    cancellationToken);

                TempData["SuccessMessage"] =
                    "Class created successfully.";

                return RedirectToAction(nameof(Index));
            }

            await PopulateGradesAsync(model, cancellationToken);
            return View(model);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);

            await PopulateGradesAsync(model, cancellationToken);
            return View(model);
        }
    }
    #endregion

    #region Enroll
    [HttpGet]
    public async Task<IActionResult> Enroll(
        Guid classId,
        CancellationToken cancellationToken)
    {
        try
        {
            var model = await BuildEnrollmentModelAsync(
                classId,
                cancellationToken);

            return View(model);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(
        EnrollLearnerViewModel model,
        CancellationToken cancellationToken)
    {
        try
        {
            if (ModelState.IsValid)
            {
                await _sender.Send(
                    new EnrollLearnerCommand(
                        model.LearnerProfileId!.Value,
                        model.ClassId),
                    cancellationToken);

                TempData["SuccessMessage"] =
                    "Learner enrolled successfully.";

                return RedirectToAction(nameof(Index));
            }

            return View(await BuildEnrollmentModelAsync(
                model.ClassId,
                cancellationToken,
                model.LearnerProfileId));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);

            try
            {
                return View(await BuildEnrollmentModelAsync(
                    model.ClassId,
                    cancellationToken,
                    model.LearnerProfileId));
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }
    }
    #endregion

    #region Roster
    [HttpGet]
    public async Task<IActionResult> Roster(
        Guid classId,
        CancellationToken cancellationToken)
    {
        try
        {
            var classes = await _sender.Send(
                new ListClassesQuery(),
                cancellationToken);

            var schoolClass = classes.FirstOrDefault(
                c => c.Id == classId);

            if (schoolClass is null)
            {
                return NotFound();
            }

            var learners = await _sender.Send(
                new ListClassLearnersQuery(classId),
                cancellationToken);

            var model = new ClassRosterViewModel
            {
                ClassId = schoolClass.Id,
                ClassName = schoolClass.Name,
                GradeName = schoolClass.GradeName,
                AcademicYear = schoolClass.AcademicYear,
                Learners = learners
            };

            return View(model);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
    #endregion

    #region Transfer
    [HttpGet]
    public async Task<IActionResult> Transfer(
    Guid enrollmentId,
    CancellationToken cancellationToken)
    {
        try
        {
            var model = await BuildTransferModelAsync(
                enrollmentId,
                cancellationToken);

            return View(model);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Transfer(
        TransferLearnerViewModel model,
        CancellationToken cancellationToken)
    {
        try
        {
            if (ModelState.IsValid)
            {
                await _sender.Send(
                    new TransferLearnerCommand(
                        model.EnrollmentId,
                        model.DestinationClassId!.Value),
                    cancellationToken);

                TempData["SuccessMessage"] =
                    "Learner transferred successfully.";

                return RedirectToAction(
                    nameof(Roster),
                    new { classId = model.DestinationClassId });
            }

            return View(await BuildTransferModelAsync(
                model.EnrollmentId,
                cancellationToken,
                model.DestinationClassId));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);

            try
            {
                return View(await BuildTransferModelAsync(
                    model.EnrollmentId,
                    cancellationToken,
                    model.DestinationClassId));
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }
    }
    #endregion

    #region PrivateMethods
    private async Task PopulateGradesAsync(
        CreateClassViewModel model,
        CancellationToken cancellationToken)
    {
        var grades = await _sender.Send(
            new ListGradesQuery(),
            cancellationToken);

        model.Grades = grades
            .Select(g => new SelectListItem
            {
                Value = g.Id.ToString(),
                Text = g.Name
            })
            .ToList();
    }

    private async Task<EnrollLearnerViewModel> BuildEnrollmentModelAsync(
    Guid classId,
    CancellationToken cancellationToken,
    Guid? selectedLearnerId = null)
    {
        var classes = await _sender.Send(
            new ListClassesQuery(),
            cancellationToken);

        var schoolClass = classes.FirstOrDefault(c => c.Id == classId);

        if (schoolClass is null)
        {
            throw new InvalidOperationException(
                "The selected class was not found.");
        }

        var learners = await _sender.Send(
            new ListEligibleLearnersQuery(classId),
            cancellationToken);

        return new EnrollLearnerViewModel
        {
            ClassId = schoolClass.Id,
            ClassName = schoolClass.Name,
            GradeName = schoolClass.GradeName,
            AcademicYear = schoolClass.AcademicYear,
            LearnerProfileId = selectedLearnerId,
            Learners = learners.Select(l => new SelectListItem
            {
                Value = l.LearnerProfileId.ToString(),
                Text = $"{l.FullName} ({l.LearnerNumber})"
            }).ToList()
        };
    }

    private async Task<TransferLearnerViewModel> BuildTransferModelAsync(
    Guid enrollmentId,
    CancellationToken cancellationToken,
    Guid? selectedDestinationId = null)
    {
        var classes = await _sender.Send(
            new ListClassesQuery(),
            cancellationToken);

        // Locate the enrollment through school-scoped roster queries.
        foreach (var schoolClass in classes)
        {
            var learners = await _sender.Send(
                new ListClassLearnersQuery(schoolClass.Id),
                cancellationToken);

            var learner = learners.FirstOrDefault(
                l => l.EnrollmentId == enrollmentId);

            if (learner is null)
            {
                continue;
            }

            return new TransferLearnerViewModel
            {
                EnrollmentId = enrollmentId,
                SourceClassId = schoolClass.Id,
                SourceClassName = schoolClass.Name,
                GradeName = schoolClass.GradeName,
                AcademicYear = schoolClass.AcademicYear,
                LearnerName = $"{learner.FirstName} {learner.LastName}",
                LearnerNumber = learner.LearnerNumber,
                DestinationClassId = selectedDestinationId,
                DestinationClasses = classes
                    .Where(c =>
                        c.Id != schoolClass.Id &&
                        c.GradeId == schoolClass.GradeId &&
                        c.AcademicYear == schoolClass.AcademicYear)
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    })
                    .ToList()
            };
        }

        throw new InvalidOperationException(
            "The active enrollment was not found.");
    }
    #endregion

}
