using MediatR;

namespace SynaptumLearn.Application.Curriculum.Topics.CreateTopic;

public sealed record CreateTopicCommand(
    Guid GradeSubjectId,
    string Title,
    string Description,
    int Term,
    int Order)
    : IRequest<Guid>;