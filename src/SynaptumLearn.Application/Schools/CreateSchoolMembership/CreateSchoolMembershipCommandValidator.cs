using FluentValidation;
using SynaptumLearn.Domain.Enums;

namespace SynaptumLearn.Application.Schools.CreateSchoolMembership;

public sealed class CreateSchoolMembershipCommandValidator
    : AbstractValidator<CreateSchoolMembershipCommand>
{
    public CreateSchoolMembershipCommandValidator()
    {
        RuleFor(x => x.SchoolId)
            .NotEmpty()
            .WithMessage("A school must be selected.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("A user must be selected.");

        RuleFor(x => x.Role)
            .IsInEnum()
            .WithMessage("A valid school role must be selected.");
    }
}