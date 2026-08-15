using MediatR;

namespace SynaptumLearn.Application.Curriculum.Grades.CreateGrade;

public sealed record CreateGradeCommand(
    string Name,
    int Order)
    : IRequest<Guid>;