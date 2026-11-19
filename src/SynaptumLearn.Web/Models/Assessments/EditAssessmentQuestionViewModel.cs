
using System.ComponentModel.DataAnnotations;

namespace SynaptumLearn.Web.Models.Assessments;

public sealed class EditAssessmentQuestionViewModel
{
    [Required]
    public Guid QuestionId { get; set; }

    public Guid AssessmentId { get; set; }

    public int Order { get; set; }

    [Required]
    [StringLength(5000)]
    public string Text { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Marks { get; set; }
}
