
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Application.Common.Security;
using SynaptumLearn.Domain.Common;

namespace SynaptumLearn.Tests;

public sealed class TestCurrentUserService : ICurrentUserService
{
    public Guid? UserId => null;

    public Guid? SchoolId => null;

    public bool IsAuthenticated => true;

    public string? IdentityUserId => null;

    public string? Email => null;

    public bool IsInRole(string role)
    {
        return role == PlatformRoles.PlatformAdmin;
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
