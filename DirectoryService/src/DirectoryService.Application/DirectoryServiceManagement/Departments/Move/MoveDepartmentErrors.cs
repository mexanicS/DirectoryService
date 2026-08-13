using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Departments.Move;

internal static class MoveDepartmentErrors
{
    public static Error DepartmentNotFound(Guid id) =>
        Error.NotFound("department.not_found", $"Department '{id}' was not found.");

    public static Error ParentNotFound(Guid id) =>
        Error.NotFound("department.parent.not_found", $"Parent department '{id}' was not found.");

    public static Error ParentIsSelf() =>
        Error.Validation("department.move.parent_is_self", "A department cannot be its own parent.", "parentId");

    public static Error Cycle() =>
        Error.Conflict("department.move.cycle", "A department cannot be moved into its own subtree.");

    public static Error ParentDeleted() =>
        Error.Conflict("department.move.parent_deleted", "A deleted department cannot be selected as parent.");
}
