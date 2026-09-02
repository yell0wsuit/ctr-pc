using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Platform;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// The tutorial sign atlas draws its icons as pure black ink whose shape and shading live
    /// entirely in alpha, so a color has to replace the ink rather than multiply it.
    /// </summary>
    public sealed class PremultipliedTintTests
    {
        [Fact]
        public void InkKeepsItsCoverageAndWearsTheTint()
        {
            // The atlas ink sits at alpha 176, which is what makes it read as gray rather than black.
            byte[] pixels = [0, 0, 0, 176];

            PremultipliedTint.Apply(pixels, RGBAColor.MakeRGBA(1f, 0f, 0f, 1f));

            Assert.Equal(new byte[] { 176, 0, 0, 176 }, pixels);
        }

        [Fact]
        public void AWhiteTintLeavesThePixelPremultipliedGray()
        {
            byte[] pixels = [0, 0, 0, 176];

            PremultipliedTint.Apply(pixels, RGBAColor.MakeRGBA(1f, 1f, 1f, 1f));

            Assert.Equal(new byte[] { 176, 176, 176, 176 }, pixels);
        }

        [Fact]
        public void AFullyOpaquePixelTakesTheTintChannelsUnscaled()
        {
            byte[] pixels = [0, 0, 0, 255];

            PremultipliedTint.Apply(pixels, RGBAColor.MakeRGBA(70f / 255f, 37f / 255f, 0f, 1f));

            Assert.Equal(new byte[] { 70, 37, 0, 255 }, pixels);
        }

        [Fact]
        public void ATransparentPixelStaysTransparent()
        {
            byte[] pixels = [0, 0, 0, 0];

            PremultipliedTint.Apply(pixels, RGBAColor.MakeRGBA(1f, 1f, 1f, 1f));

            Assert.Equal(new byte[] { 0, 0, 0, 0 }, pixels);
        }

        [Fact]
        public void EverySoftEdgePixelIsTintedIndependently()
        {
            byte[] pixels = [0, 0, 0, 255, 0, 0, 0, 128, 0, 0, 0, 0];

            PremultipliedTint.Apply(pixels, RGBAColor.MakeRGBA(0f, 1f, 0f, 1f));

            Assert.Equal(new byte[] { 0, 255, 0, 255, 0, 128, 0, 128, 0, 0, 0, 0 }, pixels);
        }
    }
}
