using CSharpFunctionalExtensions;
using FileService.Domain.Shared;
using SharedKernel;

namespace FileService.Domain.MediaAssets;

public sealed class FilePolicy
{
    private readonly Dictionary<UsageType, Rule> _rules;

    private FilePolicy(
        long maxAvatarSize,
        long maxCourseCoverSize,
        long maxMarkdownImageSize,
        long maxAttachmentSize,
        long maxLessonVideoSize,
        long maxPreviewSize)
    {
        string[] imageTypes = ["image/jpeg", "image/png", "image/webp"];
        _rules = new Dictionary<UsageType, Rule>
        {
            [UsageType.AVATAR] = new(MediaKind.IMAGE, maxAvatarSize, imageTypes),
            [UsageType.COURSE_COVER] = new(MediaKind.IMAGE, maxCourseCoverSize, imageTypes),
            [UsageType.MARKDOWN_IMAGE] = new(MediaKind.IMAGE, maxMarkdownImageSize, imageTypes),
            [UsageType.ATTACHMENT] = new(MediaKind.DOCUMENT, maxAttachmentSize,
                ["application/pdf", "text/plain", "application/msword",
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document"]),
            [UsageType.LESSON_VIDEO] = new(MediaKind.VIDEO, maxLessonVideoSize, ["video/mp4", "video/webm"]),
            [UsageType.PREVIEW] = new(MediaKind.IMAGE, maxPreviewSize, imageTypes),
        };
    }

    public static FilePolicy Default { get; } = new(
        Constants.FileSize.MAX_AVATAR_SIZE,
        Constants.FileSize.MAX_COURSE_COVER_SIZE,
        Constants.FileSize.MAX_MARKDOWN_IMAGE_SIZE,
        Constants.FileSize.MAX_ATTACHMENT_SIZE,
        Constants.FileSize.MAX_LESSON_VIDEO_SIZE,
        Constants.FileSize.MAX_PREVIEW_SIZE);

    public static Result<FilePolicy, Error> Create(
        long maxAvatarSize = Constants.FileSize.MAX_AVATAR_SIZE,
        long maxCourseCoverSize = Constants.FileSize.MAX_COURSE_COVER_SIZE,
        long maxMarkdownImageSize = Constants.FileSize.MAX_MARKDOWN_IMAGE_SIZE,
        long maxAttachmentSize = Constants.FileSize.MAX_ATTACHMENT_SIZE,
        long maxLessonVideoSize = Constants.FileSize.MAX_LESSON_VIDEO_SIZE,
        long maxPreviewSize = Constants.FileSize.MAX_PREVIEW_SIZE)
    {
        if (maxAvatarSize <= 0 || maxCourseCoverSize <= 0 || maxMarkdownImageSize <= 0 ||
            maxAttachmentSize <= 0 || maxLessonVideoSize <= 0 || maxPreviewSize <= 0)
        {
            return Error.Validation("file.policy.invalid", "Лимиты размера должны быть положительными.");
        }

        return new FilePolicy(maxAvatarSize, maxCourseCoverSize, maxMarkdownImageSize,
            maxAttachmentSize, maxLessonVideoSize, maxPreviewSize);
    }

    public UnitResult<Error> Validate(MediaKind kind, UsageType usageType, MimeType? contentType, FileSize? size)
    {
        if (!Enum.IsDefined(kind))
        {
            return FileErrors.InvalidKind();
        }

        if (!_rules.TryGetValue(usageType, out var rule))
        {
            return FileErrors.InvalidUsage();
        }

        if (contentType is null || size is null)
        {
            return FileErrors.Required(contentType is null ? nameof(contentType) : nameof(size));
        }

        if (kind != rule.Kind)
        {
            return FileErrors.KindMismatch();
        }

        if (!rule.ContentTypes.Contains(contentType.Value, StringComparer.Ordinal))
        {
            return FileErrors.MimeNotAllowed();
        }

        if (size.Value > rule.MaxSize)
        {
            return FileErrors.SizeExceeded(rule.MaxSize);
        }

        return Result.Success<Error>();
    }

    private sealed record Rule(MediaKind Kind, long MaxSize, string[] ContentTypes);
}