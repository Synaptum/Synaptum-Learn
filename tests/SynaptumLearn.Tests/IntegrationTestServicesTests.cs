using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using SynaptumLearn.Persistence.Contexts;
using Xunit;

namespace SynaptumLearn.Tests;

public class IntegrationTestServicesTests
{
    [Fact]
    public void ServiceProvider_ResolvesApplicationDbContext()
    {
        using var provider =
            IntegrationTestServices.CreateServiceProvider();

        using var scope = provider.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        Assert.NotNull(context);

        var connectionString =
            context.Database.GetDbConnection().ConnectionString;

        TestDatabaseConfiguration.Validate(connectionString);

        var actual = new SqlConnectionStringBuilder(connectionString);
        var expected = new SqlConnectionStringBuilder(
            TestDatabaseConfiguration.ConnectionString);

        Assert.Equal(expected.InitialCatalog, actual.InitialCatalog);
        Assert.Equal(expected.DataSource, actual.DataSource);
        Assert.Equal(expected.IntegratedSecurity, actual.IntegratedSecurity);
    }


    [Fact]
    public async Task Database_CanConnectToIntegrationDatabase()
    {
        using var provider =
            IntegrationTestServices.CreateServiceProvider();

        using var scope = provider.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var connectionString =
            context.Database.GetDbConnection().ConnectionString;

        TestDatabaseConfiguration.Validate(connectionString);

        var canConnect =
            await context.Database.CanConnectAsync();

        Assert.True(
            canConnect,
            "Could not connect to the dedicated integration test database.");
    }

}

