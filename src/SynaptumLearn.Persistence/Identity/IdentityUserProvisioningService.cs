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

    public async Task AssignRoleAsync(
        string identityUserId,
        string role,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var identityUser = await _userManager.FindByIdAsync(
            identityUserId);

        if (identityUser is null)
        {
            throw new InvalidOperationException(
                "The Identity account could not be found.");
        }

        var result = await _userManager.AddToRoleAsync(
            identityUser,
            role);

        if (!result.Succeeded)
        {
            var errors = string.Join(
                "; ",
                result.Errors.Select(e => e.Description));

            throw new InvalidOperationException(
                $"Identity role assignment failed: {errors}");
        }
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

    public async Task<IReadOnlyList<string>> GetUserIdsInRoleAsync(
        string role,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var users = await _userManager.GetUsersInRoleAsync(role);

        cancellationToken.ThrowIfCancellationRequested();

        return users
            .Select(user => user.Id)
            .ToList();
    }

}