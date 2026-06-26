
using SynaptumLearn.Domain.Common;
using SynaptumLearn.Domain.Content;

namespace SynaptumLearn.Domain.Curriculum;

public class GradeSubject : BaseEntity
{
    public Guid GradeId { get; set; }
    public Grade Grade { get; set; } = null!;
    public Guid SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;
    public ICollection<Topic> Topics { get; set; } = new List<Topic>();
}