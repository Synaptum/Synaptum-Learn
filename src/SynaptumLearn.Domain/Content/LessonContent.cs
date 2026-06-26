
using SynaptumLearn.Domain.Common;

namespace SynaptumLearn.Domain.Content;

public class LessonContent : BaseEntity
{
    public Guid LessonId { get; set; }
    public Lesson Lesson { get; set; } = null!;
    public LessonContentType Type { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public int Order { get; set; }

}