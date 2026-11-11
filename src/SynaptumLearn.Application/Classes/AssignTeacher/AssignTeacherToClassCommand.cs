
using MediatR;

namespace SynaptumLearn.Application.Classes.AssignTeacher;

public sealed record AssignTeacherToClassCommand(
    Guid TeacherSubjectId,
    Guid ClassId
) : IRequest<Guid>;
