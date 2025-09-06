namespace DirectoryService.Domain.Locations;

public record Address
{
    public string Value { get; }

    private Address(string value)
    {
        Value = value;
    }
}