using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SynaptumLearn.Web.Models.GradeSubjects;

public sealed class AssignSubjectToGradeViewModel
{
    [Required(ErrorMessage = "Please select a grade.")]
    public Guid? GradeId { get; set; }

    [Required(ErrorMessage = "Please select a subject.")]
    public Guid? SubjectId { get; set; }

    public IEnumerable<SelectListItem> Grades { get; set; }
        = Enumerable.Empty<SelectListItem>();

    public IEnumerable<SelectListItem> Subjects { get; set; }
        = Enumerable.Empty<SelectListItem>();
}