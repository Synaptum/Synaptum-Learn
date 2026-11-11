
namespace SynaptumLearn.Application.Classes.ListEligibleClassTeachers;

public sealed record EligibleClassTeacherDto(
    Guid TeacherSubjectId,
    string TeacherName,
    string SubjectName
);
