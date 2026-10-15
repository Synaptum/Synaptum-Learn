using MediatR;

namespace SynaptumLearn.Application.Curriculum.GradeSubjects.ListGradeSubjects;

public sealed record ListGradeSubjectsQuery()
    : IRequest<IReadOnlyList<GradeSubjectListItemDto>>;