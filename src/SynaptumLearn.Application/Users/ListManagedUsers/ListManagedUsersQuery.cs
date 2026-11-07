
using MediatR;

namespace SynaptumLearn.Application.Users.ListManagedUsers;

public sealed record ListManagedUsersQuery
    : IRequest<IReadOnlyList<ManagedUserListItemDto>>;
