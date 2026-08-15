using SynaptumLearn.Domain.Enums;

namespace SynaptumLearn.Application.Schools.GetSchoolDetails;

public sealed class SchoolDetailsDto
{
    public Guid Id { get; init; }

    public string Code { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string EMISNumber { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Phone { get; init; } = string.Empty;

    public Province Province { get; init; }

    public SchoolStatus Status { get; init; }

    public string Slug { get; init; } = string.Empty;

    public DateTime CreatedAt { get; init; }

    public DateTime? LastModifiedAt { get; init; }
}