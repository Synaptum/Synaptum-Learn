using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;

namespace SynaptumLearn.Application.Curriculum.Grades.UpdateGrade;

public sealed class UpdateGradeCommandValidator
    : AbstractValidator<UpdateGradeCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateGradeCommandValidator(
        IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.GradeId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(50)
            .MustAsync(BeUniqueName)
            .WithMessage("A grade with this name already exists.");

        RuleFor(x => x.Order)
            .GreaterThan(0);
    }

    private async Task<bool> BeUniqueName(
        UpdateGradeCommand command,
        string name,
        CancellationToken cancellationToken)
    {
        var normalized = name.Trim();

        return !await _context.Grades
            .AsNoTracking()
            .AnyAsync(
                x => x.Name == normalized
                    && x.Id != command.GradeId,
                cancellationToken);
    }
}