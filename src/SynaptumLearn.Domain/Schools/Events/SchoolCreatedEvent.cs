using SynaptumLearn.Domain.Common;

namespace SynaptumLearn.Domain.Schools.Events;

public sealed record SchoolCreatedEvent(Guid SchoolId) : IDomainEvent;