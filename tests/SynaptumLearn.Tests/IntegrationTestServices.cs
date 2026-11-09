
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Application.Common.Security;
using SynaptumLearn.Persistence.Contexts;
using SynaptumLearn.Persistence.Identity;

namespace SynaptumLearn.Tests;

public static class IntegrationTestServices
{
    public static ServiceProvider CreateServiceProvider(
    TeacherProfileFailureInterceptor? failureInterceptor = null)
    {
        var connectionString =
            TestDatabaseConfiguration.ConnectionString;

        TestDatabaseConfiguration.Validate(connectionString);

        var services = new ServiceCollection();

        services.AddLogging();

        services.AddSingleton(TimeProvider.System);

        services.AddScoped<TestCurrentUserService>();

        services.AddScoped<ICurrentUserService>(provider =>
            provider.GetRequiredService<TestCurrentUserService>());

        services.AddScoped<ISchoolAdminAccessService, SchoolAdminAccessService>();

        services.AddScoped<IDomainEventDispatcher, TestDomainEventDispatcher>();

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString);

            if (failureInterceptor is not null)
            {
                options.AddInterceptors(failureInterceptor);
            }
        });

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IIdentityUserProvisioningService,
            IdentityUserProvisioningService>();

        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = false;

            options.User.RequireUniqueEmail = true;
        })
        .AddRoles<ApplicationRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>();

        return services.BuildServiceProvider();
    }
}
