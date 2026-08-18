using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers the scene-boundary pointer transform. A pointer over a fitted, scaled group must
    /// resolve to the design-space coordinate that was drawn under it, or touch and rendering
    /// disagree the moment a scene scales its content.
    /// </summary>
    public sealed class PointerUnprojectionTests
    {
        private sealed class ProbeController : ViewController
        {
            public CTRRectangle Fitted => FittedBox;

            public float Scale => FittedScale;

            public Vector ToDesign(float x, float y)
            {
                return PointerToDesignSpace(x, y);
            }
        }

        [Fact]
        public void FittedBoxFillsAViewportOfTheDesignAspect()
        {
            ScreenPresentation.Instance = new ScreenPresentation(2560, 1440);
            _ = ScreenPresentation.Instance.SetSurfaceSize(1280, 720, true);
            ProbeController controller = new();

            // No layout pass needed: FittedBox derives from the published snapshot on read.
            Assert.Equal(0f, controller.Fitted.x, 0.01);
            Assert.Equal(2560f, controller.Fitted.w, 0.01);
            Assert.Equal(1f, controller.Scale, 0.001);
        }

        [Fact]
        public void FittedBoxIsPillarboxedInAWiderViewport()
        {
            // 3840x1080 clamps to 2700x1080, giving 3600x1440 logical. A 16:9 design box is
            // height-limited there, so it keeps 2560x1440 and centers.
            ScreenPresentation.Instance = new ScreenPresentation(2560, 1440);
            _ = ScreenPresentation.Instance.SetSurfaceSize(3840, 1080, true);
            ProbeController controller = new();

            Assert.Equal(2560f, controller.Fitted.w, 0.01);
            Assert.Equal(1440f, controller.Fitted.h, 0.01);
            Assert.Equal(520f, controller.Fitted.x, 0.01);
            Assert.Equal(1f, controller.Scale, 0.001);
        }

        [Fact]
        public void PointerAtTheFittedOriginMapsToTheDesignOrigin()
        {
            ScreenPresentation.Instance = new ScreenPresentation(2560, 1440);
            _ = ScreenPresentation.Instance.SetSurfaceSize(3840, 1080, true);
            ProbeController controller = new();

            Vector design = controller.ToDesign(controller.Fitted.x, controller.Fitted.y);

            Assert.Equal(0f, design.X, 0.01);
            Assert.Equal(0f, design.Y, 0.01);
        }

        [Fact]
        public void PointerRoundTripsThroughAScaledFit()
        {
            // A portrait viewport forces the design box to shrink, so the scale is not 1 and
            // the inverse transform has to divide by it.
            ScreenPresentation.Instance = new ScreenPresentation(2560, 1440);
            _ = ScreenPresentation.Instance.SetSurfaceSize(720, 1280, true);
            ProbeController controller = new();

            float logicalX = controller.Fitted.x + (controller.Fitted.w / 2f);
            float logicalY = controller.Fitted.y + (controller.Fitted.h / 2f);

            Vector design = controller.ToDesign(logicalX, logicalY);

            Assert.Equal(ViewportLayout.DesignWidth / 2f, design.X, 0.01);
            Assert.Equal(ViewportLayout.DesignHeight / 2f, design.Y, 0.01);
            Assert.True(controller.Scale < 1f);
        }
    }
}
