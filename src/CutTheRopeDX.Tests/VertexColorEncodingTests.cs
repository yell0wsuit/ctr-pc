using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class VertexColorEncodingTests
    {
        [Fact]
        public void PremultipliedBlendRestoresStraightRgbForSkia()
        {
            Color premultiplied = new(60, 30, 15, 85);

            Color straight = VertexColorEncoding.ForExplicitVertex(
                premultiplied,
                BlendingFactor.GLONE,
                BlendingFactor.GLONEMINUSSRCALPHA);

            Assert.Equal(new Color(180, 90, 45, 85), straight);
        }

        [Fact]
        public void TransparentPremultipliedColorBecomesTransparentBlack()
        {
            Color straight = VertexColorEncoding.ForExplicitVertex(
                new Color(120, 80, 40, 0),
                BlendingFactor.GLONE,
                BlendingFactor.GLONEMINUSSRCALPHA);

            Assert.Equal(Color.Transparent, straight);
        }

        [Theory]
        [InlineData(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONEMINUSSRCALPHA)]
        [InlineData(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONE)]
        public void StraightColorBlendModesRemainUnchanged(
            BlendingFactor source,
            BlendingFactor destination)
        {
            Color input = new(200, 100, 50, 128);

            Color result = VertexColorEncoding.ForExplicitVertex(
                input,
                source,
                destination);

            Assert.Equal(input, result);
        }

        [Fact]
        public void OpaquePremultipliedColorRemainsUnchanged()
        {
            Color input = new(200, 100, 50, 255);

            Color result = VertexColorEncoding.ForExplicitVertex(
                input,
                BlendingFactor.GLONE,
                BlendingFactor.GLONEMINUSSRCALPHA);

            Assert.Equal(input, result);
        }

        [Fact]
        public void RendererTintRemainsStraightUnderPremultipliedBlend()
        {
            Color input = new(200, 100, 50, 128);

            Color result = VertexColorEncoding.ForRendererTint(input);

            Assert.Equal(input, result);
        }
    }
}
