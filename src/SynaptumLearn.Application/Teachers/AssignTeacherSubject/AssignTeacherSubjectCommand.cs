
using MediatR;

namespace SynaptumLearn.Application.Teachers.AssignTeacherSubject;

public sealed record AssignTeacherSubjectCommand(
    Guid TeacherProfileId,
    Guid GradeSubjectId
) : IRequest<Guid>;
