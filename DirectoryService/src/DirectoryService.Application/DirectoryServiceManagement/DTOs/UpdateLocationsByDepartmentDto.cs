using System;

namespace DirectoryService.Application.DirectoryServiceManagement.DTOs;

public record UpdateLocationsByDepartmentDto(
    Guid DepartmentId, 
    Guid[] LocationIds);