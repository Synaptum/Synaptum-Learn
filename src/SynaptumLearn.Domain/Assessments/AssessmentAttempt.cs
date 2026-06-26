
using SynaptumLearn.Domain.Common;
using SynaptumLearn.Domain.Schools;

namespace SynaptumLearn.Domain.Assessments;

public class AssessmentAttempt : TenantEntity
{
    public Guid AssessmentId { get; set; }
    public Assessment Assessment { get; set; } = null!;
    public Guid LearnerProfileId { get; set; }
    public LearnerProfile LearnerProfile { get; set; } = null!;
    public decimal Score { get; set; }
    public bool Passed { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}