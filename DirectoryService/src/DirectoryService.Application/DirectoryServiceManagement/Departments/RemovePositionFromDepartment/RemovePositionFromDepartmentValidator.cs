using FluentValidation;

namespace DirectoryService.Application.DirectoryServiceManagement.Departments.RemovePositionFromDepartment;

public class RemovePositionFromDepartmentValidator : AbstractValidator<RemovePositionFromDepartmentCommand>
{
    public RemovePositionFromDepartmentValidator()
    {
        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("DepartmentId обязателен");
        
        RuleFor(x => x.PositionId)
            .NotEmpty().WithMessage("PositionId обязателен");
    }
}