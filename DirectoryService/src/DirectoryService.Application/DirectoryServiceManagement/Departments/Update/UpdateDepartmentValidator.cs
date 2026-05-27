using DirectoryService.Application.DirectoryServiceManagement.DTOs;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using FluentValidation;

namespace DirectoryService.Application.DirectoryServiceManagement.Departments.Update;

public class UpdateDepartmentValidator : AbstractValidator<UpdateDepartmentDto>
{
    public UpdateDepartmentValidator()
    {
        RuleFor(x => x.Name)
            .MustBeValueObject(DepartmentName.Create);

        RuleFor(x => x.Identifier)
            .MustBeValueObject(Identifier.Create);

        RuleFor(x => x.DepartmentId)
            .NotNull().WithMessage("Идентификатор не может отсутстововать");
    }
}    