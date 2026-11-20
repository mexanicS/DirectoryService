using FileService.Domain.MediaAssets;
using FileService.Domain.Shared;

namespace FileService.UnitTests.MediaAssets;

public class FilePolicyTests
{
    public static IEnumerable<object[]> Policies()
    {
        yield return [MediaKind.IMAGE, UsageType.AVATAR, "image/jpeg", Constants.FileSize.MAX_AVATAR_SIZE];
        yield return [MediaKind.IMAGE, UsageType.COURSE_COVER, "image/png", Constants.FileSize.MAX_COURSE_COVER_SIZE];
        yield return [MediaKind.IMAGE, UsageType.MARKDOWN_IMAGE, "image/webp", Constants.FileSize.MAX_MARKDOWN_IMAGE_SIZE];
        yield return [MediaKind.DOCUMENT, UsageType.ATTACHMENT, "application/pdf", Constants.FileSize.MAX_ATTACHMENT_SIZE];
        yield return [MediaKind.VIDEO, UsageType.LESSON_VIDEO, "video/mp4", Constants.FileSize.MAX_LESSON_VIDEO_SIZE];
        yield return [MediaKind.IMAGE, UsageType.PREVIEW, "image/jpeg", Constants.FileSize.MAX_PREVIEW_SIZE];
    }

    [Theory]
    [MemberData(nameof(Policies))]
    public void Validate_AllowsMaximum_AndRejectsOneByteAbove(
        MediaKind kind, UsageType usageType, string mime, long maximum)
    {
        var type = MimeType.Create(mime).Value;
        var valid = FilePolicy.Default.Validate(kind, usageType, type, FileSize.Create(maximum).Value);
        var invalid = FilePolicy.Default.Validate(kind, usageType, type, FileSize.Create(maximum + 1).Value);

        Assert.True(valid.IsSuccess);
        Assert.True(invalid.IsFailure);
        Assert.Equal("file.size_exceeded", invalid.Error.Code);
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("image/webp")]
    public void Validate_AllowsSupportedImages(string mime)
    {
        Assert.True(FilePolicy.Default.Validate(MediaKind.IMAGE, UsageType.AVATAR,
            MimeType.Create(mime).Value, FileSize.Create(1).Value).IsSuccess);
    }

    [Theory]
    [InlineData("application/pdf")]
    [InlineData("text/plain")]
    [InlineData("application/msword")]
    [InlineData("application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    public void Validate_AllowsSupportedDocuments(string mime)
    {
        Assert.True(FilePolicy.Default.Validate(MediaKind.DOCUMENT, UsageType.ATTACHMENT,
            MimeType.Create(mime).Value, FileSize.Create(1).Value).IsSuccess);
    }

    [Theory]
    [InlineData("video/mp4")]
    [InlineData("video/webm")]
    public void Validate_AllowsSupportedVideos(string mime)
    {
        Assert.True(FilePolicy.Default.Validate(MediaKind.VIDEO, UsageType.LESSON_VIDEO,
            MimeType.Create(mime).Value, FileSize.Create(1).Value).IsSuccess);
    }

    [Theory]
    [InlineData(0, 1, "image/png", "file.kind.invalid")]
    [InlineData(99, 1, "image/png", "file.kind.invalid")]
    [InlineData(1, 0, "image/png", "file.usage.invalid")]
    [InlineData(1, 99, "image/png", "file.usage.invalid")]
    [InlineData(3, 1, "video/mp4", "file.kind_mismatch")]
    [InlineData(1, 1, "video/mp4", "file.mime_not_allowed")]
    [InlineData(1, 1, "image/svg+xml", "file.mime_not_allowed")]
    [InlineData(2, 4, "application/x-custom", "file.mime_not_allowed")]
    public void Validate_RejectsUnknownAndIncompatibleTypes(
        int kind, int usageType, string mime, string expectedCode)
    {
        var result = FilePolicy.Default.Validate((MediaKind)kind, (UsageType)usageType,
            MimeType.Create(mime).Value, FileSize.Create(1).Value);

        Assert.True(result.IsFailure);
        Assert.Equal(expectedCode, result.Error.Code);
    }

    [Fact]
    public void Create_ValidatesEveryConfiguredLimit()
    {
        Assert.True(FilePolicy.Create(maxAvatarSize: 0).IsFailure);
        Assert.True(FilePolicy.Create(maxCourseCoverSize: -1).IsFailure);
        Assert.True(FilePolicy.Create(maxMarkdownImageSize: 0).IsFailure);
        Assert.True(FilePolicy.Create(maxAttachmentSize: 0).IsFailure);
        Assert.True(FilePolicy.Create(maxLessonVideoSize: 0).IsFailure);
        Assert.True(FilePolicy.Create(maxPreviewSize: 0).IsFailure);
    }

    [Fact]
    public void Validate_UsesConfiguredLimit_WithoutChangingDefaultPolicy()
    {
        var policy = FilePolicy.Create(maxAvatarSize: 1).Value;
        var mime = MimeType.Create("image/png").Value;
        var size = FileSize.Create(2).Value;

        Assert.True(policy.Validate(MediaKind.IMAGE, UsageType.AVATAR, mime, size).IsFailure);
        Assert.True(FilePolicy.Default.Validate(MediaKind.IMAGE, UsageType.AVATAR, mime, size).IsSuccess);
        Assert.True(policy.Validate(MediaKind.IMAGE, UsageType.AVATAR, null, size).IsFailure);
        Assert.True(policy.Validate(MediaKind.IMAGE, UsageType.AVATAR, mime, null).IsFailure);
    }
}