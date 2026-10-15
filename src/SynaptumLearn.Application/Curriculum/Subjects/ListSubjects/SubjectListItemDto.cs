namespace SynaptumLearn.Application.Curriculum.Subjects.ListSubjects;

public sealed class SubjectListItemDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Code { get; init; } = string.Empty;

    public DateTime CreatedAt { get; init; }
}