using CutTheRopeDX.Desktop.Graphics;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class RenderDiagnosticsTests
    {
        [Fact]
        public void TheSoftwareReadoutCarriesTheFrameTimeAndTheDivisor()
        {
            // These two together are what a single run on an unfamiliar machine has to answer: whether the
            // scene is where the time goes, and how far the renderer already backed off to get there.
            string line = RenderDiagnostics.Format(48, 20.44, 2, 683, 384, softwareRendering: true);

            Assert.Equal("48fps 20.4ms 1/2 683x384 sw", line);
        }

        [Fact]
        public void TheHardwareReadoutOmitsWhatNothingMeasures()
        {
            // Nothing feeds the policy on the hardware path, so a frame time here would be a stale zero.
            string line = RenderDiagnostics.Format(60, 0d, 1, 1920, 1080, softwareRendering: false);

            Assert.Equal("60fps 1920x1080 hw", line);
        }

        [Fact]
        public void TheReadoutIsCultureInvariant()
        {
            // The overlay draws through a bitmap font with a limited glyph set, and a decimal comma from a
            // localized machine is not in it.
            System.Globalization.CultureInfo previous = System.Globalization.CultureInfo.CurrentCulture;
            try
            {
                System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("de-DE");
                string line = RenderDiagnostics.Format(30, 33.5, 3, 455, 256, softwareRendering: true);

                Assert.Contains("33.5ms", line);
            }
            finally
            {
                System.Globalization.CultureInfo.CurrentCulture = previous;
            }
        }
    }
}
