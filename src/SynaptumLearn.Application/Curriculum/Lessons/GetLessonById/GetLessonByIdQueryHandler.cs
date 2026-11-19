
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Application.Common.Security;
using SynaptumLearn.Application.Curriculum.Lessons.ListLessons;

namespace SynaptumLearn.Application.Curriculum.Lessons.GetLessonById;

public sealed class GetLessonByIdQueryHandler
    : IRequestHandler<GetLessonByIdQuery, LessonListItemDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetLessonByIdQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<LessonListItemDto?> Handle(
        GetLessonByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.IsInRole(PlatformRoles.PlatformAdmin))
        {
            throw new UnauthorizedAccessException(
                "Only Prime Synaptum Platform Administrators can retrieve lesson management details.");
        }

        if (request.LessonId == Guid.Empty)
            return null;

        return await _context.Lessons
            .AsNoTracking()
            .Where(x => x.Id == request.LessonId)
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
            .FirstOrDefaultAsync(cancellationToken);
    }
}
