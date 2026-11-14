
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
}
