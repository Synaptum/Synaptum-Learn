
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Application.Common.Security;
using SynaptumLearn.Domain.Content;

namespace SynaptumLearn.Application.Curriculum.Lessons.UpdateLessonContent;

public sealed class UpdateLessonContentCommandHandler
    : IRequestHandler<UpdateLessonContentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateLessonContentCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(
        UpdateLessonContentCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.IsInRole(PlatformRoles.PlatformAdmin))
        {
            throw new UnauthorizedAccessException(
                "Only Prime Synaptum Platform Administrators can edit lesson content.");
        }

        var block = await _context.LessonContents
            .Include(x => x.Lesson)
            .FirstOrDefaultAsync(
                x => x.Id == request.ContentId,
                cancellationToken);

        if (block is null)
            throw new InvalidOperationException("Content block not found.");

        if (block.Lesson.Status != LessonStatus.Draft)
        {
            throw new InvalidOperationException(
                "Only content belonging to draft lessons can be edited.");
        }

        var positionOccupied = await _context.LessonContents
            .AsNoTracking()
            .AnyAsync(
                x => x.LessonId == block.LessonId &&
                     x.Order == request.Order &&
                     x.Id != block.Id,
                cancellationToken);

        if (positionOccupied)
        {
            throw new InvalidOperationException(
                "Another content block already occupies this position.");
        }

        block.Type = request.Type;
        block.Title = request.Title.Trim();
        block.Content = request.Content.Trim();
        block.Order = request.Order;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
