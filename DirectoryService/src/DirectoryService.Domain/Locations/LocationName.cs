using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Locations;

public record LocationName
{
    private const int MIN_LENGTH = 3;
    
    private const int MAX_LENGTH = 120;
    
    public string Value { get; }

    private LocationName(string value)
    {
        Value = value;
    }
    
    public static Result<LocationName, string> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length < MIN_LENGTH ||
            value.Length > MAX_LENGTH)
        {
            return "LocationName name is invalid";
        }
        return new LocationName(value);
    }
}