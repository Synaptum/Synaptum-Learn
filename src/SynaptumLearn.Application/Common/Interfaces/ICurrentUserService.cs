namespace SynaptumLearn.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
   /* Guid? SchoolId { get; }

    bool IsAuthenticated { get; }

    string? IdentityUserId { get; }*/
}