
using SynaptumLearn.Domain.Common;
using SynaptumLearn.Domain.Content;
using SynaptumLearn.Domain.Schools;

namespace SynaptumLearn.Domain.Analytics;

public class LessonProgress : TenantEntity
{
    public Guid LearnerProfileId { get; set; }
    public LearnerProfile LearnerProfile { get; set; } = null!;
    public Guid LessonId { get; set; }
    public Lesson Lesson { get; set; } = null!;
    public bool Completed { get; set; }
    public DateTime? CompletedAt { get; set; }
    public decimal ProgressPercentage { get; set; }
}