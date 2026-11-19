
using MediatR;

namespace SynaptumLearn.Application.Curriculum.Assessments.GetAssessmentIdByQuestion;

public sealed record GetAssessmentIdByQuestionQuery(Guid QuestionId)
    : IRequest<Guid?>;
