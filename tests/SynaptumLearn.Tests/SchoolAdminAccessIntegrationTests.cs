
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Application.Users.CreateUser;
using SynaptumLearn.Domain.Enums;
using SynaptumLearn.Domain.Schools;
using SynaptumLearn.Persistence.Contexts;
using SynaptumLearn.Persistence.Identity;
using Xunit;

namespace SynaptumLearn.Tests;

public class SchoolAdminAccessIntegrationTests
{
    [Fact]
    public async Task ActiveSchoolAdmin_CanResolveAuthorizedSchool()
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

        var email = $"access-{uniqueId}@example.com";

        var school = new School
        {
            Name = $"Access Test School {uniqueId}",
            EMISNumber = uniqueId[..12],
            Email = $"school-{uniqueId}@example.com",
            Phone = "0123456789",
            Province = Province.FreeState,
            Slug = $"access-school-{uniqueId}",
            Code = $"AT{uniqueId[..8]}",
            Status = SchoolStatus.Active
        };

        context.Schools.Add(school);
        await context.SaveChangesAsync();

        try
        {
            var handler = new CreateUserCommandHandler(
                context,
                provisioningService,
                currentUser);

            var command = new CreateUserCommand(
                "Access",
                "Administrator",
                email,
                "TestPassword123!",
                UserAccountType.SchoolAdmin,
                school.Id);

            var domainUserId = await handler.Handle(
                command,
                CancellationToken.None);

            currentUser.UserId = domainUserId;
            currentUser.IsPlatformAdmin = false;

            var authorizedSchoolId =
                await accessService.GetAuthorizedSchoolIdAsync();

            Assert.Equal(school.Id, authorizedSchoolId);
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

            context.Schools.Remove(school);
            await context.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task InactiveSchoolAdminMembership_IsRejected()
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
        var email = $"inactive-admin-{uniqueId}@example.com";

        var school = new School
        {
            Name = $"Inactive Admin Test School {uniqueId}",
            EMISNumber = uniqueId[..12],
            Email = $"school-{uniqueId}@example.com",
            Phone = "0123456789",
            Province = Province.FreeState,
            Slug = $"inactive-admin-school-{uniqueId}",
            Code = $"IA{uniqueId[..8]}",
            Status = SchoolStatus.Active
        };

        context.Schools.Add(school);
        await context.SaveChangesAsync();

        try
        {
            var handler = new CreateUserCommandHandler(
                context,
                provisioningService,
                currentUser);

            var domainUserId = await handler.Handle(
                new CreateUserCommand(
                    "Inactive",
                    "Administrator",
                    email,
                    "TestPassword123!",
                    UserAccountType.SchoolAdmin,
                    school.Id),
                CancellationToken.None);

            currentUser.UserId = domainUserId;
            currentUser.IsPlatformAdmin = false;

            // Confirm access before deactivation.
            var authorizedSchoolId =
                await accessService.GetAuthorizedSchoolIdAsync();

            Assert.Equal(school.Id, authorizedSchoolId);

            // Deactivate the school membership.
            var membership = await context.SchoolMemberships
                .SingleAsync(m =>
                    m.UserId == domainUserId &&
                    m.SchoolId == school.Id);

            membership.IsActive = false;
            await context.SaveChangesAsync();

            // The same authenticated user must now be rejected.
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => accessService.GetAuthorizedSchoolIdAsync());
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

            context.Schools.Remove(school);
            await context.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task UnauthenticatedUser_IsRejected()
    {
        using var provider =
            IntegrationTestServices.CreateServiceProvider();

        using var scope = provider.CreateScope();

        var services = scope.ServiceProvider;

        var currentUser =
            services.GetRequiredService<TestCurrentUserService>();

        var accessService =
            services.GetRequiredService<ISchoolAdminAccessService>();

        currentUser.IsAuthenticated = false;
        currentUser.UserId = null;
        currentUser.IsPlatformAdmin = false;

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => accessService.GetAuthorizedSchoolIdAsync());
    }

