using System;

namespace DirectoryService.Application.DirectoryServiceManagement.Departments.RemovePositionFromDepartment;

public record RemovePositionFromDepartmentCommand(
    Guid DepartmentId, 
    Guid PositionId);