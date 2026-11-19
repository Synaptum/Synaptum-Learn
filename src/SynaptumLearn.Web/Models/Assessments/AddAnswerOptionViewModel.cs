
using System.ComponentModel.DataAnnotations;

namespace SynaptumLearn.Web.Models.Assessments;

public sealed class AddAnswerOptionViewModel
{
    [Required]
    public Guid QuestionId { get; set; }

    public Guid AssessmentId { get; set; }

    public string QuestionText { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Text { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }
}
