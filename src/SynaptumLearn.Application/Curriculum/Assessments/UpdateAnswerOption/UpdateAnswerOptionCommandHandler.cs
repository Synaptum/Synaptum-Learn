
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Application.Common.Security;
using SynaptumLearn.Domain.Content;

namespace SynaptumLearn.Application.Curriculum.Assessments.UpdateAnswerOption;

public sealed class UpdateAnswerOptionCommandHandler
    : IRequestHandler<UpdateAnswerOptionCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateAnswerOptionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(
        UpdateAnswerOptionCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.IsInRole(PlatformRoles.PlatformAdmin))
        {
            throw new UnauthorizedAccessException(
                "Only Platform Administrators can edit answer options.");
        }

        var trimmedText = request.Text?.Trim();

        if (request.AnswerOptionId == Guid.Empty ||
            string.IsNullOrWhiteSpace(trimmedText) ||
            trimmedText.Length > 2000)
        {
            throw new InvalidOperationException(
                "Invalid answer option details.");
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

        if (!assessment.IsRequiredForProgression ||
            assessment.IsPublished ||
            assessment.Lesson.Status != LessonStatus.Draft)
        {
            throw new InvalidOperationException(
                "Only unpublished mastery assessments belonging to draft lessons can be edited.");
        }

        var otherOptions = await _context.AnswerOptions
            .Where(x =>
                x.QuestionId == option.QuestionId &&
                x.Id != option.Id)
            .ToListAsync(cancellationToken);

        if (otherOptions.Any(x =>
                string.Equals(
                    x.Text.Trim(),
                    trimmedText,
                    StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                "Another answer option already has this text.");
        }

        option.Text = trimmedText;

        if (request.IsCorrect)
        {
            foreach (var other in otherOptions)
            {
                if (other.IsCorrect)
                    other.IsCorrect = false;
            }

            option.IsCorrect = true;
        }
        else
        {
            option.IsCorrect = false;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return assessment.Id;
    }
}
