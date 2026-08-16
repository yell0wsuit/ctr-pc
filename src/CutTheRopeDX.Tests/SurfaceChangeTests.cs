using CutTheRopeDX.Commons;
using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
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
        private sealed class ProbeApplication : Application
        {
            public static RootController Root
            {
                get => root;
                set => root = value;
            }
        }

        private sealed class CountingRootController : RootController
        {
            public CountingRootController()
                : base(null)
            {
            }

            public int RelayoutCount { get; private set; }

            protected override void Relayout(ViewportLayoutSnapshot snapshot)
            {
                RelayoutCount++;
            }
        }

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

        [Fact]
        public void MatchingInitialSurfaceStillInitializesTheLegacyGlobals()
        {
            ScreenPresentation.Instance = new ScreenPresentation(2560, 1440);
            FrameworkTypes.REAL_SCREEN_WIDTH = 480f;
            FrameworkTypes.REAL_SCREEN_HEIGHT = 800f;

            CtrRenderer.OnSurfaceChanged(2560, 1440);

            Assert.Equal(2560f, FrameworkTypes.REAL_SCREEN_WIDTH);
            Assert.Equal(1440f, FrameworkTypes.REAL_SCREEN_HEIGHT);
        }

        [Fact]
        public void SurfaceChangeBeforeApplicationLaunchDoesNotCreateTheRootController()
        {
            RootController previousRoot = ProbeApplication.Root;
            try
            {
                ProbeApplication.Root = null;
                ScreenPresentation.Instance = new ScreenPresentation(2560, 1440);

                CtrRenderer.OnSurfaceChanged(1600, 900);

                Assert.Null(ProbeApplication.Root);
            }
            finally
            {
                ProbeApplication.Root = previousRoot;
            }
        }

        [Fact]
        public void GenuineChangePushesOnceWhileEqualCallbackOnlyRefreshesLegacyGlobals()
        {
            RootController previousRoot = ProbeApplication.Root;
            try
            {
                CountingRootController root = new();
                ProbeApplication.Root = root;
                ScreenPresentation.Instance = new ScreenPresentation(2560, 1440);

                CtrRenderer.OnSurfaceChanged(1600, 900);

                Assert.Equal(1, root.RelayoutCount);

                FrameworkTypes.REAL_SCREEN_WIDTH = 480f;
                FrameworkTypes.REAL_SCREEN_HEIGHT = 800f;
                CtrRenderer.OnSurfaceChanged(1600, 900);

                Assert.Equal(1, root.RelayoutCount);
                Assert.Equal(1600f, FrameworkTypes.REAL_SCREEN_WIDTH);
                Assert.Equal(900f, FrameworkTypes.REAL_SCREEN_HEIGHT);
            }
            finally
            {
                ProbeApplication.Root = previousRoot;
            }
        }

        [Fact]
        public void ChangingOnlyTheDevicePixelRatioPublishesANewSnapshot()
        {
            // Moving a window between displays of different density changes nothing about the
            // surface size but must still reach HUD sizing, so it has to republish.
            ScreenPresentation.Instance = new ScreenPresentation(2560, 1440);
            _ = ScreenPresentation.Instance.SetSurfaceSize(1280, 720, true, 1f);

            bool changed = ScreenPresentation.Instance.SetSurfaceSize(1280, 720, true, 2f);

            Assert.True(changed);
            Assert.Equal(2f, ScreenPresentation.Instance.Snapshot.DevicePixelRatio);
        }

        [Fact]
        public void RepublishingAnIdenticalSurfaceAndRatioIsStillANoOp()
        {
            ScreenPresentation.Instance = new ScreenPresentation(2560, 1440);
            _ = ScreenPresentation.Instance.SetSurfaceSize(1280, 720, true, 2f);

            bool changed = ScreenPresentation.Instance.SetSurfaceSize(1280, 720, true, 2f);

            Assert.False(changed);
        }

        [Fact]
        public void BootPublishesTheHostDevicePixelRatio()
        {
            // A first frame published at the wrong ratio sizes physically-constrained chrome wrongly
            // and then corrects itself, which reads as a flicker rather than as a bug.
            ScreenPresentation.Instance = new ScreenPresentation(2560, 1440);

            CtrRenderer.OnSurfaceChanged(1280, 720, true, 2f);

            Assert.Equal(2f, ScreenPresentation.Instance.Snapshot.DevicePixelRatio);
        }
    }
}
