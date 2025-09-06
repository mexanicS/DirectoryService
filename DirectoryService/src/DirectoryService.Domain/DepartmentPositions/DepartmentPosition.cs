using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;

namespace DirectoryService.Domain.DepartmentPositions;

public class DepartmentPosition
{
    public DepartmentPositionId Id { get; init; }
    
    public DepartmentId  DepartmentId { get; init; }
    
    public PositionId  PositionId { get; init; }
}