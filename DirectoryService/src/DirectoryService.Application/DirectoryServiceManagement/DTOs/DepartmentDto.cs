using DirectoryService.Contract;

namespace DirectoryService.Application.DirectoryServiceManagement.DTOs;


public record GetDepartmentsDto(IReadOnlyList<DepartmentResponse> Departments);