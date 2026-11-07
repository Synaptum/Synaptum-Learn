
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Application.Common.Security;
using SynaptumLearn.Domain.Enums;

namespace SynaptumLearn.Application.Users.ListManagedUsers;

public sealed class ListManagedUsersQueryHandler
    : IRequestHandler<
        ListManagedUsersQuery,
        IReadOnlyList<ManagedUserListItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityUserProvisioningService _identityService;
    private readonly ICurrentUserService _currentUser;

    public ListManagedUsersQueryHandler(
        IApplicationDbContext context,
        IIdentityUserProvisioningService identityService,
        ICurrentUserService currentUser)
    {
        _context = context;
        _identityService = identityService;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<ManagedUserListItemDto>> Handle(
        ListManagedUsersQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.IsInRole(PlatformRoles.PlatformAdmin))
        {
            throw new UnauthorizedAccessException(
                "Only Platform Administrators can view managed users.");
        }

        var platformAdminIdentityIds =
            await _identityService.GetUserIdsInRoleAsync(
                PlatformRoles.PlatformAdmin,
                cancellationToken);

        var platformAdmins = await _context.BusinessUsers
            .AsNoTracking()
            .Where(u =>
                platformAdminIdentityIds.Contains(u.IdentityUserId))
            .Select(u => new ManagedUserListItemDto(
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                "Platform Administrator",
                null,
                null,
                u.IsActive))
            .ToListAsync(cancellationToken);

        var schoolAdmins = await (
            from membership in _context.SchoolMemberships.AsNoTracking()
            join user in _context.BusinessUsers.AsNoTracking()
                on membership.UserId equals user.Id
            join school in _context.Schools.AsNoTracking()
                on membership.SchoolId equals school.Id
            where membership.Role == SchoolRole.SchoolAdmin
            select new ManagedUserListItemDto(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                "School Administrator",
                school.Id,
                school.Name,
                user.IsActive && membership.IsActive)
        ).ToListAsync(cancellationToken);

        return platformAdmins
            .Concat(schoolAdmins)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToList();
    }
}
