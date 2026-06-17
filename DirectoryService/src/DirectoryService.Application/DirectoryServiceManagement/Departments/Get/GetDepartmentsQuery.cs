namespace DirectoryService.Application.DirectoryServiceManagement.Departments.Get;

public record GetDepartmentsQuery(
    string? Search,
    string? SortBy,
    string? SortDir,
    PaginationRequest Pagination);
    
public record PaginationRequest(int Page = 1, int PageSize = 20);