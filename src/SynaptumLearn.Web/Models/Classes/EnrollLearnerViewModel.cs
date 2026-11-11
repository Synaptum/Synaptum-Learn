
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SynaptumLearn.Web.Models.Classes;

public sealed class EnrollLearnerViewModel
{
    [Required]
    public Guid ClassId { get; set; }

    [Required(ErrorMessage = "Please select a learner.")]
    public Guid? LearnerProfileId { get; set; }

    public string ClassName { get; set; } = string.Empty;

    public string GradeName { get; set; } = string.Empty;

    public int AcademicYear { get; set; }

    public List<SelectListItem> Learners { get; set; } = new();
}
