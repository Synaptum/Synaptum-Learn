
using MediatR;
using SynaptumLearn.Application.Curriculum.Lessons.ListLessons;

namespace SynaptumLearn.Application.Curriculum.Lessons.GetLessonById;

public sealed record GetLessonByIdQuery(Guid LessonId)
    : IRequest<LessonListItemDto?>;
