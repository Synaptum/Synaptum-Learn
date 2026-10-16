using MediatR;
using SynaptumLearn.Domain.Enums;

namespace SynaptumLearn.Application.Schools.CreateSchoolMembership;

public record CreateSchoolMembershipCommand(
    Guid SchoolId,
    Guid UserId,
    SchoolRole Role
) : IRequest<Guid>;