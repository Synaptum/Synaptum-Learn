
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Curriculum.LearnerCurriculum;
using SynaptumLearn.Domain.Assessments;
using SynaptumLearn.Domain.Content;
using SynaptumLearn.Domain.Curriculum;
using SynaptumLearn.Domain.Analytics;
using Xunit;

namespace SynaptumLearn.Tests;

public class GetLearnerLessonsIntegrationTests
{
    [Fact]
    public async Task FirstPublishedLesson_IsUnlocked_AndSecondLesson_IsLockedWithoutPassingAttempt()
    {
        await using var fixture = new LearnerCurriculumTestFixture();
        await fixture.InitializeAsync();

        var (topic, lessons) = await CreateLessonsAsync(fixture, 2);

        await CreateAssessmentAsync(fixture, lessons[0]);

        var result = await GetLessonsAsync(fixture, topic.Id);

        Assert.Equal(2, result.Count);
        Assert.Equal(lessons[0].Id, result[0].LessonId);
        Assert.True(result[0].IsUnlocked);
        Assert.False(result[0].IsCompleted);
        Assert.Equal(lessons[1].Id, result[1].LessonId);
        Assert.False(result[1].IsUnlocked);
        Assert.False(result[1].IsCompleted);
    }

    [Fact]
    public async Task PassingRequiredAssessment_UnlocksNextLesson()
    {
        await using var fixture = new LearnerCurriculumTestFixture();
        await fixture.InitializeAsync();

        var (topic, lessons) = await CreateLessonsAsync(fixture, 2);
        var assessment = await CreateAssessmentAsync(fixture, lessons[0]);

        await CreateAttemptAsync(fixture, assessment, passed: true);

        var result = await GetLessonsAsync(fixture, topic.Id);

        Assert.Equal(2, result.Count);
        Assert.True(result[0].IsUnlocked);
        Assert.True(result[1].IsUnlocked);
        Assert.False(result[0].IsCompleted);
        Assert.False(result[1].IsCompleted);
    }

    [Fact]
    public async Task FailedAssessment_KeepsNextLessonLocked()
    {
        await using var fixture = new LearnerCurriculumTestFixture();
        await fixture.InitializeAsync();

        var (topic, lessons) = await CreateLessonsAsync(fixture, 2);
        var assessment = await CreateAssessmentAsync(fixture, lessons[0]);

        await CreateAttemptAsync(fixture, assessment, passed: false);

        var result = await GetLessonsAsync(fixture, topic.Id);

        Assert.True(result[0].IsUnlocked);
        Assert.False(result[1].IsUnlocked);
    }

    [Fact]
    public async Task OptionalAssessment_DoesNotUnlockNextLesson()
    {
        await using var fixture = new LearnerCurriculumTestFixture();
        await fixture.InitializeAsync();

        var (topic, lessons) = await CreateLessonsAsync(fixture, 2);

        var assessment = await CreateAssessmentAsync(
            fixture,
            lessons[0],
            required: false);

        await CreateAttemptAsync(fixture, assessment, passed: true);

        var result = await GetLessonsAsync(fixture, topic.Id);

        Assert.True(result[0].IsUnlocked);
        Assert.False(result[1].IsUnlocked);
    }

    [Fact]
    public async Task UnpublishedRequiredAssessment_DoesNotUnlockNextLesson()
    {
        await using var fixture = new LearnerCurriculumTestFixture();
        await fixture.InitializeAsync();

        var (topic, lessons) = await CreateLessonsAsync(fixture, 2);

        var assessment = await CreateAssessmentAsync(
            fixture,
            lessons[0],
            published: false);

        await CreateAttemptAsync(fixture, assessment, passed: true);

        var result = await GetLessonsAsync(fixture, topic.Id);

        Assert.True(result[0].IsUnlocked);
        Assert.False(result[1].IsUnlocked);
    }

