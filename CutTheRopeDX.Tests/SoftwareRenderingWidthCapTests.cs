using CutTheRopeDX.Desktop;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class SoftwareRenderingWidthCapTests
    {
        [Fact]
        public void HardwareRenderingLeavesTheWidthAlone()
        {
            Assert.Equal(1920, ScreenSizeManager.CapWidthForSoftwareRendering(1920, softwareRendering: false));
        }

        [Fact]
        public void SoftwareRenderingCapsAWidthAboveTheLimit()
        {
            // 1080p is where SwiftShader stops being playable, so it must come down to the cap.
            Assert.Equal(
                ScreenSizeManager.MAX_SOFTWARE_WINDOW_WIDTH,
                ScreenSizeManager.CapWidthForSoftwareRendering(1920, softwareRendering: true));
        }

        [Fact]
        public void SoftwareRenderingLeavesAWidthAlreadyUnderTheLimitAlone()
        {
            Assert.Equal(1280, ScreenSizeManager.CapWidthForSoftwareRendering(1280, softwareRendering: true));
        }

        [Fact]
        public void SoftwareRenderingNeverCapsBelowTheMinimumWindowWidth()
        {
            // The cap must not fight the floor the rest of the sizing code relies on.
            Assert.True(ScreenSizeManager.MAX_SOFTWARE_WINDOW_WIDTH >= ScreenSizeManager.MIN_WINDOW_WIDTH);
        }
    }
}
