using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using SharedKernel;

namespace DirectoryService.Domain.Departments;

public record Path
{
    public string Value { get; }

    private Path(string value)
    {
        Value = value;
    }

    public static Result<Path, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length > Constants.MAX_LENGTH_DEPARTMENT_PATH)
        {
            return GeneralErrors.ValueIsInvalid(nameof(Path));
        }

        return new Path(value);
    }
    
}