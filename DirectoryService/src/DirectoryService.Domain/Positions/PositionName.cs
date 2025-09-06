namespace DirectoryService.Domain.Positions;

public record PositionName
{
    public string Value { get; }

    private PositionName(string value)
    {
        Value = value;
    }
}