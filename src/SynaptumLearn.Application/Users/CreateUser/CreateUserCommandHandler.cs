using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Domain.Users;

namespace SynaptumLearn.Application.Users.CreateUser;

public sealed class CreateUserCommandHandler
    : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityUserProvisioningService _identityService;

    public CreateUserCommandHandler(
        IApplicationDbContext context,
        IIdentityUserProvisioningService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<Guid> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
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

                domainUserId = user.Id;
            },
            cancellationToken
        );

        return domainUserId;

    }
}