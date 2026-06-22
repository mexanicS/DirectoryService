using System.Data;

namespace DirectoryService.Application.Database;

public interface ISqlConnectionFactory
{
    IDbConnection Create();
}
