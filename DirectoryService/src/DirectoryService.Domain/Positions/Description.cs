using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using SharedKernel;

namespace DirectoryService.Domain.Positions;

public record Description
{
    public string Value { get; }

    private Description(string value)
    { 
        Value = value;
    }
    
    public static Result<Description, Error> Create(string value)
    {
        if (value.Length < Constants.MAX_LENGTH_DESCRIPTION)
        {
            return GeneralErrors.ValueIsInvalid(nameof(Description));
        }
        return new Description(value);
    }
}