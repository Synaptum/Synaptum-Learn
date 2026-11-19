
using FluentValidation;

namespace SynaptumLearn.Application.Curriculum.Assessments.AddAssessmentQuestion;

public sealed class AddAssessmentQuestionCommandValidator
    : AbstractValidator<AddAssessmentQuestionCommand>
{
    public AddAssessmentQuestionCommandValidator()
    {
        RuleFor(x => x.AssessmentId)
            .NotEmpty();

        RuleFor(x => x.Text)
            .NotEmpty()
            .MaximumLength(5000);

        RuleFor(x => x.Marks)
            .GreaterThan(0);

        RuleFor(x => x.Order)
            .GreaterThan(0);
    }
}
