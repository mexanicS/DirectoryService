using FluentValidation;

namespace DirectoryService.Application.DirectoryServiceManagement.Departments.AddPositionToDepartment;

public class AddPositionToDepartmentValidator : AbstractValidator<AddPositionToDepartmentCommand>
{
    public AddPositionToDepartmentValidator()
    {
        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("DepartmentId обязателен");
        
        RuleFor(x => x.PositionId)
            .NotEmpty().WithMessage("PositionId обязателен");
    }
} 