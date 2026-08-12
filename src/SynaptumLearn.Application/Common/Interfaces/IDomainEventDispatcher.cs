using SynaptumLearn.Domain.Common;

namespace SynaptumLearn.Application.Common.Interfaces;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default
    );
}