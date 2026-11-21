namespace FileService.Domain.MediaAssets;

public enum AssetStatus
{
    AWAITING_UPLOAD = 1,
    UPLOADING = 2,
    UPLOADED = 3,
    READY = 4,
    FAILED = 5,
    DELETED = 6,
}