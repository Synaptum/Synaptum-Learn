
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Application.Common.Security;

namespace SynaptumLearn.Application.Curriculum.Lessons.ListLessonContents;

public sealed class ListLessonContentsQueryHandler
    : IRequestHandler<ListLessonContentsQuery,
        IReadOnlyList<LessonContentItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ListLessonContentsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<LessonContentItemDto>> Handle(
        ListLessonContentsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.IsInRole(PlatformRoles.PlatformAdmin))
        {
            throw new UnauthorizedAccessException(
                "Only Prime Synaptum Platform Administrators can manage lesson content.");
        }

        if (request.LessonId == Guid.Empty)
        {
            return Array.Empty<LessonContentItemDto>();
        }

        return await _context.LessonContents
            .AsNoTracking()
            .Where(x => x.LessonId == request.LessonId)
            .OrderBy(x => x.Order)
            .ThenBy(x => x.Id)
            .Select(x => new LessonContentItemDto
            {
                Id = x.Id,
                LessonId = x.LessonId,
                Type = x.Type,
                Title = x.Title,
                Content = x.Content,
                Order = x.Order
            })
            .ToListAsync(cancellationToken);
    }
}
