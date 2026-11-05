
using Microsoft.Data.SqlClient;

namespace SynaptumLearn.Tests;

public static class TestDatabaseConfiguration
{
    public const string DatabaseName =
        "SynaptumLearnIntegrationTests";

    public static string ConnectionString
    {
        get
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = @"(localdb)\MSSQLLocalDB",
                InitialCatalog = DatabaseName,
                IntegratedSecurity = true,
                TrustServerCertificate = true
            };

            Validate(builder.ConnectionString);

            return builder.ConnectionString;
        }
    }

    public static void Validate(string connectionString)
    {
        var builder =
            new SqlConnectionStringBuilder(connectionString);

        if (!string.Equals(
                builder.InitialCatalog,
                DatabaseName,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Integration tests must use the dedicated test database.");
        }

        if (!string.Equals(
                builder.DataSource,
                @"(localdb)\MSSQLLocalDB",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Integration tests must use the configured LocalDB instance.");
        }
    }
}
