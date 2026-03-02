namespace DirectoryService.Domain.DepartmentLocations;

public record DepartmentLocationId(Guid Value)
{
    public static implicit operator Guid(DepartmentLocationId departmentLocationId)
    {
        return departmentLocationId.Value;
    }
    
    public static DepartmentLocationId Create(Guid id) => new DepartmentLocationId(id);
}