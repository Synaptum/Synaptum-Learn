
using MediatR;

namespace SynaptumLearn.Application.Curriculum.Lessons.UpdateLesson;

public sealed record UpdateLessonCommand(
    Guid LessonId,
    string Title,
    string Summary,
    int Order)
    : IRequest;
