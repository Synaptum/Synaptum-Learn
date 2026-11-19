
using FluentValidation;

namespace SynaptumLearn.Application.Curriculum.Assessments.UpdateAnswerOption;

public sealed class UpdateAnswerOptionCommandValidator
    : AbstractValidator<UpdateAnswerOptionCommand>
{
    public UpdateAnswerOptionCommandValidator()
    {
        RuleFor(x => x.AnswerOptionId)
            .NotEmpty();

        RuleFor(x => x.Text)
            .NotEmpty()
            .MaximumLength(2000);
    }
}
