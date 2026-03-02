using System.Linq;
using DirectoryService.Application.DirectoryServiceManagement.DTOs;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using FluentValidation;

namespace DirectoryService.Application.DirectoryServiceManagement.Departments.Create;

public class CreateDepartmentValidator : AbstractValidator<CreateDepartmentDto>
{
    public CreateDepartmentValidator()
    {
        RuleFor(x => x.Name)
            .MustBeValueObject(DepartmentName.Create);

        RuleFor(x => x.Identifier)
            .MustBeValueObject(Identifier.Create);

        RuleFor(x => x.LocationIds)
            .NotEmpty().WithMessage("Список локаций не может быть пустым")
            .Must(locations => locations.Distinct().Count() == locations.Length)
            .WithMessage("Список локаций содержит дубликаты");
    }
}