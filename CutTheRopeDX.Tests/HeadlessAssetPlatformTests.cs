using CutTheRopeDX.Framework.Platform;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class HeadlessAssetPlatformTests
    {
        [Fact]
        public void ImageDimensions_ResolvesAtlaslessImage()
        {
            HeadlessAssetPlatform platform = new();

            (int W, int H)? dims = platform.ImageDimensions("images/obj_axe");

            Assert.NotNull(dims);
            Assert.True(dims.Value.W > 0);
            Assert.True(dims.Value.H > 0);
        }

        [Fact]
        public void ImageDimensions_ResolvesImageInSubdirectory()
        {
            HeadlessAssetPlatform platform = new();

            Assert.NotNull(platform.ImageDimensions("images/backgrounds/bgr_01_p1"));
        }

        [Fact]
        public void ImageDimensions_ReturnsNull_ForMissingAsset()
        {
            HeadlessAssetPlatform platform = new();

            Assert.Null(platform.ImageDimensions("images/there_is_no_such_image"));
        }

        [Fact]
        public void ImageTexture_IsNull()
        {
            Assert.Null(new HeadlessAssetPlatform().ImageTexture("images/obj_axe"));
        }
    }
}
