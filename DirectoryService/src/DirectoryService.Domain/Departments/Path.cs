using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Departments;

public record Path
{
    public string Value { get; }

    private Path(string value)
    {
        Value = value;
    }

    public static Result<Path, string> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length > Constants.MAX_LENGTH_DEPARTMENT_PATH)
        {
            return "Path is invalid";
        }

        return new Path(value);
    }
    
}