using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Application.Users.CreateUser;
using SynaptumLearn.Application.Users.CreateSchoolUser;
using SynaptumLearn.Domain.Curriculum;
using SynaptumLearn.Domain.Enums;
using SynaptumLearn.Domain.Schools;
using SynaptumLearn.Persistence.Contexts;
using SynaptumLearn.Persistence.Identity;
using Xunit;

namespace SynaptumLearn.Tests;

public sealed class TeacherClassAssignmentTestFixture : IAsyncDisposable
{
    private readonly ServiceProvider _provider;
    private readonly IServiceScope _scope;

    public ApplicationDbContext Context { get; }
    public TestCurrentUserService CurrentUser { get; }
    public ISchoolAdminAccessService AccessService { get; }
    public IIdentityUserProvisioningService ProvisioningService { get; }
    public UserManager<ApplicationUser> UserManager { get; }

    public string UniqueId { get; } = Guid.NewGuid().ToString("N");

    public School SchoolA { get; private set; } = null!;
    public School SchoolB { get; private set; } = null!;

    public Grade Grade10 { get; private set; } = null!;
    public Grade Grade11 { get; private set; } = null!;
    public Subject Subject { get; private set; } = null!;
    public GradeSubject GradeSubject { get; private set; } = null!;

    public Class SchoolAClass { get; private set; } = null!;
    public Class SchoolBClass { get; private set; } = null!;
    public Class SchoolAGrade11Class { get; private set; } = null!;

    public Guid SchoolAAdminId { get; private set; }
    public Guid SchoolBAdminId { get; private set; }

    public Guid SchoolATeacherId { get; private set; }
    public Guid SchoolBTeacherId { get; private set; }

    public TeacherSubject SchoolATeacherSubject { get; private set; } = null!;
    public TeacherSubject SchoolBTeacherSubject { get; private set; } = null!;

    public TeacherClassAssignmentTestFixture()
    {
        _provider = IntegrationTestServices.CreateServiceProvider();
        _scope = _provider.CreateScope();

        var services = _scope.ServiceProvider;

        Context = services.GetRequiredService<ApplicationDbContext>();
        CurrentUser = services.GetRequiredService<TestCurrentUserService>();
        AccessService = services.GetRequiredService<ISchoolAdminAccessService>();
        ProvisioningService =
            services.GetRequiredService<IIdentityUserProvisioningService>();
        UserManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        TestDatabaseConfiguration.Validate(
            Context.Database.GetDbConnection().ConnectionString);
    }

