using FluentValidation;

namespace SynaptumLearn.Application.Users.CreateUser;

public class CreateUserCommandValidator
    : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8);

        RuleFor(x => x.AccountType)
            .IsInEnum()
            .WithMessage("Select a valid account type.");

        RuleFor(x => x.SchoolId)
            .NotNull()
            .WithMessage("A school must be selected for a School Administrator.")
            .When(x => x.AccountType == UserAccountType.SchoolAdmin);

        RuleFor(x => x.SchoolId)
            .Null()
            .WithMessage("A Platform Administrator must not be assigned to a school.")
            .When(x => x.AccountType == UserAccountType.PlatformAdmin);
    }
}