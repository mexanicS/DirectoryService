using System.Data;

using DirectoryService.Application.Database;

using Microsoft.Extensions.Configuration;
using Npgsql;

namespace DirectoryService.Infrastructure.DataBase;

public class SqlConnectionFactory(IConfiguration configuration) : ISqlConnectionFactory
{
    public IDbConnection Create()
    {
        return new NpgsqlConnection(configuration.GetConnectionString("Database"));
    }
}
