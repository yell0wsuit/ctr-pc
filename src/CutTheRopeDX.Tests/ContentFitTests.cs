using CutTheRopeDX.Framework.Platform;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers the content scale curve: the one function that decides how much larger than its
    /// authored size design-space content is drawn, which every controller, scene and element
    /// reads rather than deriving its own.
    /// </summary>
    public sealed class ContentFitTests
    {
        [Fact]
        public void TheDesignShapeDrawsContentAtItsAuthoredSize()
        {
            // The invariant the whole shipped composition rests on: at the shape the game was
            // drawn for, every layout rule must reduce to the constant it was authored with.
            Assert.Equal(1f, ContentFit.ScaleForAspect(ContentFit.DesignAspect), 0.0001);
        }

        [Theory]
        [InlineData(2.5f)]
        [InlineData(2.0f)]
        [InlineData(1.0f)]
        [InlineData(0.6f)]
        [InlineData(0.4f)]
        public void ContentNeverShrinksBelowItsAuthoredSize(float aspect)
        {
            // Departing from the design shape only ever spends room the composition is not using.
            // A scale below one would mean shrinking content to fit a screen that has room spare.
            Assert.True(ContentFit.ScaleForAspect(aspect) >= 1f);
        }

        [Fact]
        public void ScaleIsClampedOutsideTheSupportedAspectRange()
        {
            // Beyond the supported range the viewport is cropped to the limit, so the scale has to
            // stop there too; otherwise content keeps growing for width that is not being drawn.
            Assert.Equal(
                ContentFit.ScaleForAspect(ViewportLayout.MaxAspect),
                ContentFit.ScaleForAspect(ViewportLayout.MaxAspect * 2f),
                0.0001);
            Assert.Equal(
                ContentFit.ScaleForAspect(ViewportLayout.MinAspect),
                ContentFit.ScaleForAspect(ViewportLayout.MinAspect / 2f),
                0.0001);
        }

        [Fact]
        public void TheCurveIsContinuousAcrossTheDesignShape()
        {
            // The two branches meet at the design aspect. A step here would read as the whole
            // composition jumping size as a window is dragged through the shipped proportion.
            float justBelow = ContentFit.ScaleForAspect(ContentFit.DesignAspect - 0.001f);
            float justAbove = ContentFit.ScaleForAspect(ContentFit.DesignAspect + 0.001f);

            Assert.Equal(1f, justBelow, 0.01);
            Assert.Equal(1f, justAbove, 0.01);
        }

        [Fact]
        public void NarrowerViewportsGrowContentMonotonically()
        {
            // Every step toward portrait must grow content, never shrink it back: a curve that
            // reversed would make the UI pulse as a window is resized through the middle.
            float previous = ContentFit.ScaleForAspect(ContentFit.DesignAspect);
            for (float aspect = ContentFit.DesignAspect - 0.05f; aspect >= ViewportLayout.MinAspect; aspect -= 0.05f)
            {
                float current = ContentFit.ScaleForAspect(aspect);
                Assert.True(current >= previous, $"scale fell back at aspect {aspect}");
                previous = current;
            }
        }

        [Fact]
        public void ScaleTracksThePublishedViewport()
        {
            // The property and the pure function have to agree, or a caller reading one and a
            // test asserting the other would be checking nothing. Run through WithSurface so the
            // process-wide surface size is restored: the suite is serial, and gameplay tests
            // frame the world against whatever size the previous test left behind.
            LayoutSurfaces.WithSurface(720, 1280, () =>
            {
                Assert.Equal(
                    ContentFit.ScaleForAspect(ScreenPresentation.Instance.Snapshot.Aspect),
                    ContentFit.Scale,
                    0.0001);
                Assert.True(ContentFit.Scale > 1f);
            });
        }
    }
}
