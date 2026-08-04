using CutTheRopeDX.Desktop.Graphics;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class SoftwareRenderScaleTests
    {
        /// <summary>
        /// Feeds whole windows of identical frames, the shape the policy makes decisions on.
        /// </summary>
        private static void Feed(SoftwareRenderScale scale, double milliseconds, int windows = 1)
        {
            for (int window = 0; window < windows; window++)
            {
                for (int frame = 0; frame < SoftwareRenderScale.WindowFrames; frame++)
                {
                    scale.RecordFrame(milliseconds);
                }
            }
        }

        /// <summary>
        /// Windows a step down needs under a constant slow load: one to act, one spent settling, one to act
        /// again.
        /// </summary>
        private const double SlowMs = SoftwareRenderScale.StepDownAboveMs + 5d;

        private const double FastMs = SoftwareRenderScale.StepUpBelowMs - 2d;

        private const double BudgetMs = (SoftwareRenderScale.StepDownAboveMs + SoftwareRenderScale.StepUpBelowMs) / 2d;

        [Fact]
        public void StartsAtFullResolution()
        {
            Assert.Equal(SoftwareRenderScale.MinDivisor, new SoftwareRenderScale().Divisor);
        }

        [Fact]
        public void FullDivisorLeavesTheSizeAlone()
        {
            (int width, int height) = SoftwareRenderScale.Apply(1366, 768, 1);

            Assert.Equal(1366, width);
            Assert.Equal(768, height);
        }

        [Fact]
        public void HalvingProducesAWholeUpscaleOfTheOnScreenSize()
        {
            // 683 doubles back to 1366 and 384 to 768, so the blit stays on the point-sampled path.
            (int width, int height) = SoftwareRenderScale.Apply(1366, 768, 2);

            Assert.Equal(683, width);
            Assert.Equal(384, height);
        }

        [Fact]
        public void ApplyNeverProducesADegenerateSize()
        {
            (int width, int height) = SoftwareRenderScale.Apply(2, 1, SoftwareRenderScale.MaxDivisor);

            Assert.True(width > 0);
            Assert.True(height > 0);
        }

        [Fact]
        public void APartialWindowDecidesNothing()
        {
            SoftwareRenderScale scale = new();

            for (int frame = 0; frame < SoftwareRenderScale.WindowFrames - 1; frame++)
            {
                scale.RecordFrame(SlowMs);
            }

            Assert.Equal(SoftwareRenderScale.MinDivisor, scale.Divisor);
        }

        [Fact]
        public void OneSlowWindowStepsDownOnce()
        {
            SoftwareRenderScale scale = new();

            Feed(scale, SlowMs);

            Assert.Equal(2, scale.Divisor);
        }

        [Fact]
        public void TheWindowAfterAChangeIsSpentSettling()
        {
            SoftwareRenderScale scale = new();

            // The window straddling the change measures frames drawn at both sizes, so it must not act on
            // them: two slow windows are one step down, not two.
            Feed(scale, SlowMs, windows: 2);

            Assert.Equal(2, scale.Divisor);
        }

        [Fact]
        public void SustainedSlowLoadWalksToTheCoarsestDivisorAndStops()
        {
            SoftwareRenderScale scale = new();

            Feed(scale, SlowMs, windows: 40);

            Assert.Equal(SoftwareRenderScale.MaxDivisor, scale.Divisor);
        }

        [Fact]
        public void OneStalledFrameDoesNotDropTheResolution()
        {
            SoftwareRenderScale scale = new();

            // A garbage collection or a texture upload stalls a single frame. The median is what the policy
            // reads precisely so that frame cannot decide anything.
            for (int frame = 0; frame < SoftwareRenderScale.WindowFrames - 1; frame++)
            {
                scale.RecordFrame(FastMs);
            }
            scale.RecordFrame(500d);

            Assert.Equal(SoftwareRenderScale.MinDivisor, scale.Divisor);
        }

        [Fact]
        public void AQuietRunTooShortToQualifyDoesNotStepUp()
        {
            SoftwareRenderScale scale = new();
            Feed(scale, SlowMs);
            Assert.Equal(2, scale.Divisor);

            // One window settles the change; the rest are quiet but stop short of the required run.
            Feed(scale, FastMs, windows: SoftwareRenderScale.BaseGoodWindowsForStepUp);

            Assert.Equal(2, scale.Divisor);
        }

        [Fact]
        public void ASustainedQuietRunStepsBackUp()
        {
            SoftwareRenderScale scale = new();
            Feed(scale, SlowMs);

            Feed(scale, FastMs, windows: SoftwareRenderScale.BaseGoodWindowsForStepUp + 1);

            Assert.Equal(SoftwareRenderScale.MinDivisor, scale.Divisor);
        }

        [Fact]
        public void FramesInsideTheBudgetBreakTheQuietRun()
        {
            SoftwareRenderScale scale = new();
            Feed(scale, SlowMs);

            // Frame times between the two thresholds are the rate the divisor was chosen for. They are not
            // evidence that a coarser scene has become unnecessary, so the run has to start over.
            for (int repeat = 0; repeat < 5; repeat++)
            {
                Feed(scale, FastMs, windows: SoftwareRenderScale.BaseGoodWindowsForStepUp - 1);
                Feed(scale, BudgetMs);
            }

            Assert.Equal(2, scale.Divisor);
        }

        [Fact]
        public void AStepUpThatHadToBeUndoneMakesTheNextOneHarderToEarn()
        {
            SoftwareRenderScale scale = new();
            Feed(scale, SlowMs);
            Feed(scale, FastMs, windows: SoftwareRenderScale.BaseGoodWindowsForStepUp + 1);
            Assert.Equal(SoftwareRenderScale.MinDivisor, scale.Divisor);

            // Stepping up made the frame slow again, which says the quiet windows were measuring a cheaper
            // scene rather than a machine that had caught up.
            Feed(scale, SlowMs, windows: 2);
            Assert.Equal(2, scale.Divisor);

            // The run that earned the first step up must now no longer be enough.
            Feed(scale, FastMs, windows: SoftwareRenderScale.BaseGoodWindowsForStepUp + 1);
            Assert.Equal(2, scale.Divisor);

            Feed(scale, FastMs, windows: SoftwareRenderScale.BaseGoodWindowsForStepUp + 1);
            Assert.Equal(SoftwareRenderScale.MinDivisor, scale.Divisor);
        }

        [Fact]
        public void ResetDropsThePartialWindow()
        {
            SoftwareRenderScale scale = new();

            for (int frame = 0; frame < SoftwareRenderScale.WindowFrames - 1; frame++)
            {
                scale.RecordFrame(SlowMs);
            }
            scale.Reset();
            scale.RecordFrame(SlowMs);

            Assert.Equal(SoftwareRenderScale.MinDivisor, scale.Divisor);
        }

        [Fact]
        public void ResetDropsTheQuietRun()
        {
            SoftwareRenderScale scale = new();
            Feed(scale, SlowMs);
            Feed(scale, FastMs, windows: SoftwareRenderScale.BaseGoodWindowsForStepUp);

            scale.Reset();
            Feed(scale, FastMs);

            Assert.Equal(2, scale.Divisor);
        }

        [Fact]
        public void UnusableFrameTimesAreIgnored()
        {
            SoftwareRenderScale scale = new();

            for (int frame = 0; frame < SoftwareRenderScale.WindowFrames * 4; frame++)
            {
                scale.RecordFrame(double.NaN);
                scale.RecordFrame(-1d);
            }

            Assert.Equal(SoftwareRenderScale.MinDivisor, scale.Divisor);
        }

        [Fact]
        public void TheDivisorStaysInsideTheLadder()
        {
            SoftwareRenderScale scale = new();

            for (int repeat = 0; repeat < 10; repeat++)
            {
                Feed(scale, SlowMs, windows: 8);
                Assert.InRange(scale.Divisor, SoftwareRenderScale.MinDivisor, SoftwareRenderScale.MaxDivisor);
                Feed(scale, FastMs, windows: 8);
                Assert.InRange(scale.Divisor, SoftwareRenderScale.MinDivisor, SoftwareRenderScale.MaxDivisor);
            }
        }
    }
}
