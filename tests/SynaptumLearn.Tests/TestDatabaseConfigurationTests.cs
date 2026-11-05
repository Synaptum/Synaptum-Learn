
using Xunit;

namespace SynaptumLearn.Tests;

public class TestDatabaseConfigurationTests
{
    [Fact]
    public void TestConnectionString_UsesDedicatedDatabase()
    {
        var connectionString =
            TestDatabaseConfiguration.ConnectionString;

        TestDatabaseConfiguration.Validate(connectionString);
    }

    [Fact]
    public void DevelopmentDatabase_IsRejected()
    {
        var developmentConnectionString =
            @"Server=(localdb)\MSSQLLocalDB;Database=SynaptumLearnDb;Trusted_Connection=True;";

        Assert.Throws<InvalidOperationException>(() =>
            TestDatabaseConfiguration.Validate(
                developmentConnectionString));
    }

    [Fact]
    public void UnexpectedServer_IsRejected()
    {
        var connectionString =
            @"Server=localhost;Database=SynaptumLearnIntegrationTests;Trusted_Connection=True;";

        Assert.Throws<InvalidOperationException>(() =>
            TestDatabaseConfiguration.Validate(connectionString));
    }
}