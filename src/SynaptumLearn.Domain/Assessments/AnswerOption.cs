
using SynaptumLearn.Domain.Common;

namespace SynaptumLearn.Domain.Assessments;

public class AnswerOption : BaseEntity
{
    public Guid QuestionId { get; set; }
    public Question Question { get; set; } = null!;
    public string Text { get; set; } = null!;
    public bool IsCorrect { get; set; }
}