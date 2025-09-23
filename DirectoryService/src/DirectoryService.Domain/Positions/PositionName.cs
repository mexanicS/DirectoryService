using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using SharedKernel;

namespace DirectoryService.Domain.Positions;

public record PositionName
{
    public string Value { get; }

    private PositionName(string value)
    {
        Value = value;
    }
    
    public static Result<PositionName, Error> Create(string value)
    {
        if (value.Length < Constants.MIN_LENGTH_POSITION_NAME ||
            value.Length > Constants.MAX_LENGTH_POSITION_NAME)
        {
            return GeneralErrors.ValueIsInvalid(nameof(PositionName));
        }
        return new PositionName(value);
    }
}