using CSharpFunctionalExtensions;
using FileService.Domain.Shared;
using SharedKernel;

namespace FileService.Domain.StorageReferences;

public record Sha256Checksum
{
    private Sha256Checksum(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<Sha256Checksum, Error> Create(string? value)
    {
        if (value is null || value.Length != Constants.SHA256_LENGTH ||
            !value.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F'))
        {
            return Error.Validation("file.checksum.invalid", "SHA-256 должен содержать 64 hex-символа.");
        }

        return new Sha256Checksum(value.ToLowerInvariant());
    }
}