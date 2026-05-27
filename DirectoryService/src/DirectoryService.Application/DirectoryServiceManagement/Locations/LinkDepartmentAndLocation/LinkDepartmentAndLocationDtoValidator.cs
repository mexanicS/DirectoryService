using FluentValidation;

namespace DirectoryService.Application.DirectoryServiceManagement.Locations.LinkDepartmentAndLocation;

public class LinkDepartmentAndLocationDtoValidator : AbstractValidator<LinkDepartmentAndLocationCommand>
{
    public LinkDepartmentAndLocationDtoValidator()
    {
        RuleFor(x => x.DepartmentId)
            .NotNull().WithMessage("Идентификатор отдела не может отсутстововать");
        
        RuleFor(x => x.LocationId)
            .NotNull().WithMessage("Идентификатор локации не может отсутстововать");
    }
}