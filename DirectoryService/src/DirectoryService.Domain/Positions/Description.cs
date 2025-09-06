namespace DirectoryService.Domain.Positions;

public record Description
{
    public string Value { get; }

    private Description(string value)
    {
        Value = value;
    }
}