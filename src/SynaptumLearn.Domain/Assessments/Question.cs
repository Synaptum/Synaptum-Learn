
using SynaptumLearn.Domain.Common;

namespace SynaptumLearn.Domain.Assessments;

public class Question : BaseEntity
{
    public Guid AssessmentId { get; set; }
    public Assessment Assessment { get; set; } = null!;
    public string Text { get; set; } = null!;
    public int Marks { get; set; }
    public int Order { get; set; }
    public ICollection<AnswerOption> AnswerOptions { get; set; } = new List<AnswerOption>();
}