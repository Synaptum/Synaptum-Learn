
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Application.Users.CreateSchoolUser;
using SynaptumLearn.Application.Users.CreateUser;
using SynaptumLearn.Domain.Enums;
using SynaptumLearn.Domain.Schools;
using SynaptumLearn.Persistence.Contexts;
using SynaptumLearn.Persistence.Identity;
using Xunit;

namespace SynaptumLearn.Tests;


public class CreateSchoolUserIntegrationTests
{
    [Fact]
    public async Task CreateSchoolUser_CreatesTeacherWithMembershipAndProfile()
    {
        using var provider =
            IntegrationTestServices.CreateServiceProvider();

        using var scope = provider.CreateScope();
        var services = scope.ServiceProvider;

        var context =
            services.GetRequiredService<ApplicationDbContext>();

        TestDatabaseConfiguration.Validate(
            context.Database.GetDbConnection().ConnectionString);

        Assert.True(await context.Database.CanConnectAsync());

        var userManager =
            services.GetRequiredService<UserManager<ApplicationUser>>();

        var provisioningService =
            services.GetRequiredService<IIdentityUserProvisioningService>();

        var currentUser =
            services.GetRequiredService<TestCurrentUserService>();

        var accessService =
            services.GetRequiredService<ISchoolAdminAccessService>();

        var uniqueId = Guid.NewGuid().ToString("N");
        var adminEmail = $"teacher-admin-{uniqueId}@example.com";
        var teacherEmail = $"teacher-{uniqueId}@example.com";

        var school = new School
        {
            Name = $"Teacher Test School {uniqueId}",
            EMISNumber = uniqueId[..12],
            Email = $"school-{uniqueId}@example.com",
            Phone = "0123456789",
            Province = Province.FreeState,
            Slug = $"teacher-test-school-{uniqueId}",
            Code = $"TT{uniqueId[..8]}",
            Status = SchoolStatus.Active
        };

        context.Schools.Add(school);
        await context.SaveChangesAsync();

        try
        {
            var adminHandler = new CreateUserCommandHandler(
                context,
                provisioningService,
                currentUser);

            var adminId = await adminHandler.Handle(
                new CreateUserCommand(
                    "School",
                    "Administrator",
                    adminEmail,
                    "TestPassword123!",
                    UserAccountType.SchoolAdmin,
                    school.Id),
                CancellationToken.None);

            currentUser.UserId = adminId;
            currentUser.IsPlatformAdmin = false;

            var teacherHandler =
                new CreateSchoolUserCommandHandler(
                    accessService,
                    context,
                    provisioningService);

            var teacherId = await teacherHandler.Handle(
                new CreateSchoolUserCommand(
                    "Integration",
                    "Teacher",
                    teacherEmail,
                    "TestPassword123!",
                    SchoolUserAccountType.Teacher,
                    EmployeeNumber: $"EMP-{uniqueId[..8]}",
                    IsClassTeacher: true),
                CancellationToken.None);

            var domainUser = await context.BusinessUsers
                .SingleAsync(u => u.Id == teacherId);

            var identityUser =
                await userManager.FindByEmailAsync(teacherEmail);

            Assert.NotNull(identityUser);
            Assert.Equal(
                identityUser.Id,
                domainUser.IdentityUserId);

            Assert.Equal("Integration", domainUser.FirstName);
            Assert.Equal("Teacher", domainUser.LastName);
            Assert.True(domainUser.IsActive);

            var membership = await context.SchoolMemberships
                .SingleAsync(m =>
                    m.UserId == teacherId &&
                    m.SchoolId == school.Id);

            Assert.Equal(SchoolRole.Teacher, membership.Role);
            Assert.True(membership.IsActive);

            var profile = await context.TeacherProfiles
                .SingleAsync(t =>
                    t.UserId == teacherId &&
                    t.SchoolId == school.Id);

            Assert.Equal(
                $"EMP-{uniqueId[..8]}",
                profile.EmployeeNumber);

            Assert.True(profile.IsClassTeacher);

            var duplicateEmail =
                $"duplicate-teacher-{uniqueId}@example.com";

            var duplicateCommand = new CreateSchoolUserCommand(
                "Duplicate",
                "Teacher",
                duplicateEmail,
                "TestPassword123!",
                SchoolUserAccountType.Teacher,
                EmployeeNumber: $"EMP-{uniqueId[..8]}",
                IsClassTeacher: false);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => teacherHandler.Handle(
                    duplicateCommand,
                    CancellationToken.None));

            Assert.False(
                await context.BusinessUsers
                    .AnyAsync(u => u.Email == duplicateEmail));

            Assert.Null(
                await userManager.FindByEmailAsync(duplicateEmail));

            var oversizedTeacherEmail =
                $"oversized-teacher-{uniqueId}@example.com";

            var oversizedEmployeeNumber = new string('E', 51);

            var oversizedTeacherException =
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => teacherHandler.Handle(
                        new CreateSchoolUserCommand(
                            "Oversized",
                            "Teacher",
                            oversizedTeacherEmail,
                            "TestPassword123!",
                            SchoolUserAccountType.Teacher,
                            EmployeeNumber: oversizedEmployeeNumber),
                        CancellationToken.None));

