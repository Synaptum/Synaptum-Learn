
using SynaptumLearn.Domain.Common;
using SynaptumLearn.Domain.Users;

namespace SynaptumLearn.Domain.Content;

public class LessonApproval : TenantEntity
{
    public Guid LessonId { get; set; }
    public Lesson Lesson { get; set; } = null!;
    public bool Approved { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid ApprovedByUserId { get; set; }
    public User ApprovedByUser { get; set; } = null!;
    public string? Notes { get; set; }
}