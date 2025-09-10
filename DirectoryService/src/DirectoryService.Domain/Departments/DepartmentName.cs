using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Departments;

public record DepartmentName
{
    public string Value { get; }
    
    private DepartmentName(string value)
    { 
         Value = value;
    }

    public static Result<DepartmentName, string> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length < Constants.MIN_LENGTH_DEPARTMENT_NAME ||
            value.Length > Constants.MAX_LENGTH_DEPARTMENT_NAME)
        {
            return "DepartmentName name is invalid";
        }
        return new DepartmentName(value);
    }
    
    public static implicit operator string(DepartmentName departmentName)
    {
        ArgumentNullException.ThrowIfNull(departmentName);
        
        return departmentName.Value;
    }
}