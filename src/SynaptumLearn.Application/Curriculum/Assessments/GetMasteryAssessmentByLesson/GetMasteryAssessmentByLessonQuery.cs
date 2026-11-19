
using MediatR;

namespace SynaptumLearn.Application.Curriculum.Assessments.GetMasteryAssessmentByLesson;

public sealed record GetMasteryAssessmentByLessonQuery(Guid LessonId)
    : IRequest<Guid?>;
