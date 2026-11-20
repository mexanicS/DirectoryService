using CSharpFunctionalExtensions;
using SharedKernel;

namespace FileService.Domain.MediaAssets;

public record MediaAssetId
{
    private MediaAssetId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static MediaAssetId New() => new(Guid.CreateVersion7());

    public static Result<MediaAssetId, Error> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Error.Validation("file.id.empty", "Идентификатор файла не может быть пустым.");
        }

        return new MediaAssetId(value);
    }

    public static implicit operator Guid(MediaAssetId id) => id.Value;
}