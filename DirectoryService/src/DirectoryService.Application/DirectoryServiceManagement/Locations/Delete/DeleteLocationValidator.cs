using FluentValidation;

namespace DirectoryService.Application.DirectoryServiceManagement.Locations.Delete;

public class DeleteLocationValidator : AbstractValidator<DeleteLocationCommand>
{
    public DeleteLocationValidator()
    {
        RuleFor(x => x.LocationId)
            .NotEmpty().WithMessage("Идентификатор не может отсутствовать");
    }
}