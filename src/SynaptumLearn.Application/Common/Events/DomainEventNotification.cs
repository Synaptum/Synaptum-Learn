using MediatR;
using SynaptumLearn.Domain.Common;

namespace SynaptumLearn.Application.Common.Events;

public sealed record DomainEventNotification<TDomainEvent>(
    TDomainEvent DomainEvent) : INotification
    where TDomainEvent : IDomainEvent;