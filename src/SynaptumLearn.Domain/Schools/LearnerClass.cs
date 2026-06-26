using SynaptumLearn.Domain.Common;

namespace SynaptumLearn.Domain.Schools;

public class LearnerClass : TenantEntity
{
    public Guid LearnerId { get; set; }

    public LearnerProfile Learner { get; set; } = null!;

    public Guid ClassId { get; set; }

    //public Class Class { get; set; } = null!;

    public int AcademicYear { get; set; }

    public int Term { get; set; }
}