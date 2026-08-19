using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// The result panel is placed by fitting a 2560x1440 design box, but its pieces are authored
    /// against the menu_results art canvas, which is 2560x1597. The offset that reconciles the two
    /// is measured from the art's own anchor markers, so it is only as stable as the art: these
    /// pin the measurement to the shipped sheet, and fail if a re-export moves the markers.
    /// </summary>
    public class ResultPanelCenteringTests
    {
        [Fact]
        public void CenteringOffsetMatchesTheShippedArtMarkers()
        {
            _ = HeadlessGame.Boot();

            Vector offset = BoxOpenClose.PanelCenteringOffset();

            // Markers 0-11 span x 1051..1494 and y 377..1185, centering the body on (1272.5, 781)
            // against the design box's own (1280, 720).
            Assert.Equal(7.5f, offset.X, 1);
            Assert.Equal(-61f, offset.Y, 1);
        }

        [Fact]
        public void CenteringLiftsThePanelRatherThanDroppingIt()
        {
            _ = HeadlessGame.Boot();

            // The composition is authored low in a canvas taller than the box it is read in, so
            // the correction can only ever be upward. A positive value here would mean the
            // measurement had picked up the design box instead of the art.
            Assert.True(BoxOpenClose.PanelCenteringOffset().Y < 0f);
        }
    }
}
