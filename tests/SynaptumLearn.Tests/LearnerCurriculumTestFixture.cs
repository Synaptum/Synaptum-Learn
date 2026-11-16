
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Application.Users.CreateSchoolUser;
using SynaptumLearn.Application.Users.CreateUser;
using SynaptumLearn.Domain.Curriculum;
using SynaptumLearn.Domain.Enums;
using SynaptumLearn.Domain.Schools;
using SynaptumLearn.Persistence.Contexts;
using SynaptumLearn.Persistence.Identity;

namespace SynaptumLearn.Tests;

public sealed class LearnerCurriculumTestFixture : IAsyncDisposable
{
    private readonly ServiceProvider _provider;
    private readonly IServiceScope _scope;
    private readonly UserManager<ApplicationUser> _userManager;

    private readonly string _uniqueId = Guid.NewGuid().ToString("N");

    private string AdminEmail => $"curriculum-admin-{_uniqueId}@example.com";
    private string LearnerEmail => $"curriculum-learner-{_uniqueId}@example.com";

    public IServiceProvider Services => _scope.ServiceProvider;

    public ApplicationDbContext Context { get; }
    public TestCurrentUserService CurrentUser { get; }

    public School School { get; private set; } = null!;
    public Grade Grade { get; private set; } = null!;
    public Guid LearnerId { get; private set; }

    public LearnerCurriculumTestFixture()
    {
        _provider = IntegrationTestServices.CreateServiceProvider();
        _scope = _provider.CreateScope();

        var services = _scope.ServiceProvider;

        Context = services.GetRequiredService<ApplicationDbContext>();
        CurrentUser = services.GetRequiredService<TestCurrentUserService>();

        _userManager =
            services.GetRequiredService<UserManager<ApplicationUser>>();

        TestDatabaseConfiguration.Validate(
            Context.Database.GetDbConnection().ConnectionString);
    }

    public async Task InitializeAsync()
    {
        if (!await Context.Database.CanConnectAsync())
        {
            throw new InvalidOperationException(
                "The integration test database is unavailable.");
        }

        School = new School
        {
            Name = $"Learner Curriculum School {_uniqueId}",
            EMISNumber = _uniqueId[..12],
            Email = $"school-{_uniqueId}@example.com",
            Phone = "0123456789",
            Province = Province.FreeState,
            Slug = $"learner-curriculum-{_uniqueId}",
            Code = $"LC{_uniqueId[..8]}",
            Status = SchoolStatus.Active,
            CurriculumAccessMode = CurriculumAccessMode.Progressive,
            CurrentTerm = 1
        };

        Grade = new Grade
        {
            Name = $"Grade {_uniqueId[..8]}",
            Order = 10
        };

        Context.Schools.Add(School);
        Context.Grades.Add(Grade);

        await Context.SaveChangesAsync();

        var services = _scope.ServiceProvider;

        var provisioningService =
            services.GetRequiredService<IIdentityUserProvisioningService>();

        var accessService =
            services.GetRequiredService<ISchoolAdminAccessService>();

        var adminHandler = new CreateUserCommandHandler(
            Context,
            provisioningService,
            CurrentUser);

        var adminId = await adminHandler.Handle(
            new CreateUserCommand(
                "Curriculum",
                "Administrator",
                AdminEmail,
                "TestPassword123!",
                UserAccountType.SchoolAdmin,
                School.Id),
            CancellationToken.None);

        CurrentUser.UserId = adminId;
        CurrentUser.IsAuthenticated = true;
        CurrentUser.IsPlatformAdmin = false;

        var learnerHandler = new CreateSchoolUserCommandHandler(
            accessService,
            Context,
            provisioningService);

        LearnerId = await learnerHandler.Handle(
            new CreateSchoolUserCommand(
                "Curriculum",
                "Learner",
                LearnerEmail,
                "TestPassword123!",
                SchoolUserAccountType.Learner,
                LearnerNumber: $"LRN-{_uniqueId[..8]}",
                GradeId: Grade.Id),
            CancellationToken.None);

        // Subsequent tests execute as the learner.
        CurrentUser.UserId = LearnerId;
        CurrentUser.SchoolId = School.Id;
        CurrentUser.IsPlatformAdmin = false;
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            Context.ChangeTracker.Clear();

            if (Grade is not null)
            {
                var gradeSubjectIds = await Context.GradeSubjects
                    .Where(gs => gs.GradeId == Grade.Id)
                    .Select(gs => gs.Id)
                    .ToListAsync();

                if (gradeSubjectIds.Count > 0)
                {
                    await Context.Topics
                        .Where(t => gradeSubjectIds.Contains(t.GradeSubjectId))
                        .ExecuteDeleteAsync();

                    var subjectIds = await Context.GradeSubjects
                        .Where(gs => gs.GradeId == Grade.Id)
                        .Select(gs => gs.SubjectId)
                        .ToListAsync();

                    await Context.GradeSubjects
                        .Where(gs => gs.GradeId == Grade.Id)
                        .ExecuteDeleteAsync();

                    await Context.Subjects
                        .Where(s => subjectIds.Contains(s.Id))
                        .ExecuteDeleteAsync();
                }
            }

            if (School is not null)
            {
                var profiles = await Context.LearnerProfiles
                    .Where(x => x.SchoolId == School.Id)
                    .ToListAsync();

                Context.LearnerProfiles.RemoveRange(profiles);

                var memberships = await Context.SchoolMemberships
                    .Where(x => x.SchoolId == School.Id)
                    .ToListAsync();

                Context.SchoolMemberships.RemoveRange(memberships);

                await Context.SaveChangesAsync();
            }

            foreach (var email in new[] { LearnerEmail, AdminEmail })
            {
                var user = await Context.BusinessUsers
                    .SingleOrDefaultAsync(x => x.Email == email);

                if (user is not null)
                {
                    Context.BusinessUsers.Remove(user);
                    await Context.SaveChangesAsync();
                }

                var identityUser =
                    await _userManager.FindByEmailAsync(email);

                if (identityUser is not null)
                {
                    var result =
                        await _userManager.DeleteAsync(identityUser);

                    if (!result.Succeeded)
                    {
                        throw new InvalidOperationException(
                            string.Join("; ",
                                result.Errors.Select(x => x.Description)));
                    }
                }
            }

            if (School is not null)
            {
                await Context.Schools
                    .Where(x => x.Id == School.Id)
                    .ExecuteDeleteAsync();
            }

            if (Grade is not null)
            {
                await Context.Grades
                    .Where(x => x.Id == Grade.Id)
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
