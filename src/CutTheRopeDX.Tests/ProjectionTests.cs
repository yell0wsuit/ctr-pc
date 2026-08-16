using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Platform;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers the orthographic projection the canvas builds. It must describe the region the
    /// game actually draws into, or content laid out against the visible bounds is clipped by a
    /// projection that describes a smaller one.
    /// </summary>
    public sealed class ProjectionTests
    {
        [Fact]
        public void ProjectionMatchesTheDesignSizeAtSixteenNine()
        {
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(1280, 720, () =>
            {
                GLCanvas canvas = FrameworkTypes.Canvas;

                Assert.Equal(2560f, canvas.ProjectionWidth, 0.01);
                Assert.Equal(1440f, canvas.ProjectionHeight, 0.01);
            });
        }

        [Fact]
        public void ProjectionWidensWithAnUltrawideViewport()
        {
            // 2560x1080 is 21:9, inside the clamp. Visible bounds are 3413x1440, so the
            // projection must be that wide or the extra world is drawn outside it.
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(2560, 1080, () =>
            {
                GLCanvas canvas = FrameworkTypes.Canvas;
                CTRRectangle visible = ScreenPresentation.Instance.Snapshot.VisibleBounds;

                Assert.Equal(visible.w, canvas.ProjectionWidth, 0.01);
                Assert.Equal(visible.h, canvas.ProjectionHeight, 0.01);
                Assert.True(canvas.ProjectionWidth > 2560f);
            });
        }

        [Fact]
        public void ProjectionTallensWithAPortraitViewport()
        {
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(720, 1280, () =>
            {
                GLCanvas canvas = FrameworkTypes.Canvas;
                CTRRectangle visible = ScreenPresentation.Instance.Snapshot.VisibleBounds;

                Assert.Equal(visible.w, canvas.ProjectionWidth, 0.01);
                Assert.Equal(visible.h, canvas.ProjectionHeight, 0.01);
                Assert.True(canvas.ProjectionHeight > canvas.ProjectionWidth);
            });
        }

        [Fact]
        public void ViewportIsTheRenderViewportNotTheLetterbox()
        {
            // 3840x1080 is 3.556, outside the clamp, so the render viewport is a centered
            // 2700x1080 crop. The canvas viewport must be that crop.
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(3840, 1080, () =>
            {
                GLCanvas canvas = FrameworkTypes.Canvas;
                CTRRectangle render = ScreenPresentation.Instance.Snapshot.RenderViewport;

                Assert.Equal((int)render.x, canvas.xOffset);
                Assert.Equal((int)render.y, canvas.yOffset);
                Assert.Equal((int)render.w, canvas.backingWidth);
                Assert.Equal((int)render.h, canvas.backingHeight);
            });
        }
    }
}
