
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Domain.Content;

namespace SynaptumLearn.Application.Curriculum.Lessons.AddLessonContent;

public sealed class AddLessonContentCommandValidator
    : AbstractValidator<AddLessonContentCommand>
{
    public AddLessonContentCommandValidator(
        IApplicationDbContext context)
    {
        RuleFor(x => x.LessonId)
            .NotEmpty();

        RuleFor(x => x.Type)
            .IsInEnum();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Content)
            .NotEmpty()
            .MaximumLength(10000);

        RuleFor(x => x.Order)
            .GreaterThan(0);

        RuleFor(x => x.LessonId)
            .MustAsync(async (lessonId, cancellationToken) =>
                await context.Lessons
                    .AsNoTracking()
                    .AnyAsync(
                        x => x.Id == lessonId &&
                             x.Status == LessonStatus.Draft,
                        cancellationToken))
            .WithMessage(
                "Content can only be added to an existing draft lesson.");

        RuleFor(x => x)
            .MustAsync(async (command, cancellationToken) =>
                !await context.LessonContents
                    .AsNoTracking()
                    .AnyAsync(
                        x => x.LessonId == command.LessonId &&
                             x.Order == command.Order,
                        cancellationToken))
            .WithMessage(
                "Another content block already occupies this position.");
    }
}
