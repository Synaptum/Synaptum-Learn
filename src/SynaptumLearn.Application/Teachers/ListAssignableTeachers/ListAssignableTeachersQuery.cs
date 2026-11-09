
using MediatR;

namespace SynaptumLearn.Application.Teachers.ListAssignableTeachers;

public sealed record ListAssignableTeachersQuery()
    : IRequest<IReadOnlyList<AssignableTeacherDto>>;
