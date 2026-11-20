using FileService.Domain.MediaAssets;
using FileService.Domain.Shared;
using FileService.Domain.StorageReferences;

namespace FileService.UnitTests.MediaAssets;

public class ValueObjectsTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(".")]
    [InlineData("..")]
    [InlineData("../avatar.png")]
    [InlineData("images/avatar.png")]
    [InlineData("images\\avatar.png")]
    [InlineData("avatar\n.png")]
    [InlineData("avatar.png\r\n")]
    public void FileName_WithInvalidValue_ReturnsError(string? value)
    {
        Assert.True(FileName.Create(value).IsFailure);
    }

    [Fact]
    public void FileName_PreservesCaseAndUnicode_AndAllowsMissingExtension()
    {
        Assert.Equal("Мой Аватар.PNG", FileName.Create(" Мой Аватар.PNG ").Value.Value);
        Assert.Equal("README", FileName.Create("README").Value.Value);
    }

    [Fact]
    public void FileName_ChecksLengthBoundary()
    {
        Assert.True(FileName.Create(new string('a', Constants.MAX_LENGTH_FILE_NAME)).IsSuccess);
        var invalid = FileName.Create(new string('a', Constants.MAX_LENGTH_FILE_NAME + 1));

        Assert.Equal("file.name.too_long", invalid.Error.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("image")]
    [InlineData("/png")]
    [InlineData("image/")]
    [InlineData("image/png/extra")]
    [InlineData("image /png")]
    [InlineData("image/png; charset=utf-8")]
    [InlineData("image/*")]
    [InlineData("*/*")]
    [InlineData("image/png,image/jpeg")]
    [InlineData("image/png\r\n")]
    [InlineData("изображение/png")]
    public void MimeType_WithInvalidValue_ReturnsDomainError(string? value)
    {
        var result = MimeType.Create(value);

        Assert.True(result.IsFailure);
        Assert.Equal("file.mime.invalid", result.Error.Code);
    }

    [Theory]
    [InlineData(" IMAGE/PNG ", "image/png")]
    [InlineData("Application/Vnd.Example+Json", "application/vnd.example+json")]
    [InlineData("application/x-custom", "application/x-custom")]
    public void MimeType_NormalizesValidValue(string value, string expected)
    {
        Assert.Equal(expected, MimeType.Create(value).Value.Value);
    }

    [Fact]
    public void MimeType_ChecksLengthBoundary()
    {
        var maximum = "a/" + new string('b', Constants.MAX_LENGTH_MIME_TYPE - 2);

        Assert.True(MimeType.Create(maximum).IsSuccess);
        Assert.True(MimeType.Create(maximum + "b").IsFailure);
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    [InlineData(long.MinValue)]
    public void FileSize_RejectsNonPositiveValues(long value)
    {
        Assert.Equal("file.size.invalid", FileSize.Create(value).Error.Code);
    }

    [Fact]
    public void FileSize_SupportsValuesLargerThanInt32()
    {
        Assert.Equal(long.MaxValue, FileSize.Create(long.MaxValue).Value.Value);
    }

    [Fact]
    public void Identifiers_RejectEmptyGuid_AndAssetIdUsesVersion7()
    {
        Assert.True(MediaAssetId.Create(Guid.Empty).IsFailure);
        Assert.True(UploaderId.Create(Guid.Empty).IsFailure);
        Assert.True(DraftId.Create(Guid.Empty).IsFailure);

        var first = MediaAssetId.New();
        var second = MediaAssetId.New();

        Assert.NotEqual(first, second);
        Assert.NotEqual(Guid.Empty, first.Value);
        Assert.Equal('7', first.Value.ToString("D")[14]);
        Assert.True(UploaderId.Create(Guid.NewGuid()).IsSuccess);
        Assert.True(DraftId.Create(Guid.NewGuid()).IsSuccess);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("1user")]
    [InlineData("user.type")]
    [InlineData("user/type")]
    [InlineData("user type")]
    [InlineData("пользователь")]
    public void EntityReference_RejectsInvalidType(string? type)
    {
        Assert.True(EntityReference.Create(type, Guid.NewGuid()).IsFailure);
    }

    [Fact]
    public void EntityReference_ValidatesIdAndTypeLength_AndHasValueEquality()
    {
        var id = Guid.NewGuid();

        Assert.True(EntityReference.Create("user", Guid.Empty).IsFailure);
        Assert.True(EntityReference.Create(new string('a', Constants.MAX_LENGTH_ENTITY_TYPE), id).IsSuccess);
        Assert.True(EntityReference.Create(new string('a', Constants.MAX_LENGTH_ENTITY_TYPE + 1), id).IsFailure);
        Assert.Equal(EntityReference.Create(" User ", id).Value, EntityReference.Create("user", id).Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("zzzz")]
    public void Checksum_RejectsInvalidValues(string? value)
    {
        Assert.True(Sha256Checksum.Create(value).IsFailure);
    }

    [Fact]
    public void Checksum_ValidatesHexAndNormalizesCase()
    {
        Assert.Equal(new string('a', 64), Sha256Checksum.Create(new string('A', 64)).Value.Value);
        Assert.True(Sha256Checksum.Create(new string('g', 64)).IsFailure);
        Assert.True(Sha256Checksum.Create(new string('a', 65)).IsFailure);
    }

    [Fact]
    public void ETag_PreservesOpaqueValue_AndValidatesBoundaries()
    {
        Assert.Equal("\"AbC-123\"", ETag.Create("\"AbC-123\"").Value.Value);
        Assert.True(ETag.Create(null).IsFailure);
        Assert.True(ETag.Create(" ").IsFailure);
        Assert.True(ETag.Create("tag\r\n").IsFailure);
        Assert.True(ETag.Create(new string('a', Constants.MAX_LENGTH_ETAG)).IsSuccess);
        Assert.True(ETag.Create(new string('a', Constants.MAX_LENGTH_ETAG + 1)).IsFailure);
    }

    [Theory]
    [InlineData(null, "Upload failed")]
    [InlineData("upload.failed", null)]
    [InlineData("", "Upload failed")]
    [InlineData("upload.failed", " ")]
    [InlineData("Upload.Failed", "Upload failed")]
    [InlineData(".failed", "Upload failed")]
    [InlineData("upload/failed", "Upload failed")]
    public void FailureInfo_RejectsInvalidValues(string? code, string? message)
    {
        Assert.True(FailureInfo.Create(code, message).IsFailure);
    }

    [Fact]
    public void FailureInfo_ChecksLengthBoundaries()
    {
        var maximumCode = new string('a', Constants.MAX_LENGTH_FAILURE_CODE);
        var maximumMessage = new string('b', Constants.MAX_LENGTH_FAILURE_MESSAGE);

        Assert.True(FailureInfo.Create(maximumCode, maximumMessage).IsSuccess);
        Assert.True(FailureInfo.Create(maximumCode + "a", "Failed").IsFailure);
        Assert.True(FailureInfo.Create("upload.failed", maximumMessage + "a").IsFailure);
    }
}