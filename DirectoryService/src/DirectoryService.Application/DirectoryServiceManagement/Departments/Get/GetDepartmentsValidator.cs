using System;
using System.Collections.Generic;
using DirectoryService.Contract;

using FluentValidation;

namespace DirectoryService.Application.DirectoryServiceManagement.Departments.Get;

public class GetDepartmentsValidator : AbstractValidator<GetDepartmentsQuery>
{
    private static readonly HashSet<string> _allowedSortFields = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(DepartmentResponse.Name),
        nameof(DepartmentResponse.CreatedAt)
    };

    private static readonly HashSet<string> _allowedDirections = new(StringComparer.OrdinalIgnoreCase)
    {
        "asc", "desc"
    };

    public GetDepartmentsValidator()
    {
        RuleFor(x => x.Pagination.Page).GreaterThan(0);
        RuleFor(x => x.Pagination.PageSize).GreaterThan(0).LessThanOrEqualTo(100);

        RuleFor(x => x.SortBy)
            .Must(sortBy => string.IsNullOrEmpty(sortBy) || _allowedSortFields.Contains(sortBy))
            .WithMessage($"Сортировка возможна только по полям: {string.Join(", ", _allowedSortFields)}");

        RuleFor(x => x.SortDir)
            .Must(dir => string.IsNullOrEmpty(dir) || _allowedDirections.Contains(dir))
            .WithMessage("Направление сортировки должно быть 'asc' или 'desc'");
    }
}