            Assert.Equal(
                "An employee number cannot exceed 50 characters.",
                oversizedTeacherException.Message);

            Assert.Null(
                await userManager.FindByEmailAsync(oversizedTeacherEmail));

            Assert.False(await context.BusinessUsers.AnyAsync(
                u => u.Email == oversizedTeacherEmail));


            var invalidEmails = new[]
            {
                "not-an-email",
                "missing-at-sign.example.com",
                "@example.com",
                "teacher@",
                "teacher name@example.com"
            };

            foreach (var invalidEmail in invalidEmails)
            {
                var exception =
                    await Assert.ThrowsAsync<InvalidOperationException>(
                        () => teacherHandler.Handle(
                            new CreateSchoolUserCommand(
                                "Invalid",
                                "Email",
                                invalidEmail,
                                "TestPassword123!",
                                SchoolUserAccountType.Teacher,
                                EmployeeNumber: $"INVALID-{uniqueId[..8]}"),
                            CancellationToken.None));

                Assert.Equal(
                    "A valid email address is required.",
                    exception.Message);

                Assert.Null(
                    await userManager.FindByEmailAsync(invalidEmail));

                Assert.False(await context.BusinessUsers.AnyAsync(
                    u => u.Email == invalidEmail));
            }

            var uppercaseDuplicateEmail = teacherEmail.ToUpperInvariant();

