using CutTheRopeDX.Framework.Visual;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// A scrollable container withholds a press from its children for <c>touchPassTimeout</c> so it
    /// can tell a tap from a scroll, then hands it over on release. A pointer that wobbles by a
    /// unit between press and release must still count as a tap: at zero tolerance the wobble
    /// cancelled the hand-over and the click was swallowed, which made the skin selector need
    /// repeated clicking.
    /// </summary>
    public class ScrollableContainerTapTests
    {
        private sealed class RecordingChild : BaseElement
        {
            public int Downs;

            public int Ups;

            public override bool OnTouchDownXY(float tx, float ty)
            {
                Downs++;
                return true;
            }

            public override bool OnTouchUpXY(float tx, float ty)
            {
                Ups++;
                return true;
            }
        }

        private static (ScrollableContainer Container, RecordingChild Child) Build()
        {
            BaseElement content = new() { width = 400, height = 400 };
            RecordingChild child = new() { width = 100, height = 100, touchable = true };
            _ = content.AddChild(child);

            ScrollableContainer container =
                new ScrollableContainer().InitWithWidthHeightContainer(200f, 200f, content);
            container.width = 200;
            container.height = 200;
            return (container, child);
        }

        private static void Settle(ScrollableContainer container)
        {
            // Drain the deferred release the container schedules after handing over a tap.
            for (int i = 0; i < 60; i++)
            {
                container.Update(1f / 60f);
            }
        }

        [Fact]
        public void StillTapReachesTheChild()
        {
            (ScrollableContainer container, RecordingChild child) = Build();
            _ = container.OnTouchDownXY(50f, 50f);
            _ = container.OnTouchUpXY(50f, 50f);
            Settle(container);

            Assert.Equal(1, child.Downs);
            Assert.Equal(1, child.Ups);
        }

        [Fact]
        public void TapThatWobblesByOneUnitStillReachesTheChild()
        {
            (ScrollableContainer container, RecordingChild child) = Build();
            _ = container.OnTouchDownXY(50f, 50f);
            _ = container.OnTouchMoveXY(51f, 50f);
            _ = container.OnTouchUpXY(51f, 50f);
            Settle(container);

            Assert.Equal(1, child.Downs);
            Assert.Equal(1, child.Ups);
        }

        [Fact]
        public void RealDragIsNotDeliveredAsATap()
        {
            (ScrollableContainer container, RecordingChild child) = Build();
            _ = container.OnTouchDownXY(50f, 50f);
            for (int step = 1; step <= 10; step++)
            {
                _ = container.OnTouchMoveXY(50f + (step * 6f), 50f);
            }

            _ = container.OnTouchUpXY(110f, 50f);
            Settle(container);

            Assert.Equal(0, child.Downs);
        }
    }
}
