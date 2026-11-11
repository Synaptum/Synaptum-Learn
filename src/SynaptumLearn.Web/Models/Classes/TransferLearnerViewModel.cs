
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SynaptumLearn.Web.Models.Classes;

public sealed class TransferLearnerViewModel
{
    public Guid EnrollmentId { get; set; }

    [Required(ErrorMessage = "Please select a destination class.")]
    public Guid? DestinationClassId { get; set; }

    public Guid SourceClassId { get; set; }

    public string LearnerName { get; set; } = string.Empty;

    public string LearnerNumber { get; set; } = string.Empty;

    public string SourceClassName { get; set; } = string.Empty;

    public string GradeName { get; set; } = string.Empty;

    public int AcademicYear { get; set; }

    public List<SelectListItem> DestinationClasses { get; set; } = new();
}
