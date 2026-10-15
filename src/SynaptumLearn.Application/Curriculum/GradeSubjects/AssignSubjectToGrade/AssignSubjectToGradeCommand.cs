using MediatR;

namespace SynaptumLearn.Application.Curriculum.GradeSubjects.AssignSubjectToGrade;

public sealed record AssignSubjectToGradeCommand(
    Guid GradeId,
    Guid SubjectId)
    : IRequest<Guid>;