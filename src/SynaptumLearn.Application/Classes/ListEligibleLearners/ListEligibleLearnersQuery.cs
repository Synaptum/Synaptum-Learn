
using MediatR;

namespace SynaptumLearn.Application.Classes.ListEligibleLearners;

public sealed record ListEligibleLearnersQuery(Guid ClassId)
    : IRequest<IReadOnlyList<EligibleLearnerDto>>;
