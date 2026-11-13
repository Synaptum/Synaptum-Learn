using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SynaptumLearn.Application.Classes.AssignTeacher;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Application.Users.CreateUser;
using SynaptumLearn.Application.Users.CreateSchoolUser;
using SynaptumLearn.Application.Classes.RemoveClassTeacher;
using SynaptumLearn.Persistence.Contexts;
using SynaptumLearn.Persistence.Identity;
using SynaptumLearn.Domain.Curriculum;
using SynaptumLearn.Domain.Enums;
using SynaptumLearn.Domain.Schools;
using Xunit;

namespace SynaptumLearn.Tests;

public class TeacherClassAssignmentIntegrationTests
{
    [Fact]
    public async Task AssignTeacher_UnauthorizedUser_IsRejected()
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

        // Simulate an authenticated user without
        // an authorized school administrator membership.
        currentUser.IsAuthenticated = true;
        currentUser.UserId = Guid.NewGuid();
        currentUser.IsPlatformAdmin = false;

        var teacherSubjectId = Guid.NewGuid();
        var classId = Guid.NewGuid();

        var handler = new AssignTeacherToClassCommandHandler(
            context,
            accessService);

        var command = new AssignTeacherToClassCommand(
            teacherSubjectId,
            classId);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(
                command,
                CancellationToken.None));

        // Confirm that no assignment was created.
        Assert.False(
            await context.TeacherClassSubjects
                .AnyAsync(x =>
                    x.TeacherSubjectId == teacherSubjectId &&
                    x.ClassId == classId));
    }

    [Fact]
    public async Task AssignTeacher_FromAnotherSchool_IsRejected_Independently()
    {
        var fixture = new TeacherClassAssignmentTestFixture();

        try
        {
            await fixture.InitializeAsync();

            fixture.CurrentUser.UserId = fixture.SchoolAAdminId;
            fixture.CurrentUser.IsPlatformAdmin = false;

            var handler = new AssignTeacherToClassCommandHandler(
                fixture.Context,
                fixture.AccessService);

            var exception =
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => handler.Handle(
                        new AssignTeacherToClassCommand(
                            fixture.SchoolBTeacherSubject.Id,
                            fixture.SchoolAClass.Id),
                        CancellationToken.None));

            Assert.Equal(
                "The teacher-subject assignment was not found in your school.",
                exception.Message);

            Assert.False(
                await fixture.Context.TeacherClassSubjects.AnyAsync(x =>
                    x.SchoolId == fixture.SchoolA.Id &&
                    x.TeacherSubjectId == fixture.SchoolBTeacherSubject.Id &&
                    x.ClassId == fixture.SchoolAClass.Id));
        }
        finally
        {
            await fixture.DisposeAsync();
        }
    }

    [Fact]
    public async Task AssignTeacher_ToAnotherSchoolsClass_IsRejected()
    {
        var fixture = new TeacherClassAssignmentTestFixture();

        try
        {
            await fixture.InitializeAsync();

            fixture.CurrentUser.UserId = fixture.SchoolAAdminId;
            fixture.CurrentUser.IsPlatformAdmin = false;

            var handler = new AssignTeacherToClassCommandHandler(
                fixture.Context,
                fixture.AccessService);

            var exception =
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => handler.Handle(
                        new AssignTeacherToClassCommand(
                            fixture.SchoolATeacherSubject.Id,
                            fixture.SchoolBClass.Id),
                        CancellationToken.None));

            Assert.Equal(
                "The class was not found in your school.",
                exception.Message);

            Assert.False(
                await fixture.Context.TeacherClassSubjects.AnyAsync(x =>
                    x.TeacherSubjectId == fixture.SchoolATeacherSubject.Id &&
                    x.ClassId == fixture.SchoolBClass.Id));
        }
        finally
        {
            await fixture.DisposeAsync();
        }
    }

    [Fact]
    public async Task AssignTeacher_WithMismatchedGrade_IsRejected()
    {
        var fixture = new TeacherClassAssignmentTestFixture();

        try
        {
            await fixture.InitializeAsync();

            fixture.CurrentUser.UserId = fixture.SchoolAAdminId;
            fixture.CurrentUser.IsPlatformAdmin = false;

            var handler = new AssignTeacherToClassCommandHandler(
                fixture.Context,
                fixture.AccessService);

            var exception =
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => handler.Handle(
                        new AssignTeacherToClassCommand(
                            fixture.SchoolATeacherSubject.Id,
                            fixture.SchoolAGrade11Class.Id),
                        CancellationToken.None));

            Assert.Equal(
                "The teacher's assigned subject does not match the class grade.",
                exception.Message);

            Assert.False(
                await fixture.Context.TeacherClassSubjects.AnyAsync(x =>
                    x.TeacherSubjectId == fixture.SchoolATeacherSubject.Id &&
                    x.ClassId == fixture.SchoolAGrade11Class.Id));
        }
        finally
        {
            await fixture.DisposeAsync();
        }
    }

    [Fact]
    public async Task AssignTeacher_WhenTeacherAccountIsInactive_IsRejected()
    {
        var fixture = new TeacherClassAssignmentTestFixture();

        try
        {
            await fixture.InitializeAsync();

            fixture.CurrentUser.UserId = fixture.SchoolAAdminId;
            fixture.CurrentUser.IsPlatformAdmin = false;

            var teacher = await fixture.Context.BusinessUsers
                .SingleAsync(u => u.Id == fixture.SchoolATeacherId);

            teacher.IsActive = false;
            await fixture.Context.SaveChangesAsync();

            var handler = new AssignTeacherToClassCommandHandler(
                fixture.Context,
                fixture.AccessService);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => handler.Handle(
                    new AssignTeacherToClassCommand(
                        fixture.SchoolATeacherSubject.Id,
                        fixture.SchoolAClass.Id),
                    CancellationToken.None));

            Assert.Equal(
                "The teacher-subject assignment was not found in your school.",
                exception.Message);

            Assert.False(await fixture.Context.TeacherClassSubjects.AnyAsync(x =>
                x.TeacherSubjectId == fixture.SchoolATeacherSubject.Id &&
                x.ClassId == fixture.SchoolAClass.Id));
        }
        finally
        {
            await fixture.DisposeAsync();
        }
    }

    [Fact]
    public async Task AssignTeacher_WhenTeacherMembershipIsInactive_IsRejected()
    {
        var fixture = new TeacherClassAssignmentTestFixture();

        try
        {
            await fixture.InitializeAsync();

            fixture.CurrentUser.UserId = fixture.SchoolAAdminId;
            fixture.CurrentUser.IsPlatformAdmin = false;

            // The teacher's user account remains active.
            var teacher = await fixture.Context.BusinessUsers
                .SingleAsync(u => u.Id == fixture.SchoolATeacherId);

            Assert.True(teacher.IsActive);

            var membership = await fixture.Context.SchoolMemberships
                .SingleAsync(m =>
                    m.SchoolId == fixture.SchoolA.Id &&
                    m.UserId == fixture.SchoolATeacherId &&
                    m.Role == SchoolRole.Teacher);

            membership.IsActive = false;
            await fixture.Context.SaveChangesAsync();

            var handler = new AssignTeacherToClassCommandHandler(
                fixture.Context,
                fixture.AccessService);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => handler.Handle(
                    new AssignTeacherToClassCommand(
                        fixture.SchoolATeacherSubject.Id,
                        fixture.SchoolAClass.Id),
                    CancellationToken.None));

            Assert.Equal(
                "The teacher-subject assignment was not found in your school.",
                exception.Message);

            Assert.False(await fixture.Context.TeacherClassSubjects.AnyAsync(x =>
                x.TeacherSubjectId == fixture.SchoolATeacherSubject.Id &&
                x.ClassId == fixture.SchoolAClass.Id));
        }
        finally
        {
            await fixture.DisposeAsync();
        }
    }

    [Fact]
    public async Task AssignTeacher_WhenAssignmentAlreadyExists_IsRejected()
    {
        var fixture = new TeacherClassAssignmentTestFixture();

        try
        {
            await fixture.InitializeAsync();

            fixture.CurrentUser.UserId = fixture.SchoolAAdminId;
            fixture.CurrentUser.IsPlatformAdmin = false;

            var handler = new AssignTeacherToClassCommandHandler(
                fixture.Context,
                fixture.AccessService);

            var command = new AssignTeacherToClassCommand(
                fixture.SchoolATeacherSubject.Id,
                fixture.SchoolAClass.Id);

            var assignmentId = await handler.Handle(
                command,
                CancellationToken.None);

            Assert.NotEqual(Guid.Empty, assignmentId);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => handler.Handle(command, CancellationToken.None));

            Assert.Equal(
                "This teacher is already assigned to the selected class and subject.",
                exception.Message);

            Assert.Equal(1, await fixture.Context.TeacherClassSubjects.CountAsync(x =>
                x.SchoolId == fixture.SchoolA.Id &&
                x.TeacherSubjectId == fixture.SchoolATeacherSubject.Id &&
                x.ClassId == fixture.SchoolAClass.Id));
        }
        finally
        {
            await fixture.DisposeAsync();
        }
    }

    [Fact]
    public async Task RemoveClassTeacher_FromAnotherSchool_IsRejected()
    {
        var fixture = new TeacherClassAssignmentTestFixture();

        try
        {
            await fixture.InitializeAsync();

            fixture.CurrentUser.UserId = fixture.SchoolAAdminId;
            fixture.CurrentUser.IsPlatformAdmin = false;

            var assignHandler = new AssignTeacherToClassCommandHandler(
                fixture.Context,
                fixture.AccessService);

            var assignmentId = await assignHandler.Handle(
                new AssignTeacherToClassCommand(
                    fixture.SchoolATeacherSubject.Id,
                    fixture.SchoolAClass.Id),
                CancellationToken.None);

            // School B must not remove School A's assignment.
            fixture.CurrentUser.UserId = fixture.SchoolBAdminId;

            var removeHandler = new RemoveClassTeacherCommandHandler(
                fixture.Context,
                fixture.AccessService);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => removeHandler.Handle(
                    new RemoveClassTeacherCommand(assignmentId),
                    CancellationToken.None));

            Assert.Equal(
                "The teacher-class assignment was not found in your school.",
                exception.Message);

            Assert.True(await fixture.Context.TeacherClassSubjects.AnyAsync(x =>
                x.Id == assignmentId &&
                x.SchoolId == fixture.SchoolA.Id));
        }
        finally
        {
            await fixture.DisposeAsync();
        }
    }

    [Fact]
    public async Task RemoveClassTeacher_ValidRemoval_PreservesTeacherSubject()
    {
        var fixture = new TeacherClassAssignmentTestFixture();

        try
        {
            await fixture.InitializeAsync();

            fixture.CurrentUser.UserId = fixture.SchoolAAdminId;
            fixture.CurrentUser.IsPlatformAdmin = false;

            var assignHandler = new AssignTeacherToClassCommandHandler(
                fixture.Context,
                fixture.AccessService);

            var assignmentId = await assignHandler.Handle(
                new AssignTeacherToClassCommand(
                    fixture.SchoolATeacherSubject.Id,
                    fixture.SchoolAClass.Id),
                CancellationToken.None);

            var removeHandler = new RemoveClassTeacherCommandHandler(
                fixture.Context,
                fixture.AccessService);

            await removeHandler.Handle(
                new RemoveClassTeacherCommand(assignmentId),
                CancellationToken.None);

            Assert.False(await fixture.Context.TeacherClassSubjects
                .AnyAsync(x => x.Id == assignmentId));

            Assert.True(await fixture.Context.TeacherSubjects.AnyAsync(x =>
                x.Id == fixture.SchoolATeacherSubject.Id &&
                x.SchoolId == fixture.SchoolA.Id));

            Assert.True(await fixture.Context.TeacherProfiles.AnyAsync(x =>
                x.Id == fixture.SchoolATeacherSubject.TeacherProfileId &&
                x.SchoolId == fixture.SchoolA.Id));
        }
        finally
        {
            await fixture.DisposeAsync();
        }
    }

}
