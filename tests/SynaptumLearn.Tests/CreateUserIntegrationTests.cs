
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SynaptumLearn.Application.Users.CreateUser;
using SynaptumLearn.Persistence.Contexts;
using SynaptumLearn.Persistence.Identity;
using SynaptumLearn.Application.Common.Security;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Domain.Schools;
using SynaptumLearn.Domain.Enums;
using Xunit;


namespace SynaptumLearn.Tests;

public class CreateUserIntegrationTests
{
    [Fact]
    public async Task CreateUser_CreatesIdentityAndDomainUser()
    {
        using var provider =
            IntegrationTestServices.CreateServiceProvider();

        using var scope = provider.CreateScope();

        var services = scope.ServiceProvider;

        var context =
            services.GetRequiredService<ApplicationDbContext>();

        TestDatabaseConfiguration.Validate(
            context.Database.GetDbConnection().ConnectionString);

        Assert.True(
            await context.Database.CanConnectAsync(),
            "Cannot connect to the integration test database.");

        var userManager =
            services.GetRequiredService<UserManager<ApplicationUser>>();


        var roleManager =
            services.GetRequiredService<RoleManager<ApplicationRole>>();

        const string platformAdminRole = "PlatformAdmin";

        if (!await roleManager.RoleExistsAsync(platformAdminRole))
        {
            var roleResult = await roleManager.CreateAsync(
                new ApplicationRole
                {
                    Name = platformAdminRole
                });

            Assert.True(
                roleResult.Succeeded,
                string.Join("; ",
                    roleResult.Errors.Select(e => e.Description)));
        }

        var provisioningService =
            services.GetRequiredService<
                SynaptumLearn.Application.Common.Interfaces
                    .IIdentityUserProvisioningService>();

        var currentUser =
            services.GetRequiredService<ICurrentUserService>();

        var handler = new CreateUserCommandHandler(
            context,
            provisioningService,
            currentUser);

        var email =
            $"integration-{Guid.NewGuid():N}@example.com";

        var command = new CreateUserCommand(
            "Integration",
            "Test",
            email,
            "TestPassword123!",
            UserAccountType.PlatformAdmin,
            null);

        try
        {
            var domainUserId =
                await handler.Handle(
                    command,
                    CancellationToken.None);

            var domainUser = await context.BusinessUsers
                .SingleAsync(x => x.Id == domainUserId);

            var identityUser =
                await userManager.FindByEmailAsync(email);

            Assert.NotNull(identityUser);


            Assert.True(
                await userManager.IsInRoleAsync(
                    identityUser,
                    PlatformRoles.PlatformAdmin));

            Assert.Equal(
                identityUser.Id,
                domainUser.IdentityUserId);

            Assert.Equal("Integration", domainUser.FirstName);
            Assert.Equal("Test", domainUser.LastName);
            Assert.Equal(email, domainUser.Email);
            Assert.True(domainUser.IsActive);
        }
        finally
        {
            await CleanupTestUserAsync(context, userManager, email);
        }
    }


    [Fact]
    public async Task CreateUser_RejectsUnauthorizedCaller()
    {
        using var provider =
            IntegrationTestServices.CreateServiceProvider();

        using var scope = provider.CreateScope();

        var services = scope.ServiceProvider;

        var context =
            services.GetRequiredService<ApplicationDbContext>();

        TestDatabaseConfiguration.Validate(
            context.Database.GetDbConnection().ConnectionString);

        Assert.True(
            await context.Database.CanConnectAsync(),
            "Cannot connect to the integration test database.");

        var userManager =
            services.GetRequiredService<UserManager<ApplicationUser>>();

        var provisioningService =
            services.GetRequiredService<IIdentityUserProvisioningService>();

        var unauthorizedUser = new UnauthorizedTestUserService();

        var handler = new CreateUserCommandHandler(
            context,
            provisioningService,
            unauthorizedUser);

        var email =
            $"unauthorized-{Guid.NewGuid():N}@example.com";

        var command = new CreateUserCommand(
            "Unauthorized",
            "Test",
            email,
            "TestPassword123!",
            UserAccountType.PlatformAdmin,
            null);

        try
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => handler.Handle(
                    command,
                    CancellationToken.None));

            var domainUserExists =
                await context.BusinessUsers.AnyAsync(
                    x => x.Email == email);

            var identityUser =
                await userManager.FindByEmailAsync(email);

