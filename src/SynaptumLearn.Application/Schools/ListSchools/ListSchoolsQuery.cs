using MediatR;

namespace SynaptumLearn.Application.Schools.ListSchools;

public sealed record ListSchoolsQuery
    : IRequest<IReadOnlyList<SchoolListItemDto>>;