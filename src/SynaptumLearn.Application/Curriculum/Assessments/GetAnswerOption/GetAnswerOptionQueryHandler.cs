
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Application.Common.Security;
using SynaptumLearn.Domain.Content;

namespace SynaptumLearn.Application.Curriculum.Assessments.GetAnswerOption;

public sealed class GetAnswerOptionQueryHandler
    : IRequestHandler<GetAnswerOptionQuery, AnswerOptionDetailsDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetAnswerOptionQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<AnswerOptionDetailsDto?> Handle(
        GetAnswerOptionQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.IsInRole(PlatformRoles.PlatformAdmin))
        {
            throw new UnauthorizedAccessException(
                "Only Platform Administrators can manage answer options.");
        }

        if (request.AnswerOptionId == Guid.Empty)
            return null;

        return await _context.AnswerOptions
            .AsNoTracking()
            .Where(x =>
                x.Id == request.AnswerOptionId &&
                x.Question.Assessment.IsRequiredForProgression)
            .Select(x => new AnswerOptionDetailsDto
            {
                Id = x.Id,
                AssessmentId = x.Question.AssessmentId,
                QuestionText = x.Question.Text,
                Text = x.Text,
                IsCorrect = x.IsCorrect,
                CanEdit =
                    !x.Question.Assessment.IsPublished &&
                    x.Question.Assessment.Lesson.Status == LessonStatus.Draft
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
