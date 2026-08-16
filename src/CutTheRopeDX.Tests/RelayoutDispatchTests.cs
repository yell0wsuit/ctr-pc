using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers the layout pass dispatch: a controller is told to lay itself out once per genuine
    /// surface change, once when it becomes active, and never on an ordinary frame.
    /// </summary>
    public sealed class RelayoutDispatchTests
    {
        private sealed class CountingController : ViewController
        {
            public int RelayoutCount { get; private set; }

            public ViewportLayoutSnapshot LastSnapshot { get; private set; }

            /// <summary>Registers a view under slot 0 so <c>ShowView(0)</c> has something to show.</summary>
            public void WithView()
            {
                AddViewwithID(new View(), 0);
            }

            protected override void Relayout(ViewportLayoutSnapshot snapshot)
            {
                RelayoutCount++;
                LastSnapshot = snapshot;
            }
        }

        [Fact]
        public void ShowingAViewLaysTheControllerOutForTheCurrentSnapshot()
        {
            // ShowView rather than Activate: every real controller calls base.Activate() before
            // it builds its views, so laying out from Activate would always see an empty tree.
            _ = HeadlessGame.Boot();
            _ = ScreenPresentation.Instance.SetSurfaceSize(1280, 720);
            CountingController controller = new();
            controller.WithView();

            controller.ShowView(0);

            Assert.Equal(1, controller.RelayoutCount);
            Assert.Equal(1280, controller.LastSnapshot.SurfaceWidth);
        }

        [Fact]
        public void OrdinaryFramesDoNotLayOut()
        {
            _ = HeadlessGame.Boot();
            _ = ScreenPresentation.Instance.SetSurfaceSize(1280, 720);
            CountingController controller = new();
            controller.WithView();
            controller.ShowView(0);

            controller.Update(0.016f);
            controller.Update(0.016f);
            controller.Update(0.016f);

            Assert.Equal(1, controller.RelayoutCount);
        }

        [Fact]
        public void APushLaysOutExactlyOnce()
        {
            _ = HeadlessGame.Boot();
            _ = ScreenPresentation.Instance.SetSurfaceSize(1280, 720);
            CountingController controller = new();
            controller.WithView();
            controller.ShowView(0);

            _ = ScreenPresentation.Instance.SetSurfaceSize(1600, 900);
            controller.RelayoutTree(ScreenPresentation.Instance.Snapshot);

            Assert.Equal(2, controller.RelayoutCount);
            Assert.Equal(1600, controller.LastSnapshot.SurfaceWidth);
        }

        [Fact]
        public void APushReachesTheActiveChild()
        {
            _ = HeadlessGame.Boot();
            _ = ScreenPresentation.Instance.SetSurfaceSize(1280, 720);
            CountingController parent = new();
            CountingController child = new();
            parent.AddChildwithID(child, 0);
            parent.ActivateChild(0);
            int before = child.RelayoutCount;

            parent.RelayoutTree(ScreenPresentation.Instance.Snapshot);

            Assert.Equal(before + 1, child.RelayoutCount);
        }

        [Fact]
        public void APushTerminatesOnAControllerWithNoActiveChild()
        {
            // ActiveChild() would throw here: activeChildID is -1 and childs is a dictionary.
            // The walk must guard on that rather than rely on a null return.
            _ = HeadlessGame.Boot();
            _ = ScreenPresentation.Instance.SetSurfaceSize(1280, 720);
            CountingController parent = new();
            CountingController inactive = new();
            parent.AddChildwithID(inactive, 1);

            parent.RelayoutTree(ScreenPresentation.Instance.Snapshot);

            Assert.Equal(1, parent.RelayoutCount);
            Assert.Equal(0, inactive.RelayoutCount);
        }
    }
}
