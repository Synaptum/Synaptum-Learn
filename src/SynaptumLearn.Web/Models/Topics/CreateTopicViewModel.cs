
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SynaptumLearn.Web.Models.Topics;

public sealed class CreateTopicViewModel
{
    [Required(ErrorMessage = "Please select a grade and subject.")]
    public Guid? GradeSubjectId { get; set; }

    [Required(ErrorMessage = "Please enter a topic title.")]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a term.")]
    [Range(1, 4, ErrorMessage = "Term must be between 1 and 4.")]
    public int? Term { get; set; }

    [Required(ErrorMessage = "Please enter the topic position.")]
    [Range(1, int.MaxValue,
        ErrorMessage = "Topic position must be greater than zero.")]
    public int? Order { get; set; }

    public List<SelectListItem> GradeSubjects { get; set; } = new();

    public List<SelectListItem> Terms { get; set; } = new()
    {
        new SelectListItem { Value = "1", Text = "Term 1" },
        new SelectListItem { Value = "2", Text = "Term 2" },
        new SelectListItem { Value = "3", Text = "Term 3" },
        new SelectListItem { Value = "4", Text = "Term 4" }
    };
}
