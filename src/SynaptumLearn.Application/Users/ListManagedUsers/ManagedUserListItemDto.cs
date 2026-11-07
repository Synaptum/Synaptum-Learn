
namespace SynaptumLearn.Application.Users.ListManagedUsers;

public sealed record ManagedUserListItemDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string AccountType,
    Guid? SchoolId,
    string? SchoolName,
    bool IsActive
);
