using MediatR;

namespace SynaptumLearn.Application.Curriculum.Subjects.UpdateSubject;

public sealed record UpdateSubjectCommand(
    Guid SubjectId,
    string Name,
    string Code)
    : IRequest<bool>;
