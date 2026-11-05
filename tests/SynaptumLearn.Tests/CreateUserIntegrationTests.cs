
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SynaptumLearn.Application.Users.CreateUser;
using SynaptumLearn.Persistence.Contexts;
using SynaptumLearn.Persistence.Identity;
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

        var provisioningService =
            services.GetRequiredService<
                SynaptumLearn.Application.Common.Interfaces
                    .IIdentityUserProvisioningService>();

        var handler = new CreateUserCommandHandler(
            context,
            provisioningService);

        var email =
            $"integration-{Guid.NewGuid():N}@example.com";

        var command = new CreateUserCommand(
            "Integration",
            "Test",
            email,
            "TestPassword123!");

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
