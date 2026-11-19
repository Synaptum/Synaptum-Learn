
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Application.Common.Security;
using SynaptumLearn.Domain.Content;

namespace SynaptumLearn.Application.Curriculum.Assessments.UpdateAssessmentQuestion;

public sealed class UpdateAssessmentQuestionCommandHandler
    : IRequestHandler<UpdateAssessmentQuestionCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateAssessmentQuestionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(
        UpdateAssessmentQuestionCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.IsInRole(PlatformRoles.PlatformAdmin))
        {
            throw new UnauthorizedAccessException(
                "Only Platform Administrators can edit assessment questions.");
        }

        var trimmedText = request.Text?.Trim();

        if (request.QuestionId == Guid.Empty ||
            string.IsNullOrWhiteSpace(trimmedText) ||
            trimmedText.Length > 5000 ||
            request.Marks <= 0)
        {
            throw new InvalidOperationException(
                "Invalid assessment question details.");
        }

        var question = await _context.Questions
            .Include(x => x.Assessment)
                .ThenInclude(x => x.Lesson)
            .FirstOrDefaultAsync(
                x => x.Id == request.QuestionId,
                cancellationToken);

        if (question is null)
        {
            throw new InvalidOperationException(
                "Assessment question not found.");
        }

        if (!question.Assessment.IsRequiredForProgression)
        {
            throw new InvalidOperationException(
                "This question does not belong to a mastery assessment.");
        }

        if (question.Assessment.IsPublished ||
            question.Assessment.Lesson.Status != LessonStatus.Draft)
        {
            throw new InvalidOperationException(
                "Questions can only be edited in unpublished mastery assessments belonging to draft lessons.");
        }

        question.Text = trimmedText;
        question.Marks = request.Marks;

        await _context.SaveChangesAsync(cancellationToken);

        return question.AssessmentId;
    }
}
