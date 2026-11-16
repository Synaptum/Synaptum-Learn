using SynaptumLearn.Domain.Enums;

namespace SynaptumLearn.Application.Curriculum.LearnerCurriculum;

public sealed record LearnerCurriculumDto(
    Guid GradeId,
    CurriculumAccessMode AccessMode,
    int CurrentTerm,
    IReadOnlyList<LearnerSubjectDto> Subjects
);