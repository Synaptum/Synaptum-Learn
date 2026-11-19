
namespace SynaptumLearn.Application.Curriculum.Assessments.GetMasteryAssessment;

public sealed class MasteryAssessmentDto
{
    public Guid Id { get; init; }
    public Guid LessonId { get; init; }
    public string LessonTitle { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Instructions { get; init; } = string.Empty;
    public int PassMark { get; init; }
    public bool IsPublished { get; init; }

    public List<MasteryAssessmentQuestionDto> Questions { get; init; } = new();
}

public sealed class MasteryAssessmentQuestionDto
{
    public Guid Id { get; init; }
    public string Text { get; init; } = string.Empty;
    public int Marks { get; init; }
    public int Order { get; init; }

    public int AnswerOptionCount { get; init; }

    public List<MasteryAssessmentAnswerOptionDto> AnswerOptions { get; init; } = new();
}

public sealed class MasteryAssessmentAnswerOptionDto
{
    public Guid Id { get; init; }
    public string Text { get; init; } = string.Empty;
    public bool IsCorrect { get; init; }
}
