
using MediatR;

namespace SynaptumLearn.Application.Curriculum.Lessons.DeleteLessonContent;

public sealed record DeleteLessonContentCommand(Guid ContentId)
    : IRequest<Guid>;
