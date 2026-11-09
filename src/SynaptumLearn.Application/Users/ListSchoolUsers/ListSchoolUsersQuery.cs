
using MediatR;

namespace SynaptumLearn.Application.Users.ListSchoolUsers;

public sealed record ListSchoolUsersQuery
    : IRequest<IReadOnlyList<SchoolUserListItemDto>>;
