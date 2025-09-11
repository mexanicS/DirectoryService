using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Locations;

public record LocationName
{
    public string Value { get; }

    private LocationName(string value)
    {
        Value = value;
    }
    
    public static Result<LocationName, string> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length < Constants.MIN_LENGTH_LOCATION_NAME ||
            value.Length > Constants.MAX_LENGTH_LOCATION_NAME)
        {
            return "LocationName name is invalid";
        }
        return new LocationName(value);
    }
}