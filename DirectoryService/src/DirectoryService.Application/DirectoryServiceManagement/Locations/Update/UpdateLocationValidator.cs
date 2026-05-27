using DirectoryService.Application.DirectoryServiceManagement.DTOs;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Locations;
using FluentValidation;

namespace DirectoryService.Application.DirectoryServiceManagement.Locations.Update;

public class UpdateLocationValidator : AbstractValidator<UpdateLocationCommand>
{
    public UpdateLocationValidator()
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
        
        RuleFor(x => x.LocationId)
            .NotNull().WithMessage("Идентификатор не может отсутстововать");
    }
}