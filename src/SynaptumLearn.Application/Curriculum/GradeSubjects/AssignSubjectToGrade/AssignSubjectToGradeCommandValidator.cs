using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;

namespace SynaptumLearn.Application.Curriculum.GradeSubjects.AssignSubjectToGrade;

public sealed class AssignSubjectToGradeCommandValidator
    : AbstractValidator<AssignSubjectToGradeCommand>
{
    private readonly IApplicationDbContext _context;

    public AssignSubjectToGradeCommandValidator(
        IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.GradeId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MustAsync(GradeExists)
            .WithMessage("The selected grade does not exist.");

        RuleFor(x => x.SubjectId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MustAsync(SubjectExists)
            .WithMessage("The selected subject does not exist.");

        RuleFor(x => x)
            .MustAsync(AssignmentDoesNotExist)
            .When(x =>
                x.GradeId != Guid.Empty &&
                x.SubjectId != Guid.Empty)
            .WithMessage(
                "This subject is already assigned to the selected grade.");
    }

    private async Task<bool> GradeExists(
        Guid gradeId,
        CancellationToken cancellationToken)
    {
        return await _context.Grades
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == gradeId,
                cancellationToken);
    }

    private async Task<bool> SubjectExists(
        Guid subjectId,
        CancellationToken cancellationToken)
    {
        return await _context.Subjects
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == subjectId,
                cancellationToken);
    }

    private async Task<bool> AssignmentDoesNotExist(
        AssignSubjectToGradeCommand command,
        CancellationToken cancellationToken)
    {
        return !await _context.GradeSubjects
            .AsNoTracking()
            .AnyAsync(
                x => x.GradeId == command.GradeId &&
                     x.SubjectId == command.SubjectId,
                cancellationToken);
    }
}