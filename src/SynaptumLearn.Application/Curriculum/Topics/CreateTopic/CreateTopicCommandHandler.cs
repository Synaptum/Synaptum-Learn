
using MediatR;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Application.Common.Security;
using SynaptumLearn.Domain.Content;

namespace SynaptumLearn.Application.Curriculum.Topics.CreateTopic;

public sealed class CreateTopicCommandHandler
    : IRequestHandler<CreateTopicCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateTopicCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(
        CreateTopicCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.IsInRole(PlatformRoles.PlatformAdmin))
        {
            throw new UnauthorizedAccessException(
                "Only Prime Synaptum Platform Administrators can create curriculum topics.");
        }

        var topic = new Topic
        {
            GradeSubjectId = request.GradeSubjectId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Term = request.Term,
            Order = request.Order
        };

        _context.Topics.Add(topic);

        await _context.SaveChangesAsync(cancellationToken);

        return topic.Id;
    }
}
