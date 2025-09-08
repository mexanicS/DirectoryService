using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Departments;

public record DepartmentName
{
    private const int MIN_LENGTH_TEXT = 3;
    
    private const int MAX_LENGTH_TEXT = 150;
    
    public string Value { get; }
    
    private DepartmentName(string value)
    { 
         Value = value;
    }

    public static Result<DepartmentName, string> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length < MIN_LENGTH_TEXT ||
            value.Length > MAX_LENGTH_TEXT)
        {
            return "DepartmentName name is invalid";
        }
        return new DepartmentName(value);
    }
}