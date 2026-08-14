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

        RuleFor(x => x.ParentId)
            .Must((command, parentId) => parentId != command.DepartmentId)
            .When(command => command.ParentId.HasValue && command.ParentId != Guid.Empty)
            .WithMessage("A department cannot be its own parent.")
            .WithErrorCode("department.move.parent_is_self");
    }
}
