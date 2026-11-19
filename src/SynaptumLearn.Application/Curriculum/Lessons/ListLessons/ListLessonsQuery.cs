
using MediatR;

namespace SynaptumLearn.Application.Curriculum.Lessons.ListLessons;

public sealed record ListLessonsQuery(Guid? TopicId = null)
    : IRequest<IReadOnlyList<LessonListItemDto>>;
