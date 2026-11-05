using MediatR;

namespace SynaptumLearn.Application.Users.CreateUser;

public record CreateUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password
) : IRequest<Guid>;