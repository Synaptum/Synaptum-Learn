
namespace SynaptumLearn.Application.Curriculum.LearnerCurriculum;

public sealed record LearnerLessonDto(
    Guid LessonId,
    string Title,
    string Summary,
    int Order,
    bool IsUnlocked,
    bool IsCompleted
);
