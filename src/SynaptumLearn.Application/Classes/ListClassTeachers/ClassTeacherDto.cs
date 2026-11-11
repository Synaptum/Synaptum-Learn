
namespace SynaptumLearn.Application.Classes.ListClassTeachers;

public sealed record ClassTeacherDto(
    Guid AssignmentId,
    string TeacherName,
    string SubjectName
);
