using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Application.Common.Security;
using SynaptumLearn.Domain.Users;
using SynaptumLearn.Domain.Schools;
using SynaptumLearn.Domain.Enums;

namespace SynaptumLearn.Application.Users.CreateUser;

public sealed class CreateUserCommandHandler
    : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityUserProvisioningService _identityService;
    private readonly ICurrentUserService _currentUser;

    public CreateUserCommandHandler(
        IApplicationDbContext context,
        IIdentityUserProvisioningService identityService,
        ICurrentUserService currentUser)
    {
        _context = context;
        _identityService = identityService;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.IsInRole(PlatformRoles.PlatformAdmin))
        {
            throw new UnauthorizedAccessException(
                "Only Platform Administrator can create these accounts.");
        }

        if (!Enum.IsDefined(request.AccountType))
        {
            throw new InvalidOperationException(
                "Invalid account type.");
        }

        if (request.AccountType == UserAccountType.SchoolAdmin)
        {
            if (request.SchoolId is null ||
                request.SchoolId == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "A valid school must be selected.");
            }

            var schoolExists = await _context.Schools
                .AnyAsync(
                    s => s.Id == request.SchoolId,
                    cancellationToken);

            if (!schoolExists)
            {
                throw new InvalidOperationException(
                   "The selected school does not exist.");
            }
        }
        else if (request.SchoolId is not null)
        {
            throw new InvalidOperationException(
                "Platform Administrators cannot have a school assignment.");
        }

        var email = request.Email.Trim();

        var existingUser = await _context.BusinessUsers
            .AnyAsync(
                u => u.Email == email,
                cancellationToken);

        if (existingUser)
        {
            throw new InvalidOperationException(
                "A domain user with this email already exists.");
        }

        Guid domainUserId = Guid.Empty;

        await _context.ExecuteInTransactionAsync(
            async ct =>
            {
                var identityUserId =
                    await _identityService.CreateUserAsync(
                        email,
                        request.Password,
                        ct);

                var user = new User
                {
                    IdentityUserId = identityUserId,
                    FirstName = request.FirstName.Trim(),
                    LastName = request.LastName.Trim(),
                    Email = email,
                    IsActive = true
                };

                _context.BusinessUsers.Add(user);

                await _context.SaveChangesAsync(ct);

                if (request.AccountType == UserAccountType.PlatformAdmin)
                {
                    await _identityService.AssignRoleAsync(
                        identityUserId,
                        PlatformRoles.PlatformAdmin,
                        ct
                    );
                }
                else
                {
                    var membership = new SchoolMembership
                    {
                        SchoolId = request.SchoolId!.Value,
                        UserId = user.Id,
                        Role = SchoolRole.SchoolAdmin,
                        IsActive = true
                    };

                    _context.SchoolMemberships.Add(membership);
                    await _context.SaveChangesAsync(ct);
                }

                domainUserId = user.Id;
            },
            cancellationToken
        );

        return domainUserId;

    }
}