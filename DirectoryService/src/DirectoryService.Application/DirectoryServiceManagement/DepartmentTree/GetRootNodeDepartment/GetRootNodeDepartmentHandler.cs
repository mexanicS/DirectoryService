using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Application.DirectoryServiceManagement.Departments.GetDepartmentsHierarchyLtree;
using DirectoryService.Contract;
using FluentValidation;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.DepartmentTree.GetRootNodeDepartment;

public class GetRootNodeDepartmentHandler(
    ISqlConnectionFactory connectionFactory)
{
    public async Task<Result<List<DepartmentWithChildren>, Errors>> Handle(CancellationToken cancellationToken)
    {
        
        const string dapperSql = """
                                    select department.id, 
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
                                    where nlevel(department.path) = 1 and department.is_deleted = false
                                    order by department.depth
                                 """;
        
        using var connection = connectionFactory.Create();

        var command = new CommandDefinition(commandText: dapperSql, cancellationToken: cancellationToken);
        
        var departmentRootNodes =
            (await connection.QueryAsync<DepartmentWithChildren>(command)).ToList();

        return departmentRootNodes;
    }
}