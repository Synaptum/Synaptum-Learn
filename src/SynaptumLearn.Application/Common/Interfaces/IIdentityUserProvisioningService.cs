namespace SynaptumLearn.Application.Common.Interfaces;

public interface IIdentityUserProvisioningService
{
    Task<string> CreateUserAsync(
        string email,
        string password,
        CancellationToken cancellationToken);

    Task AssignRoleAsync(
        string identityUserId,
        string role,
        CancellationToken cancellationToken);

    Task DeleteUserAsync(
        string identityUserId,
        CancellationToken cancellationToken);


    Task<IReadOnlyList<string>> GetUserIdsInRoleAsync(
        string role,
        CancellationToken cancellationToken);

}