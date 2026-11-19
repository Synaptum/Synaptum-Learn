
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SynaptumLearn.Web.Models.Lessons;

public sealed class CreateLessonViewModel
{
    [Required]
    public Guid? TopicId { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(3000)]
    public string Summary { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue)]
    public int? Order { get; set; }

    public List<SelectListItem> Topics { get; set; } = [];
}
