
using SynaptumLearn.Domain.Common;
using SynaptumLearn.Domain.Curriculum;

namespace SynaptumLearn.Domain.Content;

public class Topic : BaseEntity
{
    public Guid GradeSubjectId { get; set; }
    public GradeSubject GradeSubject { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}