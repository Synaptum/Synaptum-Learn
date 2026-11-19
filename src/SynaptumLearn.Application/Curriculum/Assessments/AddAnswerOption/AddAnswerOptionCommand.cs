
using MediatR;

namespace SynaptumLearn.Application.Curriculum.Assessments.AddAnswerOption;

public sealed record AddAnswerOptionCommand(
    Guid QuestionId,
    string Text,
    bool IsCorrect
) : IRequest<Guid>;
