
using System.ComponentModel.DataAnnotations;
using SynaptumLearn.Domain.Enums;

namespace SynaptumLearn.Web.Models.CurriculumSettings;

public sealed class CurriculumSettingsViewModel
{
    [Required]
    [EnumDataType(typeof(CurriculumAccessMode))]
    [Display(Name = "Curriculum Access Mode")]
    public CurriculumAccessMode AccessMode { get; set; }
        = CurriculumAccessMode.Progressive;

    [Required]
    [Range(1, 4, ErrorMessage = "The current academic term must be between 1 and 4.")]
    [Display(Name = "Current Academic Term")]
    public int CurrentTerm { get; set; } = 1;
}
