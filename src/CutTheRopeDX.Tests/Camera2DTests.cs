using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Platform;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers the camera's screen-space to world-space conversion, the single place a
    /// camera scale term is applied.
    /// </summary>
    public sealed class Camera2DTests
    {
        [Fact]
        public void ScreenToWorldOffsetsByCameraPosition()
        {
            Camera2D camera = new();
            camera.MoveToXYImmediate(100f, 50f, true);

            Vector world = camera.ScreenToWorld(10f, 20f);

            Assert.Equal(110f, world.X);
            Assert.Equal(70f, world.Y);
        }

        [Fact]
        public void ScreenToWorldComponentAccessorsMatchTheVectorForm()
        {
            Camera2D camera = new();
            camera.MoveToXYImmediate(-30f, 7.5f, true);

            Vector world = camera.ScreenToWorld(4f, 6f);

            Assert.Equal(world.X, camera.ScreenToWorldX(4f));
            Assert.Equal(world.Y, camera.ScreenToWorldY(6f));
        }

        [Fact]
        public void ScreenToWorldIsIdentityWhenCameraIsAtOrigin()
        {
            Camera2D camera = new();

            Vector world = camera.ScreenToWorld(123f, 456f);

            Assert.Equal(123f, world.X);
            Assert.Equal(456f, world.Y);
        }

        [Fact]
        public void ScaleDefaultsToOneSoConversionIsUnchanged()
        {
            Camera2D camera = new();

            Assert.Equal(1f, camera.Scale);
        }

        [Fact]
        public void ScreenToWorldDividesByTheCameraScale()
        {
            Camera2D camera = new();
            camera.MoveToXYImmediate(100f, 50f, true);
            camera.Scale = 2f;

            // At 2x, a screen offset of 10 covers 5 world units.
            Vector world = camera.ScreenToWorld(10f, 20f);

            Assert.Equal(105f, world.X, 0.01);
            Assert.Equal(60f, world.Y, 0.01);
        }

        [Fact]
        public void ApplyFitAdoptsTheScaleAndVisibleOrigin()
        {
            Camera2D camera = new();
            CameraFit fit = LayoutMath.FitCamera(
                new CTRRectangle(0f, 0f, 2560f, 1440f),
                new CTRRectangle(0f, 0f, 3600f, 1440f),
                anchorX: 0.5f,
                anchorY: 0.5f);

            camera.ApplyFit(fit);

            Assert.Equal(1f, camera.Scale, 0.001);
            Assert.Equal(-520f, camera.pos.X, 0.01);
            Assert.Equal(0f, camera.pos.Y, 0.01);
        }
    }
}