    [Fact]
    public async Task AuthenticatedUserWithoutSchoolAdminMembership_IsRejected()
    {
        using var provider =
            IntegrationTestServices.CreateServiceProvider();

        using var scope = provider.CreateScope();

        var services = scope.ServiceProvider;

        var currentUser =
            services.GetRequiredService<TestCurrentUserService>();

        var accessService =
            services.GetRequiredService<ISchoolAdminAccessService>();

        currentUser.IsAuthenticated = true;
        currentUser.UserId = Guid.NewGuid();
        currentUser.IsPlatformAdmin = false;

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => accessService.GetAuthorizedSchoolIdAsync());
    }


    [Theory]
    [InlineData(SchoolRole.Teacher)]
    [InlineData(SchoolRole.Learner)]
    public async Task NonAdminSchoolMembership_IsRejected(
        SchoolRole role)
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
        var email = $"nonadmin-role-{uniqueId}@example.com";

        var school = new School
        {
            Name = $"Role Test School {uniqueId}",
            EMISNumber = uniqueId[..12],
            Email = $"school-{uniqueId}@example.com",
            Phone = "0123456789",
            Province = Province.FreeState,
            Slug = $"role-test-school-{uniqueId}",
            Code = $"RT{uniqueId[..8]}",
            Status = SchoolStatus.Active
        };

        context.Schools.Add(school);
        await context.SaveChangesAsync();

        try
        {
            // Provision an account using the existing tested workflow.
            var handler = new CreateUserCommandHandler(
                context,
                provisioningService,
                currentUser);

            var domainUserId = await handler.Handle(
                new CreateUserCommand(
                    "Role",
                    "Test",
                    email,
                    "TestPassword123!",
                    UserAccountType.SchoolAdmin,
                    school.Id),
                CancellationToken.None);

            // Change the membership to the role under test.
            var membership = await context.SchoolMemberships
                .SingleAsync(m =>
                    m.UserId == domainUserId &&
                    m.SchoolId == school.Id);

            membership.Role = role;
            await context.SaveChangesAsync();

            currentUser.UserId = domainUserId;
            currentUser.IsPlatformAdmin = false;

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => accessService.GetAuthorizedSchoolIdAsync());
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

            context.Schools.Remove(school);
            await context.SaveChangesAsync();
        }
    }


    [Fact]
    public async Task InactiveDomainUser_IsRejected()
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
        var email = $"inactive-user-{uniqueId}@example.com";

        var school = new School
        {
            Name = $"Inactive User Test School {uniqueId}",
            EMISNumber = uniqueId[..12],
            Email = $"school-{uniqueId}@example.com",
            Phone = "0123456789",
            Province = Province.FreeState,
            Slug = $"inactive-user-school-{uniqueId}",
            Code = $"IU{uniqueId[..8]}",
            Status = SchoolStatus.Active
        };

        context.Schools.Add(school);
        await context.SaveChangesAsync();

        try
        {
            var handler = new CreateUserCommandHandler(
                context,
                provisioningService,
                currentUser);

            var domainUserId = await handler.Handle(
                new CreateUserCommand(
                    "Inactive",
                    "User",
                    email,
                    "TestPassword123!",
                    UserAccountType.SchoolAdmin,
                    school.Id),
                CancellationToken.None);

            currentUser.UserId = domainUserId;
            currentUser.IsPlatformAdmin = false;

            // Verify the account initially has access.
            Assert.Equal(
                school.Id,
                await accessService.GetAuthorizedSchoolIdAsync());

            // Deactivate the domain user without changing membership.
            var domainUser = await context.BusinessUsers
                .SingleAsync(u => u.Id == domainUserId);

            domainUser.IsActive = false;
            await context.SaveChangesAsync();

            // Access must now be denied.
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => accessService.GetAuthorizedSchoolIdAsync());
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

            context.Schools.Remove(school);
            await context.SaveChangesAsync();
        }
    }


    [Theory]
    [InlineData(SchoolStatus.Pending)]
    [InlineData(SchoolStatus.Suspended)]
    [InlineData(SchoolStatus.Archived)]
    public async Task NonActiveSchool_IsRejected(
        SchoolStatus status)
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
        var email = $"school-status-{uniqueId}@example.com";

        var school = new School
        {
            Name = $"Status Test School {uniqueId}",
            EMISNumber = uniqueId[..12],
            Email = $"school-{uniqueId}@example.com",
            Phone = "0123456789",
            Province = Province.FreeState,
            Slug = $"status-test-school-{uniqueId}",
            Code = $"ST{uniqueId[..8]}",
            Status = SchoolStatus.Active
        };

        context.Schools.Add(school);
        await context.SaveChangesAsync();

        try
        {
            var handler = new CreateUserCommandHandler(
                context,
                provisioningService,
                currentUser);

            var domainUserId = await handler.Handle(
                new CreateUserCommand(
                    "Status",
                    "Administrator",
                    email,
                    "TestPassword123!",
                    UserAccountType.SchoolAdmin,
                    school.Id),
                CancellationToken.None);

            currentUser.UserId = domainUserId;
            currentUser.IsPlatformAdmin = false;

            // An active school must initially be accessible.
            Assert.Equal(
                school.Id,
                await accessService.GetAuthorizedSchoolIdAsync());

            // Change the school status.
            school.Status = status;
            await context.SaveChangesAsync();

            // Access must be revoked without requiring logout.
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => accessService.GetAuthorizedSchoolIdAsync());
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

            context.Schools.Remove(school);
            await context.SaveChangesAsync();
        }
    }


}
