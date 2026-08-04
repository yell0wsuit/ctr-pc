using CutTheRopeDX.Desktop;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class SoftwareRenderingWidthCapTests
    {
        [Fact]
        public void HardwareRenderingLeavesTheRenderSizeAlone()
        {
            (int width, int height) = ScreenSizeManager.CapRenderSize(1920, 1080, softwareRendering: false);

            Assert.Equal(1920, width);
            Assert.Equal(1080, height);
        }

        [Fact]
        public void SoftwareRenderingCapsASizeAboveTheLimit()
        {
            // 1080p is where SwiftShader stops being playable, so it must come down to the cap.
            (int width, int height) = ScreenSizeManager.CapRenderSize(1920, 1080, softwareRendering: true);

            Assert.Equal(ScreenSizeManager.MAX_SOFTWARE_RENDER_WIDTH, width);
            Assert.Equal(768, height);
        }

        [Fact]
        public void SoftwareRenderingLeavesASizeAlreadyUnderTheLimitAlone()
        {
            (int width, int height) = ScreenSizeManager.CapRenderSize(1280, 720, softwareRendering: true);

            Assert.Equal(1280, width);
            Assert.Equal(720, height);
        }

        [Fact]
        public void SoftwareRenderingPreservesAspectRatioWhenCapping()
        {
            // The capped target is stretched back over the full on-screen rect, so a changed
            // aspect ratio here would show up as a distorted picture.
            (int width, int height) = ScreenSizeManager.CapRenderSize(2560, 1440, softwareRendering: true);

            Assert.Equal(ScreenSizeManager.MAX_SOFTWARE_RENDER_WIDTH, width);
            Assert.Equal(2560d / 1440d, width / (double)height, precision: 2);
        }

        [Fact]
        public void SoftwareRenderingNeverProducesADegenerateSize()
        {
            (int width, int height) = ScreenSizeManager.CapRenderSize(4000, 1, softwareRendering: true);

            Assert.True(width > 0);
            Assert.True(height > 0);
        }
    }
}
