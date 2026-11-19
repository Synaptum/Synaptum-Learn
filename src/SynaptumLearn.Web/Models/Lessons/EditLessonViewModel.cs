
using System.ComponentModel.DataAnnotations;

namespace SynaptumLearn.Web.Models.Lessons;

public sealed class EditLessonViewModel
{
    [Required]
    public Guid LessonId { get; set; }

    public string TopicTitle { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(3000)]
    public string Summary { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue)]
    public int Order { get; set; }
}
