using SynaptumLearn.Domain.Common;
using SynaptumLearn.Domain.Schools;

namespace SynaptumLearn.Domain.Users;

public class User : BaseEntity
{
    public string IdentityUserId { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public ICollection<SchoolMembership> SchoolMemberships {get; set;} = new List<SchoolMembership>();
    public ICollection<TeacherProfile> TeacherProfiles {get; set;} = new List<TeacherProfile>();
    public ICollection<LearnerProfile> LearnerProfiles {get;set;} = new List<LearnerProfile>();
}
