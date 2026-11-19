
using System.ComponentModel.DataAnnotations;

namespace SynaptumLearn.Web.Models.Assessments;

public sealed class CreateMasteryAssessmentViewModel
{
    [Required]
    public Guid LessonId { get; set; }

    public string LessonTitle { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(3000)]
    public string Instructions { get; set; } = string.Empty;

    [Range(1, 100)]
    public int PassMark { get; set; } = 70;
}
