namespace DirectoryService.Application.DirectoryServiceManagement.DepartmentTree.GetNodesByName;

public record GetNodesByNameHandlerCommand(
    string? DepartmentName);