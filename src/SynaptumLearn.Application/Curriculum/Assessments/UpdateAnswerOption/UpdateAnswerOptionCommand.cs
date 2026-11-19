
using MediatR;

namespace SynaptumLearn.Application.Curriculum.Assessments.UpdateAnswerOption;

public sealed record UpdateAnswerOptionCommand(
    Guid AnswerOptionId,
    string Text,
    bool IsCorrect
) : IRequest<Guid>;
