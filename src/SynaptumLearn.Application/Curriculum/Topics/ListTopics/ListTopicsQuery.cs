
using MediatR;

namespace SynaptumLearn.Application.Curriculum.Topics.ListTopics;

public sealed record ListTopicsQuery()
    : IRequest<IReadOnlyList<TopicListItemDto>>;
