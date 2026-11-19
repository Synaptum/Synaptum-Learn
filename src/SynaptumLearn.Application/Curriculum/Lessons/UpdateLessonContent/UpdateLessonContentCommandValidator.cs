
using FluentValidation;
using SynaptumLearn.Domain.Content;

namespace SynaptumLearn.Application.Curriculum.Lessons.UpdateLessonContent;

public sealed class UpdateLessonContentCommandValidator
    : AbstractValidator<UpdateLessonContentCommand>
{
    public UpdateLessonContentCommandValidator()
    {
        RuleFor(x => x.ContentId)
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
    }
}
