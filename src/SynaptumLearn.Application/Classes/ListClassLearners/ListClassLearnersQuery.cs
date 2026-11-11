
using MediatR;

namespace SynaptumLearn.Application.Classes.ListClassLearners;

public sealed record ListClassLearnersQuery(Guid ClassId)
    : IRequest<IReadOnlyList<ClassLearnerDto>>;
