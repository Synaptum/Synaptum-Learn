
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Curriculum.Topics.CreateTopic;
using Xunit;

namespace SynaptumLearn.Tests;

public sealed class CreateTopicIntegrationTests
{
    [Fact]
    public async Task PlatformAdmin_CanCreateTopic()
    {
        await using var fixture = new CreateTopicTestFixture();
        await fixture.InitializeAsync();

        var command = new CreateTopicCommand(
            fixture.GradeSubject.Id,
            "Algebra",
            "Introduction to algebra",
            1,
            1);

        var validator = new CreateTopicCommandValidator(
            fixture.Context);

        await validator.ValidateAndThrowAsync(command);

        var handler = new CreateTopicCommandHandler(
            fixture.Context,
            fixture.CurrentUser);

        var topicId = await handler.Handle(
            command,
            CancellationToken.None);

        var topic = await fixture.Context.Topics
            .AsNoTracking()
            .SingleAsync(x => x.Id == topicId);

        Assert.Equal(fixture.GradeSubject.Id, topic.GradeSubjectId);
        Assert.Equal("Algebra", topic.Title);
        Assert.Equal(1, topic.Term);
        Assert.Equal(1, topic.Order);
    }

    [Fact]
    public async Task NonPlatformAdmin_CannotCreateTopic()
    {
        await using var fixture = new CreateTopicTestFixture();
        await fixture.InitializeAsync();

        fixture.CurrentUser.IsPlatformAdmin = false;

        var handler = new CreateTopicCommandHandler(
            fixture.Context,
            fixture.CurrentUser);

        var command = new CreateTopicCommand(
            fixture.GradeSubject.Id,
            "Unauthorized Topic",
            "Should not be saved",
            1,
            1);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(
                command,
                CancellationToken.None));

        Assert.False(await fixture.Context.Topics
            .AnyAsync(x => x.GradeSubjectId == fixture.GradeSubject.Id));
    }

    [Fact]
    public async Task UnauthenticatedUser_CannotCreateTopic()
    {
        await using var fixture = new CreateTopicTestFixture();
        await fixture.InitializeAsync();

        fixture.CurrentUser.IsAuthenticated = false;

        var handler = new CreateTopicCommandHandler(
            fixture.Context,
            fixture.CurrentUser);

        var command = new CreateTopicCommand(
            fixture.GradeSubject.Id,
            "Unauthorized Topic",
            "Should not be saved",
            1,
            1);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(
                command,
                CancellationToken.None));

        Assert.False(await fixture.Context.Topics
            .AnyAsync(x => x.GradeSubjectId == fixture.GradeSubject.Id));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    public async Task InvalidTerm_IsRejected(int term)
    {
        await using var fixture = new CreateTopicTestFixture();
        await fixture.InitializeAsync();

        var validator = new CreateTopicCommandValidator(
            fixture.Context);

        var command = new CreateTopicCommand(
            fixture.GradeSubject.Id,
            "Invalid Term Topic",
            "Invalid term",
            term,
            1);

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            x => x.PropertyName == nameof(CreateTopicCommand.Term));
    }

    [Fact]
    public async Task NonexistentGradeSubject_IsRejected()
    {
        await using var fixture = new CreateTopicTestFixture();
        await fixture.InitializeAsync();

        var validator = new CreateTopicCommandValidator(
            fixture.Context);

        var command = new CreateTopicCommand(
            Guid.NewGuid(),
            "Algebra",
            "Introduction",
            1,
            1);

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            x => x.PropertyName == nameof(CreateTopicCommand.GradeSubjectId));
    }

    [Fact]
    public async Task DuplicatePosition_IsRejected()
    {
        await using var fixture = new CreateTopicTestFixture();
        await fixture.InitializeAsync();

        var handler = new CreateTopicCommandHandler(
            fixture.Context,
            fixture.CurrentUser);

        var validator = new CreateTopicCommandValidator(
            fixture.Context);

        var first = new CreateTopicCommand(
            fixture.GradeSubject.Id,
            "Algebra",
            "First topic",
            1,
            1);

        await validator.ValidateAndThrowAsync(first);

        await handler.Handle(first, CancellationToken.None);

        var duplicate = new CreateTopicCommand(
            fixture.GradeSubject.Id,
            "Geometry",
            "Duplicate position",
            1,
            1);

        var result = await validator.ValidateAsync(duplicate);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task SamePositionInDifferentTerms_IsAllowed()
    {
        await using var fixture = new CreateTopicTestFixture();
        await fixture.InitializeAsync();

        var handler = new CreateTopicCommandHandler(
            fixture.Context,
            fixture.CurrentUser);

        var validator = new CreateTopicCommandValidator(
            fixture.Context);

        var first = new CreateTopicCommand(
            fixture.GradeSubject.Id,
            "Term 1 Algebra",
            "Term 1",
            1,
            1);

        var second = new CreateTopicCommand(
            fixture.GradeSubject.Id,
            "Term 2 Algebra",
            "Term 2",
            2,
            1);

        await validator.ValidateAndThrowAsync(first);
        await handler.Handle(first, CancellationToken.None);

        await validator.ValidateAndThrowAsync(second);
        await handler.Handle(second, CancellationToken.None);

        var count = await fixture.Context.Topics
            .CountAsync(x =>
                x.GradeSubjectId == fixture.GradeSubject.Id);

        Assert.Equal(2, count);
    }
}
