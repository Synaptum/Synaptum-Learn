using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;

namespace SynaptumLearn.Application.Schools.ListSchools;

public sealed class ListSchoolsQueryHandler
    : IRequestHandler<ListSchoolsQuery, IReadOnlyList<SchoolListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public ListSchoolsQueryHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<SchoolListItemDto>> Handle(
        ListSchoolsQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Schools
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new SchoolListItemDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                EMISNumber = x.EMISNumber,
                Email = x.Email,
                Phone = x.Phone,
                Province = x.Province,
                Status = x.Status,
                Slug = x.Slug,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}