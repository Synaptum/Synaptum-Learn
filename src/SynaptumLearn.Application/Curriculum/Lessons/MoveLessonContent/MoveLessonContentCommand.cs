
using MediatR;

namespace SynaptumLearn.Application.Curriculum.Lessons.MoveLessonContent;

public enum LessonContentMoveDirection
{
    Up = 1,
    Down = 2
}

public sealed record MoveLessonContentCommand(
    Guid ContentId,
    LessonContentMoveDirection Direction
) : IRequest<Guid>;
