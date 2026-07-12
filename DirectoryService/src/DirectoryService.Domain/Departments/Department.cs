using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Positions;
using SharedKernel;

namespace DirectoryService.Domain.Departments;

public sealed class Department
{
    //EF Core
    private Department()
    {
    }

    private List<Department> _departmentsChildren = [];
    
    private List<DepartmentLocation> _departmentLocations = [];
    
    private List<DepartmentPosition> _departmentPositions = [];

    private Department(
        DepartmentId departmentId,
        DepartmentName name, 
        Identifier identifier, 
        Path path,
        IEnumerable<DepartmentLocation> departmentLocations,
        DepartmentDepth depth,
        DepartmentId? parentId = null)
    {
        Id = departmentId;
        Name = name;
        Identifier = identifier;
        Path = path;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        Depth = depth;
        _departmentLocations = departmentLocations.ToList();
        ParentId = parentId;
    }
     public DepartmentId Id { get; private set; }
     
     public DepartmentName Name { get; private set; }
     
     public Identifier Identifier { get; private set; }
     
     public DepartmentId? ParentId { get; private set; }
     
     public Path Path { get; private set; }
     
     public DepartmentDepth Depth { get; private set; }
     
     public bool IsActive { get; private set; }
     
     public DateTime CreatedAt { get; private set; }
     
     public DateTime? UpdatedAt { get; private set; }
     
     public DateTime? SoftDeletedAt { get; private set; }

     public bool IsDeleted { get; private set; }
     
     public IReadOnlyList<Department> DepartmentsChildren => _departmentsChildren;
     
     public IReadOnlyList<DepartmentLocation> DepartmentLocations => _departmentLocations;
     
     public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departmentPositions;

     public static Result<Department, Error> CreateParent(
         DepartmentName name,
         Identifier identifier,
         IEnumerable<DepartmentLocation> departmentLocations,
         DepartmentId departmentId)
     {
         var locations = departmentLocations.ToList();

         if (locations.Count == 0)
         {
             return Error.Validation("department.location", "Department locations should contain at least one location");
         }

         var path = Path.CreateParent(identifier);
         if (path.IsFailure)
         {
             return path.Error;
         }

         var departmentDepth = DepartmentDepth.Create(0).Value;

         return new Department(
             departmentId,
             name,
             identifier,
             path.Value,
             locations,
             departmentDepth);
     }

     public static Result<Department, Error> CreateChild(
         DepartmentName departmentName,
         Identifier identifier,
         Department departmentParent,
         IEnumerable<DepartmentLocation> departmentLocations,
         DepartmentId departmentId)
     {
         var path = departmentParent.Path.CreateChild(identifier);
         if (path.IsFailure)
         {
             return path.Error;
         }

         var locations = departmentLocations.ToList();
         if (locations.Count == 0)
         {
             return Error.Validation("department.location",
                 "Department locations should contain at least one location");
         }

         var departmentDepth = DepartmentDepth.Create(departmentParent.Depth.Value + 1).Value;
         
         return new Department(
             departmentId,
             departmentName,
             identifier,
             path.Value,
             locations,
             departmentDepth,
             departmentParent.Id);
     }
     
     public UnitResult<Error> AddPosition(Guid positionId)
     {
         var departmentPosition = _departmentPositions
             .FirstOrDefault(x => x.PositionId.Value == positionId);

         if (departmentPosition is null)
         {
             _departmentPositions.Add(DepartmentPosition.Create(Id, new PositionId(positionId)).Value);

             return Result.Success<Error>();
         }

         return GeneralErrors.AlreadyExist();
     }
     
     public Result UpdateMainInformation(DepartmentName departmentName,
         Identifier identifier)
     {
         Name = departmentName;
         Identifier = identifier;
         UpdatedAt = DateTime.UtcNow;

         return Result.Success();
     }
     public void MoveUpInHierarchy(string oldParentPath, string newParentPath, DepartmentId? newParentId)
     {
         var currentDepthValue = Depth.Value;
         if (currentDepthValue > 0)
         {
             Depth = DepartmentDepth.Create(currentDepthValue - 1).Value;
         }

         string currentPathValue = Path.Value;
         if (currentPathValue.StartsWith(oldParentPath))
         {
             string updatedPath = currentPathValue.Replace(oldParentPath, newParentPath);
             Path = Path.Create(updatedPath).Value;
         }

         if (ParentId == newParentId) 
         {
             ParentId = newParentId; 
         }
        
         UpdatedAt = DateTime.UtcNow;
     }
     
     public void UpdateParent(DepartmentId? newParentId)
     {
         ParentId = newParentId;
         UpdatedAt = DateTime.UtcNow;
     }
     
     public string GetParentPathValue()
     {
         if (ParentId is null || string.IsNullOrEmpty(Path.Value))
         {
             return string.Empty;
         }

         var lastDotIndex = Path.Value.LastIndexOf('/');
         return lastDotIndex == -1 ? string.Empty : Path.Value.Substring(0, lastDotIndex);
     }
     
     public UnitResult<Error> RemovePosition(Guid positionId)
     {
         var departmentPosition = _departmentPositions
             .FirstOrDefault(x => x.PositionId.Value == positionId);

         if (departmentPosition is null)
         {
             return GeneralErrors.NotFound(positionId, nameof(DepartmentPosition));
         }

         _departmentPositions.Remove(departmentPosition);

         return Result.Success<Error>();
     }

     public void SoftDelete()
     {
         IsDeleted = true;
         SoftDeletedAt = DateTime.UtcNow;
     }
}