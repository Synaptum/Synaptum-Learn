
using MediatR;
using SynaptumLearn.Domain.Content;

namespace SynaptumLearn.Application.Curriculum.Lessons.UpdateLessonContent;

public sealed record UpdateLessonContentCommand(
    Guid ContentId,
    LessonContentType Type,
    string Title,
    string Content,
    int Order
) : IRequest;
