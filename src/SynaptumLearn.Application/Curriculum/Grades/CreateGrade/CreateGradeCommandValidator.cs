using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;

namespace SynaptumLearn.Application.Curriculum.Grades.CreateGrade;

public sealed class CreateGradeCommandValidator
    : AbstractValidator<CreateGradeCommand>
{
    private readonly IApplicationDbContext _context;

    public CreateGradeCommandValidator(
        IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(50)
            .MustAsync(BeUniqueName)
            .WithMessage("A grade with this name already exists.");

        RuleFor(x => x.Order)
            .GreaterThan(0);
    }

    private async Task<bool> BeUniqueName(
        string name,
        CancellationToken cancellationToken)
    {
        var normalized = name.Trim();

        return !await _context.Grades
            .AsNoTracking()
            .AnyAsync(
                x => x.Name == normalized,
                cancellationToken);
    }
}