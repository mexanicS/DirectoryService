using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using SharedKernel;

namespace DirectoryService.Domain.DepartmentPositions;

public sealed class DepartmentPosition
{
    private DepartmentPosition()
    {
    }
    
    private DepartmentPosition(DepartmentId departmentId, PositionId positionId)
    {
        DepartmentId = departmentId;
        PositionId = positionId;
    }
    
    public DepartmentPositionId Id { get; init; }
    
    public DepartmentId  DepartmentId { get; init; }
    
    public PositionId  PositionId { get; init; }
    
    public static Result<DepartmentPosition, Error> Create(DepartmentId departmentId, PositionId positionId)
    {
        return new DepartmentPosition(departmentId, positionId);
    }
}