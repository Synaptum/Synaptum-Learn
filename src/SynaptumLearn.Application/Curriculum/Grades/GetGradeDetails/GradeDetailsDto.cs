namespace SynaptumLearn.Application.Curriculum.Grades.GetGradeDetails;

public sealed class GradeDetailsDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public int Order { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime? LastModifiedAt { get; init; }
}