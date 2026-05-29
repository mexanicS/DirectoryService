using FluentValidation;

namespace DirectoryService.Application.DirectoryServiceManagement.Positions.Delete;

public class DeletePositionValidator : AbstractValidator<DeletePositionCommand>
{
    public DeletePositionValidator()
    {
        RuleFor(x => x.PositionId)
            .NotEmpty().WithMessage("PositionId is required");
    }
}