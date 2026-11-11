
using MediatR;

namespace SynaptumLearn.Application.Classes.TransferLearner;

public sealed record TransferLearnerCommand(
    Guid EnrollmentId,
    Guid DestinationClassId
) : IRequest<Guid>;
