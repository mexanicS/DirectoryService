using FluentValidation;

namespace DirectoryService.Application.DirectoryServiceManagement.Departments.UpdateLocations;

public class UpdateLocationsByDepartmentValidator : AbstractValidator<UpdateLocationsByDepartmentCommand>
{
    public UpdateLocationsByDepartmentValidator() 
    {
        RuleFor(x => x.LocationIds)
            .NotEmpty().WithMessage("Список локаций не может быть пустым")
            .Must(locations => locations.Distinct().Count() == locations.Length)
            .WithMessage("Список локаций содержит дубликаты");
    }
}