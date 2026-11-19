
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;

namespace SynaptumLearn.Application.Curriculum.Lessons.CreateLesson;

public sealed class CreateLessonCommandValidator
    : AbstractValidator<CreateLessonCommand>
{
    public CreateLessonCommandValidator(
        IApplicationDbContext context)
    {
        RuleFor(x => x.TopicId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MustAsync(async (topicId, cancellationToken) =>
                await context.Topics
                    .AsNoTracking()
                    .AnyAsync(
                        x => x.Id == topicId,
                        cancellationToken))
            .WithMessage("The selected topic does not exist.");

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
                !await context.Lessons
                    .AsNoTracking()
                    .AnyAsync(
                        x => x.TopicId == command.TopicId &&
                             x.Order == command.Order,
                        cancellationToken))
            .When(x =>
                x.TopicId != Guid.Empty &&
                x.Order > 0)
            .WithMessage(
                "Another lesson already occupies this position in the selected topic.");
    }
}
