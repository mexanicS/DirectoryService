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
                                         department.id as "Id", 
                                         department.name as "Name", 
                                         department.identifier as "Identifier", 
                                         department.path as "Path", 
                                         department.depth as "Depth",
                                         department.parent_id as "ParentId",
                                         department.is_active as "IsActive",
                                         department.created_at as "CreatedAt",
                                         department.update_at as "UpdatedAt"
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