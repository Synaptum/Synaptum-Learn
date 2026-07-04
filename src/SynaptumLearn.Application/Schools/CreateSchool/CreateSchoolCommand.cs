using MediatR;
using SynaptumLearn.Domain.Enums;

namespace SynaptumLearn.Application.Schools.CreateSchool;

public sealed class CreateSchoolCommand : IRequest<Guid>
{
    public string Name { get; set; } = string.Empty;

    public string EMISNumber { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public Province Province { get; set; }
}