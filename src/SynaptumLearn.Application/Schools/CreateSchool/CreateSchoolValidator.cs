using FluentValidation;

namespace SynaptumLearn.Application.Schools.CreateSchool;

public sealed class CreateSchoolValidator : AbstractValidator<CreateSchoolCommand>
{
    public CreateSchoolValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.EMISNumber)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(150);

        RuleFor(x => x.Phone)
            .NotEmpty()
            .MaximumLength(30);

        RuleFor(x => x.Province)
            .NotEmpty()
            .MaximumLength(100);
    }
}