using MediatR;
using SynaptumLearn.Domain.Enums;

namespace SynaptumLearn.Application.Schools.UpdateSchool;

public sealed record UpdateSchoolCommand(
    Guid Id,
    string Name,
    string EMISNumber,
    string Email,
    string Phone,
    Province Province)
    : IRequest<bool>;