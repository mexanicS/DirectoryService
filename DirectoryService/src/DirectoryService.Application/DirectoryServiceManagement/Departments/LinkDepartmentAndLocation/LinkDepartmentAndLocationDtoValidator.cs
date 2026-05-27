using FluentValidation;

namespace DirectoryService.Application.DirectoryServiceManagement.Departments.LinkDepartmentAndLocation;

public class LinkDepartmentAndLocationDtoValidator : AbstractValidator<DepartmentAndLocationCommand>
{
    public LinkDepartmentAndLocationDtoValidator()
    {
        RuleFor(x => x.DepartmentId)
            .NotNull().WithMessage("Идентификатор отдела не может отсутстововать");
        
        RuleFor(x => x.LocationId)
            .NotNull().WithMessage("Идентификатор локации не может отсутстововать");
    }
}