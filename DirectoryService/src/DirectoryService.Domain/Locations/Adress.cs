using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Locations;

public record Address
{
    public const int MAX_LENGTH = 200;
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
        if (string.IsNullOrWhiteSpace(city) || city.Length > MAX_LENGTH)
            return"city is invalid";
            
        if (string.IsNullOrWhiteSpace(street) || street.Length > MAX_LENGTH)
            return "street is invalid";
            
        if (string.IsNullOrWhiteSpace(houseNumber) || houseNumber.Length > MAX_LENGTH)
            return "house is invalid";
            
        if (street.Length > MAX_LENGTH)
            return "zipCode is invalid";
            
        return new Address(city, street, houseNumber, zipCode);
    }
}