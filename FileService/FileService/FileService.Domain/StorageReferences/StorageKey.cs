using CSharpFunctionalExtensions;
using FileService.Domain.Shared;
using SharedKernel;

namespace FileService.Domain.StorageReferences;

public record StorageKey
{
    private StorageKey(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<StorageKey, Error> Create(string? value)
    {
        if (string.IsNullOrEmpty(value) || value.Length > Constants.MAX_LENGTH_STORAGE_KEY)
        {
            return Invalid();
        }

        var segments = value.Split('/');
        if (segments.SkipLast(1).Any(segment => !IsSegment(segment)))
        {
            return Invalid();
        }

        var nameParts = segments[^1].Split('.');
        if (nameParts.Length > 2 ||
            !IsSegment(nameParts[0]) ||
            (nameParts.Length == 2 && (nameParts[1].Length == 0 || !nameParts[1].All(IsLetterOrDigit))))
        {
            return Invalid();
        }

        return new StorageKey(value);
    }

    private static bool IsSegment(string value) =>
        value.Length > 0 && IsLetterOrDigit(value[0]) &&
        value.All(c => IsLetterOrDigit(c) || c is '_' or '-');

    private static bool IsLetterOrDigit(char character) =>
        character is >= 'a' and <= 'z' or >= '0' and <= '9';

    private static Error Invalid() =>
        Error.Validation("file.storage_key.invalid", "Ключ должен быть безопасным относительным ключом хранения.");
}