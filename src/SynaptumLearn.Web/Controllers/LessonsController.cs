
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SynaptumLearn.Domain.Content;
using SynaptumLearn.Application.Common.Security;
using SynaptumLearn.Application.Curriculum.Lessons.CreateLesson;
using SynaptumLearn.Application.Curriculum.Lessons.ListLessons;
using SynaptumLearn.Application.Curriculum.Lessons.UpdateLesson;
using SynaptumLearn.Application.Curriculum.Lessons.GetLessonById;
using SynaptumLearn.Application.Curriculum.Lessons.ListLessonContents;
using SynaptumLearn.Application.Curriculum.Lessons.AddLessonContent;
using SynaptumLearn.Application.Curriculum.Lessons.UpdateLessonContent;
using SynaptumLearn.Application.Curriculum.Lessons.GetLessonContentById;
using SynaptumLearn.Application.Curriculum.Lessons.DeleteLessonContent;
using SynaptumLearn.Application.Curriculum.Lessons.MoveLessonContent;
using SynaptumLearn.Application.Curriculum.Lessons.PublicationReadiness;
using SynaptumLearn.Application.Curriculum.Topics.ListTopics;
using SynaptumLearn.Application.Curriculum.Assessments.CreateMasteryAssessment;
using SynaptumLearn.Application.Curriculum.Assessments.GetMasteryAssessment;
using SynaptumLearn.Application.Curriculum.Assessments.AddAssessmentQuestion;
using SynaptumLearn.Application.Curriculum.Assessments.GetMasteryAssessmentByLesson;
using SynaptumLearn.Application.Curriculum.Assessments.AddAnswerOption;
using SynaptumLearn.Application.Curriculum.Assessments.GetAssessmentIdByQuestion;
using SynaptumLearn.Application.Curriculum.Assessments.GetAnswerOption;
using SynaptumLearn.Application.Curriculum.Assessments.UpdateAnswerOption;
using SynaptumLearn.Application.Curriculum.Assessments.DeleteAnswerOption;
using SynaptumLearn.Application.Curriculum.Assessments.GetAssessmentQuestion;
using SynaptumLearn.Application.Curriculum.Assessments.UpdateAssessmentQuestion;
using SynaptumLearn.Web.Models.Assessments;
using SynaptumLearn.Web.Models.Lessons;

namespace SynaptumLearn.Web.Controllers;

[Authorize(Roles = PlatformRoles.PlatformAdmin)]
public sealed class LessonsController : Controller
{
    private readonly ISender _sender;

    public LessonsController(ISender sender)
    {
        _sender = sender;
    }

    #region Index
    [HttpGet]
    public async Task<IActionResult> Index(
        Guid? topicId,
        CancellationToken cancellationToken)
    {
        var lessons = await _sender.Send(
            new ListLessonsQuery(topicId),
            cancellationToken);

        return View(lessons);
    }
    #endregion

    #region Create
    [HttpGet]
    public async Task<IActionResult> Create(
        Guid? topicId,
        CancellationToken cancellationToken)
    {
        var model = new CreateLessonViewModel
        {
            TopicId = topicId
        };

        await PopulateTopicsAsync(model, cancellationToken);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateLessonViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateTopicsAsync(model, cancellationToken);
            return View(model);
        }

        try
        {
            await _sender.Send(
                new CreateLessonCommand(
                    model.TopicId!.Value,
                    model.Title,
                    model.Summary,
                    model.Order!.Value),
                cancellationToken);
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                var propertyName = error.PropertyName;

                if (propertyName != nameof(model.TopicId) &&
                    propertyName != nameof(model.Title) &&
                    propertyName != nameof(model.Summary) &&
                    propertyName != nameof(model.Order))
                {
                    propertyName = string.Empty;
                }

                ModelState.AddModelError(
                    propertyName,
                    error.ErrorMessage);
            }

