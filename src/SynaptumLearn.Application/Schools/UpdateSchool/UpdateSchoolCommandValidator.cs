using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;

namespace SynaptumLearn.Application.Schools.UpdateSchool;

public sealed class UpdateSchoolCommandValidator
    : AbstractValidator<UpdateSchoolCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateSchoolCommandValidator(
        IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.Id)
            .NotEmpty();

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
        UpdateSchoolCommand command,
        string emisNumber,
        CancellationToken cancellationToken)
    {
        var normalized = emisNumber.Trim();

        return !await _context.Schools
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.EMISNumber == normalized &&
                    x.Id != command.Id,
                cancellationToken);
    }
}