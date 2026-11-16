
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Domain.Content;
using SynaptumLearn.Domain.Enums;

namespace SynaptumLearn.Application.Curriculum.LearnerCurriculum;

public sealed class GetLearnerLessonsQueryHandler
    : IRequestHandler<GetLearnerLessonsQuery, IReadOnlyList<LearnerLessonDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetLearnerLessonsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<LearnerLessonDto>> Handle(
        GetLearnerLessonsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            _currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedAccessException(
                "An authenticated learner account is required.");
        }

        if (_currentUser.SchoolId is not Guid currentSchoolId)
        {
            throw new UnauthorizedAccessException(
                "An active school context is required.");
        }

        var learner = await (
            from profile in _context.LearnerProfiles.AsNoTracking()
            join membership in _context.SchoolMemberships.AsNoTracking()
                on new { profile.UserId, profile.SchoolId }
                equals new { membership.UserId, membership.SchoolId }
            join school in _context.Schools.AsNoTracking()
                on profile.SchoolId equals school.Id
            join user in _context.BusinessUsers.AsNoTracking()
                on profile.UserId equals user.Id
            where profile.UserId == userId
                && profile.SchoolId == currentSchoolId
                && membership.IsActive
                && membership.Role == SchoolRole.Learner
                && school.Status == SchoolStatus.Active
                && user.IsActive
            select new
            {
                profile.Id,
                profile.GradeId,
                school.CurriculumAccessMode,
                school.CurrentTerm
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (learner is null)
        {
            throw new UnauthorizedAccessException(
                "No active learner profile and school membership were found.");
        }

        if (learner.CurrentTerm is < 1 or > 4 ||
            learner.CurriculumAccessMode is not (
                CurriculumAccessMode.Progressive or
                CurriculumAccessMode.FullYear))
        {
            throw new InvalidOperationException(
                "The school's curriculum settings are invalid.");
        }

        var visibleTerm =
            learner.CurriculumAccessMode == CurriculumAccessMode.FullYear
                ? 4
                : learner.CurrentTerm;

        var topicExists = await _context.Topics
            .AsNoTracking()
            .AnyAsync(
                topic => topic.Id == request.TopicId
                    && topic.GradeSubject.GradeId == learner.GradeId
                    && topic.Term >= 1
                    && topic.Term <= visibleTerm,
                cancellationToken);

        if (!topicExists)
        {
            throw new UnauthorizedAccessException(
                "The requested topic is not accessible to this learner.");
        }

        var lessons = await _context.Lessons
            .AsNoTracking()
            .Where(lesson =>
                lesson.TopicId == request.TopicId &&
                lesson.Status == LessonStatus.Published)
            .OrderBy(lesson => lesson.Order)
            .ThenBy(lesson => lesson.Id)
            .Select(lesson => new
            {
                lesson.Id,
                lesson.Title,
                lesson.Summary,
                lesson.Order
            })
            .ToListAsync(cancellationToken);

        if (lessons.Count == 0)
        {
            return Array.Empty<LearnerLessonDto>();
        }

        var lessonIds = lessons.Select(x => x.Id).ToArray();

        var completedLessonIds = await _context.LessonProgresses
            .AsNoTracking()
            .Where(progress =>
                progress.LearnerProfileId == learner.Id &&
                lessonIds.Contains(progress.LessonId) &&
                progress.Completed)
            .Select(progress => progress.LessonId)
            .ToListAsync(cancellationToken);

        var completedSet = completedLessonIds.ToHashSet();

        var requiredAssessments = await _context.Assessments
            .AsNoTracking()
            .Where(assessment =>
                lessonIds.Contains(assessment.LessonId) &&
                assessment.IsRequiredForProgression &&
                assessment.IsPublished)
            .Select(assessment => new
            {
                assessment.Id,
                assessment.LessonId
            })
            .ToListAsync(cancellationToken);

        var assessmentByLesson = requiredAssessments
            .ToDictionary(x => x.LessonId, x => x.Id);

        var requiredAssessmentIds = requiredAssessments
            .Select(x => x.Id)
            .ToArray();

        var passedAssessmentIds = await _context.AssessmentAttempts
            .AsNoTracking()
            .Where(attempt =>
                attempt.SchoolId == currentSchoolId &&
                attempt.LearnerProfileId == learner.Id &&
                requiredAssessmentIds.Contains(attempt.AssessmentId) &&
                attempt.Passed &&
                attempt.CompletedAt != null)
            .Select(attempt => attempt.AssessmentId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var passedSet = passedAssessmentIds.ToHashSet();

        var result = new List<LearnerLessonDto>(lessons.Count);

        var isUnlocked = true;

        foreach (var lesson in lessons)
        {
            result.Add(new LearnerLessonDto(
                lesson.Id,
                lesson.Title,
                lesson.Summary,
                lesson.Order,
                isUnlocked,
                isUnlocked && completedSet.Contains(lesson.Id)
            ));

            // The next lesson requires a passing attempt
            // for this lesson's published progression assessment.
            isUnlocked = isUnlocked &&
                assessmentByLesson.TryGetValue(
                    lesson.Id,
                    out var assessmentId) &&
                passedSet.Contains(assessmentId);
        }

        return result;
    }
}
