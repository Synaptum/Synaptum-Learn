
using MediatR;

namespace SynaptumLearn.Application.Curriculum.Assessments.GetMasteryAssessment;

public sealed record GetMasteryAssessmentQuery(Guid AssessmentId)
    : IRequest<MasteryAssessmentDto?>;
