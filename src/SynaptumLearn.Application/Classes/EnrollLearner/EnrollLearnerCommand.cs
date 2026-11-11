
using MediatR;

namespace SynaptumLearn.Application.Classes.EnrollLearner;

public sealed record EnrollLearnerCommand(
    Guid LearnerProfileId,
    Guid ClassId
) : IRequest<Guid>;
