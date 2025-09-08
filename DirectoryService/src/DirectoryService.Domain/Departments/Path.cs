using CSharpFunctionalExtensions;

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
        if(string.IsNullOrWhiteSpace(value))
            return "Path cannot be null or empty";

        return new Path(value);
    }
    
}