
using MediatR;

namespace SynaptumLearn.Application.Curriculum.Lessons.ListLessonContents;

public sealed record ListLessonContentsQuery(Guid LessonId)
    : IRequest<IReadOnlyList<LessonContentItemDto>>;
