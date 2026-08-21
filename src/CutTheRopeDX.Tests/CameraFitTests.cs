using System.Reflection;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers gameplay camera policy: the camera shows a fixed 960 world units of width on every
    /// window shape, and a level larger than that scrolls rather than being shrunk to fit inside it.
    /// </summary>
    public sealed class CameraFitTests
    {
        /// <summary>Pack 0 level 1 is authored 640 wide, twice the usual level width.</summary>
        private const int WidePack = 0;

        /// <summary>Level index of the wide level within <see cref="WidePack"/>.</summary>
        private const int WideLevel = 1;

        /// <summary>The world width the camera shows at once, on every window shape.</summary>
        private const float LockedViewWidth = 960f;

        [Theory]
        [MemberData(nameof(Surfaces))]
        public void TheVisibleWidthIsLockedOnEveryWindowShape(string name, int width, int height)
        {
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(width, height, () =>
            {
                GameScene scene = HeadlessGame.LoadLevel(WidePack, WideLevel);
                Camera2D camera = ReadCamera(scene);
                CTRRectangle viewport = ScreenPresentation.Instance.Snapshot.VisibleBounds;

                ApplyFit(scene);

                // The scale is whatever it takes to spread 960 world units across the window.
                Assert.Equal(viewport.w, LockedViewWidth * camera.Scale, 0.01);
                Assert.False(string.IsNullOrEmpty(name));
            });
        }

        [Theory]
        [InlineData(2560, 1440)]
        [InlineData(1000, 1000)]
        [InlineData(720, 1280)]
        public void AWideLevelScrollsHorizontallyOnEveryWindowShape(int width, int height)
        {
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(width, height, () =>
            {
                GameScene scene = HeadlessGame.LoadLevel(WidePack, WideLevel);
                Camera2D camera = ReadCamera(scene);
                CTRRectangle bounds = ReadCameraBounds(scene);

                camera.MoveToXYImmediate(bounds.x, 0f, true);
                ApplyFit(scene);
                float atLeftEdge = camera.RenderPos.X;

                camera.MoveToXYImmediate(bounds.x + bounds.w, 0f, true);
                ApplyFit(scene);
                float atRightEdge = camera.RenderPos.X;

                // The level is 1920 world units wide against a 960-unit screen, so the camera
                // travels the 960 units between its two edges whatever shape the window is.
                Assert.Equal(bounds.x, atLeftEdge, 0.001);
                Assert.Equal(bounds.x + bounds.w - LockedViewWidth, atRightEdge, 0.001);
            });
        }

        [Fact]
        public void ANarrowLevelFillsTheWindowWidthExactly()
        {
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(2560, 1440, () =>
            {
                // Pack 0 level 0 is authored 320 wide: exactly the locked screen width.
                GameScene scene = HeadlessGame.LoadLevel(0, 0);
                Camera2D camera = ReadCamera(scene);
                CTRRectangle bounds = ReadCameraBounds(scene);

                camera.MoveToXYImmediate(bounds.x + bounds.w, 0f, true);
                ApplyFit(scene);

                // Nowhere to scroll horizontally, and the level's own edges are the screen's.
                Assert.Equal(bounds.x, camera.RenderPos.X, 0.001);
                Assert.Equal(LockedViewWidth, bounds.w, 0.001);
                Assert.Equal(2560f / LockedViewWidth, camera.Scale, 0.001);
            });
        }

        [Fact]
        public void ATallerWindowShowsMoreOfTheLevelHeight()
        {
            _ = HeadlessGame.Boot();

            // 9:16 is 1440 x 2560 logical units, so the locked width scales by 1.5 and the window
            // shows 2560 / 1.5 world units of height - more than the level's own 1440.
            LayoutSurfaces.WithSurface(720, 1280, () =>
            {
                GameScene scene = HeadlessGame.LoadLevel(WidePack, WideLevel);
                Camera2D camera = ReadCamera(scene);

                ApplyFit(scene);

                Assert.Equal(1.5f, camera.Scale, 0.001);
                Assert.Equal(-((2560f / 1.5f) - 1440f) / 2f, camera.RenderPos.Y, 0.01);
            });
        }

        private static void ApplyFit(GameScene scene)
        {
            MethodInfo apply = typeof(GameScene).GetMethod(
                "ApplyCameraFit",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(apply);
            _ = apply.Invoke(scene, [ScreenPresentation.Instance.Snapshot]);
        }

        private static Camera2D ReadCamera(GameScene scene)
        {
            FieldInfo field = typeof(GameScene).GetField(
                "camera",
                BindingFlags.Instance | BindingFlags.NonPublic);
            return Assert.IsType<Camera2D>(field?.GetValue(scene));
        }

        private static CTRRectangle ReadCameraBounds(GameScene scene)
        {
            FieldInfo field = typeof(GameScene).GetField(
                "cameraBounds",
                BindingFlags.Instance | BindingFlags.NonPublic);
            return Assert.IsType<CTRRectangle>(field?.GetValue(scene));
        }

        public static TheoryData<string, int, int> Surfaces()
        {
            return LayoutSurfaces.Theory();
        }
    }
}
