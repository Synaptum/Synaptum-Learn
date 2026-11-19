
using MediatR;

namespace SynaptumLearn.Application.Curriculum.Assessments.GetAnswerOption;

public sealed record GetAnswerOptionQuery(Guid AnswerOptionId)
    : IRequest<AnswerOptionDetailsDto?>;

public sealed class AnswerOptionDetailsDto
{
    public Guid Id { get; init; }
    public Guid AssessmentId { get; init; }
    public string QuestionText { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
    public bool IsCorrect { get; init; }
    public bool CanEdit { get; init; }
}
