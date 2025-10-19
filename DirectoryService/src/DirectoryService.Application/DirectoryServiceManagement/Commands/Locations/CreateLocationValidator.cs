using DirectoryService.Application.DirectoryServiceManagement.DTOs;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;
using FluentValidation;

namespace DirectoryService.Application.DirectoryServiceManagement.Commands.Locations;

public class CreateLocationValidator : AbstractValidator<CreateLocationDto>
{
    public CreateLocationValidator()
    {
        RuleFor(x => x.LocationName)
            .MustBeValueObject(LocationName.Create);
        
        RuleFor(x => x.Address)
            .MustBeValueObject(a => Address.Create(
                a.City,
                a.Street,
                a.HouseNumber,
                a.ZipCode));
        
        RuleFor(x => x.Timezone)
            .MustBeValueObject(Timezone.Create);
    }
}