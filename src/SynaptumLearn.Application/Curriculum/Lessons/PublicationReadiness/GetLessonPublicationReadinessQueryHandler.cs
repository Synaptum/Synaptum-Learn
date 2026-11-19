
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Application.Common.Security;
using SynaptumLearn.Domain.Content;

namespace SynaptumLearn.Application.Curriculum.Lessons.PublicationReadiness;

public sealed class GetLessonPublicationReadinessQueryHandler
    : IRequestHandler<
        GetLessonPublicationReadinessQuery,
        LessonPublicationReadinessDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetLessonPublicationReadinessQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<LessonPublicationReadinessDto> Handle(
        GetLessonPublicationReadinessQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.IsInRole(PlatformRoles.PlatformAdmin))
        {
            throw new UnauthorizedAccessException(
                "Only Platform Administrators can check publication readiness.");
        }

        var lesson = await _context.Lessons
            .AsNoTracking()
            .Where(x => x.Id == request.LessonId)
            .Select(x => new
            {
                x.Id,
                x.Title,
                x.Summary,
                x.Status
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (lesson is null)
            throw new InvalidOperationException("Lesson not found.");

        var issues = new List<string>();

        if (lesson.Status != LessonStatus.Draft)
            issues.Add("The lesson must be in Draft status.");

        if (string.IsNullOrWhiteSpace(lesson.Title) ||
            lesson.Title.Length > 200)
        {
            issues.Add("The lesson must have a valid title.");
        }

        if (string.IsNullOrWhiteSpace(lesson.Summary) ||
            lesson.Summary.Length > 3000)
        {
            issues.Add("The lesson must have a valid summary.");
        }

        var contents = await _context.LessonContents
            .AsNoTracking()
            .Where(x => x.LessonId == request.LessonId)
            .Select(x => new
            {
                x.Title,
                x.Content,
                x.Order,
                x.Type
            })
            .ToListAsync(cancellationToken);

        if (contents.Count == 0)
        {
            issues.Add("The lesson must contain learning material.");
        }

        if (contents.Any(x =>
                string.IsNullOrWhiteSpace(x.Title) ||
                x.Title.Length > 200 ||
                string.IsNullOrWhiteSpace(x.Content) ||
                x.Content.Length > 10000 ||
                x.Order <= 0 ||
                !Enum.IsDefined(x.Type)))
        {
            issues.Add("One or more content blocks are invalid.");
        }

        if (contents.Select(x => x.Order).Distinct().Count()
            != contents.Count)
        {
            issues.Add("Content block positions must be unique.");
        }

        var assessments = await _context.Assessments
            .AsNoTracking()
            .Where(x =>
                x.LessonId == request.LessonId &&
                x.IsRequiredForProgression &&
                x.IsPublished)
            .Select(x => new
            {
                x.PassMark,
                Questions = x.Questions.Select(q => new
                {
                    q.Text,
                    q.Marks,
                    q.Order,
                    Options = q.AnswerOptions.Select(o => new
                    {
                        o.Text,
                        o.IsCorrect
                    }).ToList()
                }).ToList()
            })
            .ToListAsync(cancellationToken);

        if (assessments.Count != 1)
        {
            issues.Add(
                "The lesson requires one published mastery assessment.");
        }
        else
        {
            var assessment = assessments[0];

            if (assessment.PassMark < 0 ||
                assessment.PassMark > 100)
            {
                issues.Add(
                    "The mastery assessment pass mark must be between 0 and 100.");
            }

            if (assessment.Questions.Count == 0)
            {
                issues.Add(
                    "The mastery assessment must contain questions.");
            }

            if (assessment.Questions.Any(q =>
                    string.IsNullOrWhiteSpace(q.Text) ||
                    q.Marks <= 0 ||
                    q.Order <= 0 ||
                    q.Options.Count < 2 ||
                    q.Options.Count(o => o.IsCorrect) != 1 ||
                    q.Options.Any(o =>
                        string.IsNullOrWhiteSpace(o.Text))))
            {
                issues.Add(
                    "One or more mastery assessment questions are invalid.");
            }

            if (assessment.Questions
                    .Select(q => q.Order)
                    .Distinct()
                    .Count() != assessment.Questions.Count)
            {
                issues.Add(
                    "Mastery assessment question positions must be unique.");
            }
        }

        return new LessonPublicationReadinessDto
        {
            LessonId = request.LessonId,
            IsReady = issues.Count == 0,
            Issues = issues
        };
    }
}
