
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Application.Common.Security;
using SynaptumLearn.Application.Curriculum.Lessons.ListLessonContents;

namespace SynaptumLearn.Application.Curriculum.Lessons.GetLessonContentById;

public sealed class GetLessonContentByIdQueryHandler
    : IRequestHandler<GetLessonContentByIdQuery, LessonContentItemDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetLessonContentByIdQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<LessonContentItemDto?> Handle(
        GetLessonContentByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.IsInRole(PlatformRoles.PlatformAdmin))
        {
            throw new UnauthorizedAccessException(
                "Only Platform Administrators can access lesson content management.");
        }

        return await _context.LessonContents
            .AsNoTracking()
            .Where(x => x.Id == request.ContentId)
            .Select(x => new LessonContentItemDto
            {
                Id = x.Id,
                LessonId = x.LessonId,
                Type = x.Type,
                Title = x.Title,
                Content = x.Content,
                Order = x.Order
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
