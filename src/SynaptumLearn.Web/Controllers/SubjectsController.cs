using MediatR;
using Microsoft.AspNetCore.Mvc;
using SynaptumLearn.Application.Curriculum.Subjects.ListSubjects;
using SynaptumLearn.Application.Curriculum.Subjects.GetSubjectDetails;
using SynaptumLearn.Application.Curriculum.Subjects.CreateSubject;
using SynaptumLearn.Application.Curriculum.Subjects.UpdateSubject;
using SynaptumLearn.Web.Models.Subjects;
using Microsoft.AspNetCore.Authorization;
using SynaptumLearn.Application.Common.Security;

namespace SynaptumLearn.Web.Controllers;

[Authorize(Roles = PlatformRoles.PlatformAdmin)]
public sealed class SubjectsController : Controller
{
    private readonly ISender _sender;

    public SubjectsController(ISender sender)
    {
        _sender = sender;
    }
    #region Index
    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var subjects = await _sender.Send(
            new ListSubjectsQuery(),
            cancellationToken);

        return View(subjects);
    }
    #endregion

    #region Details
    [HttpGet]
    public async Task<IActionResult> Details(
        Guid id,
        CancellationToken cancellationToken)
    {
        var subject = await _sender.Send(
            new GetSubjectDetailsQuery(id),
            cancellationToken);

        if (subject is null)
            return NotFound();

        return View(subject);
    }
    #endregion

    #region Edit
    [HttpGet]
    public async Task<IActionResult> Edit(
    Guid id,
    CancellationToken cancellationToken)
    {
        var subject = await _sender.Send(
            new GetSubjectDetailsQuery(id),
            cancellationToken);

        if (subject is null)
            return NotFound();

        var model = new EditSubjectViewModel
        {
            Id = subject.Id,
            Name = subject.Name,
            Code = subject.Code
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
    Guid id,
    EditSubjectViewModel model,
    CancellationToken cancellationToken)
    {
        if (id != model.Id)
            return BadRequest();

        var updated = await _sender.Send(
            new UpdateSubjectCommand(
                model.Id,
                model.Name,
                model.Code),
            cancellationToken);

        if (!updated)
            return NotFound();

        TempData["SuccessMessage"] = "Subject updated successfully.";

        return RedirectToAction(
            nameof(Details),
            new { id = model.Id });
    }
    #endregion

    #region Create
    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateSubjectViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
    CreateSubjectViewModel model,
    CancellationToken cancellationToken)
    {
        var subjectId = await _sender.Send(
            new CreateSubjectCommand(
                model.Name,
                model.Code),
            cancellationToken);

        TempData["SuccessMessage"] = "Subject created successfully.";

        return RedirectToAction(
            nameof(Details),
            new { id = subjectId });
    }
    #endregion

}