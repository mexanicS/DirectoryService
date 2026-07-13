using System.Linq;
using DirectoryService.Application.DirectoryServiceManagement.DTOs;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Positions;
using FluentValidation;

namespace DirectoryService.Application.DirectoryServiceManagement.Positions.Create;

public class CreatePositionValidator : AbstractValidator<CreatePositionDto>
{
    public CreatePositionValidator()
    {
        RuleFor(x => x.Name)
            .MustBeValueObject(PositionName.Create);
        
        RuleFor(x => x.Description)
            .MustBeValueObject(Description.Create);
        
        RuleFor(x => x.DepartmentIds)
            .NotEmpty().WithMessage("Список подразделений не может быть пустым")
            .Must(locations => locations.Distinct().Count() == locations.Length)
            .WithMessage("Список подразделений содержит дубликаты");
    }
}