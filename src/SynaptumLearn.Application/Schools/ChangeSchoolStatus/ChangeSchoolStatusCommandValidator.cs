using FluentValidation;

namespace SynaptumLearn.Application.Schools.ChangeSchoolStatus;

public sealed class ChangeSchoolStatusCommandValidator
    : AbstractValidator<ChangeSchoolStatusCommand>
{
    public ChangeSchoolStatusCommandValidator()
    {
        RuleFor(x => x.SchoolId)
            .NotEmpty();

        RuleFor(x => x.NewStatus)
            .IsInEnum();
    }
}