
using MediatR;

namespace SynaptumLearn.Application.Teachers.ListTeacherSubjects;

public sealed record ListTeacherSubjectsQuery()
    : IRequest<IReadOnlyList<TeacherSubjectListItemDto>>;
