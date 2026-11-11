
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SynaptumLearn.Web.Models.Classes;

public sealed class AssignTeacherViewModel
{
    [Required]
    public Guid ClassId { get; set; }

    [Required(ErrorMessage = "Please select a teacher and subject.")]
    public Guid? TeacherSubjectId { get; set; }

    public string ClassName { get; set; } = string.Empty;

    public string GradeName { get; set; } = string.Empty;

    public int AcademicYear { get; set; }

    public List<SelectListItem> EligibleTeachers { get; set; } = new();
}
