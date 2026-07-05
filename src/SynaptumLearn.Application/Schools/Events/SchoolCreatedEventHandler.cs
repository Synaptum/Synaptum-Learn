using MediatR;
using SynaptumLearn.Domain.Schools.Events;

namespace SynaptumLearn.Application.Schools.Events;

public sealed class SchoolCreatedEventHandler
    : INotificationHandler<SchoolCreatedEvent>
{
    public Task Handle(
        SchoolCreatedEvent notification,
        CancellationToken cancellationToken)
    {
        // Nothing yet.
        // We'll add logic here later.

        return Task.CompletedTask;
    }
}