using System;

namespace DirectoryService.Application.DirectoryServiceManagement.Departments.AddPositionToDepartment;

public record AddPositionToDepartmentCommand(
    Guid DepartmentId, 
    Guid PositionId);