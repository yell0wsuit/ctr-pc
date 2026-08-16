using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers HUD chrome sizing. Chrome is sized from physical pixels rather than logical units
    /// so a touch target stays reachable on a dense display, where a logical size would shrink
    /// below a usable one.
    /// </summary>
    public sealed class HudMetricsTests
    {
        [Fact]
        public void ChromeGrowsWithTheLargerSurfaceDimension()
        {
            ViewportLayoutSnapshot small = ViewportLayout.Compute(1280, 720, true);
            ViewportLayoutSnapshot large = ViewportLayout.Compute(3840, 2160, true);

            // Compared in physical pixels, like the other two tests in this file: a design-unit
            // comparison would be resolution-invariant here, since both surfaces share an aspect
            // ratio and DPR, and design units are normalized against exactly that pair.
            Assert.True(
                HudMetrics.ChromeSize(large, false) * large.Scale
                > HudMetrics.ChromeSize(small, false) * small.Scale);
        }

        [Fact]
        public void ChromeNeverFallsBelowThePhysicalFloorOnDesktop()
        {
            // A small window must still produce a chrome size of at least 70 physical pixels.
            ViewportLayoutSnapshot snapshot = ViewportLayout.Compute(640, 360, true);

            float logical = HudMetrics.ChromeSize(snapshot, false);

            Assert.True(logical * snapshot.Scale >= 70f - 0.01f);
        }

        [Fact]
        public void LandscapeChromeIsSlightlySmallerThanPortrait()
        {
            // Famobi trims chrome by a tenth when the viewport is landscape, where vertical room
            // is the scarce axis.
            ViewportLayoutSnapshot landscape = ViewportLayout.Compute(2560, 1440, true);
            ViewportLayoutSnapshot portrait = ViewportLayout.Compute(1440, 2560, true);

            Assert.True(
                HudMetrics.ChromeSize(landscape, false) * landscape.Scale
                < HudMetrics.ChromeSize(portrait, false) * portrait.Scale);
        }
    }
}
