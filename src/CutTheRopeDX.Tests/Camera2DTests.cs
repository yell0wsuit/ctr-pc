using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;

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
    }
}
