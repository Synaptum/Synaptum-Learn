
using SynaptumLearn.Domain.Common;
using SynaptumLearn.Domain.Content;

namespace SynaptumLearn.Domain.Assessments;

public class Assessment : BaseEntity
{
    public Guid LessonId { get; set; }
    public Lesson Lesson { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Instructions { get; set; } = null!;
    public int PassMark { get; set; }
    public bool IsPublished { get; set; }
    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<AssessmentAttempt> Attempts { get; set; } = new List<AssessmentAttempt>();
}