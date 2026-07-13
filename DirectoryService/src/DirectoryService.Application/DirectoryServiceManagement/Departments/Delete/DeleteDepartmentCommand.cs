using System;

namespace DirectoryService.Application.DirectoryServiceManagement.Departments.Delete;

public record DeleteDepartmentCommand(
    Guid DepartmentId);