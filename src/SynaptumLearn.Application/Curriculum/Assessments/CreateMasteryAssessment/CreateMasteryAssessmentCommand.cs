
using MediatR;

namespace SynaptumLearn.Application.Curriculum.Assessments.CreateMasteryAssessment;

public sealed record CreateMasteryAssessmentCommand(
    Guid LessonId,
    string Title,
    string Instructions,
    int PassMark
) : IRequest<Guid>;
