namespace SynaptumLearn.Application.Curriculum.LearnerCurriculum;

public sealed record LearnerSubjectDto(
    Guid GradeSubjectId,
    Guid SubjectId,
    string SubjectName,
    string SubjectCode,
    IReadOnlyList<LearnerTopicDto> Topics
);