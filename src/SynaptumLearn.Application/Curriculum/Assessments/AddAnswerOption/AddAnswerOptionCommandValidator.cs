
using FluentValidation;

namespace SynaptumLearn.Application.Curriculum.Assessments.AddAnswerOption;

public sealed class AddAnswerOptionCommandValidator
    : AbstractValidator<AddAnswerOptionCommand>
{
    public AddAnswerOptionCommandValidator()
    {
        RuleFor(x => x.QuestionId)
            .NotEmpty();

        RuleFor(x => x.Text)
            .NotEmpty()
            .MaximumLength(2000);
    }
}
