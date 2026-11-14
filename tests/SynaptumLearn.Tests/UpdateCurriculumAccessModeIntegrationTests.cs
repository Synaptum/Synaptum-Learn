using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Application.Users.CreateUser;
using SynaptumLearn.Application.Schools.UpdateCurriculumAccessMode;
using SynaptumLearn.Application.Schools.GetCurriculumAccessMode;
using SynaptumLearn.Domain.Schools;
using SynaptumLearn.Domain.Enums;
using SynaptumLearn.Persistence.Identity;
using SynaptumLearn.Persistence.Contexts;
using Xunit;

namespace SynaptumLearn.Tests;

public sealed class UpdateCurriculumAccessModeIntegrationTests
{
    [Fact]
    public async Task UnauthenticatedUser_CannotUpdateCurriculumAccessMode()
    {
        using var provider =
            IntegrationTestServices.CreateServiceProvider();

        using var scope = provider.CreateScope();

        var services = scope.ServiceProvider;

        var context = services
            .GetRequiredService<ApplicationDbContext>();

        var currentUser = services
            .GetRequiredService<TestCurrentUserService>();

        var accessService = services
            .GetRequiredService<ISchoolAdminAccessService>();

        currentUser.IsAuthenticated = false;
        currentUser.UserId = null;

        var handler = new UpdateCurriculumAccessModeCommandHandler(
            context,
            accessService);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(
                new UpdateCurriculumAccessModeCommand(
                    CurriculumAccessMode.FullYear),
                CancellationToken.None));
    }

    [Fact]
    public async Task InvalidAccessMode_IsRejectedByValidator()
    {
        var validator =
            new UpdateCurriculumAccessModeCommandValidator();

        var command = new UpdateCurriculumAccessModeCommand(
            (CurriculumAccessMode)999);

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == "AccessMode");
    }

    [Fact]
    public async Task AuthenticatedUserWithoutSchoolAdminMembership_IsRejected()
    {
        using var provider =
            IntegrationTestServices.CreateServiceProvider();

        using var scope = provider.CreateScope();

        var services = scope.ServiceProvider;

        var context = services
            .GetRequiredService<ApplicationDbContext>();

        var currentUser = services
            .GetRequiredService<TestCurrentUserService>();

        var accessService = services
            .GetRequiredService<ISchoolAdminAccessService>();

        currentUser.IsAuthenticated = true;
        currentUser.UserId = Guid.NewGuid();
        currentUser.IsPlatformAdmin = false;

        var handler = new UpdateCurriculumAccessModeCommandHandler(
            context,
            accessService);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(
                new UpdateCurriculumAccessModeCommand(
                    CurriculumAccessMode.FullYear),
                CancellationToken.None));
    }

    [Fact]
    public async Task ActiveSchoolAdmin_CanUpdateOwnSchoolCurriculumAccessMode()
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

        var email = $"curriculum-admin-{uniqueId}@example.com";

        var school = new School
        {
            Name = $"Curriculum Test School {uniqueId}",
            EMISNumber = uniqueId[..12],
            Email = $"school-{uniqueId}@example.com",
            Phone = "0123456789",
            Province = Province.FreeState,
            Slug = $"curriculum-school-{uniqueId}",
            Code = $"CT{uniqueId[..8]}",
            Status = SchoolStatus.Active,
            CurriculumAccessMode = CurriculumAccessMode.Progressive
        };

        var otherSchool = new School
        {
            Name = $"Other Curriculum School {uniqueId}",
            EMISNumber = uniqueId[12..24],
            Email = $"other-school-{uniqueId}@example.com",
            Phone = "0123456789",
            Province = Province.FreeState,
            Slug = $"other-curriculum-school-{uniqueId}",
            Code = $"OC{uniqueId[..8]}",
            Status = SchoolStatus.Active,
            CurriculumAccessMode = CurriculumAccessMode.Progressive
        };

        context.Schools.AddRange(school, otherSchool);
        await context.SaveChangesAsync();

        try
        {
            var createUserHandler = new CreateUserCommandHandler(
                context,
                provisioningService,
                currentUser);

            var domainUserId = await createUserHandler.Handle(
                new CreateUserCommand(
                    "Curriculum",
                    "Administrator",
                    email,
                    "TestPassword123!",
                    UserAccountType.SchoolAdmin,
                    school.Id),
                CancellationToken.None);

            currentUser.UserId = domainUserId;
            currentUser.IsPlatformAdmin = false;

            var queryHandler =
                new GetCurriculumAccessModeQueryHandler(
                    context,
                    accessService);

            var initialMode = await queryHandler.Handle(
                new GetCurriculumAccessModeQuery(),
                CancellationToken.None);

            Assert.Equal(
                CurriculumAccessMode.Progressive,
                initialMode);

            var updateHandler =
                new UpdateCurriculumAccessModeCommandHandler(
                    context,
                    accessService);

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => updateHandler.Handle(
                    new UpdateCurriculumAccessModeCommand(
                        (CurriculumAccessMode)999),
                    CancellationToken.None));

            await updateHandler.Handle(
                new UpdateCurriculumAccessModeCommand(
                    CurriculumAccessMode.FullYear),
                CancellationToken.None);

            // Clear tracked entities so verification reads the database.
            context.ChangeTracker.Clear();

            var savedMode = await context.Schools
                .AsNoTracking()
                .Where(s => s.Id == school.Id)
                .Select(s => s.CurriculumAccessMode)
                .SingleAsync();

            Assert.Equal(
                CurriculumAccessMode.FullYear,
                savedMode);

            var updatedMode = await queryHandler.Handle(
                new GetCurriculumAccessModeQuery(),
                CancellationToken.None);

            Assert.Equal(
                CurriculumAccessMode.FullYear,
                updatedMode);

            var otherSchoolMode = await context.Schools
                .AsNoTracking()
                .Where(s => s.Id == otherSchool.Id)
                .Select(s => s.CurriculumAccessMode)
                .SingleAsync();

            Assert.Equal(
                CurriculumAccessMode.Progressive,
                otherSchoolMode);

        }
        finally
        {
            var memberships = await context.SchoolMemberships
                .Where(m => m.SchoolId == school.Id)
                .ToListAsync();

            context.SchoolMemberships.RemoveRange(memberships);
            await context.SaveChangesAsync();

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

            // Re-query because ChangeTracker.Clear() detached the school.
            var savedSchools = await context.Schools
                .Where(s =>
                    s.Id == school.Id ||
                    s.Id == otherSchool.Id)
                .ToListAsync();

            context.Schools.RemoveRange(savedSchools);
            await context.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task InvalidAccessMode_IsRejectedByHandler()
    {
        using var provider =
            IntegrationTestServices.CreateServiceProvider();

        using var scope = provider.CreateScope();

        var services = scope.ServiceProvider;

        var context =
            services.GetRequiredService<ApplicationDbContext>();

        var currentUser =
            services.GetRequiredService<TestCurrentUserService>();

        var accessService =
            services.GetRequiredService<ISchoolAdminAccessService>();

        currentUser.IsAuthenticated = false;
        currentUser.UserId = null;

        var handler =
            new UpdateCurriculumAccessModeCommandHandler(
                context,
                accessService);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(
                new UpdateCurriculumAccessModeCommand(
                    (CurriculumAccessMode)999),
                CancellationToken.None));
    }


}
