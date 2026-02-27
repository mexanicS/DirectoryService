using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using SharedKernel;

namespace DirectoryService.Domain.Departments;

public record DepartmentName
{
    public string Value { get; }
    
    private DepartmentName(string value)
    { 
         Value = value;
    }

    public static Result<DepartmentName, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GeneralErrors.ValueIsRequired(nameof(DepartmentName));
        }

        if (value.Length < Constants.MIN_LENGTH_DEPARTMENT_NAME ||
            value.Length > Constants.MAX_LENGTH_DEPARTMENT_NAME)
        {
            return GeneralErrors.ValueIsMustBeBetween(
                Constants.MIN_LENGTH_DEPARTMENT_NAME,
                Constants.MAX_LENGTH_DEPARTMENT_NAME,
                nameof(DepartmentName));
        }
            
        return new DepartmentName(value);
    }
    
    public static implicit operator string(DepartmentName departmentName)
    {
        ArgumentNullException.ThrowIfNull(departmentName);
        
        return departmentName.Value;
    }
}