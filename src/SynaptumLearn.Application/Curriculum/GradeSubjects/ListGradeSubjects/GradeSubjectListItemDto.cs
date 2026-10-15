namespace SynaptumLearn.Application.Curriculum.GradeSubjects.ListGradeSubjects;

public sealed class GradeSubjectListItemDto
{
    public Guid Id { get; set; }

    public Guid GradeId { get; set; }

    public string GradeName { get; set; } = string.Empty;

    public Guid SubjectId { get; set; }

    public string SubjectName { get; set; } = string.Empty;

    public string SubjectCode { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}