
using SynaptumLearn.Domain.Common;
using SynaptumLearn.Domain.Curriculum;

namespace SynaptumLearn.Domain.Content;

public class Topic : BaseAuditableEntity
{
    public Guid GradeSubjectId { get; set; }
    public GradeSubject GradeSubject { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int Term { get; set; }

    public int Order { get; set; }

    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}