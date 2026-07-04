using SynaptumLearn.Domain.Common;

namespace SynaptumLearn.Domain.Curriculum;

public class Grade : BaseAuditableEntity
{
    public string Name { get; set; } = null!;
    public int Order { get; set; }
    public ICollection<GradeSubject> GradeSubjects { get; set; } = new List<GradeSubject>();
}