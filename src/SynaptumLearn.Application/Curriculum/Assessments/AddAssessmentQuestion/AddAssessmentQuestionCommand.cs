
using MediatR;

namespace SynaptumLearn.Application.Curriculum.Assessments.AddAssessmentQuestion;

public sealed record AddAssessmentQuestionCommand(
    Guid AssessmentId,
    string Text,
    int Marks,
    int Order
) : IRequest<Guid>;
