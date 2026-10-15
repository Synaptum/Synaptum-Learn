
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SynaptumLearn.Persistence.Contexts;
using SynaptumLearn.Persistence.Identity;

namespace SynaptumLearn.Web.Identity;

public sealed class ApplicationUserClaimsPrincipalFactory
    : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>
{
    private readonly ApplicationDbContext _dbContext;

    public ApplicationUserClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IOptions<IdentityOptions> options,
        ApplicationDbContext dbContext)
        : base(userManager, roleManager, options)
    {
        _dbContext = dbContext;
    }

    protected override async Task<ClaimsIdentity>
        GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        var domainUser = await _dbContext.BusinessUsers
            .AsNoTracking()
            .SingleOrDefaultAsync(u =>
                u.IdentityUserId == user.Id);

        if (domainUser is not null)
        {
            identity.AddClaim(new Claim(
                "DomainUserId",
                domainUser.Id.ToString()));
        }

        return identity;
    }
}
