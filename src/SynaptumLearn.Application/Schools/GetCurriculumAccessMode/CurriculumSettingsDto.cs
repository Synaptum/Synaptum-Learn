
using SynaptumLearn.Domain.Enums;

namespace SynaptumLearn.Application.Schools.GetCurriculumAccessMode;

public sealed record CurriculumSettingsDto(
    CurriculumAccessMode AccessMode,
    int CurrentTerm);
