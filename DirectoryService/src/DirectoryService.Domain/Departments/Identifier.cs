namespace DirectoryService.Domain.Departments;

public record Identifier
{
    public string Value { get; }

    private Identifier(string value)
    {
        Value = value;
    }
}