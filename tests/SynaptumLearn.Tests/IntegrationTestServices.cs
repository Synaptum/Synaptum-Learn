
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Persistence.Contexts;
using SynaptumLearn.Persistence.Identity;

namespace SynaptumLearn.Tests;

public static class IntegrationTestServices
{
    public static ServiceProvider CreateServiceProvider()
    {
        var connectionString =
            TestDatabaseConfiguration.ConnectionString;

        TestDatabaseConfiguration.Validate(connectionString);

        var services = new ServiceCollection();

        services.AddLogging();

        services.AddSingleton(TimeProvider.System);

        services.AddScoped<ICurrentUserService, TestCurrentUserService>();

        services.AddScoped<IDomainEventDispatcher, TestDomainEventDispatcher>();

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
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
        })
        .AddEntityFrameworkStores<ApplicationDbContext>();

        return services.BuildServiceProvider();
    }
}
