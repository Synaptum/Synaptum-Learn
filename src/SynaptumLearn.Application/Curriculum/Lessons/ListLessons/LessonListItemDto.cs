
using SynaptumLearn.Domain.Content;

namespace SynaptumLearn.Application.Curriculum.Lessons.ListLessons;

public sealed class LessonListItemDto
{
    public Guid Id { get; set; }

    public Guid TopicId { get; set; }

    public string TopicTitle { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public int Order { get; set; }

    public LessonStatus Status { get; set; }

    public DateTime? PublishedAt { get; set; }
}
