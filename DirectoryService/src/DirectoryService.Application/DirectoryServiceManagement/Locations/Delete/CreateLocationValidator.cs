using DirectoryService.Application.DirectoryServiceManagement.DTOs;
using DirectoryService.Application.DirectoryServiceManagement.Locations.Delete;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Locations;
using FluentValidation;

namespace DirectoryService.Application.DirectoryServiceManagement.Locations.Create;

public class DeleteLocationValidator : AbstractValidator<DeleteLocationCommand>
{
    public DeleteLocationValidator()
    {
        RuleFor(x => x.LocationId)
            .NotEmpty().WithMessage("Идентификатор не может отсутствовать");
    }
}