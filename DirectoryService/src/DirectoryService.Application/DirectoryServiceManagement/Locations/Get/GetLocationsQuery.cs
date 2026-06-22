namespace DirectoryService.Application.DirectoryServiceManagement.Locations.Get;

public record GetLocationsQuery(
    string? Search,
    int? MinDepartmentCount,
    string? SortBy,
    string? SortDir,
    int Page = 1,
    int PageSize = 20);
