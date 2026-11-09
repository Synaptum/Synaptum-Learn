using MediatR;

namespace SynaptumLearn.Application.Users.CreateSchoolUser;

public sealed record CreateSchoolUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    SchoolUserAccountType AccountType,
    string? EmployeeNumber = null,
    bool IsClassTeacher = false,
    string? LearnerNumber = null,
    Guid? GradeId = null
) : IRequest<Guid>;