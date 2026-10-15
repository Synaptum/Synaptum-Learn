using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;

namespace SynaptumLearn.Application.Curriculum.Subjects.ListSubjects;

public sealed class ListSubjectsQueryHandler
    : IRequestHandler<ListSubjectsQuery, IReadOnlyList<SubjectListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public ListSubjectsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<SubjectListItemDto>> Handle(
        ListSubjectsQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Subjects
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new SubjectListItemDto
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}