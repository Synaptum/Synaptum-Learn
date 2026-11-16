
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SynaptumLearn.Domain.Enums;
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

            if (domainUser.IsActive)
            {
                var schoolIds = await _dbContext.SchoolMemberships
                    .AsNoTracking()
                    .Where(m =>
                        m.UserId == domainUser.Id &&
                        m.IsActive)
                    .Join(
                        _dbContext.Schools.AsNoTracking(),
                        membership => membership.SchoolId,
                        school => school.Id,
                        (membership, school) => new
                        {
                            school.Id,
                            school.Status
                        })
                    .Where(x => x.Status == SchoolStatus.Active)
                    .Select(x => x.Id)
                    .Distinct()
                    .Take(2)
                    .ToListAsync();

                if (schoolIds.Count == 1)
                {
                    identity.AddClaim(new Claim(
                        "SchoolId",
                        schoolIds[0].ToString()));
                }
            }
        }


        return identity;
    }
}
