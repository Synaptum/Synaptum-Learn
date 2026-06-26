using SynaptumLearn.Domain.Common;
using SynaptumLearn.Domain.Users;
using SynaptumLearn.Domain.Enums;

namespace SynaptumLearn.Domain.Schools;

public class SchoolMembership : TenantEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public School School { get; set; } = null!;
    public SchoolRole Role { get; set; }
    public bool IsActive { get; set; } = true;
}