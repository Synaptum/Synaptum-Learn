
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SynaptumLearn.Web.Models.TeacherSubjects;

public sealed class AssignTeacherSubjectViewModel
{
    [Required(ErrorMessage = "Please select a teacher.")]
    public Guid? TeacherProfileId { get; set; }

    [Required(ErrorMessage = "Please select a grade and subject.")]
    public Guid? GradeSubjectId { get; set; }

    public List<SelectListItem> Teachers { get; set; } = new();

    public List<SelectListItem> GradeSubjects { get; set; } = new();
}
