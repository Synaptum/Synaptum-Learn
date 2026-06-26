
using SynaptumLearn.Domain.Common;
using SynaptumLearn.Domain.Curriculum;

namespace SynaptumLearn.Domain.Schools;

public class TeacherSubject : TenantEntity
{
    public Guid TeacherProfileId { get; set; }
    public TeacherProfile TeacherProfile { get; set; } = null!;
    public Guid GradeSubjectId { get; set; }
    public GradeSubject GradeSubject { get; set; } = null!;
}