    [Fact]
    public async Task IncompletePassingAttempt_DoesNotUnlockNextLesson()
    {
        await using var fixture = new LearnerCurriculumTestFixture();
        await fixture.InitializeAsync();

        var (topic, lessons) = await CreateLessonsAsync(fixture, 2);
        var assessment = await CreateAssessmentAsync(fixture, lessons[0]);

        await CreateAttemptAsync(
            fixture,
            assessment,
            passed: true,
            completed: false);

        var result = await GetLessonsAsync(fixture, topic.Id);

        Assert.True(result[0].IsUnlocked);
        Assert.False(result[1].IsUnlocked);
    }

    [Fact]
    public async Task MissingRequiredAssessment_KeepsNextLessonLocked()
    {
        await using var fixture = new LearnerCurriculumTestFixture();
        await fixture.InitializeAsync();

        var (topic, _) = await CreateLessonsAsync(fixture, 2);

        var result = await GetLessonsAsync(fixture, topic.Id);

        Assert.Equal(2, result.Count);
        Assert.True(result[0].IsUnlocked);
        Assert.False(result[1].IsUnlocked);
    }

    [Fact]
    public async Task ThreeLessons_UnlockSequentially()
    {
        await using var fixture = new LearnerCurriculumTestFixture();
        await fixture.InitializeAsync();

        var (topic, lessons) = await CreateLessonsAsync(fixture, 3);

        var firstAssessment = await CreateAssessmentAsync(
            fixture,
            lessons[0]);

        await CreateAssessmentAsync(fixture, lessons[1]);

        await CreateAttemptAsync(
            fixture,
            firstAssessment,
            passed: true);

        var result = await GetLessonsAsync(fixture, topic.Id);

        Assert.Equal(3, result.Count);
        Assert.True(result[0].IsUnlocked);
        Assert.True(result[1].IsUnlocked);
        Assert.False(result[2].IsUnlocked);
    }

    [Fact]
    public async Task UnpublishedLessons_AreExcluded()
    {
        await using var fixture = new LearnerCurriculumTestFixture();
        await fixture.InitializeAsync();

        var (topic, lessons) = await CreateLessonsAsync(fixture, 3);

        lessons[1].Status = LessonStatus.Draft;
        lessons[2].Status = LessonStatus.Archived;

        await fixture.Context.SaveChangesAsync();

        var result = await GetLessonsAsync(fixture, topic.Id);

        Assert.Single(result);
        Assert.Equal(lessons[0].Id, result[0].LessonId);
        Assert.True(result[0].IsUnlocked);
    }

