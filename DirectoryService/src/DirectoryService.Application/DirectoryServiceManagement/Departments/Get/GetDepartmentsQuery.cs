namespace DirectoryService.Application.DirectoryServiceManagement.Departments.Get;

public record GetDepartmentsQuery(string Search,
    string SortBy,
    string DortDir,
    PaginationRequest);
    
    public record PaginationRequest(int Page, int PageSize);