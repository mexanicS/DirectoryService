using DirectoryService.Application.DirectoryServiceManagement.DTOs;
using DirectoryService.Domain.Shared;
using FluentValidation;

namespace DirectoryService.Application.DirectoryServiceManagement.Commands.Locations;

public class CreateLocationValidator : AbstractValidator<CreateLocationDto>
{
    public CreateLocationValidator()
    {
        RuleFor(x => x.LocationName)
            .NotNull()
            .WithMessage("Location name required.")
            .Length(Constants.MIN_LENGTH_LOCATION_NAME, Constants.MAX_LENGTH_LOCATION_NAME)
            .WithMessage($"Location name must be between {Constants.MIN_LENGTH_LOCATION_NAME} and {Constants.MAX_LENGTH_LOCATION_NAME} characters.");
        
        RuleFor(x => x.Address.City)
            .NotEmpty()
            .WithMessage("City is required.")
            .MaximumLength(Constants.Address.MAX_LENGTH_ADDRESS_CITY)
            .WithMessage($"City must be less than {Constants.Address.MAX_LENGTH_ADDRESS_CITY} characters.");
        
        RuleFor(x => x.Address.Street)
            .NotEmpty()
            .WithMessage("Street is required.")
            .MaximumLength(Constants.Address.MAX_LENGTH_ADDRESS_STREET)
            .WithMessage($"Street must be less than {Constants.Address.MAX_LENGTH_ADDRESS_STREET} characters.");
        
        RuleFor(x => x.Address.HouseNumber)
            .NotEmpty()
            .WithMessage("House number is required.")
            .MaximumLength(Constants.Address.MAX_LENGTH_ADDRESS_HOUSE_NUMBER)
            .WithMessage($"House number must be less than {Constants.Address.MAX_LENGTH_ADDRESS_HOUSE_NUMBER} characters.");
        
        RuleFor(x => x.Address.ZipCode)
            .MaximumLength(Constants.Address.MAX_LENGTH_ADDRESS_ZIP_CODE)
            .WithMessage($"Zip code must be less than {Constants.Address.MAX_LENGTH_ADDRESS_ZIP_CODE} characters.")
            .When(x => !string.IsNullOrEmpty(x.Address.ZipCode));

        RuleFor(x => x.Timezone)
            .NotNull()
            .WithMessage("Timezone required.");
    }
}