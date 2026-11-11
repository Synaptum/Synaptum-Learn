
using MediatR;

namespace SynaptumLearn.Application.Classes.ListClasses;

public sealed record ListClassesQuery()
    : IRequest<IReadOnlyList<ClassListItemDto>>;
