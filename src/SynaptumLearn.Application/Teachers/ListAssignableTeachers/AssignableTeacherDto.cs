
namespace SynaptumLearn.Application.Teachers.ListAssignableTeachers;

public sealed record AssignableTeacherDto(
    Guid TeacherProfileId,
    string FirstName,
    string LastName,
    string EmployeeNumber
);
