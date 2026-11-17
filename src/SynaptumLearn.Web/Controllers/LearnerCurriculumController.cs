
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SynaptumLearn.Application.Curriculum.LearnerCurriculum;

namespace SynaptumLearn.Web.Controllers;

[Authorize]
public sealed class LearnerCurriculumController : Controller
{
    private readonly ISender _sender;

    public LearnerCurriculumController(ISender sender)
    {
        _sender = sender;
    }

    #region Index
    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        try
        {
            var curriculum = await _sender.Send(
                new GetLearnerCurriculumQuery(),
                cancellationToken);

            return View(curriculum);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
    #endregion

    #region Lessons
    [HttpGet]
    public async Task<IActionResult> Lessons(
        Guid topicId,
        CancellationToken cancellationToken)
    {
        if (topicId == Guid.Empty)
        {
            return BadRequest("A valid topic is required.");
        }

        try
        {
            var lessons = await _sender.Send(
                new GetLearnerLessonsQuery(topicId),
                cancellationToken);

            ViewData["TopicId"] = topicId;

            return View(lessons);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
    #endregion

}
