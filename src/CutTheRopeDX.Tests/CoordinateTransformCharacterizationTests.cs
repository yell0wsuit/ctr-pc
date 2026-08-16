using CutTheRopeDX.Commons;
using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Platform;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Pins the screen coordinate transforms as they behave today.
    /// </summary>
    public sealed class CoordinateTransformCharacterizationTests
    {
        [Theory]
        [MemberData(nameof(Surfaces))]
        public void ViewToGameRoundTripsTheCenterOfTheRenderViewport(string name, int width, int height)
        {
            ScreenPresentation presentation = new(2560, 1440);
            _ = presentation.SetSurfaceSize(width, height);

            ViewportLayoutSnapshot snapshot = presentation.Snapshot;
            CTRRectangle visible = snapshot.VisibleBounds;

            float viewX = snapshot.RenderViewport.w / 2f;
            float viewY = snapshot.RenderViewport.h / 2f;

            float gameX = presentation.TransformViewToGameX(viewX);
            float gameY = presentation.TransformViewToGameY(viewY);

            // 3.5 rather than a tighter bound because odd scaled-view heights round the
            // half-way pixel: 400x1280 gives a 225-high view whose center maps to 716.8.
            Assert.Equal(visible.w / 2f, gameX, 3.5);
            Assert.Equal(visible.h / 2f, gameY, 3.5);
            Assert.False(string.IsNullOrEmpty(name));
        }

        [Theory]
        [MemberData(nameof(Surfaces))]
        public void RealScreenGlobalsFollowTheSurfaceThroughTheResizeEntryPoint(
            string name,
            int width,
            int height)
        {
            _ = HeadlessGame.Boot();
            LayoutSurfaces.WithSurface(width, height, () =>
            {
                CtrRenderer.OnSurfaceChanged(width, height);

                Assert.Equal(width, FrameworkTypes.REAL_SCREEN_WIDTH);
                Assert.Equal(height, FrameworkTypes.REAL_SCREEN_HEIGHT);
                Assert.False(string.IsNullOrEmpty(name));
            });
        }

        public static TheoryData<string, int, int> Surfaces()
        {
            return LayoutSurfaces.Theory();
        }
    }
}
