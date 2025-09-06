namespace DirectoryService.Domain.Departments;

public record Path
{
    public string Value { get; }

    private Path(string value)
    {
        Value = value;
    }
}