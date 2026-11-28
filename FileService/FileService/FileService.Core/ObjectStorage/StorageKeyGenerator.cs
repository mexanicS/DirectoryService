using CSharpFunctionalExtensions;
using FileService.Domain.MediaAssets;
using FileService.Domain.StorageReferences;
using SharedKernel;

namespace FileService.Core.ObjectStorage;

public static class StorageKeyGenerator
{
    public static Result<StorageKey, Error> CreateOriginal(MediaAssetId? assetId)
    {
        if (assetId is null)
        {
            return GeneralErrors.ValueIsRequired(nameof(MediaAssetId));
        }

        return StorageKey.Create($"originals/{assetId.Value:N}");
    }
}