
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SynaptumLearn.Domain.Curriculum;
using SynaptumLearn.Persistence.Contexts;
using Xunit;

namespace SynaptumLearn.Tests;

public sealed class CreateTopicTestFixture : IAsyncDisposable
{
    private readonly ServiceProvider _provider;
    private readonly IServiceScope _scope;

    public ApplicationDbContext Context { get; }
    public TestCurrentUserService CurrentUser { get; }

    public Grade Grade { get; private set; } = null!;
    public Subject Subject { get; private set; } = null!;
    public GradeSubject GradeSubject { get; private set; } = null!;

    private readonly string _uniqueId =
        Guid.NewGuid().ToString("N");

    public CreateTopicTestFixture()
    {
        _provider = IntegrationTestServices.CreateServiceProvider();
        _scope = _provider.CreateScope();

        Context = _scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        CurrentUser = _scope.ServiceProvider
            .GetRequiredService<TestCurrentUserService>();

        TestDatabaseConfiguration.Validate(
            Context.Database.GetDbConnection().ConnectionString);
    }

    public async Task InitializeAsync()
    {
        Assert.True(await Context.Database.CanConnectAsync());

        Grade = new Grade
        {
            Name = $"Topic Test Grade {_uniqueId}",
            Order = 10
        };

        Subject = new Subject
        {
            Name = $"Topic Test Subject {_uniqueId}",
            Code = $"T{_uniqueId[..8]}"
        };

        Context.Grades.Add(Grade);
        Context.Subjects.Add(Subject);

        await Context.SaveChangesAsync();

        GradeSubject = new GradeSubject
        {
            GradeId = Grade.Id,
            SubjectId = Subject.Id
        };

        Context.GradeSubjects.Add(GradeSubject);

        await Context.SaveChangesAsync();

        CurrentUser.IsAuthenticated = true;
        CurrentUser.IsPlatformAdmin = true;
        CurrentUser.UserId = Guid.NewGuid();
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            Context.ChangeTracker.Clear();

            if (GradeSubject is not null)
            {
                await Context.Topics
                    .Where(x => x.GradeSubjectId == GradeSubject.Id)
                    .ExecuteDeleteAsync();

                await Context.GradeSubjects
                    .Where(x => x.Id == GradeSubject.Id)
                    .ExecuteDeleteAsync();
            }

            if (Grade is not null)
            {
                await Context.Grades
                    .Where(x => x.Id == Grade.Id)
                    .ExecuteDeleteAsync();
            }

            if (Subject is not null)
            {
                await Context.Subjects
                    .Where(x => x.Id == Subject.Id)
                    .ExecuteDeleteAsync();
            }
        }
        finally
        {
            _scope.Dispose();
            _provider.Dispose();
        }
    }
}
