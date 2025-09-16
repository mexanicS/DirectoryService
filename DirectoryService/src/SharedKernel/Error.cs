namespace SharedKernel;

public class Error
{
    public string Code { get; }
    
    public string Message { get; }
    
    public ErrorType Type { get; }
    
    public string? InvalidField { get; }

    private Error(string code, string message, ErrorType type, string? invalidField = null)
    {
        Code = code;
        Message = message;
        Type = type;
        InvalidField = invalidField;
    }
    
    public static Error Validation(string code, string message, string? invalidField = null) =>
        new(code, message, ErrorType.VALIDATION, invalidField);

    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NOT_FOUND);

    public static Error Failure(string code, string message) => new(code, message, ErrorType.FAILURE);

    public static Error Conflict(string code, string message) => new(code, message, ErrorType.CONFLICT);

    public static Error Authentication(string code, string message) => new(code, message, ErrorType.AUTHENTICATION);

    public static Error Authorization(string code, string message) => new(code, message, ErrorType.AUTHORIZATION);
    
    public Errors ToErrors() => new([this]);
}

public enum ErrorType
{
    VALIDATION,
    NOT_FOUND,
    FAILURE,
    CONFLICT,
    AUTHENTICATION,
    AUTHORIZATION,
}