using CutTheRopeDX.Commons;
using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Platform;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Pins the screen coordinate transforms as they behave today. These record current behavior
    /// rather than asserting it is right: the square-with-crop case below produces a content
    /// rectangle wider than the surface, which is deliberately recorded rather than corrected.
    /// </summary>
    public sealed class CoordinateTransformCharacterizationTests
    {
        [Theory]
        [MemberData(nameof(Surfaces))]
        public void ScaledViewRectIsPinnedWithoutCropWidth(string name, int width, int height)
        {
            ScreenPresentation presentation = new(2560, 1440) { FullScreenCropWidth = false };
            presentation.SetSurfaceSize(width, height);

            (int expectedX, int expectedY, int expectedWidth, int expectedHeight) = name switch
            {
                "Native" => (0, 0, 2560, 1440),
                "SixteenNine" => (0, 0, 1280, 720),
                "FourThree" => (0, 96, 1024, 576),
                "Ultrawide" => (0, -180, 2560, 1440),
                "Superwide" => (0, -540, 3840, 2160),
                "Square" => (0, 218, 1000, 563),
                "Portrait" => (0, 437, 720, 405),
                "TallPortrait" => (0, 527, 400, 225),
                _ => throw new Xunit.Sdk.XunitException($"Missing expectations for {name}."),
            };

            Assert.Equal(expectedX, presentation.ScaledViewX);
            Assert.Equal(expectedY, presentation.ScaledViewY);
            Assert.Equal(expectedWidth, presentation.ScaledViewWidth);
            Assert.Equal(expectedHeight, presentation.ScaledViewHeight);
        }

        [Fact]
        public void SquareSurfaceWithCropWidthOverflowsTheSurfaceHorizontally()
        {
            // Recorded, not endorsed. Crop-width keeps full height and derives width from the
            // design aspect, which at 1:1 is wider than the surface, so x goes negative.
            ScreenPresentation presentation = new(2560, 1440) { FullScreenCropWidth = true };
            presentation.SetSurfaceSize(1000, 1000);

            Assert.Equal(1778, presentation.ScaledViewWidth);
            Assert.Equal(1000, presentation.ScaledViewHeight);
            Assert.Equal(-389, presentation.ScaledViewX);
        }

        [Theory]
        [MemberData(nameof(Surfaces))]
        public void ViewToGameRoundTripsTheCenterOfTheScaledView(string name, int width, int height)
        {
            ScreenPresentation presentation = new(2560, 1440) { FullScreenCropWidth = false };
            presentation.SetSurfaceSize(width, height);

            int viewX = presentation.ScaledViewWidth / 2;
            int viewY = presentation.ScaledViewHeight / 2;

            float gameX = presentation.TransformViewToGameX(viewX);
            float gameY = presentation.TransformViewToGameY(viewY);

            // 3.5 rather than a tighter bound because odd scaled-view heights round the
            // half-way pixel: 400x1280 gives a 225-high view whose center maps to 716.8.
            Assert.Equal(1280f, gameX, 3.5);
            Assert.Equal(720f, gameY, 3.5);
            Assert.False(string.IsNullOrEmpty(name));
        }

        [Theory]
        [MemberData(nameof(Surfaces))]
        public void LegacyPortraitViewGlobalsArePinnedAtEachSurface(string name, int width, int height)
        {
            LayoutSurfaces.WithSurface(width, height, () =>
            {
                CtrRenderer.OnSurfaceChanged(width, height);

                (float expectedX, float expectedY, float expectedWidth, float expectedHeight) =
                    name switch
                    {
                        "Native" => (800f, 0f, 960f, 1440f),
                        "SixteenNine" => (400f, 0f, 480f, 720f),
                        "FourThree" => (256f, 0f, 512f, 768f),
                        "Ultrawide" => (920f, 0f, 720f, 1080f),
                        "Superwide" => (1560f, 0f, 720f, 1080f),
                        "Square" => (166.66666f, 0f, 666.6667f, 1000f),
                        "Portrait" => (0f, 100f, 720f, 1080f),
                        "TallPortrait" => (0f, 340f, 400f, 600f),
                        _ => throw new Xunit.Sdk.XunitException($"Missing expectations for {name}."),
                    };

                Assert.Equal(expectedX, FrameworkTypes.VIEW_OFFSET_X, 0.01);
                Assert.Equal(expectedY, FrameworkTypes.VIEW_OFFSET_Y, 0.01);
                Assert.Equal(expectedWidth, FrameworkTypes.VIEW_SCREEN_WIDTH, 0.01);
                Assert.Equal(expectedHeight, FrameworkTypes.VIEW_SCREEN_HEIGHT, 0.01);
            });
        }

        [Theory]
        [MemberData(nameof(Surfaces))]
        public void RealAndLogicalTransformsAreInverses(string name, int width, int height)
        {
            LayoutSurfaces.WithSurface(width, height, () =>
            {
                CtrRenderer.OnSurfaceChanged(width, height);

                float roundTrippedX = FrameworkTypes.TransformFromRealX(
                    FrameworkTypes.TransformToRealX(640f));
                float roundTrippedY = FrameworkTypes.TransformFromRealY(
                    FrameworkTypes.TransformToRealY(360f));
                float roundTrippedWidth = FrameworkTypes.TransformFromRealW(
                    FrameworkTypes.TransformToRealW(640f));
                float roundTrippedHeight = FrameworkTypes.TransformFromRealH(
                    FrameworkTypes.TransformToRealH(360f));

                Assert.Equal(640f, roundTrippedX, 0.5);
                Assert.Equal(360f, roundTrippedY, 0.5);
                Assert.Equal(640f, roundTrippedWidth, 0.5);
                Assert.Equal(360f, roundTrippedHeight, 0.5);
                Assert.False(string.IsNullOrEmpty(name));
            });
        }

        [Theory]
        [MemberData(nameof(Surfaces))]
        public void RealScreenGlobalsFollowTheSurfaceThroughTheResizeEntryPoint(
            string name,
            int width,
            int height)
        {
            LayoutSurfaces.WithSurface(width, height, () =>
            {
                CtrRenderer.OnSurfaceChanged(width, height);

                Assert.Equal(width, FrameworkTypes.REAL_SCREEN_WIDTH);
                Assert.Equal(height, FrameworkTypes.REAL_SCREEN_HEIGHT);
                Assert.False(string.IsNullOrEmpty(name));
            });
        }

        public static TheoryData<string, int, int> Surfaces()
        {
            return LayoutSurfaces.Theory();
        }
    }
}
