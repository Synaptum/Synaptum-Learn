namespace SynaptumLearn.Application.Common.Interfaces;

public interface ISchoolAdminAccessService
{
    Task<Guid> GetAuthorizedSchoolIdAsync(
        CancellationToken cancellationToken = default);
}