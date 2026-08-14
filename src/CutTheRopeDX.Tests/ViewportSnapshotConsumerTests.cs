using CutTheRopeDX.Framework.Platform;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Guards the invariant the renderer and the pointer path both depend on: the rectangle
    /// content is drawn into and the rectangle pointer coordinates are unprojected through
    /// are the same rectangle, so a pointer lands on what was drawn under it.
    /// </summary>
    public sealed class ViewportSnapshotConsumerTests
    {
        [Theory]
        [InlineData(1280, 720)]
        [InlineData(2560, 1080)]
        [InlineData(1024, 768)]
        [InlineData(1000, 1000)]
        [InlineData(720, 1280)]
        public void PointerRoundTripsThroughTheLegacyContentBounds(int width, int height)
        {
            ScreenPresentation presentation = new(2560, 1440);
            _ = presentation.SetSurfaceSize(width, height, true);

            // Take the center of the drawn rectangle in surface pixels.
            float surfaceX = presentation.Snapshot.LegacyContentBounds.x
                + (presentation.Snapshot.LegacyContentBounds.w / 2f);
            float surfaceY = presentation.Snapshot.LegacyContentBounds.y
                + (presentation.Snapshot.LegacyContentBounds.h / 2f);

            float logicalX = presentation.TransformViewToGameX(
                presentation.TransformWindowToViewX((int)surfaceX));
            float logicalY = presentation.TransformViewToGameY(
                presentation.TransformWindowToViewY((int)surfaceY));

            // The center of the drawn rectangle is the center of the design space. Tolerance 1.0
            // because the center is taken in whole surface pixels: at 1024x768 the rectangle is
            // 1365 wide, so the integer center is half a pixel off and maps to 1279.06.
            Assert.Equal(ViewportLayout.DesignWidth / 2f, logicalX, 1.0);
            Assert.Equal(ViewportLayout.DesignHeight / 2f, logicalY, 1.0);
        }
    }
}
