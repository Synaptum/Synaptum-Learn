
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SynaptumLearn.Application.Curriculum.LearnerCurriculum;
using SynaptumLearn.Persistence.Contexts;
using SynaptumLearn.Domain.Enums;
using SynaptumLearn.Domain.Content;
using SynaptumLearn.Domain.Curriculum;
using Xunit;

namespace SynaptumLearn.Tests;

public sealed class GetLearnerCurriculumIntegrationTests
{
    [Fact]
    public async Task UnauthenticatedUser_CannotAccessLearnerCurriculum()
    {
        using var provider =
            IntegrationTestServices.CreateServiceProvider();

        using var scope = provider.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        TestDatabaseConfiguration.Validate(
            context.Database.GetDbConnection().ConnectionString);

        Assert.True(await context.Database.CanConnectAsync());

        var currentUser = scope.ServiceProvider
            .GetRequiredService<TestCurrentUserService>();

        currentUser.IsAuthenticated = false;
        currentUser.UserId = null;

        var handler = new GetLearnerCurriculumQueryHandler(
            context,
            currentUser);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(
                new GetLearnerCurriculumQuery(),
                CancellationToken.None));
    }

    [Fact]
    public async Task AuthenticatedUserWithoutLearnerMembership_IsRejected()
    {
        using var provider =
            IntegrationTestServices.CreateServiceProvider();

        using var scope = provider.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        TestDatabaseConfiguration.Validate(
            context.Database.GetDbConnection().ConnectionString);

        Assert.True(await context.Database.CanConnectAsync());

        var currentUser = scope.ServiceProvider
            .GetRequiredService<TestCurrentUserService>();

        currentUser.IsAuthenticated = true;
        currentUser.UserId = Guid.NewGuid();

        var handler = new GetLearnerCurriculumQueryHandler(
            context,
            currentUser);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(
                new GetLearnerCurriculumQuery(),
                CancellationToken.None));
    }

    [Fact]
    public async Task LearnerWithInactiveMembership_CannotAccessCurriculum()
    {
        await using var fixture = new LearnerCurriculumTestFixture();
        await fixture.InitializeAsync();

        var context = fixture.Context;

        var membership = await context.SchoolMemberships
            .SingleAsync(x =>
                x.UserId == fixture.LearnerId &&
                x.SchoolId == fixture.School.Id);

        membership.IsActive = false;
        await context.SaveChangesAsync();

        var handler = new GetLearnerCurriculumQueryHandler(
            context,
            fixture.CurrentUser);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(
                new GetLearnerCurriculumQuery(),
                CancellationToken.None));
    }

    [Fact]
    public async Task ActiveLearner_CanRetrieveCurriculum()
    {
        await using var fixture = new LearnerCurriculumTestFixture();
        await fixture.InitializeAsync();

        var handler = new GetLearnerCurriculumQueryHandler(
            fixture.Context,
            fixture.CurrentUser);

        var result = await handler.Handle(
            new GetLearnerCurriculumQuery(),
            CancellationToken.None);

        Assert.Equal(fixture.Grade.Id, result.GradeId);
        Assert.Equal(CurriculumAccessMode.Progressive, result.AccessMode);
        Assert.Equal(1, result.CurrentTerm);
        Assert.Empty(result.Subjects);
    }

    [Fact]
    public async Task ProgressiveAccess_ReturnsOnlyTopicsUpToCurrentTerm()
    {
        await using var fixture = new LearnerCurriculumTestFixture();
        await fixture.InitializeAsync();

        var context = fixture.Context;

        fixture.School.CurriculumAccessMode =
            CurriculumAccessMode.Progressive;

        fixture.School.CurrentTerm = 2;

        var subject = new Subject
        {
            Name = "Mathematics",
            Code = $"MATH-{Guid.NewGuid():N}"[..13]
        };

        var gradeSubject = new GradeSubject
        {
            GradeId = fixture.Grade.Id,
            Subject = subject
        };

        var topics = Enumerable.Range(1, 4)
            .Select(term => new Topic
            {
                GradeSubject = gradeSubject,
                Title = $"Term {term} Topic",
                Description = $"Content for term {term}",
                Term = term,
                Order = 1
            })
            .ToList();

        context.Subjects.Add(subject);
        context.GradeSubjects.Add(gradeSubject);
        context.Topics.AddRange(topics);

        await context.SaveChangesAsync();

        var handler = new GetLearnerCurriculumQueryHandler(
            context,
            fixture.CurrentUser);

        var result = await handler.Handle(
            new GetLearnerCurriculumQuery(),
            CancellationToken.None);

        Assert.Equal(CurriculumAccessMode.Progressive, result.AccessMode);
        Assert.Equal(2, result.CurrentTerm);

        var learnerSubject = Assert.Single(result.Subjects);

        Assert.Equal(subject.Id, learnerSubject.SubjectId);

        Assert.Collection(
            learnerSubject.Topics,
            first => Assert.Equal(1, first.Term),
            second => Assert.Equal(2, second.Term));
    }

    [Fact]
    public async Task FullYearAccess_ReturnsTopicsFromAllFourTerms()
    {
        await using var fixture = new LearnerCurriculumTestFixture();
        await fixture.InitializeAsync();

        var context = fixture.Context;

        fixture.School.CurriculumAccessMode =
            CurriculumAccessMode.FullYear;

        fixture.School.CurrentTerm = 1;

        var subject = new Subject
        {
            Name = "Physical Sciences",
            Code = $"SCI-{Guid.NewGuid():N}"[..12]
        };

        var gradeSubject = new GradeSubject
        {
            GradeId = fixture.Grade.Id,
            Subject = subject
        };

        var topics = Enumerable.Range(1, 4)
            .Select(term => new Topic
            {
                GradeSubject = gradeSubject,
                Title = $"Term {term} Topic",
                Description = $"Physical Sciences term {term}",
                Term = term,
                Order = 1
            })
            .ToList();

        context.Subjects.Add(subject);
        context.GradeSubjects.Add(gradeSubject);
        context.Topics.AddRange(topics);

        await context.SaveChangesAsync();

        var handler = new GetLearnerCurriculumQueryHandler(
            context,
            fixture.CurrentUser);

        var result = await handler.Handle(
            new GetLearnerCurriculumQuery(),
            CancellationToken.None);

        Assert.Equal(CurriculumAccessMode.FullYear, result.AccessMode);
        Assert.Equal(1, result.CurrentTerm);

        var learnerSubject = Assert.Single(result.Subjects);

        Assert.Equal(subject.Id, learnerSubject.SubjectId);

        Assert.Equal(
            new[] { 1, 2, 3, 4 },
            learnerSubject.Topics.Select(t => t.Term).ToArray());
    }

    [Fact]
    public async Task LearnerCurriculum_ExcludesSubjectsFromOtherGrades()
    {
        await using var fixture = new LearnerCurriculumTestFixture();
        await fixture.InitializeAsync();

        var context = fixture.Context;

        var otherGrade = new Grade
        {
            Name = $"Other {Guid.NewGuid():N}"[..14],
            Order = 11
        };

        var subject = new Subject
        {
            Name = "Mathematics",
            Code = $"M-{Guid.NewGuid():N}"[..12]
        };

        var learnerGradeSubject = new GradeSubject
        {
            GradeId = fixture.Grade.Id,
            Subject = subject
        };

        var otherGradeSubject = new GradeSubject
        {
            Grade = otherGrade,
            Subject = subject
        };

        context.Grades.Add(otherGrade);
        context.Subjects.Add(subject);
        context.GradeSubjects.AddRange(
            learnerGradeSubject,
            otherGradeSubject);

        await context.SaveChangesAsync();

        try
        {
            var handler = new GetLearnerCurriculumQueryHandler(
                context,
                fixture.CurrentUser);

            var result = await handler.Handle(
                new GetLearnerCurriculumQuery(),
                CancellationToken.None);

            var learnerSubject = Assert.Single(result.Subjects);

            Assert.Equal(
                learnerGradeSubject.Id,
                learnerSubject.GradeSubjectId);

            Assert.DoesNotContain(
                result.Subjects,
                s => s.GradeSubjectId == otherGradeSubject.Id);
        }
        finally
        {
            // Remove the other grade's relationship before fixture cleanup.
            await context.GradeSubjects
                .Where(gs => gs.Id == otherGradeSubject.Id)
                .ExecuteDeleteAsync();

            context.ChangeTracker.Clear();

            await context.Grades
                .Where(g => g.Id == otherGrade.Id)
                .ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task LearnerCurriculum_OrdersTopicsByTermThenOrder()
    {
        await using var fixture = new LearnerCurriculumTestFixture();
        await fixture.InitializeAsync();

        var context = fixture.Context;

        fixture.School.CurrentTerm = 3;

        var subject = new Subject
        {
            Name = "Geography",
            Code = $"GEO-{Guid.NewGuid():N}"[..12]
        };

        var gradeSubject = new GradeSubject
        {
            GradeId = fixture.Grade.Id,
            Subject = subject
        };

        context.Subjects.Add(subject);
        context.GradeSubjects.Add(gradeSubject);

        context.Topics.AddRange(
            new Topic
            {
                GradeSubject = gradeSubject,
                Title = "Term 3 - Second",
                Description = "Third term, second topic",
                Term = 3,
                Order = 2
            },
            new Topic
            {
                GradeSubject = gradeSubject,
                Title = "Term 1 - Second",
                Description = "First term, second topic",
                Term = 1,
                Order = 2
            },
            new Topic
            {
                GradeSubject = gradeSubject,
                Title = "Term 2 - First",
                Description = "Second term, first topic",
                Term = 2,
                Order = 1
            },
            new Topic
            {
                GradeSubject = gradeSubject,
                Title = "Term 1 - First",
                Description = "First term, first topic",
                Term = 1,
                Order = 1
            },
            new Topic
            {
                GradeSubject = gradeSubject,
                Title = "Term 3 - First",
                Description = "Third term, first topic",
                Term = 3,
                Order = 1
            });

        await context.SaveChangesAsync();

        var handler = new GetLearnerCurriculumQueryHandler(
            context,
            fixture.CurrentUser);

        var result = await handler.Handle(
            new GetLearnerCurriculumQuery(),
            CancellationToken.None);

        var learnerSubject = Assert.Single(result.Subjects);

        Assert.Equal(
            new[]
            {
            "Term 1 - First",
            "Term 1 - Second",
            "Term 2 - First",
            "Term 3 - First",
            "Term 3 - Second"
            },
            learnerSubject.Topics.Select(t => t.Title).ToArray());
    }

    [Fact]
    public async Task InactiveLearner_CannotAccessCurriculum()
    {
        await using var fixture = new LearnerCurriculumTestFixture();
        await fixture.InitializeAsync();

        var context = fixture.Context;

        var learner = await context.BusinessUsers
            .SingleAsync(u => u.Id == fixture.LearnerId);

        learner.IsActive = false;
        await context.SaveChangesAsync();

        var handler = new GetLearnerCurriculumQueryHandler(
            context,
            fixture.CurrentUser);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(
                new GetLearnerCurriculumQuery(),
                CancellationToken.None));
    }

    [Fact]
    public async Task LearnerAtInactiveSchool_CannotAccessCurriculum()
    {
        await using var fixture = new LearnerCurriculumTestFixture();
        await fixture.InitializeAsync();

        var context = fixture.Context;

        fixture.School.Status = SchoolStatus.Archived;
        await context.SaveChangesAsync();

        var handler = new GetLearnerCurriculumQueryHandler(
            context,
            fixture.CurrentUser);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(
                new GetLearnerCurriculumQuery(),
                CancellationToken.None));
    }

    [Fact]
    public async Task LearnerWithMismatchedSchoolContext_CannotAccessCurriculum()
    {
        await using var fixture = new LearnerCurriculumTestFixture();
        await fixture.InitializeAsync();

        // Simulate a school context belonging to another tenant.
        fixture.CurrentUser.SchoolId = Guid.NewGuid();

        var handler = new GetLearnerCurriculumQueryHandler(
            fixture.Context,
            fixture.CurrentUser);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(
                new GetLearnerCurriculumQuery(),
                CancellationToken.None));
    }

}
