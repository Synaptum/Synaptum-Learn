
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;

namespace SynaptumLearn.Application.Curriculum.Lessons.UpdateLesson;

public sealed class UpdateLessonCommandValidator
    : AbstractValidator<UpdateLessonCommand>
{
    public UpdateLessonCommandValidator(
        IApplicationDbContext context)
    {
        RuleFor(x => x.LessonId)
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Summary)
            .NotNull()
            .MaximumLength(3000);

        RuleFor(x => x.Order)
            .GreaterThan(0);

        RuleFor(x => x)
            .MustAsync(async (command, cancellationToken) =>
            {
                var lesson = await context.Lessons
                    .AsNoTracking()
                    .Where(x => x.Id == command.LessonId)
                    .Select(x => new { x.TopicId })
                    .FirstOrDefaultAsync(cancellationToken);

                if (lesson is null)
                    return false;

                return !await context.Lessons
                    .AsNoTracking()
                    .AnyAsync(
                        x => x.TopicId == lesson.TopicId &&
                             x.Order == command.Order &&
                             x.Id != command.LessonId,
                        cancellationToken);
            })
            .When(x =>
                x.LessonId != Guid.Empty &&
                x.Order > 0)
            .WithMessage(
                "The lesson does not exist or another lesson already occupies the selected position.");
    }
}
