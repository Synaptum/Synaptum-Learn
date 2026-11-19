
using FluentValidation;

namespace SynaptumLearn.Application.Curriculum.Assessments.CreateMasteryAssessment;

public sealed class CreateMasteryAssessmentCommandValidator
    : AbstractValidator<CreateMasteryAssessmentCommand>
{
    public CreateMasteryAssessmentCommandValidator()
    {
        RuleFor(x => x.LessonId)
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Instructions)
            .NotNull()
            .MaximumLength(3000);

        RuleFor(x => x.PassMark)
            .InclusiveBetween(1, 100);
    }
}
