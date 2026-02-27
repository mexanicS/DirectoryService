using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using SharedKernel;

namespace DirectoryService.Domain.Departments;

public record Path
{
    private const char SEPARATOR = '/';
    public string Value { get; }

    private Path(string value)
    {
        Value = value;
    }

    public static Result<Path, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) )
        {
            return GeneralErrors.ValueIsRequired(nameof(Path));
        }

        if (value.Length > Constants.MAX_LENGTH_DEPARTMENT_PATH)
        {
            return GeneralErrors.ValueIsMustBeLess(
                Constants.MAX_LENGTH_DEPARTMENT_PATH, 
                nameof(Path));
        }

        return new Path(value);
    }
    
    public static Result<Path, Error> CreateParent(Identifier identifier)
    {
        return Create(identifier.Value);
    }

    public Result<Path, Error> CreateChild(Identifier childIdentifier)
    {
        return Create(Value + SEPARATOR + childIdentifier.Value);
    }
}