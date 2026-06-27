using System.ComponentModel.DataAnnotations;
using SynaptumLearn.Domain.Enums;

namespace SynaptumLearn.Web.ViewModels.Schools;

public class CreateSchoolViewModel
{
    [Required]
    [Display(Name = "School Name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Display(Name = "EMIS Number")]
    public string EMISNumber { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Phone { get; set; } = string.Empty;

    [Required]
    public Province Province { get; set; }
}