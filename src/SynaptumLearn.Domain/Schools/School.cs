using SynaptumLearn.Domain.Common;
using SynaptumLearn.Domain.Enums;

namespace SynaptumLearn.Domain.Schools;

public class School : BaseEntity
{
    public string Name { get; set; } = null!;
    public string EMISNumber { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public Province Province { get; set; }
    public string Slug { get; set; } = null!;
    public string Code { get; set; } = null!;
    public SchoolStatus Status {get; set;}
    public ICollection<SchoolMembership> SchoolMemberships { get; set; } = new List<SchoolMembership>();
    public ICollection<TeacherProfile> TeacherProfiles { get; set; } = new List<TeacherProfile>();
    public ICollection<LearnerProfile> LearnerProfiles { get; set; } = new List<LearnerProfile>();
}
