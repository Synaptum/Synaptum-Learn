
using MediatR;

namespace SynaptumLearn.Application.Classes.RemoveClassTeacher;

public sealed record RemoveClassTeacherCommand(
    Guid AssignmentId
) : IRequest;
