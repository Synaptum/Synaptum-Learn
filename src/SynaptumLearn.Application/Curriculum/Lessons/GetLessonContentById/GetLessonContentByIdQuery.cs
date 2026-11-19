
using MediatR;
using SynaptumLearn.Application.Curriculum.Lessons.ListLessonContents;

namespace SynaptumLearn.Application.Curriculum.Lessons.GetLessonContentById;

public sealed record GetLessonContentByIdQuery(Guid ContentId)
    : IRequest<LessonContentItemDto?>;
