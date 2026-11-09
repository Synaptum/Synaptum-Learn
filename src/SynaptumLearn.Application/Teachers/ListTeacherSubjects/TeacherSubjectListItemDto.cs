
namespace SynaptumLearn.Application.Teachers.ListTeacherSubjects;

public sealed record TeacherSubjectListItemDto(
    Guid Id,
    Guid TeacherProfileId,
    string TeacherName,
    string EmployeeNumber,
    string GradeName,
    string SubjectName
);