    [Fact]
    public async Task FutureTermTopic_IsNotAccessible()
    {
        await using var fixture = new LearnerCurriculumTestFixture();
        await fixture.InitializeAsync();

        var (topic, _) = await CreateLessonsAsync(
            fixture,
            1,
            term: 4);

        // The fixture's school must use Progressive mode
        // with CurrentTerm below 4 for this assertion.
        fixture.School.CurriculumAccessMode =
            SynaptumLearn.Domain.Enums.CurriculumAccessMode.Progressive;
        fixture.School.CurrentTerm = 1;

        await fixture.Context.SaveChangesAsync();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => GetLessonsAsync(fixture, topic.Id));
    }

    [Fact]
    public async Task TopicFromAnotherGrade_IsNotAccessible()
    {
        await using var fixture = new LearnerCurriculumTestFixture();
        await fixture.InitializeAsync();

        var (topic, _) = await CreateLessonsAsync(fixture, 1);

        var otherGrade = new Grade
        {
            Name = $"Other Grade {Guid.NewGuid():N}"[..20],
            Order = 12
        };

        fixture.Context.Grades.Add(otherGrade);
        await fixture.Context.SaveChangesAsync();

        // Reassign the test topic's grade-subject relationship
        // to a grade the learner does not belong to.
        var gradeSubject = await fixture.Context.GradeSubjects
            .SingleAsync(gs => gs.Id == topic.GradeSubjectId);

        gradeSubject.GradeId = otherGrade.Id;
        await fixture.Context.SaveChangesAsync();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => GetLessonsAsync(fixture, topic.Id));

        // Restore the relationship before fixture cleanup.
        gradeSubject.GradeId = fixture.Grade.Id;
        await fixture.Context.SaveChangesAsync();

        fixture.Context.Grades.Remove(otherGrade);
        await fixture.Context.SaveChangesAsync();
    }

    [Fact]
    public async Task CompletedLesson_ReturnsCompletedStatus()
    {
        await using var fixture = new LearnerCurriculumTestFixture();
        await fixture.InitializeAsync();

        var (topic, lessons) = await CreateLessonsAsync(fixture, 2);

        var learnerProfileId = await fixture.Context.LearnerProfiles
            .Where(profile =>
                profile.UserId == fixture.LearnerId &&
                profile.SchoolId == fixture.School.Id)
            .Select(profile => profile.Id)
            .SingleAsync();

        var progress = new LessonProgress
        {
            SchoolId = fixture.School.Id,
            LearnerProfileId = learnerProfileId,
            LessonId = lessons[0].Id,
            Completed = true,
            CompletedAt = DateTime.UtcNow,
            ProgressPercentage = 100
        };

        fixture.Context.LessonProgresses.Add(progress);
        await fixture.Context.SaveChangesAsync();

        var result = await GetLessonsAsync(fixture, topic.Id);

        Assert.Equal(2, result.Count);

        Assert.True(result[0].IsUnlocked);
        Assert.True(result[0].IsCompleted);

        Assert.False(result[1].IsUnlocked);
        Assert.False(result[1].IsCompleted);
    }

    [Fact]
    public async Task PassingAttemptFromAnotherSchool_DoesNotUnlockNextLesson()
    {
        await using var fixture = new LearnerCurriculumTestFixture();
        await fixture.InitializeAsync();

        var (topic, lessons) = await CreateLessonsAsync(fixture, 2);
        var assessment = await CreateAssessmentAsync(fixture, lessons[0]);

        var learnerProfileId = await fixture.Context.LearnerProfiles
            .Where(profile =>
                profile.UserId == fixture.LearnerId &&
                profile.SchoolId == fixture.School.Id)
            .Select(profile => profile.Id)
            .SingleAsync();

        // Use a different SchoolId to simulate an attempt
        // that does not belong to the current school.
        var attempt = new AssessmentAttempt
        {
            SchoolId = Guid.NewGuid(),
            AssessmentId = assessment.Id,
            LearnerProfileId = learnerProfileId,
            Score = 80,
            Passed = true,
            StartedAt = DateTime.UtcNow.AddMinutes(-5),
            CompletedAt = DateTime.UtcNow
        };

        fixture.Context.AssessmentAttempts.Add(attempt);
        await fixture.Context.SaveChangesAsync();

        var result = await GetLessonsAsync(fixture, topic.Id);

        Assert.True(result[0].IsUnlocked);
        Assert.False(result[1].IsUnlocked);
    }

    [Fact]
    public async Task AnotherLearnersPassingAttempt_DoesNotUnlockNextLesson()
    {
        await using var fixture = new LearnerCurriculumTestFixture();
        await fixture.InitializeAsync();

        // Arrange: Two learners belong to the same school and grade.
        var (topic, lessons) = await CreateLessonsAsync(fixture, 2);

        var assessment = await CreateAssessmentAsync(
            fixture,
            lessons[0]);

        var originalLearnerId = fixture.LearnerId;

        var secondLearnerId =
            await fixture.CreateAdditionalLearnerAsync();

        var secondLearnerProfileId =
            await fixture.Context.LearnerProfiles
                .Where(profile =>
                    profile.UserId == secondLearnerId &&
                    profile.SchoolId == fixture.School.Id)
                .Select(profile => profile.Id)
                .SingleAsync();

        // Only the second learner passes the assessment.
        var attempt = new AssessmentAttempt
        {
            SchoolId = fixture.School.Id,
            AssessmentId = assessment.Id,
            LearnerProfileId = secondLearnerProfileId,
            Score = 85,
            Passed = true,
            StartedAt = DateTime.UtcNow.AddMinutes(-10),
            CompletedAt = DateTime.UtcNow
        };

        fixture.Context.AssessmentAttempts.Add(attempt);
        await fixture.Context.SaveChangesAsync();

        // Act: Retrieve lessons as the original learner.
        fixture.CurrentUser.UserId = originalLearnerId;

        var originalLearnerLessons = await GetLessonsAsync(
            fixture,
            topic.Id);

        // Assert: Another learner's success must not unlock lessons.
        Assert.Equal(2, originalLearnerLessons.Count);

        Assert.True(originalLearnerLessons[0].IsUnlocked);
        Assert.False(originalLearnerLessons[1].IsUnlocked);

        // Act: Retrieve the same lessons as the learner who passed.
        fixture.CurrentUser.UserId = secondLearnerId;

        var secondLearnerLessons = await GetLessonsAsync(
            fixture,
            topic.Id);

        // Assert: The passing learner can access the next lesson.
        Assert.Equal(2, secondLearnerLessons.Count);

        Assert.True(secondLearnerLessons[0].IsUnlocked);
        Assert.True(secondLearnerLessons[1].IsUnlocked);
    }

    #region HelperMethods
    private static async Task<(Topic Topic, List<Lesson> Lessons)>
        CreateLessonsAsync(
            LearnerCurriculumTestFixture fixture,
            int count,
            int term = 1)
    {
        var context = fixture.Context;

        var subject = new Subject
        {
            Name = "Mathematics",
            Code = $"M{Guid.NewGuid():N}"[..12]
        };

        var gradeSubject = new GradeSubject
        {
            GradeId = fixture.Grade.Id,
            Subject = subject
        };

        var topic = new Topic
        {
            GradeSubject = gradeSubject,
            Title = "Integration Test Topic",
            Description = "Lesson progression test topic",
            Term = term,
            Order = 1
        };

        context.Topics.Add(topic);
        await context.SaveChangesAsync();

        var lessons = Enumerable.Range(1, count)
            .Select(order => new Lesson
            {
                TopicId = topic.Id,
                Title = $"Lesson {order}",
                Summary = $"Summary for lesson {order}",
                Order = order,
                Status = LessonStatus.Published,
                PublishedAt = DateTime.UtcNow
            })
            .ToList();

        context.Lessons.AddRange(lessons);
        await context.SaveChangesAsync();

        return (topic, lessons);
    }

    private static async Task<Assessment> CreateAssessmentAsync(
        LearnerCurriculumTestFixture fixture,
        Lesson lesson,
        bool required = true,
        bool published = true)
    {
        var assessment = new Assessment
        {
            LessonId = lesson.Id,
            Title = $"Assessment for {lesson.Title}",
            Instructions = "Complete the assessment.",
            PassMark = 50,
            IsPublished = published,
            IsRequiredForProgression = required
        };

        fixture.Context.Assessments.Add(assessment);
        await fixture.Context.SaveChangesAsync();

        return assessment;
    }

    private static async Task CreateAttemptAsync(
        LearnerCurriculumTestFixture fixture,
        Assessment assessment,
        bool passed,
        bool completed = true)
    {
        var learnerProfileId = await fixture.Context.LearnerProfiles
            .Where(profile =>
                profile.UserId == fixture.CurrentUser.UserId &&
                profile.SchoolId == fixture.School.Id)
            .Select(profile => profile.Id)
            .SingleAsync();

        var attempt = new AssessmentAttempt
        {
            SchoolId = fixture.School.Id,
            AssessmentId = assessment.Id,
            LearnerProfileId = learnerProfileId,
            Score = passed ? 80 : 30,
            Passed = passed,
            StartedAt = DateTime.UtcNow.AddMinutes(-10),
            CompletedAt = completed ? DateTime.UtcNow : null
        };

        fixture.Context.AssessmentAttempts.Add(attempt);
        await fixture.Context.SaveChangesAsync();
    }

    private static async Task<IReadOnlyList<LearnerLessonDto>>
        GetLessonsAsync(
            LearnerCurriculumTestFixture fixture,
            Guid topicId)
    {
        var handler = new GetLearnerLessonsQueryHandler(
            fixture.Context,
            fixture.CurrentUser);

        return await handler.Handle(
            new GetLearnerLessonsQuery(topicId),
            CancellationToken.None);
    }
    #endregion

}
