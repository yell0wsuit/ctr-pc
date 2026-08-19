using CutTheRopeDX.Framework;
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

        /// <summary>A controller that keeps the base layout pass, so its views are sized by it.</summary>
        private sealed class PlainController : ViewController
        {
            public View View { get; } = new();

            public PlainController()
            {
                AddViewwithID(View, 0);
            }

            public void LayOut(ViewportLayoutSnapshot snapshot)
            {
                Relayout(snapshot);
            }
        }

        [Theory]
        [MemberData(nameof(Surfaces))]
        public void TheBaseLayoutPassSizesEveryViewToTheViewport(string name, int width, int height)
        {
            LayoutSurfaces.WithSurface(width, height, () =>
            {
                // Built at the default surface, then laid out at this one: a view that kept its
                // construction size would hold everything anchored to its edges or centered in it
                // wherever the previous viewport put them.
                PlainController controller = new();
                controller.LayOut(ScreenPresentation.Instance.Snapshot);

                CTRRectangle visible = ScreenPresentation.Instance.Snapshot.VisibleBounds;
                Assert.Equal((int)visible.w, controller.View.width);
                Assert.Equal((int)visible.h, controller.View.height);
                Assert.False(string.IsNullOrEmpty(name));
            });
        }

        public static TheoryData<string, int, int> Surfaces()
        {
            return LayoutSurfaces.Theory();
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
