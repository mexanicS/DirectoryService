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
        
        const string testdapper = """
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
                                  where path <@rootPath::ltree
                                    and nlevel(path) > nlevel(@rootPath::ltree)
                                    and nlevel(path) <= nlevel(@rootPath::ltree) + @depth 
                                    and is_deleted = false
                                  order by depth
                                  """;
        
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

        var departmentRows =
            (await connection.QueryAsync<DepartmentTreeResponse>(dapperSql, new
            {
                rootPath = query.RootPath,
            })).ToList();

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

    //Удаление всех дочерних
    public async Task<int> DeleteDepartment(GetDepartmentsHierarchyLtreeQuery query)
    {
        const string dapperSql = """
                                    delete from "DirectoryService".department
                                    where path <@ @rootPath::ltree and path != @rootPath::ltree
                                """;
        
        using var connection = connectionFactory.Create();

        var affectedRows = await connection.ExecuteAsync(dapperSql, new { rootPath = query.RootPath, });

        return affectedRows;
    }
    

}

