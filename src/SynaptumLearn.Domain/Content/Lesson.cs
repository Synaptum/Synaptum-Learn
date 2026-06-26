
using SynaptumLearn.Domain.Common;
using SynaptumLearn.Domain.Analytics;
using SynaptumLearn.Domain.Assessments;

namespace SynaptumLearn.Domain.Content;

public class Lesson : BaseEntity
{
    public Guid TopicId { get; set; }
    public Topic Topic { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Summary { get; set; } = null!;

    public int Order { get; set; }

    public LessonStatus Status { get; set; }

    public DateTime? PublishedAt { get; set; }

    public ICollection<LessonContent> LessonContents { get; set; } = new List<LessonContent>();

    public ICollection<LessonApproval> LessonApprovals { get; set; } = new List<LessonApproval>();

    public ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();

    public ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();
}