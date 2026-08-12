using MediatR;
using Microsoft.Extensions.Logging;
using SynaptumLearn.Domain.Schools.Events;
using SynaptumLearn.Application.Common.Events;

namespace SynaptumLearn.Application.Schools.Events;

public sealed class SchoolCreatedEventHandler
    : INotificationHandler<DomainEventNotification<SchoolCreatedEvent>>
{
    private readonly ILogger<SchoolCreatedEventHandler> _logger;

    public SchoolCreatedEventHandler(
        ILogger<SchoolCreatedEventHandler> logger)
    {
        _logger = logger;
    }
    public Task Handle(
        DomainEventNotification<SchoolCreatedEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation(
            "School {SchoolId} was created.", domainEvent.SchoolId);
        // Nothing yet.
        // We'll add logic here later.

        return Task.CompletedTask;
    }
}