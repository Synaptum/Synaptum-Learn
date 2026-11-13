
namespace SynaptumLearn.Application.Curriculum.Topics.ListTopics;

public sealed class TopicListItemDto
{
    public Guid Id { get; set; }

    public Guid GradeSubjectId { get; set; }

    public string GradeName { get; set; } = string.Empty;

    public string SubjectName { get; set; } = string.Empty;

    public string SubjectCode { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Term { get; set; }

    public int Order { get; set; }

    public DateTime CreatedAt { get; set; }
}
