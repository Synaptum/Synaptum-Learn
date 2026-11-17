
using MediatR;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using SynaptumLearn.Application.Curriculum.LearnerCurriculum;
using SynaptumLearn.Web.Controllers;
using Xunit;

namespace SynaptumLearn.Tests;

public sealed class LearnerCurriculumControllerTests
{
    [Fact]
    public async Task Lessons_WithValidTopic_ReturnsViewWithLessons()
    {
        var topicId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();

        IReadOnlyList<LearnerLessonDto> lessons =
        [
            new LearnerLessonDto(
                lessonId,
                "Introduction to Algebra",
                "Understanding variables",
                1,
                true,
                false)
        ];

        var sender = new TestSender
        {
            Response = lessons
        };

        var controller = new LearnerCurriculumController(sender);

        var result = await controller.Lessons(
            topicId,
            CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<
            IReadOnlyList<LearnerLessonDto>>(view.Model);

        Assert.Single(model);
        Assert.Equal(lessonId, model[0].LessonId);
        Assert.Equal(topicId, view.ViewData["TopicId"]);

        var query = Assert.IsType<GetLearnerLessonsQuery>(
            sender.LastRequest);

        Assert.Equal(topicId, query.TopicId);
    }

    [Fact]
    public async Task Lessons_WithEmptyTopicId_ReturnsBadRequest()
    {
        var sender = new TestSender();
        var controller = new LearnerCurriculumController(sender);

        var result = await controller.Lessons(
            Guid.Empty,
            CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Null(sender.LastRequest);
    }

    [Fact]
    public async Task Lessons_WithInaccessibleTopic_ReturnsForbid()
    {
        var sender = new TestSender
        {
            ExceptionToThrow = new UnauthorizedAccessException()
        };

        var controller = new LearnerCurriculumController(sender);

        var result = await controller.Lessons(
            Guid.NewGuid(),
            CancellationToken.None);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task Lessons_WithNoPublishedLessons_ReturnsEmptyViewModel()
    {
        var sender = new TestSender
        {
            Response = Array.Empty<LearnerLessonDto>()
        };

        var controller = new LearnerCurriculumController(sender);

        var result = await controller.Lessons(
            Guid.NewGuid(),
            CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);

        var model = Assert.IsAssignableFrom<
            IReadOnlyList<LearnerLessonDto>>(view.Model);

        Assert.Empty(model);
    }

    #region HelperMethods
    private sealed class TestSender : ISender
    {
        public object? Response { get; init; }

        public Exception? ExceptionToThrow { get; init; }

        public object? LastRequest { get; private set; }

        public Task<TResponse> Send<TResponse>(
            IRequest<TResponse> request,
            CancellationToken cancellationToken = default)
        {
            LastRequest = request;

            if (ExceptionToThrow is not null)
                throw ExceptionToThrow;

            return Task.FromResult((TResponse)Response!);
        }

        public Task Send<TRequest>(
            TRequest request,
            CancellationToken cancellationToken = default)
            where TRequest : IRequest
        {
            LastRequest = request;
            return Task.CompletedTask;
        }

        public Task<object?> Send(
            object request,
            CancellationToken cancellationToken = default)
        {
            LastRequest = request;

            if (ExceptionToThrow is not null)
                throw ExceptionToThrow;

            return Task.FromResult(Response);
        }


        public async IAsyncEnumerable<TResponse> CreateStream<TResponse>(
            IStreamRequest<TResponse> request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            yield break;
        }

        public async IAsyncEnumerable<object?> CreateStream(
            object request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            yield break;
        }

    }
    #endregion

}
