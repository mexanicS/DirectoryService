using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contract;
using FluentValidation;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.DepartmentTree.GetNodesByName;

public class GetNodesByNameHandler(
    ISqlConnectionFactory connectionFactory,
    IValidator<GetNodesByNameHandlerCommand> validator)
{
    public async Task<Result<List<DepartmentTreeResponse>, Errors>> Handle(
        GetNodesByNameHandlerCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }
        
        var searchPattern = $"%{command.DepartmentName!.Trim()}%";
        
        const string dapperSql = """
                                 with matched_departments as (
                                     select path
                                     from "DirectoryService".department
                                     where is_deleted = false
                                       and name ilike @SearchPattern
                                 ),
                                 all_ancestor_paths as (
                                     select distinct 
                                         dep.id as "Id", 
                                         dep.name as "Name", 
                                         dep.identifier as "Identifier", 
                                         dep.parent_id as "ParentId", 
                                         dep.path as "Path", 
                                         dep.depth as "Depth", 
                                         dep.is_active as "IsActive", 
                                         dep.created_at as "CreatedAt", 
                                         dep.update_at as "UpdatedAt"
                                     from "DirectoryService".department dep
                                     join matched_departments matched 
                                       on dep.path @> matched.path 
                                     where dep.is_deleted = false
                                 )
                                 select *
                                 from all_ancestor_paths
                                 order by nlevel("Path") asc;
                                 """;

        using var connection = connectionFactory.Create();

        var commandDapper = new CommandDefinition(
            commandText: dapperSql,
            parameters: new { SearchPattern = searchPattern },
            cancellationToken: cancellationToken);

        var ancestors = (await connection.QueryAsync<DepartmentTreeResponse>(commandDapper)).ToList();

        return ancestors;
    }
}