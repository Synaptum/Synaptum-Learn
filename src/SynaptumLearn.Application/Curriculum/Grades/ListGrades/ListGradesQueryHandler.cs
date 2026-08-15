using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;

namespace SynaptumLearn.Application.Curriculum.Grades.ListGrades;

public sealed class ListGradesQueryHandler
    : IRequestHandler<ListGradesQuery, IReadOnlyList<GradeListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public ListGradesQueryHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<GradeListItemDto>> Handle(
        ListGradesQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Grades
            .AsNoTracking()
            .OrderBy(x => x.Order)
            .Select(x => new GradeListItemDto
            {
                Id = x.Id,
                Name = x.Name,
                Order = x.Order,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}