
using MediatR;

namespace SynaptumLearn.Application.Classes.ListEligibleClassTeachers;

public sealed record ListEligibleClassTeachersQuery(
    Guid ClassId
) : IRequest<IReadOnlyList<EligibleClassTeacherDto>>;
