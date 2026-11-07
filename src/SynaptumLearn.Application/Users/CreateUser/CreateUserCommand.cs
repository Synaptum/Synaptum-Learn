using MediatR;

namespace SynaptumLearn.Application.Users.CreateUser;

public sealed record CreateUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    UserAccountType AccountType,
    Guid? SchoolId
) : IRequest<Guid>;