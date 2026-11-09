
using MediatR;

namespace SynaptumLearn.Application.Teachers.RemoveTeacherSubject;

public sealed record RemoveTeacherSubjectCommand(Guid AssignmentId)
    : IRequest;
