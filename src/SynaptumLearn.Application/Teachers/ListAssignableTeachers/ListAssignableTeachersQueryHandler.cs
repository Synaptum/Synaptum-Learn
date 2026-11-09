
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Domain.Enums;
using SynaptumLearn.Domain.Schools;

namespace SynaptumLearn.Application.Teachers.ListAssignableTeachers;

public sealed class ListAssignableTeachersQueryHandler
    : IRequestHandler<ListAssignableTeachersQuery,
        IReadOnlyList<AssignableTeacherDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ISchoolAdminAccessService _accessService;

    public ListAssignableTeachersQueryHandler(
        IApplicationDbContext context,
        ISchoolAdminAccessService accessService)
    {
        _context = context;
        _accessService = accessService;
    }

    public async Task<IReadOnlyList<AssignableTeacherDto>> Handle(
        ListAssignableTeachersQuery request,
        CancellationToken cancellationToken)
    {
        var schoolId = await _accessService
            .GetAuthorizedSchoolIdAsync(cancellationToken);

        return await _context.TeacherProfiles
            .AsNoTracking()
            .Where(t =>
                t.SchoolId == schoolId &&
                t.User.IsActive &&
                _context.SchoolMemberships.Any(m =>
                    m.SchoolId == schoolId &&
                    m.UserId == t.UserId &&
                    m.Role == SchoolRole.Teacher &&
                    m.IsActive))
            .OrderBy(t => t.User.LastName)
            .ThenBy(t => t.User.FirstName)
            .Select(t => new AssignableTeacherDto(
                t.Id,
                t.User.FirstName,
                t.User.LastName,
                t.EmployeeNumber))
            .ToListAsync(cancellationToken);
    }
}
