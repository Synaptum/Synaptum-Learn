
using MediatR;

namespace SynaptumLearn.Application.Curriculum.Assessments.DeleteAnswerOption;

public sealed record DeleteAnswerOptionCommand(Guid AnswerOptionId)
    : IRequest<Guid>;
