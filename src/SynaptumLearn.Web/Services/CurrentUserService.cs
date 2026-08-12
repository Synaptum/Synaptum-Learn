using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SynaptumLearn.Application.Common.Interfaces;

namespace SynaptumLearn.Web.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? CurrentPrincipal =>
    _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated =>
        CurrentPrincipal?.Identity?.IsAuthenticated ?? false;

    public string? IdentityUserId =>
        CurrentPrincipal?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? Email =>
        CurrentPrincipal?.FindFirstValue(ClaimTypes.Email);

    public Guid? UserId
    {
        get
        {
            var value = CurrentPrincipal?
                .FindFirstValue("DomainUserId");

            return Guid.TryParse(value, out var id)
                ? id
                : null;
        }
    }

    public Guid? SchoolId
    {
        get
        {
            var value = CurrentPrincipal?
                .FindFirstValue("SchoolId");

            return Guid.TryParse(value, out var id)
                ? id
                : null;
        }
    }
}