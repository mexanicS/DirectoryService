using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Contract;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.DepartmentTree.GetDirectChildrenByNode;

public class GetDirectChildrenByNodeHandler(
    ISqlConnectionFactory connectionFactory)
{
    public async Task<Result<List<DepartmentWithChildren>, Errors>> Handle(
        string nodePath,
        CancellationToken cancellationToken)
    {
        const string dapperSql = """
                                     select 
                                         department.id, 
                                         department.name, 
                                         department.path, 
                                         department.depth, 
                                         c.cnt as "childrenCount",
                                         (c.cnt > 0) as "hasChildren"
                                     from "DirectoryService".department department
                                     cross join lateral (
                                         select count(*) as cnt
                                         from "DirectoryService".department child
                                         where department.path @> child.path 
                                           and nlevel(child.path) = nlevel(department.path) + 1
                                           and child.is_deleted = false
                                     ) c
                                     where @NodePath::ltree @> department.path 
                                       and nlevel(department.path) = nlevel(@NodePath::ltree) + 1 
                                       and department.is_deleted = false
                                     order by department.name;
                                 """;
    
        using var connection = connectionFactory.Create();
    
        var command = new CommandDefinition(
            commandText: dapperSql, 
            parameters: new { NodePath = nodePath }, 
            cancellationToken: cancellationToken);
    
        var departmentChildren = (await connection.QueryAsync<DepartmentWithChildren>(command)).ToList();
    
        return departmentChildren;
    }
}