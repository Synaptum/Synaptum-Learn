
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Application.Common.Security;
using SynaptumLearn.Domain.Content;

namespace SynaptumLearn.Application.Curriculum.Assessments.GetAssessmentQuestion;

public sealed class GetAssessmentQuestionQueryHandler
    : IRequestHandler<GetAssessmentQuestionQuery, AssessmentQuestionDetailsDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetAssessmentQuestionQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<AssessmentQuestionDetailsDto?> Handle(
        GetAssessmentQuestionQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.IsInRole(PlatformRoles.PlatformAdmin))
        {
            throw new UnauthorizedAccessException(
                "Only Platform Administrators can manage assessment questions.");
        }

        if (request.QuestionId == Guid.Empty)
            return null;

        return await _context.Questions
            .AsNoTracking()
            .Where(x =>
                x.Id == request.QuestionId &&
                x.Assessment.IsRequiredForProgression)
            .Select(x => new AssessmentQuestionDetailsDto
            {
                Id = x.Id,
                AssessmentId = x.AssessmentId,
                Text = x.Text,
                Marks = x.Marks,
                Order = x.Order,
                CanEdit =
                    !x.Assessment.IsPublished &&
                    x.Assessment.Lesson.Status == LessonStatus.Draft
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
