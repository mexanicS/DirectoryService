using CSharpFunctionalExtensions;
using FileService.Domain.Shared;
using SharedKernel;

namespace FileService.Domain.MediaAssets;

public record FileName
{
    private FileName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<FileName, Error> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Error.Validation("file.name.required", "Имя файла обязательно.");
        }

        if (value.Any(char.IsControl))
        {
            return Error.Validation("file.name.invalid", "Имя файла не должно содержать управляющие символы.");
        }

        value = value.Trim();
        if (value.Length > Constants.MAX_LENGTH_FILE_NAME)
        {
            return Error.Validation("file.name.too_long", "Имя файла слишком длинное.");
        }

        if (value is "." or ".." || value.Any(c => char.IsControl(c) || c is '/' or '\\'))
        {
            return Error.Validation("file.name.invalid", "Имя файла не должно содержать путь или управляющие символы.");
        }

        return new FileName(value);
    }
}