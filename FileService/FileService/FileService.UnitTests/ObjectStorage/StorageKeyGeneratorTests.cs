using FileService.Core.ObjectStorage;
using FileService.Domain.MediaAssets;
using SharedKernel;

namespace FileService.UnitTests.ObjectStorage;

public class StorageKeyGeneratorTests
{
    [Theory]
    [InlineData("019b0000-0000-7000-8000-000000000001", "originals/019b0000000070008000000000000001")]
    [InlineData("ABCDEFAB-CDEF-4ABC-8ABC-ABCDEFABCDEF", "originals/abcdefabcdef4abc8abcabcdefabcdef")]
    public void CreateOriginal_ProducesCanonicalKeyForAsset(string value, string expected)
    {
        var assetId = MediaAssetId.Create(Guid.Parse(value)).Value;

        var result = StorageKeyGenerator.CreateOriginal(assetId);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value.Value);
    }

    [Fact]
    public void CreateOriginal_RepeatedCallsForSameAsset_ReturnSameKey()
    {
        var assetId = MediaAssetId.New();

        var first = StorageKeyGenerator.CreateOriginal(assetId);
        var second = StorageKeyGenerator.CreateOriginal(assetId);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Equal(first.Value, second.Value);
    }

    [Fact]
    public void CreateOriginal_WithNullAssetId_ReturnsRequiredError()
    {
        var result = StorageKeyGenerator.CreateOriginal(null);
        var expected = GeneralErrors.ValueIsRequired(nameof(MediaAssetId));

        Assert.True(result.IsFailure);
        Assert.Equal(expected.Code, result.Error.Code);
    }
}