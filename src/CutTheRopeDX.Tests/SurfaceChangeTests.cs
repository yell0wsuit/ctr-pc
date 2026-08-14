using CutTheRopeDX.Commons;
using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Platform;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers the single resize entry point: one call publishes the snapshot and brings the
    /// legacy screen globals with it, so no host can update one without the other.
    /// </summary>
    public sealed class SurfaceChangeTests
    {
        [Fact]
        public void OnSurfaceChangedPublishesTheSnapshot()
        {
            ScreenPresentation.Instance = new ScreenPresentation(2560, 1440);

            CtrRenderer.OnSurfaceChanged(1600, 900);

            Assert.Equal(1600, ScreenPresentation.Instance.Snapshot.SurfaceWidth);
            Assert.Equal(900, ScreenPresentation.Instance.Snapshot.SurfaceHeight);
        }

        [Fact]
        public void OnSurfaceChangedUpdatesTheLegacyScreenGlobals()
        {
            ScreenPresentation.Instance = new ScreenPresentation(2560, 1440);

            CtrRenderer.OnSurfaceChanged(1600, 900);

            Assert.Equal(1600f, FrameworkTypes.REAL_SCREEN_WIDTH);
            Assert.Equal(900f, FrameworkTypes.REAL_SCREEN_HEIGHT);
        }

        [Fact]
        public void TheGlobalsAgreeWithTheSnapshotAfterEveryPublish()
        {
            // The globals are derived from the snapshot inside the same call that publishes it.
            // If they can ever disagree, something updated one without the other.
            ScreenPresentation.Instance = new ScreenPresentation(2560, 1440);

            CtrRenderer.OnSurfaceChanged(1600, 900);
            CtrRenderer.OnSurfaceChanged(1024, 768);

            Assert.Equal(ScreenPresentation.Instance.Snapshot.SurfaceWidth, (int)FrameworkTypes.REAL_SCREEN_WIDTH);
            Assert.Equal(ScreenPresentation.Instance.Snapshot.SurfaceHeight, (int)FrameworkTypes.REAL_SCREEN_HEIGHT);
        }

        [Fact]
        public void RepeatingTheSameSizeLeavesTheSnapshotUntouched()
        {
            ScreenPresentation.Instance = new ScreenPresentation(2560, 1440);
            CtrRenderer.OnSurfaceChanged(1600, 900);
            ViewportLayoutSnapshot afterFirst = ScreenPresentation.Instance.Snapshot;

            CtrRenderer.OnSurfaceChanged(1600, 900);

            Assert.Equal(afterFirst, ScreenPresentation.Instance.Snapshot);
        }
    }
}
