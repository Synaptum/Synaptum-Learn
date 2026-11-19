
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Application.Common.Security;
using SynaptumLearn.Domain.Content;

namespace SynaptumLearn.Application.Curriculum.Assessments.DeleteAnswerOption;

public sealed class DeleteAnswerOptionCommandHandler
    : IRequestHandler<DeleteAnswerOptionCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteAnswerOptionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(
        DeleteAnswerOptionCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.IsInRole(PlatformRoles.PlatformAdmin))
        {
            throw new UnauthorizedAccessException(
                "Only Platform Administrators can delete answer options.");
        }

        if (request.AnswerOptionId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "A valid answer option ID is required.");
        }

        var option = await _context.AnswerOptions
            .Include(x => x.Question)
                .ThenInclude(x => x.Assessment)
                    .ThenInclude(x => x.Lesson)
            .FirstOrDefaultAsync(
                x => x.Id == request.AnswerOptionId,
                cancellationToken);

        if (option is null)
        {
            throw new InvalidOperationException(
                "Answer option not found.");
        }

        var assessment = option.Question.Assessment;

        if (!assessment.IsRequiredForProgression)
        {
            throw new InvalidOperationException(
                "This option does not belong to a mastery assessment.");
        }

        if (assessment.IsPublished ||
            assessment.Lesson.Status != LessonStatus.Draft)
        {
            throw new InvalidOperationException(
                "Answer options can only be deleted from unpublished mastery assessments belonging to draft lessons.");
        }

        var assessmentId = assessment.Id;

        _context.AnswerOptions.Remove(option);

        await _context.SaveChangesAsync(cancellationToken);

        return assessmentId;
    }
}