            Assert.False(domainUserExists);
            Assert.Null(identityUser);
        }
        finally
        {
            await CleanupTestUserAsync(
                context,
                userManager,
                email);
        }
    }


    [Fact]
    public async Task CreateUser_RejectsAuthenticatedNonAdmin()
    {
        using var provider =
            IntegrationTestServices.CreateServiceProvider();

        using var scope = provider.CreateScope();

        var services = scope.ServiceProvider;

        var context =
            services.GetRequiredService<ApplicationDbContext>();

        TestDatabaseConfiguration.Validate(
            context.Database.GetDbConnection().ConnectionString);

        Assert.True(
            await context.Database.CanConnectAsync());

        var userManager =
            services.GetRequiredService<UserManager<ApplicationUser>>();

        var provisioningService =
            services.GetRequiredService<IIdentityUserProvisioningService>();

        var currentUser = new UnauthorizedTestUserService
        {
            IsAuthenticated = true
        };

        var handler = new CreateUserCommandHandler(
            context,
            provisioningService,
            currentUser);

        var email =
            $"nonadmin-{Guid.NewGuid():N}@example.com";

        var command = new CreateUserCommand(
            "NonAdmin",
            "Test",
            email,
            "TestPassword123!",
            UserAccountType.PlatformAdmin,
            null);

        try
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => handler.Handle(
                    command,
                    CancellationToken.None));

            Assert.False(
                await context.BusinessUsers.AnyAsync(
                    x => x.Email == email));

            Assert.Null(
                await userManager.FindByEmailAsync(email));
        }
        finally
        {
            await CleanupTestUserAsync(
                context,
                userManager,
                email);
        }
    }


    [Fact]
    public async Task CreateUser_CreatesSchoolAdministratorMembership()
    {
        using var provider =
            IntegrationTestServices.CreateServiceProvider();

        using var scope = provider.CreateScope();

        var services = scope.ServiceProvider;

        var context =
            services.GetRequiredService<ApplicationDbContext>();

        TestDatabaseConfiguration.Validate(
            context.Database.GetDbConnection().ConnectionString);

        Assert.True(
            await context.Database.CanConnectAsync(),
            "Cannot connect to the integration test database.");

        var userManager =
            services.GetRequiredService<UserManager<ApplicationUser>>();

        var provisioningService =
            services.GetRequiredService<IIdentityUserProvisioningService>();

        var currentUser =
            services.GetRequiredService<ICurrentUserService>();

        var handler = new CreateUserCommandHandler(
            context,
            provisioningService,
            currentUser);

        var uniqueId = Guid.NewGuid().ToString("N");
        var email = $"schooladmin-{uniqueId}@example.com";

        var school = new School
        {
            Name = $"Integration Test School {uniqueId}",
            EMISNumber = uniqueId[..12],
            Email = $"school-{uniqueId}@example.com",
            Phone = "0123456789",
            Province = Province.FreeState,
            Slug = $"integration-school-{uniqueId}",
            Code = $"IT{uniqueId[..8]}",
            Status = SchoolStatus.Active
        };

        context.Schools.Add(school);

        await context.SaveChangesAsync();

        try
        {
            var command = new CreateUserCommand(
                "School",
                "Administrator",
                email,
                "TestPassword123!",
                UserAccountType.SchoolAdmin,
                school.Id);

            var domainUserId = await handler.Handle(
                command,
                CancellationToken.None);

            var domainUser = await context.BusinessUsers
                .SingleAsync(x => x.Id == domainUserId);

            var identityUser =
                await userManager.FindByEmailAsync(email);

            Assert.NotNull(identityUser);

            Assert.Equal(
                identityUser.Id,
                domainUser.IdentityUserId);

            var membership = await context.SchoolMemberships
                .SingleAsync(x =>
                    x.UserId == domainUserId &&
                    x.SchoolId == school.Id);

            Assert.Equal(
                SchoolRole.SchoolAdmin,
                membership.Role);

            Assert.True(membership.IsActive);
            Assert.True(domainUser.IsActive);
        }
        finally
        {
            // Remove the membership before removing the domain user.
            var memberships = await context.SchoolMemberships
                .Where(x => x.SchoolId == school.Id)
                .ToListAsync();

            context.SchoolMemberships.RemoveRange(memberships);
            await context.SaveChangesAsync();

            await CleanupTestUserAsync(
                context,
                userManager,
                email);

            context.Schools.Remove(school);
            await context.SaveChangesAsync();
        }
    }


    private static async Task CleanupTestUserAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        string email)
    {
        TestDatabaseConfiguration.Validate(
            context.Database.GetDbConnection().ConnectionString);

        var domainUser = await context.BusinessUsers
            .SingleOrDefaultAsync(x => x.Email == email);

        if (domainUser is not null)
        {
            context.BusinessUsers.Remove(domainUser);
            await context.SaveChangesAsync();
        }

        var identityUser = await userManager.FindByEmailAsync(email);

        if (identityUser is not null)
        {
            var result = await userManager.DeleteAsync(identityUser);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    "Integration test user cleanup failed: " +
                    string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }
    }

}

internal sealed class UnauthorizedTestUserService
    : ICurrentUserService
{
    public bool IsAuthenticated { get; set; }

    public Guid? UserId => null;

    public Guid? SchoolId => null;

    public string? IdentityUserId => null;

    public string? Email => null;

    public bool IsInRole(string role) => false;
}

