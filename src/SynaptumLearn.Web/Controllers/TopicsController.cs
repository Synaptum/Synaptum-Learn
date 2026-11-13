
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SynaptumLearn.Application.Common.Security;
using SynaptumLearn.Application.Curriculum.GradeSubjects.ListGradeSubjects;
using SynaptumLearn.Application.Curriculum.Topics.CreateTopic;
using SynaptumLearn.Application.Curriculum.Topics.ListTopics;
using SynaptumLearn.Web.Models.Topics;

namespace SynaptumLearn.Web.Controllers;

[Authorize(Roles = PlatformRoles.PlatformAdmin)]
public sealed class TopicsController : Controller
{
    private readonly ISender _sender;

    public TopicsController(ISender sender)
    {
        _sender = sender;
    }

    #region Index
    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var topics = await _sender.Send(
            new ListTopicsQuery(),
            cancellationToken);

        return View(topics);
    }
    #endregion

    #region Create
    [HttpGet]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken)
    {
        var model = new CreateTopicViewModel();

        await PopulateGradeSubjectsAsync(
            model,
            cancellationToken);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateTopicViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateGradeSubjectsAsync(
                model,
                cancellationToken);

            return View(model);
        }

        try
        {
            var command = new CreateTopicCommand(
                model.GradeSubjectId!.Value,
                model.Title,
                model.Description,
                model.Term!.Value,
                model.Order!.Value);

            await _sender.Send(command, cancellationToken);
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                var propertyName = error.PropertyName;

                if (propertyName != nameof(model.GradeSubjectId) &&
                    propertyName != nameof(model.Title) &&
                    propertyName != nameof(model.Description) &&
                    propertyName != nameof(model.Term) &&
                    propertyName != nameof(model.Order))
                {
                    propertyName = string.Empty;
                }

                ModelState.AddModelError(
                    propertyName,
                    error.ErrorMessage);
            }

            await PopulateGradeSubjectsAsync(
                model,
                cancellationToken);

            return View(model);
        }

        TempData["SuccessMessage"] =
            "Curriculum topic created successfully.";

        return RedirectToAction(nameof(Index));
    }
    #endregion

    #region PrivateMethods
    private async Task PopulateGradeSubjectsAsync(
        CreateTopicViewModel model,
        CancellationToken cancellationToken)
    {
        var gradeSubjects = await _sender.Send(
            new ListGradeSubjectsQuery(),
            cancellationToken);

        model.GradeSubjects = gradeSubjects
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = $"{x.GradeName} - {x.SubjectName}"
            })
            .ToList();
    }
    #endregion

}
