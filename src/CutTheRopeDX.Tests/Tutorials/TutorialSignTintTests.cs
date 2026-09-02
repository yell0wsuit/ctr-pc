using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain.Tutorials;

using Xunit;

namespace CutTheRopeDX.Tests.Tutorials
{
    /// <summary>
    /// A colored sign draws from a recolored copy of its own atlas frame, so the cache that builds
    /// those copies has to hand the same one back rather than rebuild it per prompt.
    /// </summary>
    public sealed class TutorialSignTintTests
    {
        private static readonly RGBAColor Red = RGBAColor.MakeRGBA(1f, 0f, 0f, 1f);
        private static readonly RGBAColor Green = RGBAColor.MakeRGBA(0f, 1f, 0f, 1f);

        [Fact]
        public void OneQuadAndColorAreBuiltOnce()
        {
            using TutorialSignTints tints = new();
            CTRTexture2D atlas = Atlas();

            Assert.Same(tints.Tinted(atlas, 4, Red), tints.Tinted(atlas, 4, Red));
        }

        [Fact]
        public void EachColorGetsItsOwnCopyOfTheFrame()
        {
            using TutorialSignTints tints = new();
            CTRTexture2D atlas = Atlas();

            Assert.NotSame(tints.Tinted(atlas, 4, Red), tints.Tinted(atlas, 4, Green));
        }

        [Fact]
        public void EachQuadGetsItsOwnCopyOfTheFrame()
        {
            using TutorialSignTints tints = new();
            CTRTexture2D atlas = Atlas();

            Assert.NotSame(tints.Tinted(atlas, 4, Red), tints.Tinted(atlas, 2, Red));
        }

        [Fact]
        public void TheCopyIsTheFrameStandingAloneAsQuadZero()
        {
            using TutorialSignTints tints = new();
            CTRTexture2D atlas = Atlas();

            CTRTexture2D tinted = tints.Tinted(atlas, 4, Red);

            Assert.Equal(1, tinted.quadsCount);
            Assert.Equal(0f, tinted.quadRects[0].x);
            Assert.Equal(0f, tinted.quadRects[0].y);
            Assert.Equal(atlas.quadRects[4].w, tinted.quadRects[0].w);
            Assert.Equal(atlas.quadRects[4].h, tinted.quadRects[0].h);
        }

        [Fact]
        public void TheCopyKeepsTheFramesTrimOffsetSoItDrawsWhereTheAtlasWould()
        {
            using TutorialSignTints tints = new();
            CTRTexture2D atlas = Atlas();
            atlas.quadOffsets[4] = new Vector(7f, -3f);

            CTRTexture2D tinted = tints.Tinted(atlas, 4, Red);

            Assert.Equal(7f, tinted.quadOffsets[0].X);
            Assert.Equal(-3f, tinted.quadOffsets[0].Y);
        }

        [Fact]
        public void DisposingReleasesTheCopiesInsteadOfHandingThemOutAgain()
        {
            CTRTexture2D atlas = Atlas();
            TutorialSignTints tints = new();
            CTRTexture2D before = tints.Tinted(atlas, 4, Red);
            tints.Dispose();

            using TutorialSignTints rebuilt = new();

            Assert.NotSame(before, rebuilt.Tinted(atlas, 4, Red));
        }

        /// <summary>
        /// Stands in for the loaded sign atlas: the real frame rectangles, with no platform texture
        /// behind them because nothing is drawn here.
        /// </summary>
        private static CTRTexture2D Atlas()
        {
            CTRTexture2D atlas = new CTRTexture2D().InitWithHandle(null, 256, 956);
            atlas.SetQuadsCapacity(11);
            atlas.SetQuadAt(new CTRRectangle(1f, 480f, 184f, 152f), 2);
            atlas.SetQuadAt(new CTRRectangle(1f, 243f, 246f, 235f), 4);
            return atlas;
        }
    }
}
