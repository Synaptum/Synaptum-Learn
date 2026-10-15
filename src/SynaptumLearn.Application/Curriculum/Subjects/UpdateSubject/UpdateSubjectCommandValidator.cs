using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;

namespace SynaptumLearn.Application.Curriculum.Subjects.UpdateSubject;

public sealed class UpdateSubjectCommandValidator
    : AbstractValidator<UpdateSubjectCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateSubjectCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.SubjectId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Code)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(20)
            .MustAsync(BeUniqueCode)
            .WithMessage("A subject with this code already exists.");
    }

    private async Task<bool> BeUniqueCode(
        UpdateSubjectCommand command,
        string code,
        CancellationToken cancellationToken)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();

        return !await _context.Subjects
            .AsNoTracking()
            .AnyAsync(
                x => x.Code == normalizedCode
                    && x.Id != command.SubjectId,
                cancellationToken);
    }
}