
using MediatR;

namespace SynaptumLearn.Application.Classes.ListClassTeachers;

public sealed record ListClassTeachersQuery(
    Guid ClassId
) : IRequest<IReadOnlyList<ClassTeacherDto>>;
