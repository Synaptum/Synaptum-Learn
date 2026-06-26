using SynaptumLearn.Domain.Common;
using SynaptumLearn.Domain.Analytics;
using SynaptumLearn.Domain.Assessments;
using SynaptumLearn.Domain.Users;

namespace SynaptumLearn.Domain.Schools;

public class LearnerProfile : TenantEntity
{
    public Guid UserId { get; set; }
    public string LearnerNumber { get; set; } = null!;
    public Guid GradeId { get; set; }
    public User User { get; set; } = null!;
    public School School { get; set; } = null!;
    public ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();
    public ICollection<AssessmentAttempt> AssessmentAttempts { get; set; } = new List<AssessmentAttempt>();
}