namespace SynaptumLearn.Application.Curriculum.Grades.ListGrades;

public sealed class GradeListItemDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public int Order { get; init; }

    public DateTime CreatedAt { get; init; }
}