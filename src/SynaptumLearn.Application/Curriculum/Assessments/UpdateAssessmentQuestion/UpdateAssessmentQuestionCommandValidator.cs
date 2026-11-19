
using FluentValidation;

namespace SynaptumLearn.Application.Curriculum.Assessments.UpdateAssessmentQuestion;

public sealed class UpdateAssessmentQuestionCommandValidator
    : AbstractValidator<UpdateAssessmentQuestionCommand>
{
    public UpdateAssessmentQuestionCommandValidator()
    {
        RuleFor(x => x.QuestionId)
            .NotEmpty();

        RuleFor(x => x.Text)
            .NotEmpty()
            .MaximumLength(5000);

        RuleFor(x => x.Marks)
            .GreaterThan(0);
    }
}
