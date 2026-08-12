using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;

namespace SynaptumLearn.Application.Schools.CreateSchool;

public sealed class CreateSchoolValidator
    : AbstractValidator<CreateSchoolCommand>
{
    private readonly IApplicationDbContext _context;

    public CreateSchoolValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.EMISNumber)
            .NotEmpty()
            .MaximumLength(50)
            .MustAsync(BeUniqueEmisNumber)
            .WithMessage("A school with this EMIS number already exists.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(150);

        RuleFor(x => x.Phone)
            .NotEmpty()
            .MaximumLength(30);

        RuleFor(x => x.Province)
            .IsInEnum();
    }

    private async Task<bool> BeUniqueEmisNumber(
        string emisNumber,
        CancellationToken cancellationToken)
    {
        var normalized = emisNumber.Trim();

        return !await _context.Schools
            .AnyAsync(
                x => x.EMISNumber == normalized,
                cancellationToken);
    }
}