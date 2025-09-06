using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;

namespace DirectoryService.Domain.Departments;

public sealed class Department
{
    //EF Core
    private Department()
    {
    }

    private readonly List<Department> _departmentsChildren = [];
    
    private readonly List<DepartmentLocation> _departmentLocations = [];
    
    private readonly List<DepartmentPosition> _departmentPositions = [];

    private Department(
        DepartmentId departmentId,
        DepartmentName name, 
        Identifier identifier, 
        Path path)
    {
        Id = departmentId;
        Name = name;
        Identifier = identifier;
        Path = path;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }
     public DepartmentId Id { get; private set; }
     
     public DepartmentName Name { get; private set; }
     
     public Identifier Identifier { get; private set; }
     
     public DepartmentId? ParentId { get; private set; }
     
     public Path Path { get; private set; }
     
     public int Depth { get; private set; }
     
     public bool IsActive { get; private set; }
     
     public DateTime CreatedAt { get; private set; }
     
     public DateTime UpdatedAt { get; private set; }
     
     public IReadOnlyList<Department> DepartmentsChildren => _departmentsChildren;
     
     public IReadOnlyList<DepartmentLocation> DepartmentLocations => _departmentLocations;
     
     public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departmentPositions;

     public static Result<Department> Create(
         DepartmentId departmentId,
         DepartmentName name, 
         Identifier identifier, 
         Path path)
     {
         return new Department(departmentId, name, identifier, path);
     }
}