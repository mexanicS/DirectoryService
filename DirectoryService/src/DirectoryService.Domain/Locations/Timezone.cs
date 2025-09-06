namespace DirectoryService.Domain.Locations;

public record Timezone
{
    public string Value { get; }

    private Timezone(string value)
    {
        Value = value;
    }
}