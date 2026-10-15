namespace SynaptumLearn.Application.Curriculum.Subjects.GetSubjectDetails;

public sealed class SubjectDetailsDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Code { get; init; } = string.Empty;

    public DateTime CreatedAt { get; init; }

    public DateTime? LastModifiedAt { get; init; }
}