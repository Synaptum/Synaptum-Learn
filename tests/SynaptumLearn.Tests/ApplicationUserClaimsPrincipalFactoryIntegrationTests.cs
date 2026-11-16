using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SynaptumLearn.Domain.Enums;
using SynaptumLearn.Domain.Schools;
using SynaptumLearn.Persistence.Identity;
using SynaptumLearn.Web.Identity;
using System.Security.Claims;

namespace SynaptumLearn.Tests;

public sealed class ApplicationUserClaimsPrincipalFactoryIntegrationTests
{
    [Fact]
    public async Task ActiveLearnerWithOneSchool_ReceivesCorrectClaims()
    {
        await using var fixture = new LearnerCurriculumTestFixture();
        await fixture.InitializeAsync();

        var services = fixture.Services;

        var userManager =
            services.GetRequiredService<UserManager<ApplicationUser>>();

        var roleManager =
            services.GetRequiredService<RoleManager<ApplicationRole>>();

        var options =
            services.GetRequiredService<IOptions<IdentityOptions>>();

        var domainUser = await fixture.Context.BusinessUsers
            .FindAsync(fixture.LearnerId);

        Assert.NotNull(domainUser);

        var identityUser =
            await userManager.FindByIdAsync(domainUser.IdentityUserId);

        Assert.NotNull(identityUser);

        var factory = new ApplicationUserClaimsPrincipalFactory(
            userManager,
            roleManager,
            options,
            fixture.Context);

        var principal = await factory.CreateAsync(identityUser);

        Assert.Equal(
            fixture.LearnerId.ToString(),
            principal.FindFirstValue("DomainUserId"));

        Assert.Equal(
            fixture.School.Id.ToString(),
            principal.FindFirstValue("SchoolId"));
    }

    [Fact]
    public async Task LearnerWithMultipleActiveSchools_DoesNotReceiveSchoolIdClaim()
    {
        await using var fixture = new LearnerCurriculumTestFixture();
        await fixture.InitializeAsync();

        var context = fixture.Context;
        var services = fixture.Services;

        var userManager =
            services.GetRequiredService<UserManager<ApplicationUser>>();

        var roleManager =
            services.GetRequiredService<RoleManager<ApplicationRole>>();

        var options =
            services.GetRequiredService<IOptions<IdentityOptions>>();

        var domainUser = await context.BusinessUsers
            .AsNoTracking()
            .SingleAsync(u => u.Id == fixture.LearnerId);

        var identityUser =
            await userManager.FindByIdAsync(domainUser.IdentityUserId);

        Assert.NotNull(identityUser);

        var secondSchool = new School
        {
            Name = $"Second Claims School {Guid.NewGuid():N}",
            EMISNumber = Guid.NewGuid().ToString("N")[..12],
            Email = $"claims-{Guid.NewGuid():N}@example.com",
            Phone = "0123456789",
            Province = Province.FreeState,
            Slug = $"claims-school-{Guid.NewGuid():N}",
            Code = $"CS{Guid.NewGuid():N}"[..10],
            Status = SchoolStatus.Active,
            CurriculumAccessMode = CurriculumAccessMode.Progressive,
            CurrentTerm = 1
        };

        context.Schools.Add(secondSchool);
        await context.SaveChangesAsync();

        try
        {
            context.SchoolMemberships.Add(new SchoolMembership
            {
                SchoolId = secondSchool.Id,
                UserId = fixture.LearnerId,
                Role = SchoolRole.Learner,
                IsActive = true
            });

            await context.SaveChangesAsync();

            var factory = new ApplicationUserClaimsPrincipalFactory(
                userManager,
                roleManager,
                options,
                context);

            var principal = await factory.CreateAsync(identityUser);

            Assert.Equal(
                fixture.LearnerId.ToString(),
                principal.FindFirstValue("DomainUserId"));

            Assert.Null(principal.FindFirst("SchoolId"));
        }
        finally
        {
            context.ChangeTracker.Clear();

            await context.SchoolMemberships
                .Where(m => m.SchoolId == secondSchool.Id)
                .ExecuteDeleteAsync();

            await context.Schools
                .Where(s => s.Id == secondSchool.Id)
                .ExecuteDeleteAsync();
        }
    }

}
