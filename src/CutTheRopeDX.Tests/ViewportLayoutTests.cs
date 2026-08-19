using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Platform;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers the pure surface-size to snapshot computation.
    /// </summary>
    public sealed class ViewportLayoutTests
    {
        [Fact]
        public void SnapshotCarriesTheSurfaceSizeItWasComputedFrom()
        {
            ViewportLayoutSnapshot snapshot = ViewportLayout.Compute(1600, 900);

            Assert.Equal(1600, snapshot.SurfaceWidth);
            Assert.Equal(900, snapshot.SurfaceHeight);
        }

        [Fact]
        public void EqualSurfaceSizesProduceEqualSnapshots()
        {
            ViewportLayoutSnapshot a = ViewportLayout.Compute(1600, 900);
            ViewportLayoutSnapshot b = ViewportLayout.Compute(1600, 900);

            Assert.Equal(a, b);
        }

        [Fact]
        public void SixteenNineExposesExactlyTheDesignSpace()
        {
            ViewportLayoutSnapshot snapshot = ViewportLayout.Compute(1280, 720);

            Assert.Equal(0.5f, snapshot.Scale);
            Assert.Equal(2560f, snapshot.VisibleBounds.w, 0.01);
            Assert.Equal(1440f, snapshot.VisibleBounds.h, 0.01);
            Assert.Equal(LayoutOrientation.Landscape, snapshot.Orientation);
        }

        [Fact]
        public void WideSurfaceInsideTheClampExposesExtraWidth()
        {
            // 2560x1080 is 2.370, inside the 2.5 limit, so nothing is cropped.
            ViewportLayoutSnapshot snapshot = ViewportLayout.Compute(2560, 1080);

            Assert.Equal(2560f, snapshot.RenderViewport.w);
            Assert.Equal(1080f, snapshot.RenderViewport.h);
            Assert.Equal(0.75f, snapshot.Scale);
            Assert.Equal(3413.33f, snapshot.VisibleBounds.w, 0.01);
            Assert.Equal(1440f, snapshot.VisibleBounds.h, 0.01);
        }

        [Fact]
        public void SurfaceWiderThanTheScaleCurveIsStillDrawnWhole()
        {
            // 3840x1080 is 3.555, past the widest ratio the content scale distinguishes. It used
            // to be cropped to 2.5 and centered, which is what put black bars down the sides of an
            // ultrawide window; the window is the player's to shape, so the game fills it.
            ViewportLayoutSnapshot snapshot = ViewportLayout.Compute(3840, 1080);

            Assert.Equal(3840f, snapshot.RenderViewport.w, 0.01);
            Assert.Equal(1080f, snapshot.RenderViewport.h, 0.01);
            Assert.Equal(0f, snapshot.RenderViewport.x, 0.01);
            Assert.Equal(0f, snapshot.RenderViewport.y, 0.01);
            Assert.Equal(5120f, snapshot.VisibleBounds.w, 0.01);
            Assert.Equal(1440f, snapshot.VisibleBounds.h, 0.01);
        }

        [Fact]
        public void PortraitInsideTheClampExposesExtraHeight()
        {
            // 720x1280 is 0.5625, inside the 0.4 limit.
            ViewportLayoutSnapshot snapshot = ViewportLayout.Compute(720, 1280);

            Assert.Equal(0.5f, snapshot.Scale);
            Assert.Equal(1440f, snapshot.VisibleBounds.w, 0.01);
            Assert.Equal(2560f, snapshot.VisibleBounds.h, 0.01);
            Assert.Equal(LayoutOrientation.Portrait, snapshot.Orientation);
        }

        [Fact]
        public void SurfaceTallerThanTheScaleCurveIsStillDrawnWhole()
        {
            // 400x1280 is 0.3125, past the narrowest ratio the content scale distinguishes, and
            // drawn whole for the same reason a wider one is.
            ViewportLayoutSnapshot snapshot = ViewportLayout.Compute(400, 1280);

            Assert.Equal(400f, snapshot.RenderViewport.w, 0.01);
            Assert.Equal(1280f, snapshot.RenderViewport.h, 0.01);
            Assert.Equal(0f, snapshot.RenderViewport.x, 0.01);
            Assert.Equal(0f, snapshot.RenderViewport.y, 0.01);
            Assert.Equal(1440f, snapshot.VisibleBounds.w, 0.01);
            Assert.Equal(4608f, snapshot.VisibleBounds.h, 0.01);
        }

        [Fact]
        public void SquareSurfaceIsLandscape()
        {
            ViewportLayoutSnapshot snapshot = ViewportLayout.Compute(1000, 1000);

            Assert.Equal(LayoutOrientation.Landscape, snapshot.Orientation);
            Assert.Equal(1440f, snapshot.VisibleBounds.w, 0.01);
            Assert.Equal(1440f, snapshot.VisibleBounds.h, 0.01);
        }

        [Fact]
        public void SixteenNineSurfaceReportsSixteenNineAspect()
        {
            ViewportLayoutSnapshot snapshot = ViewportLayout.Compute(1280, 720);

            Assert.Equal(16f / 9f, snapshot.Aspect, 0.001);
        }

        [Fact]
        public void AspectIsTheSurfaceRatioAtAnyShape()
        {
            // Nothing is cropped away, so what the layout measures against is the shape of the
            // window itself, however far that is from anything the content scale distinguishes.
            Assert.Equal(3840f / 1080f, ViewportLayout.Compute(3840, 1080).Aspect, 0.001);
            Assert.Equal(400f / 1280f, ViewportLayout.Compute(400, 1280).Aspect, 0.001);
        }

        [Fact]
        public void DevicePixelRatioDefaultsToOne()
        {
            ViewportLayoutSnapshot snapshot = ViewportLayout.Compute(1280, 720);

            Assert.Equal(1f, snapshot.DevicePixelRatio);
        }

        [Fact]
        public void DevicePixelRatioIsCarriedOntoTheSnapshot()
        {
            ViewportLayoutSnapshot snapshot = ViewportLayout.Compute(1280, 720, 2f);

            Assert.Equal(2f, snapshot.DevicePixelRatio);
        }

        [Fact]
        public void RenderTargetPixelsDoNotCarryTheRenderOrigin()
        {
            // The frame is drawn into a target the size of the drawn region, and where that region
            // sits on the surface is applied when it is copied to the screen. A scissor rectangle
            // that added the origin as well was pushed sideways by it, which cut the edge off
            // every element it clipped - a credits column, a grid of skins. Computed surfaces put
            // that origin at zero now, so the offset is put in by hand here.
            ViewportLayoutSnapshot offset = ViewportLayout.Compute(1280, 720) with
            {
                RenderViewport = new CTRRectangle(90f, 40f, 1280f, 720f)
            };

            CTRRectangle target = offset.ToRenderTarget(new CTRRectangle(0f, 0f, 100f, 50f));

            Assert.Equal(0f, target.x);
            Assert.Equal(0f, target.y);
            Assert.Equal(100f * offset.Scale, target.w, 0.0001);
            Assert.Equal(50f * offset.Scale, target.h, 0.0001);
        }

        [Fact]
        public void RenderTargetPixelsScaleFromLogicalSpace()
        {
            ViewportLayoutSnapshot snapshot = ViewportLayout.Compute(1280, 720);

            CTRRectangle target = snapshot.ToRenderTarget(new CTRRectangle(10f, 20f, 30f, 40f));

            Assert.Equal(10f * snapshot.Scale, target.x, 0.0001);
            Assert.Equal(20f * snapshot.Scale, target.y, 0.0001);
            Assert.Equal(30f * snapshot.Scale, target.w, 0.0001);
            Assert.Equal(40f * snapshot.Scale, target.h, 0.0001);
        }

        [Fact]
        public void DevicePixelRatioDoesNotAffectAnyGeometry()
        {
            // It is a reporting channel for physical sizing, not an input to the layout maths.
            ViewportLayoutSnapshot one = ViewportLayout.Compute(1280, 720, 1f);
            ViewportLayoutSnapshot two = ViewportLayout.Compute(1280, 720, 3f);

            Assert.Equal(one.VisibleBounds, two.VisibleBounds);
            Assert.Equal(one.RenderViewport, two.RenderViewport);
            Assert.Equal(one.Scale, two.Scale);
        }
    }
}
