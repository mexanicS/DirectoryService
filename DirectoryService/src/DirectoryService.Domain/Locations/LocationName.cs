using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using SharedKernel;

namespace DirectoryService.Domain.Locations;

public record LocationName
{
    public string Value { get; }

    private LocationName(string value)
    {
        Value = value;
    }
    
    public static Result<LocationName, Error> Create(string value)
    {
        if(string.IsNullOrWhiteSpace(value))
            return GeneralErrors.ValueIsRequired(nameof(LocationName));
            
        if (value.Length < Constants.MIN_LENGTH_LOCATION_NAME ||
            value.Length > Constants.MAX_LENGTH_LOCATION_NAME)
        {
            return GeneralErrors.ValueIsMustBeBetween(
                Constants.MIN_LENGTH_LOCATION_NAME,
                Constants.MAX_LENGTH_LOCATION_NAME,
                nameof(LocationName));
        }
        
        return new LocationName(value);
    }
}