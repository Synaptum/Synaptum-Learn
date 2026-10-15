using MediatR;

namespace SynaptumLearn.Application.Curriculum.Subjects.CreateSubject;

public sealed record CreateSubjectCommand(
    string Name,
    string Code)
    : IRequest<Guid>;