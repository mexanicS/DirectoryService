using Npgsql;

namespace FileService.Infrastructure.Postgres.Options;

public class PostgresOptions
{
    public const string SECTION_NAME = "ConnectionStrings";

    public string Database { get; set; } = string.Empty;

    internal static bool IsValidConnectionString(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return false;
        }

        try
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            return !string.IsNullOrWhiteSpace(builder.Host) && !string.IsNullOrWhiteSpace(builder.Database);
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}