using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;

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
        
    public static Result<Address, string> Create(string city, string street, string houseNumber, string? zipCode)
    {
        //TODO когда добавлю класс Error сделать сбор ошибок
        if (string.IsNullOrWhiteSpace(city) || city.Length > Constants.Address.MAX_LENGTH_ADDRESS_CITY)
            return"city is invalid";
            
        if (string.IsNullOrWhiteSpace(street) || street.Length > Constants.Address.MAX_LENGTH_ADDRESS_STREET)
            return "street is invalid";
            
        if (string.IsNullOrWhiteSpace(houseNumber) || houseNumber.Length > Constants.Address.MAX_LENGTH_ADDRESS_HOUSE_NUMBER)
            return "house is invalid";
            
        if (street.Length > Constants.Address.MAX_LENGTH_ADDRESS_ZIP_CODE)
            return "zipCode is invalid";
            
        return new Address(city, street, houseNumber, zipCode);
    }
}