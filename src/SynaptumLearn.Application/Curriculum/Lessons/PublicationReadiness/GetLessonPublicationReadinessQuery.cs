
using MediatR;

namespace SynaptumLearn.Application.Curriculum.Lessons.PublicationReadiness;

public sealed record GetLessonPublicationReadinessQuery(Guid LessonId)
    : IRequest<LessonPublicationReadinessDto>;
