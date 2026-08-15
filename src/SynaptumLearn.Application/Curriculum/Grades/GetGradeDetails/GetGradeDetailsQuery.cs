using MediatR;

namespace SynaptumLearn.Application.Curriculum.Grades.GetGradeDetails;

public sealed record GetGradeDetailsQuery(Guid GradeId)
    : IRequest<GradeDetailsDto?>;