
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Domain.Enums;

namespace SynaptumLearn.Application.Users.ListSchoolUsers;

public sealed class ListSchoolUsersQueryHandler
    : IRequestHandler<
        ListSchoolUsersQuery,
        IReadOnlyList<SchoolUserListItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ISchoolAdminAccessService _accessService;

    public ListSchoolUsersQueryHandler(
        IApplicationDbContext context,
        ISchoolAdminAccessService accessService)
    {
        _context = context;
        _accessService = accessService;
    }

    public async Task<IReadOnlyList<SchoolUserListItemDto>> Handle(
        ListSchoolUsersQuery request,
        CancellationToken cancellationToken)
    {
        var schoolId = await _accessService
            .GetAuthorizedSchoolIdAsync(cancellationToken);

        var teachers = await (
            from profile in _context.TeacherProfiles.AsNoTracking()
            join user in _context.BusinessUsers.AsNoTracking()
                on profile.UserId equals user.Id
            join membership in _context.SchoolMemberships.AsNoTracking()
                on new { profile.SchoolId, profile.UserId }
                equals new { membership.SchoolId, membership.UserId }
            where profile.SchoolId == schoolId
                && membership.Role == SchoolRole.Teacher
            select new SchoolUserListItemDto(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                "Teacher",
                profile.EmployeeNumber,
                null,
                user.IsActive && membership.IsActive)
        ).ToListAsync(cancellationToken);

        var learners = await (
            from profile in _context.LearnerProfiles.AsNoTracking()
            join user in _context.BusinessUsers.AsNoTracking()
                on profile.UserId equals user.Id
            join membership in _context.SchoolMemberships.AsNoTracking()
                on new { profile.SchoolId, profile.UserId }
                equals new { membership.SchoolId, membership.UserId }
            join grade in _context.Grades.AsNoTracking()
                on profile.GradeId equals grade.Id
            where profile.SchoolId == schoolId
                && membership.Role == SchoolRole.Learner
            select new SchoolUserListItemDto(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                "Learner",
                profile.LearnerNumber,
                grade.Name,
                user.IsActive && membership.IsActive)
        ).ToListAsync(cancellationToken);

        return teachers
            .Concat(learners)
            .OrderBy(u => u.AccountType)
            .ThenBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToList();
    }
}
