using MediatR;
using SynaptumLearn.Domain.Enums;

namespace SynaptumLearn.Application.Schools.ChangeSchoolStatus;

public sealed record ChangeSchoolStatusCommand(
    Guid SchoolId,
    SchoolStatus NewStatus)
    : IRequest<bool>;