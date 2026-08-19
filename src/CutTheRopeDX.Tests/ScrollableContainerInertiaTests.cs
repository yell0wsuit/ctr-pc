using CutTheRopeDX.Framework.Visual;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Release momentum is measured over a window of frames rather than from the single last
    /// pointer event. The desktop host samples the mouse once per frame, but the browser host
    /// forwards every DOM pointer event straight through while logic still steps at a fixed
    /// 60 Hz, so a per-event measurement made the same physical flick come out at half strength
    /// on a 120 Hz phone and die outright when the last event before liftoff was a stray pixel.
    /// </summary>
    public class ScrollableContainerInertiaTests
    {
        private const float Step = 1f / 60f;

        /// <summary>Viewport height; content is far taller so scrolling is never bounded.</summary>
        private const float ViewportHeight = 200f;

        private static ScrollableContainer Build()
        {
            BaseElement content = new() { width = 200, height = 8000 };
            ScrollableContainer container =
                new ScrollableContainer().InitWithWidthHeightContainer(200f, ViewportHeight, content);
            container.width = 200;
            container.height = (int)ViewportHeight;
            return container;
        }

        /// <summary>
        /// Drags upward by <paramref name="perFrame"/> units a frame, split into
        /// <paramref name="eventsPerFrame"/> pointer events, and leaves the finger down.
        /// </summary>
        private static float Drag(ScrollableContainer container, float perFrame, int frames, int eventsPerFrame)
        {
            float y = 150f;
            _ = container.OnTouchDownXY(100f, y);
            for (int frame = 0; frame < frames; frame++)
            {
                for (int e = 0; e < eventsPerFrame; e++)
                {
                    y -= perFrame / eventsPerFrame;
                    _ = container.OnTouchMoveXY(100f, y);
                }

                container.Update(Step);
            }

            return y;
        }

        /// <summary>Runs the container until inertia settles and reports how far it glided.</summary>
        private static float GlideAfterRelease(ScrollableContainer container)
        {
            float before = container.GetScroll().Y;
            for (int i = 0; i < 600; i++)
            {
                container.Update(Step);
            }

            return container.GetScroll().Y - before;
        }

        [Fact]
        public void FlickStrengthDoesNotDependOnPointerEventRate()
        {
            ScrollableContainer oneEventPerFrame = Build();
            float endA = Drag(oneEventPerFrame, 20f, 5, 1);
            _ = oneEventPerFrame.OnTouchUpXY(100f, endA);
            float glideA = GlideAfterRelease(oneEventPerFrame);

            ScrollableContainer twoEventsPerFrame = Build();
            float endB = Drag(twoEventsPerFrame, 20f, 5, 2);
            _ = twoEventsPerFrame.OnTouchUpXY(100f, endB);
            float glideB = GlideAfterRelease(twoEventsPerFrame);

            Assert.True(glideA > 100f, $"expected a real glide, got {glideA}");
            Assert.Equal(glideA, glideB, 1);
        }

        [Fact]
        public void StrayPixelBeforeLiftoffDoesNotKillTheFlick()
        {
            ScrollableContainer clean = Build();
            float endClean = Drag(clean, 20f, 5, 1);
            _ = clean.OnTouchUpXY(100f, endClean);
            float glideClean = GlideAfterRelease(clean);

            ScrollableContainer jittered = Build();
            float endJitter = Drag(jittered, 20f, 5, 1);
            _ = jittered.OnTouchMoveXY(100f, endJitter - 1f);
            _ = jittered.OnTouchUpXY(100f, endJitter - 1f);
            float glideJitter = GlideAfterRelease(jittered);

            Assert.Equal(glideClean, glideJitter, 1);
        }

        [Fact]
        public void HoldingStillBeforeReleaseKillsInertia()
        {
            ScrollableContainer container = Build();
            float end = Drag(container, 20f, 5, 1);
            for (int i = 0; i < 10; i++)
            {
                container.Update(Step);
            }

            _ = container.OnTouchUpXY(100f, end);

            Assert.Equal(0f, GlideAfterRelease(container), 1);
        }

        [Fact]
        public void FastDragTracksTheFingerInsteadOfBeingClamped()
        {
            ScrollableContainer container = Build();
            _ = container.OnTouchDownXY(100f, 150f);
            _ = container.OnTouchMoveXY(100f, -50f);

            Assert.Equal(200f, container.GetScroll().Y, 1);
        }

        [Fact]
        public void PointerJumpBeyondTheGuardIsIgnoredAndDoesNotStickTheDrag()
        {
            ScrollableContainer container = Build();
            _ = container.OnTouchDownXY(100f, 150f);
            _ = container.OnTouchMoveXY(100f, -600f);

            Assert.Equal(0f, container.GetScroll().Y, 1);

            // The next delta must be measured from where the pointer actually is, or every
            // event after a jump would repeat the same rejected distance.
            _ = container.OnTouchMoveXY(100f, -650f);

            Assert.Equal(50f, container.GetScroll().Y, 1);
        }
    }
}
