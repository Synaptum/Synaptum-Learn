using MediatR;

namespace SynaptumLearn.Application.Curriculum.Grades.ListGrades;

public sealed record ListGradesQuery
    : IRequest<IReadOnlyList<GradeListItemDto>>;