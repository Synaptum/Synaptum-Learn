
using SynaptumLearn.Domain.Content;

namespace SynaptumLearn.Application.Curriculum.Lessons.ListLessonContents;

public sealed class LessonContentItemDto
{
    public Guid Id { get; set; }
    public Guid LessonId { get; set; }
    public LessonContentType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Order { get; set; }
}
