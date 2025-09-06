using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Departments;

public record DepartmentName
{
    public string Value { get; }
    
    private DepartmentName(string value)
    { 
         Value = value;
    }

    public static Result<DepartmentName> Create(DepartmentName value)
    {
        return new DepartmentName(value);
    }
}