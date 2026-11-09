
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Application.Common.Security;
using SynaptumLearn.Domain.Common;

namespace SynaptumLearn.Tests;

public sealed class TestCurrentUserService : ICurrentUserService
{
    public Guid? UserId { get; set; }

    public Guid? SchoolId { get; set; }

    public bool IsAuthenticated { get; set; } = true;

    public string? IdentityUserId { get; set; }

    public string? Email { get; set; }

    public bool IsPlatformAdmin { get; set; } = true;

    public bool IsInRole(string role)
    {
        return IsPlatformAdmin &&
            role == PlatformRoles.PlatformAdmin;
    }
}

public sealed class TestDomainEventDispatcher : IDomainEventDispatcher
{
    public Task DispatchAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
