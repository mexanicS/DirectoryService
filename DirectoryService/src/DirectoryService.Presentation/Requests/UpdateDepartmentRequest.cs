namespace DirectoryService.Presentation.Requests;

public record UpdateDepartmentRequest(
    string Name,    
    string Identifier);