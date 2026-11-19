
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Application.Common.Security;
using SynaptumLearn.Domain.Content;

namespace SynaptumLearn.Application.Curriculum.Lessons.CreateLesson;

public sealed class CreateLessonCommandHandler
    : IRequestHandler<CreateLessonCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateLessonCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(
        CreateLessonCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.IsInRole(PlatformRoles.PlatformAdmin))
        {
            throw new UnauthorizedAccessException(
                "Only Prime Synaptum Platform Administrators can create lessons.");
        }

        if (!await _context.Topics
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == request.TopicId,
                cancellationToken))
        {
            throw new InvalidOperationException(
                "The selected topic does not exist.");
        }

        if (await _context.Lessons
            .AsNoTracking()
            .AnyAsync(
                x => x.TopicId == request.TopicId &&
                     x.Order == request.Order,
                cancellationToken))
        {
            throw new InvalidOperationException(
                "Another lesson already occupies this position.");
        }

        var lesson = new Lesson
        {
            TopicId = request.TopicId,
            Title = request.Title.Trim(),
            Summary = request.Summary?.Trim() ?? string.Empty,
            Order = request.Order,
            Status = LessonStatus.Draft,
            PublishedAt = null
        };

        _context.Lessons.Add(lesson);

        await _context.SaveChangesAsync(cancellationToken);

        return lesson.Id;
    }
}