            await PopulateTopicsAsync(model, cancellationToken);
            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateTopicsAsync(model, cancellationToken);
            return View(model);
        }

        TempData["SuccessMessage"] =
            "Lesson draft created successfully.";

        return RedirectToAction(nameof(Index));
    }
    #endregion

    #region Edit
    [HttpGet]
    public async Task<IActionResult> Edit(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
            return BadRequest();

        var lesson = await _sender.Send(
            new GetLessonByIdQuery(id),
            cancellationToken);

        if (lesson is null)
            return NotFound();

        var model = new EditLessonViewModel
        {
            LessonId = lesson.Id,
            TopicTitle = lesson.TopicTitle,
            Title = lesson.Title,
            Summary = lesson.Summary,
            Order = lesson.Order
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        EditLessonViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _sender.Send(
                new UpdateLessonCommand(
                    model.LessonId,
                    model.Title,
                    model.Summary,
                    model.Order),
                cancellationToken);
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                var propertyName = error.PropertyName;

                if (propertyName != nameof(model.LessonId) &&
                    propertyName != nameof(model.Title) &&
                    propertyName != nameof(model.Summary) &&
                    propertyName != nameof(model.Order))
                {
                    propertyName = string.Empty;
                }

                ModelState.AddModelError(
                    propertyName,
                    error.ErrorMessage);
            }

            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }

        TempData["SuccessMessage"] =
            "Lesson updated successfully.";

        return RedirectToAction(nameof(Index));
    }
    #endregion

    #region AddContent
    [HttpGet]
    public async Task<IActionResult> AddContent(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
            return BadRequest();

        var lesson = await _sender.Send(
            new GetLessonByIdQuery(id),
            cancellationToken);

        if (lesson is null)
            return NotFound();

        if (lesson.Status != LessonStatus.Draft)
        {
            TempData["ErrorMessage"] =
                "Content can only be added to draft lessons.";

            return RedirectToAction(
                nameof(ManageContent),
                new { id });
        }

        var existingContents = await _sender.Send(
            new ListLessonContentsQuery(id),
            cancellationToken);

        var nextOrder = existingContents.Count == 0
            ? 1
            : existingContents.Max(x => x.Order) + 1;

        var model = new AddLessonContentViewModel
        {
            LessonId = lesson.Id,
            LessonTitle = lesson.Title,
            Order = nextOrder
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddContent(
        AddLessonContentViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateAddContentLessonTitle(
                model,
                cancellationToken);

            return View(model);
        }

        try
        {
            await _sender.Send(
                new AddLessonContentCommand(
                    model.LessonId,
                    model.Type!.Value,
                    model.Title,
                    model.Content,
                    model.Order!.Value),
                cancellationToken);
        }
        catch (FluentValidation.ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                var propertyName = error.PropertyName;

                if (propertyName != nameof(model.LessonId) &&
                    propertyName != nameof(model.Type) &&
                    propertyName != nameof(model.Title) &&
                    propertyName != nameof(model.Content) &&
                    propertyName != nameof(model.Order))
                {
                    propertyName = string.Empty;
                }

                ModelState.AddModelError(
                    propertyName,
                    error.ErrorMessage);
            }

            await PopulateAddContentLessonTitle(
                model,
                cancellationToken);

            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            await PopulateAddContentLessonTitle(
                model,
                cancellationToken);

            return View(model);
        }

        TempData["SuccessMessage"] =
            "Lesson content block added successfully.";

        return RedirectToAction(
            nameof(ManageContent),
            new { id = model.LessonId });
    }

    private async Task PopulateAddContentLessonTitle(
        AddLessonContentViewModel model,
        CancellationToken cancellationToken)
    {
        var lesson = await _sender.Send(
            new GetLessonByIdQuery(model.LessonId),
            cancellationToken);

        model.LessonTitle = lesson?.Title ?? string.Empty;
    }
    #endregion

    #region EditContent
    [HttpGet]
    public async Task<IActionResult> EditContent(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
            return BadRequest();

        var block = await _sender.Send(
            new GetLessonContentByIdQuery(id),
            cancellationToken);

        if (block is null)
            return NotFound();

        var lesson = await _sender.Send(
            new GetLessonByIdQuery(block.LessonId),
            cancellationToken);

        if (lesson is null)
            return NotFound();

        if (lesson.Status != LessonStatus.Draft)
            return BadRequest("Only draft lesson content can be edited.");

        return View(new EditLessonContentViewModel
        {
            ContentId = block.Id,
            LessonId = lesson.Id,
            LessonTitle = lesson.Title,
            Type = block.Type,
            Title = block.Title,
            Content = block.Content,
            Order = block.Order
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditContent(
        EditLessonContentViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _sender.Send(
                new UpdateLessonContentCommand(
                    model.ContentId,
                    model.Type!.Value,
                    model.Title,
                    model.Content,
                    model.Order!.Value),
                cancellationToken);
        }
        catch (FluentValidation.ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                ModelState.AddModelError(
                    error.PropertyName,
                    error.ErrorMessage);
            }

            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }

        TempData["SuccessMessage"] =
            "Content block updated successfully.";

        return RedirectToAction(
            nameof(ManageContent),
            new { id = model.LessonId });
    }
    #endregion

    #region DeleteContent
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteContent(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
            return BadRequest();

        var block = await _sender.Send(
            new GetLessonContentByIdQuery(id),
            cancellationToken);

        if (block is null)
            return NotFound();

        try
        {
            var lessonId = await _sender.Send(
                new DeleteLessonContentCommand(id),
                cancellationToken);

            TempData["SuccessMessage"] =
                "Content block deleted successfully.";

            return RedirectToAction(
                nameof(ManageContent),
                new { id = lessonId });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;

            return RedirectToAction(
                nameof(ManageContent),
                new { id = block.LessonId });
        }
    }
    #endregion

    #region MoveContent
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveContent(
        Guid id,
        LessonContentMoveDirection direction,
        CancellationToken cancellationToken)
    {
        if (id == Guid.Empty ||
            !Enum.IsDefined(direction))
        {
            return BadRequest();
        }

        var block = await _sender.Send(
            new GetLessonContentByIdQuery(id),
            cancellationToken);

        if (block is null)
            return NotFound();

        try
        {
            var lessonId = await _sender.Send(
                new MoveLessonContentCommand(id, direction),
                cancellationToken);

            TempData["SuccessMessage"] =
                "Content block position updated successfully.";

            return RedirectToAction(
                nameof(ManageContent),
                new { id = lessonId });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;

            return RedirectToAction(
                nameof(ManageContent),
                new { id = block.LessonId });
        }
    }
    #endregion

    #region ManageContent
    [HttpGet]
    public async Task<IActionResult> ManageContent(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
            return BadRequest();

        var lesson = await _sender.Send(
            new GetLessonByIdQuery(id),
            cancellationToken);

        if (lesson is null)
            return NotFound();

        var contents = await _sender.Send(
            new ListLessonContentsQuery(id),
            cancellationToken);

        var model = new ManageLessonContentViewModel
        {
            LessonId = lesson.Id,
            LessonTitle = lesson.Title,
            Status = lesson.Status,
            Contents = contents
        };

        return View(model);
    }
    #endregion

    #region CreateMasteryAssessment
    [HttpGet]
    public async Task<IActionResult> CreateMasteryAssessment(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
            return BadRequest();

        var lesson = await _sender.Send(
            new GetLessonByIdQuery(id),
            cancellationToken);

        if (lesson is null)
            return NotFound();

        if (lesson.Status != LessonStatus.Draft)
        {
            TempData["ErrorMessage"] =
                "Mastery assessments can only be created for draft lessons.";

            return RedirectToAction(
                nameof(ManageContent),
                new { id });
        }

        var model = new CreateMasteryAssessmentViewModel
        {
            LessonId = lesson.Id,
            LessonTitle = lesson.Title,
            Title = $"{lesson.Title} - Mastery Assessment",
            PassMark = 70
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateMasteryAssessment(
        CreateMasteryAssessmentViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _sender.Send(
                new CreateMasteryAssessmentCommand(
                    model.LessonId,
                    model.Title,
                    model.Instructions,
                    model.PassMark),
                cancellationToken);

            TempData["SuccessMessage"] =
                "Mastery assessment created successfully.";

            return RedirectToAction(
                nameof(PublicationReadiness),
                new { id = model.LessonId });
        }
        catch (FluentValidation.ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                ModelState.AddModelError(
                    error.PropertyName,
                    error.ErrorMessage);
            }

            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            return View(model);
        }
    }
    #endregion

    #region ManageMasteryAssessment
    [HttpGet]
    public async Task<IActionResult> ManageMasteryAssessment(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
            return BadRequest();

        var assessment = await _sender.Send(
            new GetMasteryAssessmentQuery(id),
            cancellationToken);

        if (assessment is null)
            return NotFound();

        return View(assessment);
    }
    #endregion

    #region AddAssessmentQuestion
    [HttpGet]
    public async Task<IActionResult> AddAssessmentQuestion(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
            return BadRequest();

        var assessment = await _sender.Send(
            new GetMasteryAssessmentQuery(id),
            cancellationToken);

        if (assessment is null)
            return NotFound();

        if (assessment.IsPublished)
        {
            TempData["ErrorMessage"] =
                "Published assessments cannot be edited.";

            return RedirectToAction(
                nameof(ManageMasteryAssessment),
                new { id });
        }

        return View(new AddAssessmentQuestionViewModel
        {
            AssessmentId = assessment.Id,
            AssessmentTitle = assessment.Title,
            Order = assessment.Questions.Count == 0
                ? 1
                : assessment.Questions.Max(q => q.Order) + 1
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAssessmentQuestion(
        AddAssessmentQuestionViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _sender.Send(
                new AddAssessmentQuestionCommand(
                    model.AssessmentId,
                    model.Text,
                    model.Marks,
                    model.Order),
                cancellationToken);

            TempData["SuccessMessage"] =
                "Assessment question added successfully.";

            return RedirectToAction(
                nameof(ManageMasteryAssessment),
                new { id = model.AssessmentId });
        }
        catch (FluentValidation.ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                ModelState.AddModelError(
                    error.PropertyName,
                    error.ErrorMessage);
            }

            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            return View(model);
        }
    }
    #endregion

    #region AnswerOption
    [HttpGet]
    public async Task<IActionResult> AddAnswerOption(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
            return BadRequest();

        // Look up the question's parent assessment without
        // exposing direct database access in the controller.
        var assessmentId = await _sender.Send(
            new GetAssessmentIdByQuestionQuery(id),
            cancellationToken);

        if (!assessmentId.HasValue)
            return NotFound();

        var assessment = await _sender.Send(
            new GetMasteryAssessmentQuery(assessmentId.Value),
            cancellationToken);

        if (assessment is null)
            return NotFound();

        var question = assessment.Questions
            .FirstOrDefault(q => q.Id == id);

        if (question is null)
            return NotFound();

        if (assessment.IsPublished)
        {
            TempData["ErrorMessage"] =
                "Published assessments cannot be edited.";

            return RedirectToAction(
                nameof(ManageMasteryAssessment),
                new { id = assessment.Id });
        }

        return View(new AddAnswerOptionViewModel
        {
            QuestionId = question.Id,
            AssessmentId = assessment.Id,
            QuestionText = question.Text
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAnswerOption(
        AddAnswerOptionViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _sender.Send(
                new AddAnswerOptionCommand(
                    model.QuestionId,
                    model.Text,
                    model.IsCorrect),
                cancellationToken);

            TempData["SuccessMessage"] =
                "Answer option added successfully.";

            return RedirectToAction(
                nameof(ManageMasteryAssessment),
                new { id = model.AssessmentId });
        }
        catch (FluentValidation.ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                ModelState.AddModelError(
                    error.PropertyName,
                    error.ErrorMessage);
            }

            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            return View(model);
        }
    }
    #endregion

    #region EditAnswerOption
    [HttpGet]
    public async Task<IActionResult> EditAnswerOption(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
            return BadRequest();

        var option = await _sender.Send(
            new GetAnswerOptionQuery(id),
            cancellationToken);

        if (option is null)
            return NotFound();

        if (!option.CanEdit)
        {
            TempData["ErrorMessage"] =
                "This answer option cannot be edited.";

            return RedirectToAction(
                nameof(ManageMasteryAssessment),
                new { id = option.AssessmentId });
        }

        return View(new EditAnswerOptionViewModel
        {
            AnswerOptionId = option.Id,
            AssessmentId = option.AssessmentId,
            QuestionText = option.QuestionText,
            Text = option.Text,
            IsCorrect = option.IsCorrect
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAnswerOption(
        EditAnswerOptionViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var assessmentId = await _sender.Send(
                new UpdateAnswerOptionCommand(
                    model.AnswerOptionId,
                    model.Text,
                    model.IsCorrect),
                cancellationToken);

            TempData["SuccessMessage"] =
                "Answer option updated successfully.";

            return RedirectToAction(
                nameof(ManageMasteryAssessment),
                new { id = assessmentId });
        }
        catch (FluentValidation.ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                ModelState.AddModelError(
                    error.PropertyName,
                    error.ErrorMessage);
            }

            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            return View(model);
        }
    }
    #endregion

    #region DeleteAnswerOption
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAnswerOption(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var assessmentId = await _sender.Send(
                new DeleteAnswerOptionCommand(id),
                cancellationToken);

            TempData["SuccessMessage"] =
                "Answer option deleted successfully.";

            return RedirectToAction(
                nameof(ManageMasteryAssessment),
                new { id = assessmentId });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;

            return RedirectToAction(nameof(Index));
        }
    }
    #endregion

    #region EditAssessmentQuestion
    [HttpGet]
    public async Task<IActionResult> EditAssessmentQuestion(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
            return BadRequest();

        var question = await _sender.Send(
            new GetAssessmentQuestionQuery(id),
            cancellationToken);

        if (question is null)
            return NotFound();

        if (!question.CanEdit)
        {
            TempData["ErrorMessage"] =
                "This assessment question cannot be edited.";

            return RedirectToAction(
                nameof(ManageMasteryAssessment),
                new { id = question.AssessmentId });
        }

        return View(new EditAssessmentQuestionViewModel
        {
            QuestionId = question.Id,
            AssessmentId = question.AssessmentId,
            Order = question.Order,
            Text = question.Text,
            Marks = question.Marks
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAssessmentQuestion(
        EditAssessmentQuestionViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var assessmentId = await _sender.Send(
                new UpdateAssessmentQuestionCommand(
                    model.QuestionId,
                    model.Text,
                    model.Marks),
                cancellationToken);

            TempData["SuccessMessage"] =
                "Assessment question updated successfully.";

            return RedirectToAction(
                nameof(ManageMasteryAssessment),
                new { id = assessmentId });
        }
        catch (FluentValidation.ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                ModelState.AddModelError(
                    error.PropertyName,
                    error.ErrorMessage);
            }

            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            return View(model);
        }
    }
    #endregion

    #region MasteryAssessment
    [HttpGet]
    public async Task<IActionResult> MasteryAssessment(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
            return BadRequest();

        var assessmentId = await _sender.Send(
            new GetMasteryAssessmentByLessonQuery(id),
            cancellationToken);

        if (assessmentId.HasValue)
        {
            return RedirectToAction(
                nameof(ManageMasteryAssessment),
                new { id = assessmentId.Value });
        }

        var lesson = await _sender.Send(
            new GetLessonByIdQuery(id),
            cancellationToken);

        if (lesson is null)
            return NotFound();

        if (lesson.Status == LessonStatus.Draft)
        {
            return RedirectToAction(
                nameof(CreateMasteryAssessment),
                new { id });
        }

        TempData["ErrorMessage"] =
            "This lesson does not have a mastery assessment.";

        return RedirectToAction(
            nameof(ManageContent),
            new { id });
    }
    #endregion

    #region PublicationReadiness
    [HttpGet]
    public async Task<IActionResult> PublicationReadiness(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
            return BadRequest();

        var readiness = await _sender.Send(
            new GetLessonPublicationReadinessQuery(id),
            cancellationToken);

        return View(readiness);
    }
    #endregion

    #region HelperMethods
    private async Task PopulateTopicsAsync(
        CreateLessonViewModel model,
        CancellationToken cancellationToken)
    {
        var topics = await _sender.Send(
            new ListTopicsQuery(),
            cancellationToken);

        model.Topics = topics
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text =
                    $"{x.GradeName} - {x.SubjectName} - " +
                    $"Term {x.Term} - {x.Title}"
            })
            .ToList();
    }
    #endregion
}
