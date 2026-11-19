
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Application.Common.Security;
using SynaptumLearn.Domain.Assessments;
using SynaptumLearn.Domain.Content;

namespace SynaptumLearn.Application.Curriculum.Assessments.AddAnswerOption;

public sealed class AddAnswerOptionCommandHandler
    : IRequestHandler<AddAnswerOptionCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AddAnswerOptionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(
        AddAnswerOptionCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.IsInRole(PlatformRoles.PlatformAdmin))
        {
            throw new UnauthorizedAccessException(
                "Only Platform Administrators can add assessment answer options.");
        }

        if (request.QuestionId == Guid.Empty ||
            string.IsNullOrWhiteSpace(request.Text) ||
            request.Text.Length > 2000)
        {
            throw new InvalidOperationException(
                "Invalid answer option details.");
        }

        var question = await _context.Questions
            .AsNoTracking()
            .Where(x => x.Id == request.QuestionId)
            .Select(x => new
            {
                x.Id,
                x.AssessmentId,
                AssessmentPublished = x.Assessment.IsPublished,
                LessonStatus = x.Assessment.Lesson.Status,
                IsMasteryAssessment =
                    x.Assessment.IsRequiredForProgression
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (question is null)
            throw new InvalidOperationException(
                "Assessment question not found.");

        if (!question.IsMasteryAssessment)
        {
            throw new InvalidOperationException(
                "This question does not belong to a mastery assessment.");
        }

        if (question.AssessmentPublished ||
            question.LessonStatus != LessonStatus.Draft)
        {
            throw new InvalidOperationException(
                "Answer options can only be added to unpublished mastery assessments belonging to draft lessons.");
        }

        var existingOptions = await _context.AnswerOptions
            .AsNoTracking()
            .Where(x => x.QuestionId == request.QuestionId)
            .Select(x => new
            {
                x.IsCorrect,
                x.Text
            })
            .ToListAsync(cancellationToken);

        if (existingOptions.Count >= 10)
        {
            throw new InvalidOperationException(
                "A question cannot have more than 10 answer options.");
        }

        if (request.IsCorrect &&
            existingOptions.Any(x => x.IsCorrect))
        {
            throw new InvalidOperationException(
                "This question already has a correct answer option.");
        }

        if (existingOptions.Any(x =>
                x.Text.ToLower() == request.Text.Trim().ToLower()))
        {
            throw new InvalidOperationException(
                "An identical answer option already exists.");
        }

        var option = new AnswerOption
        {
            Id = Guid.NewGuid(),
            QuestionId = request.QuestionId,
            Text = request.Text.Trim(),
            IsCorrect = request.IsCorrect
        };

        _context.AnswerOptions.Add(option);

        await _context.SaveChangesAsync(cancellationToken);

        return option.Id;
    }
}
