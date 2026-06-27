using SynaptumLearn.Domain.Enums;

namespace SynaptumLearn.Application.Schools.CreateSchool;

public sealed class SchoolDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string EMISNumber { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Phone { get; init; } = string.Empty;

    public string Province { get; init; } = string.Empty;

    public SchoolStatus Status { get; init; }
}