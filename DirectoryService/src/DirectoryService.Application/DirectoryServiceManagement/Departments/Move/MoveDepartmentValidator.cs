using FluentValidation;

namespace DirectoryService.Application.DirectoryServiceManagement.Departments.Move;

public sealed class MoveDepartmentValidator : AbstractValidator<MoveDepartmentCommand>
{
    public MoveDepartmentValidator()
    {
        RuleFor(x => x.DepartmentId)
            .NotEmpty()
            .WithMessage("DepartmentId must not be empty.")
            .WithErrorCode("department.id.invalid");

        RuleFor(x => x.ParentId)
            .Must(parentId => parentId is null || parentId != Guid.Empty)
            .WithMessage("ParentId must be null or a non-empty UUID.")
            .WithErrorCode("department.parent.id.invalid");
    }
}
