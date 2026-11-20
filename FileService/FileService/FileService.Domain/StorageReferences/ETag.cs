using CSharpFunctionalExtensions;
using FileService.Domain.Shared;
using SharedKernel;

namespace FileService.Domain.StorageReferences;

public record ETag
{
    private ETag(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<ETag, Error> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > Constants.MAX_LENGTH_ETAG || value.Any(char.IsControl))
        {
            return Error.Validation("file.etag.invalid", "ETag должен быть непустой строкой без управляющих символов.");
        }

        return new ETag(value);
    }
}