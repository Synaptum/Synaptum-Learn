using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Domain.Enums;

namespace SynaptumLearn.Application.Common.Security;

public sealed class SchoolAdminAccessService
    : ISchoolAdminAccessService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public SchoolAdminAccessService(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> GetAuthorizedSchoolIdAsync(
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.UserId.HasValue)
        {
            throw new UnauthorizedAccessException(
                "An authenticated school administrator is required.");
        }

        var schoolIds = await _context.SchoolMemberships
            .AsNoTracking()
            .Where(m =>
                m.UserId == _currentUser.UserId.Value &&
                m.Role == SchoolRole.SchoolAdmin &&
                m.IsActive &&
                m.User.IsActive &&
                m.School.Status == SchoolStatus.Active)
            .Select(m => m.SchoolId)
            .Distinct()
            .Take(2)
            .ToListAsync(cancellationToken);

        if (schoolIds.Count != 1)
        {
            throw new UnauthorizedAccessException(
                "A unique active school administrator membership is required.");
        }

        return schoolIds[0];
    }
}