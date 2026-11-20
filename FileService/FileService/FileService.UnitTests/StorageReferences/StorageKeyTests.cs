using FileService.Domain.Shared;
using FileService.Domain.StorageReferences;

namespace FileService.UnitTests.StorageReferences;

public class StorageKeyTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(".")]
    [InlineData("..")]
    [InlineData("../image.png")]
    [InlineData("originals/../image.png")]
    [InlineData("/originals/image.png")]
    [InlineData("originals//image.png")]
    [InlineData("originals/image.png/")]
    [InlineData("originals\\image.png")]
    [InlineData("c:/image.png")]
    [InlineData("originals/%2e%2e/image.png")]
    [InlineData("originals/%252e%252e/image.png")]
    [InlineData("originals/%2fimage.png")]
    [InlineData("originals/image.png?download=1")]
    [InlineData("originals/image.png#fragment")]
    [InlineData("originals/image name.png")]
    [InlineData(" originals/image.png")]
    [InlineData("originals/image.png ")]
    [InlineData("Originals/image.png")]
    [InlineData("originals/image.PNG")]
    [InlineData("originals/image\n.png")]
    [InlineData("originals/image.png\n")]
    [InlineData("originals/.image")]
    [InlineData("originals/image.")]
    [InlineData("originals/image.tar.gz")]
    public void Create_WithUnsafeOrNonCanonicalKey_ReturnsDomainError(string? value)
    {
        var result = StorageKey.Create(value);

        Assert.True(result.IsFailure);
        Assert.Equal("file.storage_key.invalid", result.Error.Code);
    }

    [Theory]
    [InlineData("originals/019b0000000070008000000000000001/image.webp")]
    [InlineData("originals/a-b_c/image-01.png")]
    [InlineData("image")]
    [InlineData("image.webp")]
    public void Create_PreservesValidKey(string value)
    {
        Assert.Equal(value, StorageKey.Create(value).Value.Value);
    }

    [Fact]
    public void Create_ChecksLengthBoundary()
    {
        Assert.True(StorageKey.Create(new string('a', Constants.MAX_LENGTH_STORAGE_KEY)).IsSuccess);
        Assert.True(StorageKey.Create(new string('a', Constants.MAX_LENGTH_STORAGE_KEY + 1)).IsFailure);
    }
}