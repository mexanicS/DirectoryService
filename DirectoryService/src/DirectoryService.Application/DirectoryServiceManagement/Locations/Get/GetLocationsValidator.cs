using System;
using System.Collections.Generic;
using DirectoryService.Contract;

using FluentValidation;

namespace DirectoryService.Application.DirectoryServiceManagement.Locations.Get;

public class GetLocationsValidator : AbstractValidator<GetLocationsQuery>
{
    private static readonly HashSet<string> _allowedSortFields = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(LocationListItemDto.Name),
        nameof(LocationListItemDto.CreatedAt),
        nameof(LocationListItemDto.DepartmentCount),
    };

    private static readonly HashSet<string> _allowedDirections = new(StringComparer.OrdinalIgnoreCase)
    {
        "asc", "desc"
    };

    public GetLocationsValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(100);

        RuleFor(x => x.MinDepartmentCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinDepartmentCount.HasValue);

        RuleFor(x => x.SortBy)
            .Must(sortBy => string.IsNullOrEmpty(sortBy) || _allowedSortFields.Contains(sortBy))
            .WithMessage($"Сортировка возможна только по полям: {string.Join(", ", _allowedSortFields)}");

        RuleFor(x => x.SortDir)
            .Must(dir => string.IsNullOrEmpty(dir) || _allowedDirections.Contains(dir))
            .WithMessage("Направление сортировки должно быть 'asc' или 'desc'");
    }
}
