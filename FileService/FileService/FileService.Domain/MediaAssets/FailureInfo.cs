using CSharpFunctionalExtensions;
using FileService.Domain.Shared;
using SharedKernel;

namespace FileService.Domain.MediaAssets;

public record FailureInfo
{
    private FailureInfo(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public string Code { get; }

    public string Message { get; }

    public static Result<FailureInfo, Error> Create(string? code, string? message)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(message))
        {
            return Error.Validation("file.failure_info.invalid", "Нужны код и описание ошибки файла.");
        }

        code = code.Trim();
        message = message.Trim();
        if (code.Length > Constants.MAX_LENGTH_FAILURE_CODE ||
            message.Length > Constants.MAX_LENGTH_FAILURE_MESSAGE ||
            code[0] is not (>= 'a' and <= 'z' or >= '0' and <= '9') ||
            !code.All(c => c is >= 'a' and <= 'z' or >= '0' and <= '9' or '.' or '_' or '-'))
        {
            return Error.Validation("file.failure_info.invalid", "Причина ошибки имеет неверный формат.");
        }

        return new FailureInfo(code, message);
    }
}