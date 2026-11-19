
using MediatR;

namespace SynaptumLearn.Application.Curriculum.Lessons.CreateLesson;

public sealed record CreateLessonCommand(
    Guid TopicId,
    string Title,
    string Summary,
    int Order)
    : IRequest<Guid>;
