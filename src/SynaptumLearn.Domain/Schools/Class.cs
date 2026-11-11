
using SynaptumLearn.Domain.Common;
using SynaptumLearn.Domain.Curriculum;

namespace SynaptumLearn.Domain.Schools;

public class Class : TenantEntity
{
    public string Name { get; set; } = null!;

    public Guid GradeId { get; set; }

    public Grade Grade { get; set; } = null!;

    public int AcademicYear { get; set; }

    public ICollection<LearnerClass> LearnerClasses { get; set; }
        = new List<LearnerClass>();
}
