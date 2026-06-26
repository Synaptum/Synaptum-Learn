
using SynaptumLearn.Domain.Common;

namespace SynaptumLearn.Domain.Curriculum;

public class Subject : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public ICollection<GradeSubject> GradeSubjects { get; set; } = new List<GradeSubject>();
}