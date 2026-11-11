
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;

namespace SynaptumLearn.Application.Classes.ListClasses;

public sealed class ListClassesQueryHandler
    : IRequestHandler<ListClassesQuery, IReadOnlyList<ClassListItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ISchoolAdminAccessService _accessService;

    public ListClassesQueryHandler(
        IApplicationDbContext context,
        ISchoolAdminAccessService accessService)
    {
        _context = context;
        _accessService = accessService;
    }

    public async Task<IReadOnlyList<ClassListItemDto>> Handle(
        ListClassesQuery request,
        CancellationToken cancellationToken)
    {
        var schoolId = await _accessService
            .GetAuthorizedSchoolIdAsync(cancellationToken);

        return await _context.Classes
            .AsNoTracking()
            .Where(c => c.SchoolId == schoolId)
            .OrderByDescending(c => c.AcademicYear)
            .ThenBy(c => c.Grade.Order)
            .ThenBy(c => c.Name)
            .Select(c => new ClassListItemDto(
                c.Id,
                c.Name,
                c.GradeId,
                c.Grade.Name,
                c.AcademicYear,
                c.LearnerClasses.Count(lc => lc.LeftAt == null)
            ))
            .ToListAsync(cancellationToken);
    }
}
