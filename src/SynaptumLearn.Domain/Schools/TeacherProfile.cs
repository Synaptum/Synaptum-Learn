using SynaptumLearn.Domain.Common;
using SynaptumLearn.Domain.Users;

namespace SynaptumLearn.Domain.Schools;

public class TeacherProfile : TenantEntity
{
    public Guid UserId { get; set; }
    public string EmployeeNumber { get; set; } = null!;
    public bool IsClassTeacher { get; set; }
    public User User { get; set; } = null!;
    public School School { get; set; } = null!;
    public ICollection<TeacherSubject> TeacherSubjects { get; set; } = new List<TeacherSubject>();
}