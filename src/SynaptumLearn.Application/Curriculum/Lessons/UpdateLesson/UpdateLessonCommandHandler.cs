
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Domain.Content;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Application.Common.Security;

namespace SynaptumLearn.Application.Curriculum.Lessons.UpdateLesson;

public sealed class UpdateLessonCommandHandler
    : IRequestHandler<UpdateLessonCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateLessonCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(
        UpdateLessonCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.IsInRole(PlatformRoles.PlatformAdmin))
        {
            throw new UnauthorizedAccessException(
                "Only Prime Synaptum Platform Administrators can edit lessons.");
        }

        var lesson = await _context.Lessons
            .FirstOrDefaultAsync(
                x => x.Id == request.LessonId,
                cancellationToken);

        if (lesson is null)
            throw new InvalidOperationException("Lesson not found.");

        if (lesson.Status != LessonStatus.Draft)
        {
            throw new InvalidOperationException(
                "Only draft lessons can be edited.");
        }

        var positionOccupied = await _context.Lessons
            .AsNoTracking()
            .AnyAsync(
                x => x.TopicId == lesson.TopicId &&
                     x.Order == request.Order &&
                     x.Id != lesson.Id,
                cancellationToken);

        if (positionOccupied)
            throw new InvalidOperationException(
                "Another lesson already occupies this position.");

        lesson.Title = request.Title.Trim();
        lesson.Summary = request.Summary?.Trim() ?? string.Empty;
        lesson.Order = request.Order;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
