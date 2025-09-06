namespace DirectoryService.Domain.Locations;

public record LocationName
{
    public string Value { get; }

    private LocationName(string value)
    {
        Value = value;
    }
}