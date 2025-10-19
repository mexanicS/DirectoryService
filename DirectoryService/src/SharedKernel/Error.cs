namespace SharedKernel;

public class Error
{
    public string Code { get; }
    
    public string Message { get; }
    
    public ErrorType Type { get; }
    
    public string? InvalidField { get; }

    private const string SEPARATOR = "||";
    
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
    
    public string Serialize()
    {
        return string.Join(SEPARATOR, Code, Message, Type);
    }
    
    public static Error Deserialize(string serialized)
    {
        var parts = serialized.Split(SEPARATOR);
        
        if(parts.Length < 2 )
            throw new FormatException("Invalid serialized format.");

        if (Enum.TryParse<ErrorType>(parts[2], out var type) == false)
            throw new FormatException("Invalid serialized format.");
        
        
        return new Error(parts[0], parts[1], type);
    }
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