namespace SynaptumLearn.Application.Common.Interfaces;

public interface IIdentityUserProvisioningService
{
    Task<string> CreateUserAsync(
        string email,
        string password,
        CancellationToken cancellationToken);

    Task DeleteUserAsync(
        string identityUserId,
        CancellationToken cancellationToken);
}