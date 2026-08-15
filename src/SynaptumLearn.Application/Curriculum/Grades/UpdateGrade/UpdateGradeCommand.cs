using MediatR;

namespace SynaptumLearn.Application.Curriculum.Grades.UpdateGrade;

public sealed record UpdateGradeCommand(
    Guid GradeId,
    string Name,
    int Order)
    : IRequest<bool>;