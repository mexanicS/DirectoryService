using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Contract;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.DepartmentTree.GetAncestorsByPath;

public class GetAncestorsByPathHandler(
    ISqlConnectionFactory connectionFactory)
{
    public async Task<Result<List<DepartmentResponse>, Errors>> Handle(
        string nodePath,
        CancellationToken cancellationToken)
    {
        const string dapperSql = """
                                     select 
                                         department.id, 
                                         department.name, 
                                         department.path, 
                                         department.depth
                                     from "DirectoryService".department department
                                     where department.path @> @NodePath::ltree
                                       and department.path != @NodePath::ltree
                                       and department.is_deleted = false
                                     order by nlevel(department.path) asc;
                                 """;

        using var connection = connectionFactory.Create();

        var command = new CommandDefinition(
            commandText: dapperSql,
            parameters: new { NodePath = nodePath },
            cancellationToken: cancellationToken);

        var ancestors = (await connection.QueryAsync<DepartmentResponse>(command)).ToList();

        return ancestors;
    }
}