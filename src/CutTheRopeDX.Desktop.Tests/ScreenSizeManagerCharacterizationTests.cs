using System;
using System.Reflection;

using Microsoft.Xna.Framework;

using Xunit;

namespace CutTheRopeDX.Desktop.Tests
{
    /// <summary>
    /// Covers the desktop resize wiring from the window bounds through the shared presentation
    /// snapshot and the Core screen metrics read from it.
    /// </summary>
    public sealed class ScreenSizeManagerCharacterizationTests
    {
        [Theory]
        // The shipped shape is unchanged, which is what keeps an existing saved window opening
        // exactly as it closed.
        [InlineData(1600, 900, 1600, 900)]
        // Every other shape survives the clamp: the window is the user's to size, and the layout
        // follows it rather than the other way round.
        [InlineData(1600, 1200, 1600, 1200)]
        [InlineData(900, 1600, 900, 1600)]
        // Each axis has its own floor, so hitting one does not drag the other with it. The floor
        // is the smallest surface the layout is tested against, not a shape the desktop implies.
        [InlineData(400, 1200, 400, 1200)]
        [InlineData(200, 1200, 320, 1200)]
        [InlineData(1600, 200, 1600, 480)]
        // A window dragged past the floor on both axes snaps back to it, and one dragged to the
        // floor exactly is left alone.
        [InlineData(100, 100, 320, 480)]
        [InlineData(320, 480, 320, 480)]
        [InlineData(319, 479, 320, 480)]
        // Neither axis may exceed the display.
        [InlineData(5000, 5000, 3840, 2160)]
        public void ClampWindowSizeKeepsTheRequestedAspect(
            int width,
            int height,
            int expectedWidth,
            int expectedHeight)
        {
            Point clamped = ScreenSizeManager.ClampWindowSize(width, height, 3840, 2160, 4096);

            Assert.Equal(expectedWidth, clamped.X);
            Assert.Equal(expectedHeight, clamped.Y);
        }

        [Theory]
        // Startup: the swapchain was created at this size and the window is already there, which
        // is the one case worth skipping - rebuilding it flashes the window black for nothing.
        [InlineData(320, 480, 320, 480, 320, 480, false)]
        // A drag past the floor is answered by sizing back up to it, which leaves the window where
        // the drag put it. A second drag then asks for a size the back buffer is already at: skip
        // that and the picture stays squeezed into a smaller window, and the swapchain is reported
        // suboptimal and rebuilt on every frame after.
        [InlineData(320, 480, 200, 300, 320, 480, true)]
        // An ordinary resize, where neither matches yet.
        [InlineData(800, 600, 800, 600, 1024, 768, true)]
        // Before the device exists there is nothing to compare against.
        [InlineData(0, 0, 800, 600, 800, 600, true)]
        public void ASwapchainIsRebuiltWheneverTheWindowAndItsBackBufferDisagree(
            int backBufferWidth,
            int backBufferHeight,
            int windowWidth,
            int windowHeight,
            int targetWidth,
            int targetHeight,
            bool expected)
        {
            Assert.Equal(
                expected,
                ScreenSizeManager.NeedsSwapchainResize(
                    new Point(backBufferWidth, backBufferHeight),
                    new Point(windowWidth, windowHeight),
                    new Point(targetWidth, targetHeight)));
        }

        [Fact]
        public void WindowResizeRefreshesTheCoreScreenMetrics()
        {
            Assembly coreAssembly = Assembly.Load("CutTheRopeDX.Core");
            Type frameworkTypes = coreAssembly.GetType("CutTheRopeDX.Framework.FrameworkTypes");
            Assert.NotNull(frameworkTypes);
            PropertyInfo realWidth = frameworkTypes.GetProperty(
                "REAL_SCREEN_WIDTH",
                BindingFlags.Public | BindingFlags.Static);
            PropertyInfo realHeight = frameworkTypes.GetProperty(
                "REAL_SCREEN_HEIGHT",
                BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(realWidth);
            Assert.NotNull(realHeight);
            ScreenSizeManager manager = new(2560, 1440);
            MethodInfo windowRectChanged = typeof(ScreenSizeManager).GetMethod(
                "WindowRectChanged",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(windowRectChanged);

            _ = windowRectChanged.Invoke(manager, [new Rectangle(0, 0, 1024, 768)]);

            Assert.Equal(1024, manager.WindowWidth);
            Assert.Equal(768, manager.WindowHeight);
            Assert.Equal(1024f, Assert.IsType<float>(realWidth.GetValue(null)));
            Assert.Equal(768f, Assert.IsType<float>(realHeight.GetValue(null)));
        }

        [Fact]
        public void WindowResizePublishesAUnitDevicePixelRatio()
        {
            // MonoGame exposes no portable DPI scale, so desktop reports 1. That is correct on a
            // non-Retina display and conservative elsewhere: it can only make a physically-sized
            // element larger than today, never smaller.
            ScreenSizeManager manager = new(2560, 1440);
            MethodInfo windowRectChanged = typeof(ScreenSizeManager).GetMethod(
                "WindowRectChanged",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(windowRectChanged);

            _ = windowRectChanged.Invoke(manager, [new Rectangle(0, 0, 1920, 1080)]);

            Assert.Equal(1f, ReadPublishedDevicePixelRatio());
        }

        private static float ReadPublishedDevicePixelRatio()
        {
            Assembly coreAssembly = Assembly.Load("CutTheRopeDX.Core");
            Type presentation = coreAssembly.GetType(
                "CutTheRopeDX.Framework.Platform.ScreenPresentation");
            Assert.NotNull(presentation);
            object instance = presentation
                .GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)
                ?.GetValue(null);
            Assert.NotNull(instance);
            object snapshot = presentation
                .GetProperty("Snapshot", BindingFlags.Public | BindingFlags.Instance)
                ?.GetValue(instance);
            Assert.NotNull(snapshot);
            object ratio = snapshot.GetType()
                .GetProperty("DevicePixelRatio", BindingFlags.Public | BindingFlags.Instance)
                ?.GetValue(snapshot);
            return Assert.IsType<float>(ratio);
        }
    }
}
