using CSharpFunctionalExtensions;
using SharedKernel;

namespace FileService.Domain.MediaAssets;

public record FileSize
{
    private FileSize(long value)
    {
        Value = value;
    }

    public long Value { get; }

    public static Result<FileSize, Error> Create(long value)
    {
        if (value <= 0)
        {
            return Error.Validation("file.size.invalid", "Размер файла должен быть больше нуля.");
        }

        return new FileSize(value);
    }
}