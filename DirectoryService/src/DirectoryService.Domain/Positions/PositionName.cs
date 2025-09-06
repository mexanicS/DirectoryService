using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Positions;

public record PositionName
{
    private const int MIN_LENGTH = 3;
    
    private const int MAX_LENGTH = 100;
    
    public string Value { get; }

    private PositionName(string value)
    {
        Value = value;
    }
    
    public static Result<PositionName, string> Create(string value)
    {
        if (value.Length < MIN_LENGTH ||
            value.Length > MAX_LENGTH)
        {
            return "PositionName name is invalid";
        }
        return new PositionName(value);
    }
}