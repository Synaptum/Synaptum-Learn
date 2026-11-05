using Microsoft.AspNetCore.Identity;
using SynaptumLearn.Application.Common.Interfaces;

namespace SynaptumLearn.Persistence.Identity;

public sealed class IdentityUserProvisioningService
    : IIdentityUserProvisioningService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityUserProvisioningService(
        UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<string> CreateUserAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var normalizedEmail = email.Trim();

        var existingUser = await _userManager.FindByEmailAsync(
            normalizedEmail);

        if (existingUser is not null)
        {
            throw new InvalidOperationException(
                "An account with this email already exists.");
        }

        var identityUser = new ApplicationUser
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(
            identityUser,
            password);

        if (!result.Succeeded)
        {
            var errors = string.Join(
                "; ",
                result.Errors.Select(e => e.Description));

            throw new InvalidOperationException(
                $"Identity account creation failed: {errors}");
        }

        return identityUser.Id;
    }

    public async Task DeleteUserAsync(
        string identityUserId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var identityUser = await _userManager.FindByIdAsync(
            identityUserId);

        if (identityUser is null)
        {
            return;
        }

        var result = await _userManager.DeleteAsync(identityUser);

        if (!result.Succeeded)
        {
            var errors = string.Join(
                "; ",
                result.Errors.Select(e => e.Description));

            throw new InvalidOperationException(
                $"Identity account deletion failed: {errors}");
        }
    }
}