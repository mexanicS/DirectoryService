using CSharpFunctionalExtensions;
using SharedKernel;

namespace FileService.Domain.MediaAssets;

public record UploaderId
{
    private UploaderId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static Result<UploaderId, Error> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Error.Validation("file.uploader_id.empty", "Идентификатор автора не может быть пустым.");
        }

        return new UploaderId(value);
    }

    public static implicit operator Guid(UploaderId id) => id.Value;
}