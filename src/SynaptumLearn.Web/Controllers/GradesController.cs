using MediatR;
using Microsoft.AspNetCore.Mvc;
using SynaptumLearn.Application.Curriculum.Grades.ListGrades;
using SynaptumLearn.Application.Curriculum.Grades.GetGradeDetails;
using SynaptumLearn.Application.Curriculum.Grades.UpdateGrade;
using SynaptumLearn.Application.Curriculum.Grades.CreateGrade;
using SynaptumLearn.Web.Models.Grades;

namespace SynaptumLearn.Web.Controllers;

public sealed class GradesController : Controller
{
    private readonly ISender _sender;

    public GradesController(ISender sender)
    {
        _sender = sender;
    }
    #region  Index
    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var grades = await _sender.Send(
            new ListGradesQuery(),
            cancellationToken);

        return View(grades);
    }
    #endregion
    #region Details
    [HttpGet]
    public async Task<IActionResult> Details(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var grade = await _sender.Send(
            new GetGradeDetailsQuery(id),
            cancellationToken
        );

        if (grade is null)
        {
            return NotFound();
        }

        return View(grade);
    }
    #endregion
    #region Edit
    [HttpGet]
    public async Task<IActionResult> Edit(
        Guid id,
        CancellationToken cancellationToken)
    {
        var grade = await _sender.Send(
            new GetGradeDetailsQuery(id),
            cancellationToken);

        if (grade is null)
        {
            return NotFound();
        }

        var model = new EditGradeViewModel
        {
            Id = grade.Id,
            Name = grade.Name,
            Order = grade.Order
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        Guid id,
        EditGradeViewModel model,
        CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var updated = await _sender.Send(
            new UpdateGradeCommand(
                model.Id,
                model.Name,
                model.Order),
            cancellationToken);

        if (!updated)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "Grade updated successfully.";

        return RedirectToAction(
            nameof(Details),
            new { id = model.Id });
    }
    #endregion
    #region Create
    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateGradeViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateGradeViewModel model,
        CancellationToken cancellationToken)
    {
        var gradeId = await _sender.Send(
            new CreateGradeCommand(
                model.Name,
                model.Order),
            cancellationToken);

        TempData["SuccessMessage"] = "Grade created successfully.";

        return RedirectToAction(
            nameof(Details),
            new { id = gradeId });
    }
    #endregion

}