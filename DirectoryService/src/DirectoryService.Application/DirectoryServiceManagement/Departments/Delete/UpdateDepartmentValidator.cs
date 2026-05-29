using FluentValidation;

namespace DirectoryService.Application.DirectoryServiceManagement.Departments.Delete;

public class DeleteDepartmentValidator : AbstractValidator<DeleteDepartmentCommand>
{
    public DeleteDepartmentValidator()
    {
        RuleFor(x => x.DepartmentId)
            .NotNull().WithMessage("Идентификатор не может отсутстововать");
    }
}    