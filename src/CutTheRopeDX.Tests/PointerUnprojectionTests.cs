using System;

using CutTheRopeDX.Commons;
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
    public sealed class PointerUnprojectionTests : IDisposable
    {
        /// <summary>
        /// Restores the default surface after each case. The surface size is process-wide and the
        /// suite runs serially, so a case that publishes a portrait viewport and walks away leaves
        /// every later test framing itself against it - which surfaces as a gameplay test whose
        /// tap lands somewhere the element is not.
        /// </summary>
        public void Dispose()
        {
            CtrRenderer.OnSurfaceChanged(HeadlessHost.DefaultWidth, HeadlessHost.DefaultHeight);
        }

        private sealed class ProbeController : ViewController
        {
            public CTRRectangle Box => DesignBox;

            public CTRRectangle Fitted => FittedBox;

            public static float Scale => FittedScale;

            public Vector ToDesign(float x, float y)
            {
                return PointerToDesignSpace(x, y);
            }
        }

        /// <summary>A controller that declares its own fixed box instead of the default.</summary>
        private sealed class FixedBoxController : ViewController
        {
            protected override CTRRectangle DesignBox => new(0f, 0f, 2560f, 1440f);

            public CTRRectangle Fitted => FittedBox;

            public static float Scale => FittedScale;
        }

        [Fact]
        public void FittedBoxFillsAViewportOfTheDesignAspect()
        {
            ScreenPresentation.Instance = new ScreenPresentation(2560, 1440);
            _ = ScreenPresentation.Instance.SetSurfaceSize(1280, 720);
            ProbeController controller = new();

            // No layout pass needed: FittedBox derives from the published snapshot on read.
            Assert.Equal(0f, controller.Fitted.x, 0.01);
            Assert.Equal(2560f, controller.Fitted.w, 0.01);
            Assert.Equal(1f, ProbeController.Scale, 0.001);
        }

        [Fact]
        public void TheDefaultBoxZoomsInRatherThanFillingAWideViewport()
        {
            // 3840x1080 is drawn whole, giving 5120x1440 logical at aspect 3.556. The box is the
            // authored one whatever the shape; the further the viewport departs from the design
            // aspect the larger the composition is drawn - up to the widest ratio the scale curve
            // distinguishes - and what it does not reach is left as margin rather than stretched
            // into.
            ScreenPresentation.Instance = new ScreenPresentation(2560, 1440);
            _ = ScreenPresentation.Instance.SetSurfaceSize(3840, 1080);
            ProbeController controller = new();

            Assert.Equal(2560f, controller.Box.w, 0.01);
            Assert.Equal(1440f, controller.Box.h, 0.01);
            Assert.Equal(1.15f, ProbeController.Scale, 0.001);
            Assert.True(
                ProbeController.Scale > 1f,
                $"a wide viewport should zoom in, got {ProbeController.Scale}");
        }

        [Fact]
        public void ANarrowerViewportDrawsTheCompositionLarger()
        {
            // 720x1280 is 9:16, giving 1440x2560 logical. Held at one scale the composition would
            // be sized for the shorter side alone and leave most of the long side unused, so it
            // grows instead. The box is wider than the viewport; what hangs over the sides is the
            // margin the background covers anyway.
            ScreenPresentation.Instance = new ScreenPresentation(2560, 1440);
            _ = ScreenPresentation.Instance.SetSurfaceSize(720, 1280);
            ProbeController controller = new();

            Assert.Equal(2560f, controller.Box.w, 0.01);
            Assert.Equal(1440f, controller.Box.h, 0.01);
            Assert.True(
                ProbeController.Scale > 1f,
                $"a portrait viewport should draw larger, got {ProbeController.Scale}");
        }

        [Fact]
        public void AControllerDeclaringItsOwnBoxKeepsThatShape()
        {
            // The declared-box path is retained for content that wants a shape of its own. It is
            // still drawn at the viewport's content scale - a modal has the same reason to be
            // legible on a phone as anything else - and centered in whatever room is left.
            ScreenPresentation.Instance = new ScreenPresentation(2560, 1440);
            _ = ScreenPresentation.Instance.SetSurfaceSize(3840, 1080);
            FixedBoxController controller = new();

            Assert.Equal(2560f * FixedBoxController.Scale, controller.Fitted.w, 0.01);
            Assert.Equal(1440f * FixedBoxController.Scale, controller.Fitted.h, 0.01);
            float visibleWidth = ScreenPresentation.Instance.Snapshot.VisibleBounds.w;
            Assert.Equal((visibleWidth - controller.Fitted.w) / 2f, controller.Fitted.x, 0.01);
        }

        [Fact]
        public void PointerAtTheFittedOriginMapsToTheDesignOrigin()
        {
            ScreenPresentation.Instance = new ScreenPresentation(2560, 1440);
            _ = ScreenPresentation.Instance.SetSurfaceSize(3840, 1080);
            ProbeController controller = new();

            Vector design = controller.ToDesign(controller.Fitted.x, controller.Fitted.y);

            Assert.Equal(0f, design.X, 0.01);
            Assert.Equal(0f, design.Y, 0.01);
        }

        [Fact]
        public void PointerRoundTripsThroughAScaledFit()
        {
            // A viewport past the design aspect scales the box, so the inverse transform has a
            // scale other than one to undo.
            ScreenPresentation.Instance = new ScreenPresentation(2560, 1440);
            _ = ScreenPresentation.Instance.SetSurfaceSize(3840, 1080);
            ProbeController controller = new();

            float logicalX = controller.Fitted.x + (controller.Fitted.w / 2f);
            float logicalY = controller.Fitted.y + (controller.Fitted.h / 2f);

            Vector design = controller.ToDesign(logicalX, logicalY);

            Assert.Equal(ViewportLayout.DesignWidth / 2f, design.X, 0.01);
            Assert.Equal(controller.Box.h / 2f, design.Y, 0.01);
            Assert.True(
                ProbeController.Scale > 1f,
                $"the fit must actually be scaled for this to prove anything, got {ProbeController.Scale}");
        }

        [Fact]
        public void SixteenNineIsTheIdentityCase()
        {
            // 2560 / (16/9) is exactly 1440, so at the aspect the game ships at the default box is
            // the authored box and the scale is 1. This is what keeps the 16:9 layout from moving.
            ScreenPresentation.Instance = new ScreenPresentation(2560, 1440);
            _ = ScreenPresentation.Instance.SetSurfaceSize(1280, 720);
            ProbeController controller = new();

            Assert.Equal(2560f, controller.Box.w, 0.01);
            Assert.Equal(1440f, controller.Box.h, 0.01);
            Assert.Equal(1f, ProbeController.Scale, 0.001);
        }
    }
}
