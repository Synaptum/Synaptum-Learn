
using SynaptumLearn.Domain.Common;

namespace SynaptumLearn.Domain.Schools;

public class TeacherClassSubject : TenantEntity
{
    public Guid TeacherSubjectId { get; set; }

    public TeacherSubject TeacherSubject { get; set; } = null!;

    public Guid ClassId { get; set; }

    public Class Class { get; set; } = null!;
}
