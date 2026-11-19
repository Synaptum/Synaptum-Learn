
using MediatR;

namespace SynaptumLearn.Application.Curriculum.Assessments.GetAssessmentQuestion;

public sealed record GetAssessmentQuestionQuery(Guid QuestionId)
    : IRequest<AssessmentQuestionDetailsDto?>;

public sealed class AssessmentQuestionDetailsDto
{
    public Guid Id { get; init; }
    public Guid AssessmentId { get; init; }
    public string Text { get; init; } = string.Empty;
    public int Marks { get; init; }
    public int Order { get; init; }
    public bool CanEdit { get; init; }
}
