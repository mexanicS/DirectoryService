using System;

namespace DirectoryService.Application.DirectoryServiceManagement.Departments.Update;

public record UpdateDepartmentCommand(
    Guid DepartmentId,
    string Name,
    string Identifier);