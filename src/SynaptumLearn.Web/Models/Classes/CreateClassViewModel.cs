
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SynaptumLearn.Web.Models.Classes;

public sealed class CreateClassViewModel
{
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a grade.")]
    public Guid? GradeId { get; set; }

    [Range(2000, 2100)]
    public int AcademicYear { get; set; } = DateTime.Today.Year;

    public List<SelectListItem> Grades { get; set; } = new();
}
