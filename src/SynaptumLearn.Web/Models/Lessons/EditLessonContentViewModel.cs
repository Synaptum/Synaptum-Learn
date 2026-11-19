
using System.ComponentModel.DataAnnotations;
using SynaptumLearn.Domain.Content;

namespace SynaptumLearn.Web.Models.Lessons;

public sealed class EditLessonContentViewModel
{
    public Guid ContentId { get; set; }

    public Guid LessonId { get; set; }

    public string LessonTitle { get; set; } = string.Empty;

    [Required]
    public LessonContentType? Type { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(10000)]
    public string Content { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue)]
    public int? Order { get; set; }
}
