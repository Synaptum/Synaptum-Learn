
using System.ComponentModel.DataAnnotations;

namespace SynaptumLearn.Web.Models.SchoolUsers;

public class CreateLearnerViewModel
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LearnerNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a grade.")]
    public Guid? GradeId { get; set; }
}
