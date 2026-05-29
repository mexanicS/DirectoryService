using DirectoryService.Application.Validation;
using DirectoryService.Domain.Positions;
using FluentValidation;

namespace DirectoryService.Application.DirectoryServiceManagement.Positions.Update;

public class UpdatePositionValidator : AbstractValidator<UpdatePositionCommand>
{
    public UpdatePositionValidator()
    {
        RuleFor(x => x.Name)
            .MustBeValueObject(PositionName.Create);
        
        RuleFor(x => x.Description)
            .MustBeValueObject(Description.Create);
        
        RuleFor(x => x.PositionId)
            .NotEmpty().WithMessage("PositionId is required");

    }
}