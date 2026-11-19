
using System.ComponentModel.DataAnnotations;

namespace SynaptumLearn.Web.Models.Assessments;

public sealed class AddAssessmentQuestionViewModel
{
    [Required]
    public Guid AssessmentId { get; set; }

    public string AssessmentTitle { get; set; } = string.Empty;

    [Required]
    [StringLength(5000)]
    public string Text { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Marks { get; set; } = 1;

    [Range(1, int.MaxValue)]
    public int Order { get; set; } = 1;
}
