using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers the pure layout primitives every scene layout is expressed in terms of.
    /// </summary>
    public sealed class LayoutMathTests
    {
        [Fact]
        public void FitInsideCentersHorizontallyWhenTheViewportIsWider()
        {
            // A 600x900 design box in a 2000x900 viewport: height-limited, so the box keeps
            // full height and is centered across the extra width.
            CTRRectangle fit = LayoutMath.FitInside(600f, 900f, new CTRRectangle(0f, 0f, 2000f, 900f));

            Assert.Equal(900f, fit.h, 0.01);
            Assert.Equal(600f, fit.w, 0.01);
            Assert.Equal(700f, fit.x, 0.01);
            Assert.Equal(0f, fit.y, 0.01);
        }

        [Fact]
        public void FitInsideCentersVerticallyWhenTheViewportIsTaller()
        {
            CTRRectangle fit = LayoutMath.FitInside(600f, 900f, new CTRRectangle(0f, 0f, 600f, 1800f));

            Assert.Equal(600f, fit.w, 0.01);
            Assert.Equal(900f, fit.h, 0.01);
            Assert.Equal(0f, fit.x, 0.01);
            Assert.Equal(450f, fit.y, 0.01);
        }

        [Fact]
        public void FitInsideHonorsTheViewportOrigin()
        {
            CTRRectangle fit = LayoutMath.FitInside(100f, 100f, new CTRRectangle(50f, 20f, 400f, 200f));

            Assert.Equal(200f, fit.w, 0.01);
            Assert.Equal(200f, fit.h, 0.01);
            Assert.Equal(150f, fit.x, 0.01);
            Assert.Equal(20f, fit.y, 0.01);
        }

        [Fact]
        public void CoverPicksTheLargerAxisRatio()
        {
            // A 2560x1440 image in a 3413x1440 viewport must scale up to cover the width.
            float scale = LayoutMath.Cover(2560f, 1440f, new CTRRectangle(0f, 0f, 3413f, 1440f));

            Assert.Equal(3413f / 2560f, scale, 0.001);
        }

        [Fact]
        public void CoverPicksTheHeightWhenTheViewportIsTall()
        {
            float scale = LayoutMath.Cover(2560f, 1440f, new CTRRectangle(0f, 0f, 1440f, 2560f));

            Assert.Equal(2560f / 1440f, scale, 0.001);
        }

        [Fact]
        public void AnchorPositionPlacesBottomLeftInsideTheInset()
        {
            Vector p = LayoutMath.AnchorPosition(
                new CTRRectangle(0f, 0f, 1000f, 800f),
                LayoutEdge.BottomLeft,
                elementWidth: 100f,
                elementHeight: 50f,
                insetX: 10f,
                insetY: 10f);

            Assert.Equal(10f, p.X, 0.01);
            Assert.Equal(740f, p.Y, 0.01);
        }

        [Fact]
        public void AnchorPositionPlacesTopRightInsideTheInset()
        {
            Vector p = LayoutMath.AnchorPosition(
                new CTRRectangle(0f, 0f, 1000f, 800f),
                LayoutEdge.TopRight,
                elementWidth: 100f,
                elementHeight: 50f,
                insetX: 10f,
                insetY: 10f);

            Assert.Equal(890f, p.X, 0.01);
            Assert.Equal(10f, p.Y, 0.01);
        }

        [Fact]
        public void AnchorPositionCentersTheElement()
        {
            Vector p = LayoutMath.AnchorPosition(
                new CTRRectangle(0f, 0f, 1000f, 800f),
                LayoutEdge.MiddleCenter,
                elementWidth: 100f,
                elementHeight: 50f,
                insetX: 0f,
                insetY: 0f);

            Assert.Equal(450f, p.X, 0.01);
            Assert.Equal(375f, p.Y, 0.01);
        }

        [Fact]
        public void RemapInterpolatesLinearlyBetweenTheOutputBounds()
        {
            Assert.Equal(900f, LayoutMath.Remap(1f, 1f, 2f, 900f, 650f), 0.01);
            Assert.Equal(650f, LayoutMath.Remap(2f, 1f, 2f, 900f, 650f), 0.01);
            Assert.Equal(775f, LayoutMath.Remap(1.5f, 1f, 2f, 900f, 650f), 0.01);
        }

        [Fact]
        public void FitCameraContainsTheLevelBounds()
        {
            // A 2560x1440 level in a 2560x1440 viewport fits exactly at 1:1.
            CameraFit fit = LayoutMath.FitCamera(
                new CTRRectangle(0f, 0f, 2560f, 1440f),
                new CTRRectangle(0f, 0f, 2560f, 1440f),
                anchorX: 0.5f,
                anchorY: 0.5f);

            Assert.Equal(1f, fit.Scale, 0.001);
            Assert.Equal(2560f, fit.VisibleWorld.w, 0.01);
            Assert.Equal(1440f, fit.VisibleWorld.h, 0.01);
        }

        [Fact]
        public void FitCameraRevealsExtraWorldOnAWiderViewport()
        {
            // The level is unchanged but the viewport is wider, so the same scale exposes more
            // world horizontally. This is what makes wide screens show more instead of bars.
            CameraFit fit = LayoutMath.FitCamera(
                new CTRRectangle(0f, 0f, 2560f, 1440f),
                new CTRRectangle(0f, 0f, 3600f, 1440f),
                anchorX: 0.5f,
                anchorY: 0.5f);

            Assert.Equal(1f, fit.Scale, 0.001);
            Assert.Equal(3600f, fit.VisibleWorld.w, 0.01);
            Assert.Equal(1440f, fit.VisibleWorld.h, 0.01);
            // Centered anchor puts the extra 1040 units half either side of the level.
            Assert.Equal(-520f, fit.VisibleWorld.x, 0.01);
        }

        [Fact]
        public void FitCameraAnchorSlidesTheVisibleWindow()
        {
            CameraFit fit = LayoutMath.FitCamera(
                new CTRRectangle(0f, 0f, 2560f, 1440f),
                new CTRRectangle(0f, 0f, 3600f, 1440f),
                anchorX: 0f,
                anchorY: 0.5f);

            Assert.Equal(0f, fit.VisibleWorld.x, 0.01);
        }
    }
}
