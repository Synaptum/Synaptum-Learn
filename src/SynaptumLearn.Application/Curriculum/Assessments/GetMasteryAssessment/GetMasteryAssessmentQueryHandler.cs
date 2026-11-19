
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Application.Common.Security;

namespace SynaptumLearn.Application.Curriculum.Assessments.GetMasteryAssessment;

public sealed class GetMasteryAssessmentQueryHandler
    : IRequestHandler<GetMasteryAssessmentQuery, MasteryAssessmentDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMasteryAssessmentQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<MasteryAssessmentDto?> Handle(
        GetMasteryAssessmentQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.IsInRole(PlatformRoles.PlatformAdmin))
        {
            throw new UnauthorizedAccessException(
                "Only Platform Administrators can manage mastery assessments.");
        }

        return await _context.Assessments
            .AsNoTracking()
            .Where(x =>
                x.Id == request.AssessmentId &&
                x.IsRequiredForProgression)
            .Select(x => new MasteryAssessmentDto
            {
                Id = x.Id,
                LessonId = x.LessonId,
                LessonTitle = x.Lesson.Title,
                Title = x.Title,
                Instructions = x.Instructions,
                PassMark = x.PassMark,
                IsPublished = x.IsPublished,
                Questions = x.Questions
                    .OrderBy(q => q.Order)
                    .Select(q => new MasteryAssessmentQuestionDto
                    {
                        Id = q.Id,
                        Text = q.Text,
                        Marks = q.Marks,
                        Order = q.Order,
                        AnswerOptionCount = q.AnswerOptions.Count,

                        AnswerOptions = q.AnswerOptions
                            .OrderBy(o => o.Text)
                            .Select(o => new MasteryAssessmentAnswerOptionDto
                            {
                                Id = o.Id,
                                Text = o.Text,
                                IsCorrect = o.IsCorrect
                            })
                            .ToList()
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
