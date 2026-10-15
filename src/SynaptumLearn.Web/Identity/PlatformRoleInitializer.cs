using Microsoft.AspNetCore.Identity;
using SynaptumLearn.Application.Common.Security;
using SynaptumLearn.Persistence.Identity;

namespace SynaptumLearn.Web.Identity;

public static class PlatformRoleInitializer
{
    public static async Task InitializeAsync(
        IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<ApplicationRole>>();

        var roleName = PlatformRoles.PlatformAdmin;

        if (await roleManager.RoleExistsAsync(roleName))
        {
            return;
        }

        var result = await roleManager.CreateAsync(
            new ApplicationRole
            {
                Name = roleName
            });

        if (!result.Succeeded)
        {
            var errors = string.Join(
                "; ",
                result.Errors.Select(x => x.Description));

            throw new InvalidOperationException(
                $"Failed to create platform role: {errors}");
        }
    }
}