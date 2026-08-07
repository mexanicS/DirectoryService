using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Contract;
using FluentValidation;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Departments.GetDepartmentsHierarchyLtree;

public class GetDepartmentsHierarchyLtreeHandler(
    ISqlConnectionFactory connectionFactory,
    IValidator<GetDepartmentsHierarchyLtreeQuery> validator)
{
    public async Task<Result<List<DepartmentTreeResponse>, Errors>> Handle(
        GetDepartmentsHierarchyLtreeQuery query,
        CancellationToken cancellationToken = default)
    {
        const string dapperSql = """
                                    select id, 
                                           name, 
                                           identifier, 
                                           parent_id, 
                                           path, 
                                           depth, 
                                           is_active, 
                                           created_at, 
                                           update_at, 
                                           soft_deleted_at, 
                                           is_deleted
                                    from "DirectoryService".department
                                    where path <@ @rootPath::ltree and is_deleted = false
                                    order by depth
                                 """;
        
        using var connection = connectionFactory.Create();

        var command = new CommandDefinition(
            commandText: dapperSql,
            parameters: new { rootPath = query.RootPath },
            cancellationToken: cancellationToken);
        
        var departmentRows = (await connection.QueryAsync<DepartmentTreeResponse>(command)).ToList();

        var departmentDict = departmentRows.ToDictionary(dr => dr.Id);

        var roots = new List<DepartmentTreeResponse>();

        foreach (var row in departmentRows)
        {
            if (row.ParentId.HasValue && departmentDict.TryGetValue(row.ParentId.Value, out var parent))
            {
                parent.Children.Add(departmentDict[row.Id]);
            }
            else
            {
                roots.Add(departmentDict[row.Id]);
            }
        }

        return roots;
    }
}

