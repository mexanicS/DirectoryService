using FluentValidation;

namespace DirectoryService.Application.DirectoryServiceManagement.DepartmentTree.GetNodesByName;

public class GetNodesByNameValidator : AbstractValidator<GetNodesByNameHandlerCommand>
{
    public GetNodesByNameValidator()
    {
        RuleFor(x => x.DepartmentName)
            .NotNull()
            .Must(s => !string.IsNullOrWhiteSpace(s))
            .WithMessage("Поисковый запрос не может быть пустой строкой.")
            .MinimumLength(2)
            .WithMessage("Длина поискового запроса должна быть не менее 2 символов.");
    }
}