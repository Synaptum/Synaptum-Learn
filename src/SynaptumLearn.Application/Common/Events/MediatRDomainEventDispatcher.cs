using MediatR;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Domain.Common;

namespace SynaptumLearn.Application.Common.Events;

public sealed class MediatRDomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IPublisher _publisher;

    public MediatRDomainEventDispatcher(IPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task DispatchAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            var notificationType = typeof(DomainEventNotification<>)
            .MakeGenericType(domainEvent.GetType());

            var notification = Activator.CreateInstance(
                notificationType,
                domainEvent);

            if (notification is null)
            {
                throw new InvalidOperationException(
                    $"Could not create notification for domain event " +
                    $"{domainEvent.GetType().Name}.");
            }

            await _publisher.Publish(notification, cancellationToken);
        }
    }
}