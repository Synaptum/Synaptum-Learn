using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;

namespace SynaptumLearn.Application.Curriculum.Grades.UpdateGrade;

public sealed class UpdateGradeCommandHandler
    : IRequestHandler<UpdateGradeCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateGradeCommandHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(
        UpdateGradeCommand request,
        CancellationToken cancellationToken)
    {
        var grade = await _context.Grades
            .SingleOrDefaultAsync(
                x => x.Id == request.GradeId,
                cancellationToken);

        if (grade is null)
        {
            return false;
        }

        grade.Name = request.Name.Trim();
        grade.Order = request.Order;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}