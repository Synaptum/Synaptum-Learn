
using MediatR;

namespace SynaptumLearn.Application.Curriculum.Assessments.UpdateAssessmentQuestion;

public sealed record UpdateAssessmentQuestionCommand(
    Guid QuestionId,
    string Text,
    int Marks
) : IRequest<Guid>;
