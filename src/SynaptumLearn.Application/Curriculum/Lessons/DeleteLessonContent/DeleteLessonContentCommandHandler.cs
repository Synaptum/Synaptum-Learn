
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Application.Common.Security;
using SynaptumLearn.Domain.Content;

namespace SynaptumLearn.Application.Curriculum.Lessons.DeleteLessonContent;

public sealed class DeleteLessonContentCommandHandler
    : IRequestHandler<DeleteLessonContentCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteLessonContentCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(
        DeleteLessonContentCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.IsInRole(PlatformRoles.PlatformAdmin))
        {
            throw new UnauthorizedAccessException(
                "Only Prime Synaptum Platform Administrators can delete lesson content.");
        }

        if (request.ContentId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "A valid content block ID is required.");
        }

        var block = await _context.LessonContents
            .Include(x => x.Lesson)
            .FirstOrDefaultAsync(
                x => x.Id == request.ContentId,
                cancellationToken);

        if (block is null)
        {
            throw new InvalidOperationException(
                "Content block not found.");
        }

        if (block.Lesson.Status != LessonStatus.Draft)
        {
            throw new InvalidOperationException(
                "Only content belonging to draft lessons can be deleted.");
        }

        var lessonId = block.LessonId;

        _context.LessonContents.Remove(block);

        await _context.SaveChangesAsync(cancellationToken);

        return lessonId;
    }
}
