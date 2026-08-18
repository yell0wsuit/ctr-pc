using System;

using CutTheRopeDX.Commons;
using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers the single resize entry point: one call publishes the snapshot, and every screen
    /// metric is read back out of it, so no host can update one without the other.
    /// </summary>
    public sealed class SurfaceChangeTests : IDisposable
    {
        /// <summary>
        /// Restores the default surface after each case, for the reason given on
        /// <see cref="PointerUnprojectionTests.Dispose"/>: every case here publishes a viewport
        /// and none of them owns the one the next test needs.
        /// </summary>
        public void Dispose()
        {
            ScreenPresentation.Instance = new ScreenPresentation(
                HeadlessHost.DefaultWidth, HeadlessHost.DefaultHeight);
            CtrRenderer.OnSurfaceChanged(HeadlessHost.DefaultWidth, HeadlessHost.DefaultHeight);
        }

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
        public void OnSurfaceChangedUpdatesTheScreenMetrics()
        {
            ScreenPresentation.Instance = new ScreenPresentation(2560, 1440);

            CtrRenderer.OnSurfaceChanged(1600, 900);

            Assert.Equal(1600f, FrameworkTypes.REAL_SCREEN_WIDTH);
            Assert.Equal(900f, FrameworkTypes.REAL_SCREEN_HEIGHT);
        }

        [Fact]
        public void TheScreenMetricsAgreeWithTheSnapshotAfterEveryPublish()
        {
            // The metrics read straight off the snapshot rather than being copied out of it when
            // it is published, so there is no second value a host could leave behind.
            ScreenPresentation.Instance = new ScreenPresentation(2560, 1440);

            CtrRenderer.OnSurfaceChanged(1600, 900);
            CtrRenderer.OnSurfaceChanged(1024, 768);

            Assert.Equal(ScreenPresentation.Instance.Snapshot.SurfaceWidth, (int)FrameworkTypes.REAL_SCREEN_WIDTH);
            Assert.Equal(ScreenPresentation.Instance.Snapshot.SurfaceHeight, (int)FrameworkTypes.REAL_SCREEN_HEIGHT);
        }

        [Fact]
        public void TheLogicalScreenSizeIsTheDesignSize()
        {
            // World coordinates mean the design box and nothing else. Publishing a surface of a
            // different shape must not move them, or every level's authored geometry moves with it.
            ScreenPresentation.Instance = new ScreenPresentation(2560, 1440);

            CtrRenderer.OnSurfaceChanged(720, 1280);

            Assert.Equal(ViewportLayout.DesignWidth, FrameworkTypes.SCREEN_WIDTH);
            Assert.Equal(ViewportLayout.DesignHeight, FrameworkTypes.SCREEN_HEIGHT);
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
        public void MatchingInitialSurfaceStillReportsTheScreenMetrics()
        {
            // A host whose first surface happens to equal the design size publishes no change.
            // The metrics still have to describe it, which they do by being read from the
            // snapshot rather than written during the transition.
            ScreenPresentation.Instance = new ScreenPresentation(2560, 1440);

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
        public void GenuineChangePushesOnceWhileAnEqualCallbackPushesNotAtAll()
        {
            RootController previousRoot = ProbeApplication.Root;
            try
            {
                CountingRootController root = new();
                ProbeApplication.Root = root;
                ScreenPresentation.Instance = new ScreenPresentation(2560, 1440);

                CtrRenderer.OnSurfaceChanged(1600, 900);

                Assert.Equal(1, root.RelayoutCount);

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
            _ = ScreenPresentation.Instance.SetSurfaceSize(1280, 720, 1f);

            bool changed = ScreenPresentation.Instance.SetSurfaceSize(1280, 720, 2f);

            Assert.True(changed);
            Assert.Equal(2f, ScreenPresentation.Instance.Snapshot.DevicePixelRatio);
        }

        [Fact]
        public void RepublishingAnIdenticalSurfaceAndRatioIsStillANoOp()
        {
            ScreenPresentation.Instance = new ScreenPresentation(2560, 1440);
            _ = ScreenPresentation.Instance.SetSurfaceSize(1280, 720, 2f);

            bool changed = ScreenPresentation.Instance.SetSurfaceSize(1280, 720, 2f);

            Assert.False(changed);
        }

        [Fact]
        public void BootPublishesTheHostDevicePixelRatio()
        {
            // A first frame published at the wrong ratio sizes physically-constrained chrome wrongly
            // and then corrects itself, which reads as a flicker rather than as a bug.
            ScreenPresentation.Instance = new ScreenPresentation(2560, 1440);

            CtrRenderer.OnSurfaceChanged(1280, 720, 2f);

            Assert.Equal(2f, ScreenPresentation.Instance.Snapshot.DevicePixelRatio);
        }
    }
}
