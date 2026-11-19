
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Application.Common.Security;
using SynaptumLearn.Domain.Content;

namespace SynaptumLearn.Application.Curriculum.Lessons.AddLessonContent;

public sealed class AddLessonContentCommandHandler
    : IRequestHandler<AddLessonContentCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AddLessonContentCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(
        AddLessonContentCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.IsInRole(PlatformRoles.PlatformAdmin))
        {
            throw new UnauthorizedAccessException(
                "Only Prime Synaptum Platform Administrators can manage lesson content.");
        }

        var lesson = await _context.Lessons
            .AsNoTracking()
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

        var positionOccupied = await _context.LessonContents
            .AsNoTracking()
            .AnyAsync(
                x => x.LessonId == request.LessonId &&
                     x.Order == request.Order,
                cancellationToken);

        if (positionOccupied)
        {
            throw new InvalidOperationException(
                "Another content block already occupies this position.");
        }

        var content = new LessonContent
        {
            LessonId = request.LessonId,
            Type = request.Type,
            Title = request.Title.Trim(),
            Content = request.Content.Trim(),
            Order = request.Order
        };

        _context.LessonContents.Add(content);

        await _context.SaveChangesAsync(cancellationToken);

        return content.Id;
    }
}