    public async Task InitializeAsync()
    {
        Assert.True(await Context.Database.CanConnectAsync());

        School CreateSchool(string suffix) => new()
        {
            Name = $"Assignment School {suffix} {UniqueId}",
            EMISNumber = $"{suffix}{UniqueId[..11]}",
            Email = $"school-{suffix}-{UniqueId}@example.com",
            Phone = "0123456789",
            Province = Province.FreeState,
            Slug = $"assignment-{suffix}-{UniqueId}",
            Code = $"{suffix}{UniqueId[..8]}",
            Status = SchoolStatus.Active
        };

        SchoolA = CreateSchool("A");
        SchoolB = CreateSchool("B");

        Grade10 = new Grade
        {
            Name = $"Grade 10 Test {UniqueId}",
            Order = 10
        };

        Grade11 = new Grade
        {
            Name = $"Grade 11 Test {UniqueId}",
            Order = 11
        };

        Subject = new Subject
        {
            Name = $"Mathematics Test {UniqueId}",
            Code = $"M{UniqueId[..8]}"
        };

        Context.Schools.AddRange(SchoolA, SchoolB);
        Context.Grades.AddRange(Grade10, Grade11);
        Context.Subjects.Add(Subject);

        await Context.SaveChangesAsync();

        GradeSubject = new GradeSubject
        {
            GradeId = Grade10.Id,
            SubjectId = Subject.Id
        };

        Context.GradeSubjects.Add(GradeSubject);
        await Context.SaveChangesAsync();

        SchoolAClass = new Class
        {
            SchoolId = SchoolA.Id,
            GradeId = Grade10.Id,
            Name = "10A",
            AcademicYear = 2026
        };

        SchoolBClass = new Class
        {
            SchoolId = SchoolB.Id,
            GradeId = Grade10.Id,
            Name = "10B",
            AcademicYear = 2026
        };

        SchoolAGrade11Class = new Class
        {
            SchoolId = SchoolA.Id,
            GradeId = Grade11.Id,
            Name = "11A",
            AcademicYear = 2026
        };

        Context.Classes.AddRange(
            SchoolAClass,
            SchoolBClass,
            SchoolAGrade11Class);

        await Context.SaveChangesAsync();

        var adminHandler = new CreateUserCommandHandler(
            Context,
            ProvisioningService,
            CurrentUser);

        CurrentUser.UserId = null;
        CurrentUser.IsPlatformAdmin = true;

        SchoolAAdminId = await adminHandler.Handle(
            new CreateUserCommand(
                "School A",
                "Administrator",
                $"class-admin-{UniqueId}@example.com",
                "TestPassword123!",
                UserAccountType.SchoolAdmin,
                SchoolA.Id),
            CancellationToken.None);

        CurrentUser.UserId = null;
        CurrentUser.IsPlatformAdmin = true;

        SchoolBAdminId = await adminHandler.Handle(
            new CreateUserCommand(
                "School B",
                "Administrator",
                $"school-b-admin-{UniqueId}@example.com",
                "TestPassword123!",
                UserAccountType.SchoolAdmin,
                SchoolB.Id),
            CancellationToken.None);

        var teacherHandler = new CreateSchoolUserCommandHandler(
            AccessService,
            Context,
            ProvisioningService);

        CurrentUser.UserId = SchoolBAdminId;
        CurrentUser.IsPlatformAdmin = false;

        SchoolBTeacherId = await teacherHandler.Handle(
            new CreateSchoolUserCommand(
                "External",
                "Teacher",
                $"class-teacher-{UniqueId}@example.com",
                "TestPassword123!",
                SchoolUserAccountType.Teacher,
                EmployeeNumber: $"EMP-B-{UniqueId[..8]}"),
            CancellationToken.None);

        var schoolBTeacherProfile =
            await Context.TeacherProfiles.SingleAsync(t =>
                t.UserId == SchoolBTeacherId &&
                t.SchoolId == SchoolB.Id);

        SchoolBTeacherSubject = new TeacherSubject
        {
            SchoolId = SchoolB.Id,
            TeacherProfileId = schoolBTeacherProfile.Id,
            GradeSubjectId = GradeSubject.Id
        };

        Context.TeacherSubjects.Add(SchoolBTeacherSubject);
        await Context.SaveChangesAsync();

        CurrentUser.UserId = SchoolAAdminId;
        CurrentUser.IsPlatformAdmin = false;

        SchoolATeacherId = await teacherHandler.Handle(
            new CreateSchoolUserCommand(
                "Internal",
                "Teacher",
                $"school-a-teacher-{UniqueId}@example.com",
                "TestPassword123!",
                SchoolUserAccountType.Teacher,
                EmployeeNumber: $"EMP-A-{UniqueId[..8]}"),
            CancellationToken.None);

        var schoolATeacherProfile =
            await Context.TeacherProfiles.SingleAsync(t =>
                t.UserId == SchoolATeacherId &&
                t.SchoolId == SchoolA.Id);

        SchoolATeacherSubject = new TeacherSubject
        {
            SchoolId = SchoolA.Id,
            TeacherProfileId = schoolATeacherProfile.Id,
            GradeSubjectId = GradeSubject.Id
        };

        Context.TeacherSubjects.Add(SchoolATeacherSubject);
        await Context.SaveChangesAsync();
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            // No database records were created if initialization
            // failed before the schools were assigned.
            if (SchoolA is null && SchoolB is null)
                return;

            var schoolIds = new[]
            {
                SchoolA?.Id,
                SchoolB?.Id
            }
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToArray();

            // 1. Remove teacher-class assignments.
            var assignments = await Context.TeacherClassSubjects
                .Where(x => schoolIds.Contains(x.SchoolId))
                .ToListAsync();

            Context.TeacherClassSubjects.RemoveRange(assignments);
            await Context.SaveChangesAsync();

            // 2. Remove teacher-subject assignments.
            var teacherSubjects = await Context.TeacherSubjects
                .Where(x => schoolIds.Contains(x.SchoolId))
                .ToListAsync();

            Context.TeacherSubjects.RemoveRange(teacherSubjects);
            await Context.SaveChangesAsync();

            // 3. Remove teacher profiles.
            var teacherProfiles = await Context.TeacherProfiles
                .Where(x => schoolIds.Contains(x.SchoolId))
                .ToListAsync();

            Context.TeacherProfiles.RemoveRange(teacherProfiles);
            await Context.SaveChangesAsync();

            // 4. Remove classes.
            var classes = await Context.Classes
                .Where(x => schoolIds.Contains(x.SchoolId))
                .ToListAsync();

            Context.Classes.RemoveRange(classes);
            await Context.SaveChangesAsync();

            // 5. Remove school memberships.
            var memberships = await Context.SchoolMemberships
                .Where(x => schoolIds.Contains(x.SchoolId))
                .ToListAsync();

            Context.SchoolMemberships.RemoveRange(memberships);
            await Context.SaveChangesAsync();

            // 6. Remove domain users created by this fixture.
            var emails = new[]
            {
            $"class-admin-{UniqueId}@example.com",
            $"school-b-admin-{UniqueId}@example.com",
            $"class-teacher-{UniqueId}@example.com",
            $"school-a-teacher-{UniqueId}@example.com"
        };

            var users = await Context.BusinessUsers
                .Where(x => emails.Contains(x.Email))
                .ToListAsync();

            Context.BusinessUsers.RemoveRange(users);
            await Context.SaveChangesAsync();

            // 7. Remove ASP.NET Identity users.
            foreach (var email in emails)
            {
                var identityUser = await UserManager.FindByEmailAsync(email);

                if (identityUser is null)
                    continue;

                var result = await UserManager.DeleteAsync(identityUser);

                Assert.True(
                    result.Succeeded,
                    string.Join("; ",
                        result.Errors.Select(e => e.Description)));
            }

            // 8. Remove curriculum relationships.
            if (Grade10 is not null && Subject is not null)
            {
                var gradeSubjects = await Context.GradeSubjects
                    .Where(x =>
                        x.GradeId == Grade10.Id &&
                        x.SubjectId == Subject.Id)
                    .ToListAsync();

                Context.GradeSubjects.RemoveRange(gradeSubjects);
                await Context.SaveChangesAsync();
            }

            // 9. Remove curriculum and schools.
            if (Subject is not null)
                Context.Subjects.Remove(Subject);

            if (Grade10 is not null)
                Context.Grades.Remove(Grade10);

            if (Grade11 is not null)
                Context.Grades.Remove(Grade11);

            if (SchoolA is not null)
                Context.Schools.Remove(SchoolA);

            if (SchoolB is not null)
                Context.Schools.Remove(SchoolB);

            await Context.SaveChangesAsync();
        }
        finally
        {
            // Always release services, even if cleanup fails.
            _scope.Dispose();
            await _provider.DisposeAsync();
        }
    }
}