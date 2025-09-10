using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Positions;

public record Description
{
    public string Value { get; }

    private Description(string value)
    { 
        Value = value;
    }
    
    public static Result<Description, string> Create(string value)
    {
        if (value.Length < Constants.MAX_LENGTH_DESCRIPTION)
        {
            return "Description name is invalid";
        }
        return new Description(value);
    }
}