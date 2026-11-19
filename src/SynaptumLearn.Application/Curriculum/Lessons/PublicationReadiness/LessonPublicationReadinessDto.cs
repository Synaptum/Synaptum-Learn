
namespace SynaptumLearn.Application.Curriculum.Lessons.PublicationReadiness;

public sealed class LessonPublicationReadinessDto
{
    public Guid LessonId { get; init; }

    public bool IsReady { get; init; }

    public List<string> Issues { get; init; } = new();
}
