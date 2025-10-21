using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using SharedKernel;

namespace DirectoryService.Domain.Locations;

public record Address
{
    public string City { get; private set; } = null!;

    public string Street { get; private set; } = null!;

    public string HouseNumber { get; private set; } = null!;

    public string? ZipCode { get; private set; }

    public override string ToString()
    {
        return $"{Street}, {HouseNumber}, {City}{(ZipCode != null ? ", " + ZipCode : "")}";
    }

    public Address(string city, string street, string houseNumber, string? zipCode)
    {
        City = city;
        Street = street;
        HouseNumber = houseNumber;
        ZipCode = zipCode;
    }
        
    public static Result<Address, Error> Create(string city, string street, string houseNumber, string? zipCode)
    {
        if (string.IsNullOrWhiteSpace(city))
            return GeneralErrors.ValueIsRequired(nameof(City));
        
        if(city.Length > Constants.Address.MAX_LENGTH_ADDRESS_CITY)
            return GeneralErrors.ValueIsMustBeLess(Constants.Address.MAX_LENGTH_ADDRESS_CITY, nameof(City));
            
        if (string.IsNullOrWhiteSpace(street))
            return GeneralErrors.ValueIsInvalid(nameof(Street));

        if (street.Length > Constants.Address.MAX_LENGTH_ADDRESS_STREET)
            return GeneralErrors.ValueIsMustBeLess(Constants.Address.MAX_LENGTH_ADDRESS_STREET, nameof(Street));
            
        if (string.IsNullOrWhiteSpace(houseNumber))
            return GeneralErrors.ValueIsInvalid(nameof(HouseNumber));
        
        if(houseNumber.Length > Constants.Address.MAX_LENGTH_ADDRESS_HOUSE_NUMBER)
            return GeneralErrors.ValueIsMustBeLess(Constants.Address.MAX_LENGTH_ADDRESS_HOUSE_NUMBER, nameof(HouseNumber));
            
        if (zipCode?.Length > Constants.Address.MAX_LENGTH_ADDRESS_ZIP_CODE)
            return GeneralErrors.ValueIsMustBeLess(Constants.Address.MAX_LENGTH_ADDRESS_ZIP_CODE, nameof(ZipCode));

        return new Address(city, street, houseNumber, zipCode);
    }
}