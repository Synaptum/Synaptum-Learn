
using MediatR;
using SynaptumLearn.Domain.Content;

namespace SynaptumLearn.Application.Curriculum.Lessons.AddLessonContent;

public sealed record AddLessonContentCommand(
    Guid LessonId,
    LessonContentType Type,
    string Title,
    string Content,
    int Order
) : IRequest<Guid>;
