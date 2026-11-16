namespace SynaptumLearn.Application.Curriculum.LearnerCurriculum;

public sealed record LearnerTopicDto(
    Guid TopicId,
    string Title,
    string Description,
    int Term,
    int Order
);