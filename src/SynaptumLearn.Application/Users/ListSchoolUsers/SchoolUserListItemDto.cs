
namespace SynaptumLearn.Application.Users.ListSchoolUsers;

public sealed record SchoolUserListItemDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string AccountType,
    string Number,
    string? Grade,
    bool IsActive
);
