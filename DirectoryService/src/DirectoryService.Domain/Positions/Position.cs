using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentPositions;

namespace DirectoryService.Domain.Positions;

public sealed class Position
{
    //EF Core
    private Position()
    {
    }

    private List<DepartmentPosition> _departmentPositions = [];
    
    public Position(
        PositionId positionId,
        PositionName name,
        Description description)
    {
        Id = positionId;
        Name = name;
        Description = description;
        IsActive = true;
        CreatedAt = DateTime.Now;
    }
    public PositionId Id { get; set; }
     
    public PositionName Name { get; set; }
    
    public Description Description { get; set; }

    public bool IsActive { get; set; }
    
    public DateTime CreatedAt { get; set; }
     
    public DateTime UpdatedAt { get; set; }
    
    public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departmentPositions;

    public static Result<Position> Create(
        PositionId positionId,
        PositionName name,
        Description description)
    {
        return new Position(positionId, name, description);
    }
}