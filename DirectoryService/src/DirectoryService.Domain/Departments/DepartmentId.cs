namespace DirectoryService.Domain.Departments;

public record DepartmentId(Guid Value)
{
    public static implicit operator Guid(DepartmentId departmentId)
    {
        return departmentId.Value;
    }
}