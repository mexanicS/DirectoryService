using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Locations;

public record Timezone
{
    public string Value { get; }

    private Timezone(string value)
    {
        Value = value;
    }

    public static Result<Timezone, string> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "Timezone is invalid";
        }

        return new Timezone(value);
    }
}