            var duplicateEmailException =
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => teacherHandler.Handle(
                        new CreateSchoolUserCommand(
                            "Duplicate",
                            "Email",
                            uppercaseDuplicateEmail,
                            "TestPassword123!",
                            SchoolUserAccountType.Teacher,
                            EmployeeNumber: $"DUP-{uniqueId[..8]}"),
                        CancellationToken.None));

            Assert.Contains(
                "already exists",
                duplicateEmailException.Message,
                StringComparison.OrdinalIgnoreCase);

            Assert.Equal(
                1,
                await context.BusinessUsers.CountAsync(u =>
                    u.Email.ToUpper() == uppercaseDuplicateEmail));

        }
        finally
        {
            var teacherProfiles = await context.TeacherProfiles
                .Where(t => t.SchoolId == school.Id)
                .ToListAsync();

            context.TeacherProfiles.RemoveRange(teacherProfiles);

            var memberships = await context.SchoolMemberships
                .Where(m => m.SchoolId == school.Id)
                .ToListAsync();

            context.SchoolMemberships.RemoveRange(memberships);
            await context.SaveChangesAsync();

            foreach (var email in new[] { teacherEmail, adminEmail })
            {
                var domainUser = await context.BusinessUsers
                    .SingleOrDefaultAsync(u => u.Email == email);

                if (domainUser is not null)
                {
                    context.BusinessUsers.Remove(domainUser);
                    await context.SaveChangesAsync();
                }

                var identityUser =
                    await userManager.FindByEmailAsync(email);

                if (identityUser is not null)
                {
                    var result =
                        await userManager.DeleteAsync(identityUser);

                    Assert.True(
                        result.Succeeded,
                        string.Join("; ",
                            result.Errors.Select(e => e.Description)));
                }
            }

            context.Schools.Remove(school);
            await context.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task CreateSchoolUser_CreatesLearnerWithMembershipAndProfile()
    {
        using var provider =
            IntegrationTestServices.CreateServiceProvider();

        using var scope = provider.CreateScope();
        var services = scope.ServiceProvider;

        var context =
            services.GetRequiredService<ApplicationDbContext>();

        TestDatabaseConfiguration.Validate(
            context.Database.GetDbConnection().ConnectionString);

        Assert.True(await context.Database.CanConnectAsync());

        var userManager =
            services.GetRequiredService<UserManager<ApplicationUser>>();

        var provisioningService =
            services.GetRequiredService<IIdentityUserProvisioningService>();

        var currentUser =
            services.GetRequiredService<TestCurrentUserService>();

        var accessService =
            services.GetRequiredService<ISchoolAdminAccessService>();

        var uniqueId = Guid.NewGuid().ToString("N");
        var adminEmail = $"learner-admin-{uniqueId}@example.com";
        var learnerEmail = $"learner-{uniqueId}@example.com";

        var school = new School
        {
            Name = $"Learner Test School {uniqueId}",
            EMISNumber = uniqueId[..12],
            Email = $"school-{uniqueId}@example.com",
            Phone = "0123456789",
            Province = Province.FreeState,
            Slug = $"learner-test-school-{uniqueId}",
            Code = $"LT{uniqueId[..8]}",
            Status = SchoolStatus.Active
        };

        var grade = new SynaptumLearn.Domain.Curriculum.Grade
        {
            Name = $"Grade 10 Test {uniqueId}",
            Order = 10
        };

        context.Schools.Add(school);
        context.Grades.Add(grade);

        await context.SaveChangesAsync();

        try
        {
            var adminHandler = new CreateUserCommandHandler(
                context,
                provisioningService,
                currentUser);

            var adminId = await adminHandler.Handle(
                new CreateUserCommand(
                    "School",
                    "Administrator",
                    adminEmail,
                    "TestPassword123!",
                    UserAccountType.SchoolAdmin,
                    school.Id),
                CancellationToken.None);

            currentUser.UserId = adminId;
            currentUser.IsPlatformAdmin = false;

            var learnerHandler =
                new CreateSchoolUserCommandHandler(
                    accessService,
                    context,
                    provisioningService);

            var learnerId = await learnerHandler.Handle(
                new CreateSchoolUserCommand(
                    "Integration",
                    "Learner",
                    learnerEmail,
                    "TestPassword123!",
                    SchoolUserAccountType.Learner,
                    LearnerNumber: $"LRN-{uniqueId[..8]}",
                    GradeId: grade.Id),
                CancellationToken.None);

            var domainUser = await context.BusinessUsers
                .SingleAsync(u => u.Id == learnerId);

            var identityUser =
                await userManager.FindByEmailAsync(learnerEmail);

            Assert.NotNull(identityUser);
            Assert.Equal(
                identityUser.Id,
                domainUser.IdentityUserId);

            Assert.Equal("Integration", domainUser.FirstName);
            Assert.Equal("Learner", domainUser.LastName);
            Assert.True(domainUser.IsActive);

            var membership = await context.SchoolMemberships
                .SingleAsync(m =>
                    m.UserId == learnerId &&
                    m.SchoolId == school.Id);

            Assert.Equal(SchoolRole.Learner, membership.Role);
            Assert.True(membership.IsActive);

            var profile = await context.LearnerProfiles
                .SingleAsync(l =>
                    l.UserId == learnerId &&
                    l.SchoolId == school.Id);

            Assert.Equal(
                $"LRN-{uniqueId[..8]}",
                profile.LearnerNumber);

            Assert.Equal(grade.Id, profile.GradeId);

            var duplicateEmail =
                $"duplicate-learner-{uniqueId}@example.com";

            var duplicateCommand = new CreateSchoolUserCommand(
                "Duplicate",
                "Learner",
                duplicateEmail,
                "TestPassword123!",
                SchoolUserAccountType.Learner,
                LearnerNumber: $"LRN-{uniqueId[..8]}",
                GradeId: grade.Id);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => learnerHandler.Handle(
                    duplicateCommand,
                    CancellationToken.None));

            Assert.False(
                await context.BusinessUsers
                    .AnyAsync(u => u.Email == duplicateEmail));

            Assert.Null(
                await userManager.FindByEmailAsync(duplicateEmail));

            var invalidGradeEmail =
                $"invalid-grade-{uniqueId}@example.com";

            var invalidGradeCommand = new CreateSchoolUserCommand(
                "Invalid",
                "Grade",
                invalidGradeEmail,
                "TestPassword123!",
                SchoolUserAccountType.Learner,
                LearnerNumber: $"INVALID-{uniqueId[..8]}",
                GradeId: Guid.NewGuid());

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => learnerHandler.Handle(
                    invalidGradeCommand,
                    CancellationToken.None));

            Assert.False(
                await context.BusinessUsers
                    .AnyAsync(u => u.Email == invalidGradeEmail));

            Assert.Null(
                await userManager.FindByEmailAsync(invalidGradeEmail));

            var oversizedLearnerEmail =
    $"oversized-learner-{uniqueId}@example.com";

            var oversizedLearnerNumber = new string('L', 51);

            var oversizedLearnerException =
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => learnerHandler.Handle(
                        new CreateSchoolUserCommand(
                            "Oversized",
                            "Learner",
                            oversizedLearnerEmail,
                            "TestPassword123!",
                            SchoolUserAccountType.Learner,
                            LearnerNumber: oversizedLearnerNumber,
                            GradeId: grade.Id),
                        CancellationToken.None));

            Assert.Equal(
                "A learner number cannot exceed 50 characters.",
                oversizedLearnerException.Message);

            Assert.Null(
                await userManager.FindByEmailAsync(oversizedLearnerEmail));

            Assert.False(await context.BusinessUsers.AnyAsync(
                u => u.Email == oversizedLearnerEmail));

        }
        finally
        {
            var learnerProfiles = await context.LearnerProfiles
                .Where(l => l.SchoolId == school.Id)
                .ToListAsync();

            context.LearnerProfiles.RemoveRange(learnerProfiles);

            var memberships = await context.SchoolMemberships
                .Where(m => m.SchoolId == school.Id)
                .ToListAsync();

            context.SchoolMemberships.RemoveRange(memberships);
            await context.SaveChangesAsync();

            foreach (var email in new[] { learnerEmail, adminEmail })
            {
                var domainUser = await context.BusinessUsers
                    .SingleOrDefaultAsync(u => u.Email == email);

                if (domainUser is not null)
                {
                    context.BusinessUsers.Remove(domainUser);
                    await context.SaveChangesAsync();
                }

                var identityUser =
                    await userManager.FindByEmailAsync(email);

                if (identityUser is not null)
                {
                    var result =
                        await userManager.DeleteAsync(identityUser);

                    Assert.True(
                        result.Succeeded,
                        string.Join("; ",
                            result.Errors.Select(e => e.Description)));
                }
            }

            context.Schools.Remove(school);
            context.Grades.Remove(grade);

            await context.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task CreateSchoolUser_RollsBackWhenTeacherProfileSaveFails()
    {
        var interceptor = new TeacherProfileFailureInterceptor();

        using var provider =
            IntegrationTestServices.CreateServiceProvider(interceptor);

        using var scope = provider.CreateScope();
        var services = scope.ServiceProvider;

        var context =
            services.GetRequiredService<ApplicationDbContext>();

        TestDatabaseConfiguration.Validate(
            context.Database.GetDbConnection().ConnectionString);

        Assert.True(await context.Database.CanConnectAsync());

        var userManager =
            services.GetRequiredService<UserManager<ApplicationUser>>();

        var provisioningService =
            services.GetRequiredService<IIdentityUserProvisioningService>();

        var currentUser =
            services.GetRequiredService<TestCurrentUserService>();

        var accessService =
            services.GetRequiredService<ISchoolAdminAccessService>();

        var uniqueId = Guid.NewGuid().ToString("N");
        var adminEmail = $"rollback-admin-{uniqueId}@example.com";
        var teacherEmail = $"rollback-teacher-{uniqueId}@example.com";
        var employeeNumber = $"ROLL-{uniqueId[..8]}";

        var school = new School
        {
            Name = $"Rollback Test School {uniqueId}",
            EMISNumber = uniqueId[..12],
            Email = $"rollback-school-{uniqueId}@example.com",
            Phone = "0123456789",
            Province = Province.FreeState,
            Slug = $"rollback-test-school-{uniqueId}",
            Code = $"RT{uniqueId[..8]}",
            Status = SchoolStatus.Active
        };

        context.Schools.Add(school);
        await context.SaveChangesAsync();

        try
        {
            var adminHandler = new CreateUserCommandHandler(
                context,
                provisioningService,
                currentUser);

            var adminId = await adminHandler.Handle(
                new CreateUserCommand(
                    "School",
                    "Administrator",
                    adminEmail,
                    "TestPassword123!",
                    UserAccountType.SchoolAdmin,
                    school.Id),
                CancellationToken.None);

            currentUser.UserId = adminId;
            currentUser.IsPlatformAdmin = false;

            var teacherHandler = new CreateSchoolUserCommandHandler(
                accessService,
                context,
                provisioningService);

            interceptor.Enabled = true;

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => teacherHandler.Handle(
                    new CreateSchoolUserCommand(
                        "Rollback",
                        "Teacher",
                        teacherEmail,
                        "TestPassword123!",
                        SchoolUserAccountType.Teacher,
                        EmployeeNumber: employeeNumber),
                    CancellationToken.None));

            Assert.Equal(
                "Simulated teacher profile save failure.",
                exception.Message);

            interceptor.Enabled = false;

            // Query using a fresh DbContext to avoid tracked entities.
            using var verificationScope = provider.CreateScope();

            var verificationContext =
                verificationScope.ServiceProvider
                    .GetRequiredService<ApplicationDbContext>();

            var verificationUserManager =
                verificationScope.ServiceProvider
                    .GetRequiredService<UserManager<ApplicationUser>>();

            Assert.Null(
                await verificationUserManager.FindByEmailAsync(teacherEmail));

            Assert.False(
                await verificationContext.BusinessUsers
                    .AnyAsync(u => u.Email == teacherEmail));

            Assert.False(
                await verificationContext.TeacherProfiles
                    .AnyAsync(t =>
                        t.SchoolId == school.Id &&
                        t.EmployeeNumber == employeeNumber));

            Assert.Equal(
                1,
                await verificationContext.SchoolMemberships
                    .CountAsync(m => m.SchoolId == school.Id));
        }
        finally
        {
            interceptor.Enabled = false;

            // Clear entities left tracked after the failed transaction.
            context.ChangeTracker.Clear();

            var memberships = await context.SchoolMemberships
                .Where(m => m.SchoolId == school.Id)
                .ToListAsync();

            context.SchoolMemberships.RemoveRange(memberships);
            await context.SaveChangesAsync();

            var adminUser = await context.BusinessUsers
                .SingleOrDefaultAsync(u => u.Email == adminEmail);

            if (adminUser is not null)
            {
                context.BusinessUsers.Remove(adminUser);
                await context.SaveChangesAsync();
            }

            var adminIdentity =
                await userManager.FindByEmailAsync(adminEmail);

            if (adminIdentity is not null)
            {
                var result = await userManager.DeleteAsync(adminIdentity);

                Assert.True(
                    result.Succeeded,
                    string.Join("; ",
                        result.Errors.Select(e => e.Description)));
            }

            var savedSchool = await context.Schools
                .SingleOrDefaultAsync(s => s.Id == school.Id);

            if (savedSchool is not null)
            {
                context.Schools.Remove(savedSchool);
                await context.SaveChangesAsync();
            }
        }
    }

    [Fact]
    public async Task CreateSchoolUser_IsolatesTeachersBetweenSchools()
    {
        using var provider =
            IntegrationTestServices.CreateServiceProvider();

        using var scope = provider.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var provisioningService = scope.ServiceProvider
            .GetRequiredService<IIdentityUserProvisioningService>();

        var currentUser = scope.ServiceProvider
            .GetRequiredService<TestCurrentUserService>();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        var accessService = scope.ServiceProvider
            .GetRequiredService<ISchoolAdminAccessService>();

        TestDatabaseConfiguration.Validate(
            context.Database.GetDbConnection().ConnectionString);

        Assert.True(await context.Database.CanConnectAsync());

        var uniqueId = Guid.NewGuid().ToString("N");

        var adminAEmail = $"isolation-admin-a-{uniqueId}@example.com";
        var adminBEmail = $"isolation-admin-b-{uniqueId}@example.com";

        var teacherAEmail = $"isolation-teacher-a-{uniqueId}@example.com";
        var teacherBEmail = $"isolation-teacher-b-{uniqueId}@example.com";

        var sharedEmployeeNumber = $"EMP-{uniqueId[..8]}";

        var schoolA = new School
        {
            Name = $"Isolation School A {uniqueId}",
            EMISNumber = $"A{uniqueId[..11]}",
            Email = $"isolation-a-{uniqueId}@example.com",
            Phone = "0123456789",
            Province = Province.FreeState,
            Slug = $"isolation-school-a-{uniqueId}",
            Code = $"IA{uniqueId[..8]}",
            Status = SchoolStatus.Active
        };

        var schoolB = new School
        {
            Name = $"Isolation School B {uniqueId}",
            EMISNumber = $"B{uniqueId[..11]}",
            Email = $"isolation-b-{uniqueId}@example.com",
            Phone = "0123456789",
            Province = Province.FreeState,
            Slug = $"isolation-school-b-{uniqueId}",
            Code = $"IB{uniqueId[..8]}",
            Status = SchoolStatus.Active
        };

        context.Schools.AddRange(schoolA, schoolB);
        await context.SaveChangesAsync();

        try
        {
            Assert.NotEqual(schoolA.Id, schoolB.Id);

            Assert.True(
                await context.Schools.AnyAsync(s =>
                    s.Id == schoolA.Id));

            Assert.True(
                await context.Schools.AnyAsync(s =>
                    s.Id == schoolB.Id));

            var adminHandler = new CreateUserCommandHandler(
                context,
                provisioningService,
                currentUser);

            var adminAId = await adminHandler.Handle(
                new CreateUserCommand(
                    "School",
                    "AdminA",
                    adminAEmail,
                    "TestPassword123!",
                    UserAccountType.SchoolAdmin,
                    schoolA.Id),
                CancellationToken.None);

            var adminBId = await adminHandler.Handle(
                new CreateUserCommand(
                    "School",
                    "AdminB",
                    adminBEmail,
                    "TestPassword123!",
                    UserAccountType.SchoolAdmin,
                    schoolB.Id),
                CancellationToken.None);

            Assert.NotEqual(adminAId, adminBId);

            Assert.True(await context.SchoolMemberships.AnyAsync(m =>
                m.UserId == adminAId &&
                m.SchoolId == schoolA.Id &&
                m.Role == SchoolRole.SchoolAdmin));

            Assert.True(await context.SchoolMemberships.AnyAsync(m =>
                m.UserId == adminBId &&
                m.SchoolId == schoolB.Id &&
                m.Role == SchoolRole.SchoolAdmin));

            var teacherHandler = new CreateSchoolUserCommandHandler(
                accessService,
                context,
                provisioningService);

            // Administrator A creates a teacher at School A.
            currentUser.IsAuthenticated = true;
            currentUser.UserId = adminAId;
            currentUser.IsPlatformAdmin = false;

            var teacherAId = await teacherHandler.Handle(
                new CreateSchoolUserCommand(
                    "Teacher",
                    "SchoolA",
                    teacherAEmail,
                    "TestPassword123!",
                    SchoolUserAccountType.Teacher,
                    EmployeeNumber: sharedEmployeeNumber),
                CancellationToken.None);

            // Administrator B creates a teacher at School B.
            currentUser.UserId = adminBId;

            var teacherBId = await teacherHandler.Handle(
                new CreateSchoolUserCommand(
                    "Teacher",
                    "SchoolB",
                    teacherBEmail,
                    "TestPassword123!",
                    SchoolUserAccountType.Teacher,
                    EmployeeNumber: sharedEmployeeNumber),
                CancellationToken.None);

            Assert.NotEqual(teacherAId, teacherBId);

            Assert.True(await context.TeacherProfiles.AnyAsync(t =>
                t.UserId == teacherAId &&
                t.SchoolId == schoolA.Id &&
                t.EmployeeNumber == sharedEmployeeNumber));

            Assert.True(await context.TeacherProfiles.AnyAsync(t =>
                t.UserId == teacherBId &&
                t.SchoolId == schoolB.Id &&
                t.EmployeeNumber == sharedEmployeeNumber));

            // Teacher A must not belong to School B.
            Assert.False(await context.SchoolMemberships.AnyAsync(m =>
                m.UserId == teacherAId &&
                m.SchoolId == schoolB.Id));

            Assert.False(await context.TeacherProfiles.AnyAsync(t =>
                t.UserId == teacherAId &&
                t.SchoolId == schoolB.Id));

            // Teacher B must not belong to School A.
            Assert.False(await context.SchoolMemberships.AnyAsync(m =>
                m.UserId == teacherBId &&
                m.SchoolId == schoolA.Id));

            Assert.False(await context.TeacherProfiles.AnyAsync(t =>
                t.UserId == teacherBId &&
                t.SchoolId == schoolA.Id));

            // Each teacher must have exactly one school membership.
            Assert.Equal(1, await context.SchoolMemberships.CountAsync(m =>
                m.UserId == teacherAId));

            Assert.Equal(1, await context.SchoolMemberships.CountAsync(m =>
                m.UserId == teacherBId));

            // Both memberships must have the Teacher role.
            Assert.True(await context.SchoolMemberships.AnyAsync(m =>
                m.UserId == teacherAId &&
                m.SchoolId == schoolA.Id &&
                m.Role == SchoolRole.Teacher));

            Assert.True(await context.SchoolMemberships.AnyAsync(m =>
                m.UserId == teacherBId &&
                m.SchoolId == schoolB.Id &&
                m.Role == SchoolRole.Teacher));

            // Give Administrator A an additional active
            // School Administrator membership at School B.
            var additionalMembership = new SchoolMembership
            {
                SchoolId = schoolB.Id,
                UserId = adminAId,
                Role = SchoolRole.SchoolAdmin,
                IsActive = true
            };

            context.SchoolMemberships.Add(additionalMembership);
            await context.SaveChangesAsync();

            // Administrator A now has two active admin memberships.
            currentUser.UserId = adminAId;
            currentUser.IsAuthenticated = true;
            currentUser.IsPlatformAdmin = false;

            // The access service must refuse to choose a school.
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => accessService.GetAuthorizedSchoolIdAsync(
                    CancellationToken.None));

            // Remove the extra membership to restore the original state.
            context.SchoolMemberships.Remove(additionalMembership);
            await context.SaveChangesAsync();

            // Administrator A should now be authorized for School A again.
            var authorizedSchoolId =
                await accessService.GetAuthorizedSchoolIdAsync(
                    CancellationToken.None);

            Assert.Equal(schoolA.Id, authorizedSchoolId);

        }
        finally
        {
            var memberships = await context.SchoolMemberships
                .Where(m =>
                    m.SchoolId == schoolA.Id ||
                    m.SchoolId == schoolB.Id)
                .ToListAsync();

            var teacherProfiles = await context.TeacherProfiles
                .Where(t =>
                    t.SchoolId == schoolA.Id ||
                    t.SchoolId == schoolB.Id)
                .ToListAsync();

            context.TeacherProfiles.RemoveRange(teacherProfiles);
            await context.SaveChangesAsync();

            context.SchoolMemberships.RemoveRange(memberships);
            await context.SaveChangesAsync();

            foreach (var email in new[]
            {
                teacherAEmail,
                teacherBEmail,
                adminAEmail,
                adminBEmail
            })
            {
                var domainUser = await context.BusinessUsers
                    .SingleOrDefaultAsync(u => u.Email == email);

                if (domainUser is not null)
                {
                    context.BusinessUsers.Remove(domainUser);
                    await context.SaveChangesAsync();
                }

                var identityUser = await userManager.FindByEmailAsync(email);

                if (identityUser is not null)
                {
                    var result = await userManager.DeleteAsync(identityUser);

                    Assert.True(
                        result.Succeeded,
                        string.Join("; ",
                            result.Errors.Select(e => e.Description)));
                }
            }

            context.Schools.RemoveRange(schoolA, schoolB);
            await context.SaveChangesAsync();
        }
    }

    [Theory]
    [InlineData("", "Valid")]
    [InlineData("   ", "Valid")]
    [InlineData("Valid", "")]
    [InlineData("Valid", "   ")]
    [InlineData(null, "Valid")]
    [InlineData("Valid", null)]
    [InlineData("FirstNameTooLong", "Valid")]
    [InlineData("Valid", "LastNameTooLong")]
    public async Task CreateSchoolUser_RejectsInvalidNames(
        string? firstName,
        string? lastName)

    {
        using var provider =
            IntegrationTestServices.CreateServiceProvider();

        using var scope = provider.CreateScope();

        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var currentUser = services.GetRequiredService<TestCurrentUserService>();
        var accessService = services.GetRequiredService<ISchoolAdminAccessService>();
        var provisioningService =
            services.GetRequiredService<IIdentityUserProvisioningService>();

        var uniqueId = Guid.NewGuid().ToString("N");
        var adminEmail = $"name-admin-{uniqueId}@example.com";
        var email = $"invalid-name-{uniqueId}@example.com";

        var school = new School
        {
            Name = $"Name Validation School {uniqueId}",
            EMISNumber = uniqueId[..12],
            Email = $"name-school-{uniqueId}@example.com",
            Phone = "0123456789",
            Province = Province.FreeState,
            Slug = $"name-validation-{uniqueId}",
            Code = $"NV{uniqueId[..8]}",
            Status = SchoolStatus.Active
        };

        context.Schools.Add(school);
        await context.SaveChangesAsync();

        try
        {
            var adminHandler = new CreateUserCommandHandler(
                context, provisioningService, currentUser);

            var adminId = await adminHandler.Handle(
                new CreateUserCommand(
                    "School", "Administrator", adminEmail,
                    "TestPassword123!",
                    UserAccountType.SchoolAdmin,
                    school.Id),
                CancellationToken.None);

            currentUser.IsAuthenticated = true;
            currentUser.UserId = adminId;
            currentUser.IsPlatformAdmin = false;

            var handler = new CreateSchoolUserCommandHandler(
                accessService, context, provisioningService);

            if (firstName == "FirstNameTooLong")
            {
                firstName = new string('F', 101);
            }

            if (lastName == "LastNameTooLong")
            {
                lastName = new string('L', 101);
            }

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => handler.Handle(
                    new CreateSchoolUserCommand(
                        firstName!,
                        lastName!,
                        email,
                        "TestPassword123!",
                        SchoolUserAccountType.Teacher,
                        EmployeeNumber: $"EMP-{uniqueId[..8]}"),
                    CancellationToken.None));

            var expectedMessage =
                string.IsNullOrWhiteSpace(firstName)
                    ? "A first name is required."
                    : string.IsNullOrWhiteSpace(lastName)
                        ? "A last name is required."
                        : firstName!.Trim().Length > 100
                            ? "A first name cannot exceed 100 characters."
                            : "A last name cannot exceed 100 characters.";

            Assert.Equal(expectedMessage, exception.Message);

            Assert.Null(await userManager.FindByEmailAsync(email));
            Assert.False(await context.BusinessUsers
                .AnyAsync(u => u.Email == email));
            Assert.False(await context.TeacherProfiles
                .AnyAsync(t => t.SchoolId == school.Id));
        }
        finally
        {
            var memberships = await context.SchoolMemberships
                .Where(m => m.SchoolId == school.Id)
                .ToListAsync();

            context.SchoolMemberships.RemoveRange(memberships);
            await context.SaveChangesAsync();

            var admin = await context.BusinessUsers
                .SingleOrDefaultAsync(u => u.Email == adminEmail);

            if (admin is not null)
            {
                context.BusinessUsers.Remove(admin);
                await context.SaveChangesAsync();
            }

            var identityAdmin = await userManager.FindByEmailAsync(adminEmail);

            if (identityAdmin is not null)
            {
                var result = await userManager.DeleteAsync(identityAdmin);
                Assert.True(
                    result.Succeeded,
                    string.Join("; ",
                        result.Errors.Select(e => e.Description)));
            }

            context.Schools.Remove(school);
            await context.SaveChangesAsync();
        }
    }


    [Fact]
    public async Task CreateSchoolUser_RejectsUnauthorizedCaller()
    {
        using var provider =
            IntegrationTestServices.CreateServiceProvider();

        using var scope = provider.CreateScope();
        var services = scope.ServiceProvider;

        var context =
            services.GetRequiredService<ApplicationDbContext>();

        TestDatabaseConfiguration.Validate(
            context.Database.GetDbConnection().ConnectionString);

        Assert.True(await context.Database.CanConnectAsync());

        var currentUser =
            services.GetRequiredService<TestCurrentUserService>();

        var accessService =
            services.GetRequiredService<ISchoolAdminAccessService>();

        var provisioningService =
            services.GetRequiredService<IIdentityUserProvisioningService>();

        var userManager =
            services.GetRequiredService<UserManager<ApplicationUser>>();

        currentUser.IsAuthenticated = true;
        currentUser.UserId = Guid.NewGuid();
        currentUser.IsPlatformAdmin = false;

        var email =
            $"unauthorized-teacher-{Guid.NewGuid():N}@example.com";

        var handler = new CreateSchoolUserCommandHandler(
            accessService,
            context,
            provisioningService);

        var command = new CreateSchoolUserCommand(
            "Unauthorized",
            "Teacher",
            email,
            "TestPassword123!",
            SchoolUserAccountType.Teacher,
            EmployeeNumber: "EMP-UNAUTHORIZED");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(
                command,
                CancellationToken.None));

        Assert.False(
            await context.BusinessUsers
                .AnyAsync(u => u.Email == email));

        Assert.Null(
            await userManager.FindByEmailAsync(email));
    }

}

