
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Application.Common.Security;
using SynaptumLearn.Domain.Content;

namespace SynaptumLearn.Application.Curriculum.Lessons.MoveLessonContent;

public sealed class MoveLessonContentCommandHandler
    : IRequestHandler<MoveLessonContentCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public MoveLessonContentCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(
        MoveLessonContentCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.IsInRole(PlatformRoles.PlatformAdmin))
        {
            throw new UnauthorizedAccessException(
                "Only Platform Administrators can reorder lesson content.");
        }

        if (request.ContentId == Guid.Empty ||
            !Enum.IsDefined(request.Direction))
        {
            throw new InvalidOperationException(
                "Invalid content move request.");
        }

        Guid lessonId = Guid.Empty;

        await _context.ExecuteInTransactionAsync(
            async ct =>
            {
                var block = await _context.LessonContents
                    .Include(x => x.Lesson)
                    .SingleOrDefaultAsync(
                        x => x.Id == request.ContentId,
                        ct);

                if (block is null)
                    throw new InvalidOperationException(
                        "Content block not found.");

                if (block.Lesson.Status != LessonStatus.Draft)
                    throw new InvalidOperationException(
                        "Only draft lesson content can be reordered.");

                lessonId = block.LessonId;

                var siblingQuery = _context.LessonContents
                    .Where(x =>
                        x.LessonId == block.LessonId &&
                        x.Id != block.Id);

                LessonContent? sibling;

                if (request.Direction == LessonContentMoveDirection.Up)
                {
                    sibling = await siblingQuery
                        .Where(x => x.Order < block.Order)
                        .OrderByDescending(x => x.Order)
                        .FirstOrDefaultAsync(ct);
                }
                else
                {
                    sibling = await siblingQuery
                        .Where(x => x.Order > block.Order)
                        .OrderBy(x => x.Order)
                        .FirstOrDefaultAsync(ct);
                }

                if (sibling is null)
                    return;

                var originalOrder = block.Order;
                var siblingOrder = sibling.Order;

                // Use a position that is currently unused.
                var maximumOrder = await _context.LessonContents
                    .Where(x => x.LessonId == block.LessonId)
                    .MaxAsync(x => x.Order, ct);

                if (maximumOrder == int.MaxValue)
                    throw new InvalidOperationException(
                        "Cannot reorder content because the order limit was reached.");

                var temporaryOrder = maximumOrder + 1;

                // Step 1: Move the selected block to a free position.
                block.Order = temporaryOrder;
                await _context.SaveChangesAsync(ct);

                // Step 2: Move the adjacent block into the old position.
                sibling.Order = originalOrder;
                await _context.SaveChangesAsync(ct);

                // Step 3: Move the selected block into its new position.
                block.Order = siblingOrder;
                await _context.SaveChangesAsync(ct);
            },
            cancellationToken);

        return lessonId;
    }
}
