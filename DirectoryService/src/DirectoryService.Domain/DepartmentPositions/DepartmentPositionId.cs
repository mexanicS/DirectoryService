namespace DirectoryService.Domain.DepartmentPositions;

public record DepartmentPositionId(Guid Value)
{
    public static implicit operator Guid(DepartmentPositionId departmentPositionId)
    {
        return departmentPositionId.Value;
    }
    
    public static DepartmentPositionId Create(Guid id) => new(id);
}