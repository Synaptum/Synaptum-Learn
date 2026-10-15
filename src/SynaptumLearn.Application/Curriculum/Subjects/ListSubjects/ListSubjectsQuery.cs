using MediatR;

namespace SynaptumLearn.Application.Curriculum.Subjects.ListSubjects;

public sealed record ListSubjectsQuery()
    : IRequest<IReadOnlyList<SubjectListItemDto>>;