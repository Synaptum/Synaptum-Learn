
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Curriculum.Topics.ListTopics;
using SynaptumLearn.Domain.Content;
using Xunit;

namespace SynaptumLearn.Tests;

public sealed class ListTopicsIntegrationTests
{
    [Fact]
    public async Task PlatformAdmin_CanListTopicsInTermAndPositionOrder()
    {

        await using var fixture = new CreateTopicTestFixture();
        await fixture.InitializeAsync();

        var context = fixture.Context;


        var topics = new[]
        {
            new Topic
            {
                GradeSubjectId = fixture.GradeSubject.Id,
                Title = "Third Topic",
                Description = "Test",
                Term = 2,
                Order = 1
            },
            new Topic
            {
                GradeSubjectId = fixture.GradeSubject.Id,
                Title = "Second Topic",
                Description = "Test",
                Term = 1,
                Order = 2
            },
            new Topic
            {
                GradeSubjectId = fixture.GradeSubject.Id,
                Title = "First Topic",
                Description = "Test",
                Term = 1,
                Order = 1
            }
        };

        context.Topics.AddRange(topics);
        await context.SaveChangesAsync();

        var handler = new ListTopicsQueryHandler(
            context,
            fixture.CurrentUser);

        var result = await handler.Handle(
            new ListTopicsQuery(),
            CancellationToken.None);

        var matchingTopics = result
            .Where(x => x.GradeSubjectId == fixture.GradeSubject.Id)
            .ToList();

        Assert.Equal(3, matchingTopics.Count);

        Assert.Equal(
            new[] { "First Topic", "Second Topic", "Third Topic" },
            matchingTopics.Select(x => x.Title));
    }

    [Fact]
    public async Task NonPlatformAdmin_CannotListTopics()
    {
        await using var fixture = new CreateTopicTestFixture();
        await fixture.InitializeAsync();

        fixture.CurrentUser.IsPlatformAdmin = false;

        var handler = new ListTopicsQueryHandler(
            fixture.Context,
            fixture.CurrentUser);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(
                new ListTopicsQuery(),
                CancellationToken.None));
    }

    [Fact]
    public async Task UnauthenticatedUser_CannotListTopics()
    {
        await using var fixture = new CreateTopicTestFixture();
        await fixture.InitializeAsync();

        fixture.CurrentUser.IsAuthenticated = false;

        var handler = new ListTopicsQueryHandler(
            fixture.Context,
            fixture.CurrentUser);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(
                new ListTopicsQuery(),
                CancellationToken.None));
    }


}
