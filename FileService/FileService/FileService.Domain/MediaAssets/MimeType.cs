using CSharpFunctionalExtensions;
using FileService.Domain.Shared;
using SharedKernel;

namespace FileService.Domain.MediaAssets;

public record MimeType
{
    private MimeType(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<MimeType, Error> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Error.Validation("file.mime.invalid", "MIME-тип обязателен.");
        }

        if (value.Any(char.IsControl))
        {
            return Error.Validation("file.mime.invalid", "MIME-тип не должен содержать управляющие символы.");
        }

        value = value.Trim();
        var parts = value.Split('/');
        if (value.Length > Constants.MAX_LENGTH_MIME_TYPE ||
            parts.Length != 2 ||
            parts.Any(part => part.Length == 0 || !part.All(IsTokenCharacter)))
        {
            return Error.Validation("file.mime.invalid", "Ожидается MIME-тип вида type/subtype без параметров.");
        }

        return new MimeType(value.ToLowerInvariant());
    }

    private static bool IsTokenCharacter(char character) =>
        char.IsAsciiLetterOrDigit(character) ||
        "!#$%&'+-.^_|~".Contains(character) ||
        character == '\u0060';
}