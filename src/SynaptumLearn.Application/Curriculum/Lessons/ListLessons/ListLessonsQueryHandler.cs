
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Application.Common.Security;

namespace SynaptumLearn.Application.Curriculum.Lessons.ListLessons;

public sealed class ListLessonsQueryHandler
    : IRequestHandler<ListLessonsQuery, IReadOnlyList<LessonListItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ListLessonsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<LessonListItemDto>> Handle(
        ListLessonsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.IsInRole(PlatformRoles.PlatformAdmin))
        {
            throw new UnauthorizedAccessException(
                "Only Prime Synaptum Platform Administrators can manage lessons.");
        }

        var query = _context.Lessons
            .AsNoTracking();

        if (request.TopicId.HasValue)
        {
            query = query.Where(
                x => x.TopicId == request.TopicId.Value);
        }

        return await query
            .OrderBy(x => x.Topic.GradeSubject.Grade.Order)
            .ThenBy(x => x.Topic.GradeSubject.Subject.Name)
            .ThenBy(x => x.Topic.Term)
            .ThenBy(x => x.Topic.Order)
            .ThenBy(x => x.Order)
            .Select(x => new LessonListItemDto
            {
                Id = x.Id,
                TopicId = x.TopicId,
                TopicTitle = x.Topic.Title,
                Title = x.Title,
                Summary = x.Summary,
                Order = x.Order,
                Status = x.Status,
                PublishedAt = x.PublishedAt
            })
            .ToListAsync(cancellationToken);
    }
}
