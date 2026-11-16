
using MediatR;

namespace SynaptumLearn.Application.Curriculum.LearnerCurriculum;

public sealed record GetLearnerLessonsQuery(Guid TopicId)
    : IRequest<IReadOnlyList<LearnerLessonDto>>;
