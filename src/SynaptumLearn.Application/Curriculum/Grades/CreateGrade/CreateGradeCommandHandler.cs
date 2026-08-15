using MediatR;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Domain.Curriculum;

namespace SynaptumLearn.Application.Curriculum.Grades.CreateGrade;

public sealed class CreateGradeCommandHandler
    : IRequestHandler<CreateGradeCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateGradeCommandHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(
        CreateGradeCommand request,
        CancellationToken cancellationToken)
    {
        var grade = new Grade
        {
            Name = request.Name.Trim(),
            Order = request.Order
        };

        _context.Grades.Add(grade);

        await _context.SaveChangesAsync(cancellationToken);

        return grade.Id;
    }
}