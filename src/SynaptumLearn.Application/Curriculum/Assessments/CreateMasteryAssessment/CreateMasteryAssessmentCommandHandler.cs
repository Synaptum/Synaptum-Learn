
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Application.Common.Security;
using SynaptumLearn.Domain.Assessments;
using SynaptumLearn.Domain.Content;

namespace SynaptumLearn.Application.Curriculum.Assessments.CreateMasteryAssessment;

public sealed class CreateMasteryAssessmentCommandHandler
    : IRequestHandler<CreateMasteryAssessmentCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateMasteryAssessmentCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(
        CreateMasteryAssessmentCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.IsInRole(PlatformRoles.PlatformAdmin))
        {
            throw new UnauthorizedAccessException(
                "Only Prime Synaptum Platform Administrators can create mastery assessments.");
        }

        if (request.LessonId == Guid.Empty ||
            string.IsNullOrWhiteSpace(request.Title) ||
            request.Title.Length > 200 ||
            request.Instructions is null ||
            request.Instructions.Length > 3000 ||
            request.PassMark < 1 ||
            request.PassMark > 100)
        {
            throw new InvalidOperationException(
                "Invalid mastery assessment details.");
        }

        var lesson = await _context.Lessons
            .AsNoTracking()
            .Where(x => x.Id == request.LessonId)
            .Select(x => new
            {
                x.Id,
                x.Status
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (lesson is null)
            throw new InvalidOperationException("Lesson not found.");

        if (lesson.Status != LessonStatus.Draft)
        {
            throw new InvalidOperationException(
                "Mastery assessments can only be created for draft lessons.");
        }

        var alreadyExists = await _context.Assessments
            .AsNoTracking()
            .AnyAsync(
                x => x.LessonId == request.LessonId &&
                     x.IsRequiredForProgression,
                cancellationToken);

        if (alreadyExists)
        {
            throw new InvalidOperationException(
                "This lesson already has a mastery assessment.");
        }

        var assessment = new Assessment
        {
            Id = Guid.NewGuid(),
            LessonId = request.LessonId,
            Title = request.Title.Trim(),
            Instructions = request.Instructions.Trim(),
            PassMark = request.PassMark,
            IsPublished = false,
            IsRequiredForProgression = true
        };

        _context.Assessments.Add(assessment);

        await _context.SaveChangesAsync(cancellationToken);

        return assessment.Id;
    }
}
