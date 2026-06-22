using System.Data;
using System.Text;
using CSharpFunctionalExtensions;

using Dapper;

using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contract;
using FluentValidation;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Locations.Get;

public class GetLocationsHandler(
    ISqlConnectionFactory connectionFactory,
    IValidator<GetLocationsQuery> validator)
{
    public async Task<Result<PagedResult<LocationListItemDto>, Errors>> Handle(
        GetLocationsQuery query,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToErrors();

        var offset = (query.Page - 1) * query.PageSize;

        var parameters = new DynamicParameters();
        parameters.Add("PageSize", query.PageSize, DbType.Int32);
        parameters.Add("Offset", offset, DbType.Int32);

        var whereClause = new StringBuilder("WHERE l.is_active = true");
        var havingClause = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            whereClause.Append(" AND l.name ILIKE '%' || @Search || '%'");
            parameters.Add("Search", query.Search, DbType.String);
        }

        if (query.MinDepartmentCount.HasValue)
        {
            havingClause.Append("HAVING COUNT(dl.department_id) >= @MinDepartmentCount");
            parameters.Add("MinDepartmentCount", query.MinDepartmentCount.Value, DbType.Int32);
        }

        var sql = $"""
            WITH filtered_locations AS (
                SELECT
                    l.id,
                    l.name,
                    l.street || ', ' || l.house_number || ', ' || l.city AS Address,
                    l.created_at AS CreatedAt,
                    COUNT(dl.department_id) AS DepartmentCount
                FROM "DirectoryService".location l
                LEFT JOIN "DirectoryService".department_locations dl ON dl.location_id = l.id
                {whereClause}
                GROUP BY l.id, l.name, l.street, l.house_number, l.city, l.created_at
                {havingClause}
            )
            SELECT Id,
                    Name,
                    Address,
                    CreatedAt,
                    DepartmentCount,
                   COUNT(*) OVER() AS TotalCount
            FROM filtered_locations
            ORDER BY {query.SortBy} {query.SortDir}
            LIMIT @PageSize OFFSET @Offset;
            """;

        using var connection = connectionFactory.Create();
        var queryResult = (await connection.QueryAsync<LocationListItemRaw>(sql, parameters)).ToList();

        var totalCount = queryResult.Count > 0 ? (int)queryResult[0].TotalCount : 0;
        var items = queryResult
            .Select(r => new LocationListItemDto(r.Id, r.Name, r.Address, r.CreatedAt, (int)r.DepartmentCount))
            .ToList();

        var result = new PagedResult<LocationListItemDto>(items, totalCount, query.Page, query.PageSize);
        
        return Result.Success<PagedResult<LocationListItemDto>, Errors>(result);
    }

    private class LocationListItemRaw
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public long DepartmentCount { get; set; }
        public long TotalCount { get; set; }
    }
}

