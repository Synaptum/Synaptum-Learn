using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;

namespace SynaptumLearn.Application.Curriculum.Topics.CreateTopic;

public sealed class CreateTopicCommandValidator
    : AbstractValidator<CreateTopicCommand>
{
    private readonly IApplicationDbContext _context;

    public CreateTopicCommandValidator(
        IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.GradeSubjectId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MustAsync(GradeSubjectExists)
            .WithMessage(
                "The selected grade-subject combination does not exist.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(2000);

        RuleFor(x => x.Term)
            .InclusiveBetween(1, 4);

        RuleFor(x => x.Order)
            .GreaterThan(0);

        RuleFor(x => x)
            .MustAsync(PositionIsAvailable)
            .When(x =>
                x.GradeSubjectId != Guid.Empty &&
                x.Term is >= 1 and <= 4 &&
                x.Order > 0)
            .WithMessage(
                "Another topic already occupies this position in the selected term.");
    }

    private async Task<bool> GradeSubjectExists(
        Guid gradeSubjectId,
        CancellationToken cancellationToken)
    {
        return await _context.GradeSubjects
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == gradeSubjectId,
                cancellationToken);
    }

    private async Task<bool> PositionIsAvailable(
        CreateTopicCommand command,
        CancellationToken cancellationToken)
    {
        return !await _context.Topics
            .AsNoTracking()
            .AnyAsync(
                x => x.GradeSubjectId == command.GradeSubjectId &&
                     x.Term == command.Term &&
                     x.Order == command.Order,
                cancellationToken);
    }
}