
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Application.Common.Security;
using SynaptumLearn.Domain.Assessments;
using SynaptumLearn.Domain.Content;

namespace SynaptumLearn.Application.Curriculum.Assessments.AddAssessmentQuestion;

public sealed class AddAssessmentQuestionCommandHandler
    : IRequestHandler<AddAssessmentQuestionCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AddAssessmentQuestionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(
        AddAssessmentQuestionCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.IsInRole(PlatformRoles.PlatformAdmin))
        {
            throw new UnauthorizedAccessException(
                "Only Platform Administrators can add mastery assessment questions.");
        }

        if (request.AssessmentId == Guid.Empty ||
            string.IsNullOrWhiteSpace(request.Text) ||
            request.Text.Length > 5000 ||
            request.Marks <= 0 ||
            request.Order <= 0)
        {
            throw new InvalidOperationException(
                "Invalid assessment question details.");
        }

        var assessment = await _context.Assessments
            .AsNoTracking()
            .Where(x => x.Id == request.AssessmentId)
            .Select(x => new
            {
                x.Id,
                x.IsPublished,
                LessonStatus = x.Lesson.Status
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (assessment is null)
            throw new InvalidOperationException(
                "Mastery assessment not found.");

        if (assessment.IsPublished ||
            assessment.LessonStatus != LessonStatus.Draft)
        {
            throw new InvalidOperationException(
                "Questions can only be added to unpublished assessments belonging to draft lessons.");
        }

        var positionOccupied = await _context.Questions
            .AsNoTracking()
            .AnyAsync(
                x => x.AssessmentId == request.AssessmentId &&
                     x.Order == request.Order,
                cancellationToken);

        if (positionOccupied)
        {
            throw new InvalidOperationException(
                "Another question already occupies this position.");
        }

        var question = new Question
        {
            Id = Guid.NewGuid(),
            AssessmentId = request.AssessmentId,
            Text = request.Text.Trim(),
            Marks = request.Marks,
            Order = request.Order
        };

        _context.Questions.Add(question);

        await _context.SaveChangesAsync(cancellationToken);

        return question.Id;
    }
}
