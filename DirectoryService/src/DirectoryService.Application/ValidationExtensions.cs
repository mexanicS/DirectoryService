using FluentValidation.Results;
using SharedKernel;

namespace DirectoryService.Application;

public static class ValidationExtensions
{
    public static Errors ToErrors(this ValidationResult validationResult)
    {
        var errors = new List<Error>();
        
        foreach (var failure in validationResult.Errors)
        {
            var errorCode = failure.ErrorCode ?? $"Validation.{failure.PropertyName}";
            var error = Error.Validation(
                code: errorCode,
                message: failure.ErrorMessage,
                invalidField: failure.PropertyName);
            
            errors.Add(error);
        }
        
        return errors;
    }
}