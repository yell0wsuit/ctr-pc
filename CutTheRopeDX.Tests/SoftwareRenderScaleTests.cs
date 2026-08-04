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

        [Theory]
        [InlineData(480, 1)]
        [InlineData(540, 1)]
        [InlineData(600, 2)]
        [InlineData(720, 2)]
        [InlineData(768, 2)]
        [InlineData(1080, 2)]
        [InlineData(1440, 3)]
        [InlineData(2160, 4)]
        public void TheBaseDivisorBringsTheDisplayToTheTargetOrBelow(int onScreenHeight, int expected)
        {
            Assert.Equal(expected, SoftwareRenderScale.BaseDivisorFor(onScreenHeight));
        }

        [Theory]
        [InlineData(768)]
        [InlineData(1080)]
        [InlineData(1440)]
        [InlineData(2160)]
        public void TheBaseDivisorNeverLeavesTheSceneAboveTheTarget(int onScreenHeight)
        {
            int divisor = SoftwareRenderScale.BaseDivisorFor(onScreenHeight);

            Assert.True(onScreenHeight / divisor <= SoftwareRenderScale.TargetRenderLines);
        }

        [Theory]
        [InlineData(768)]
        [InlineData(1080)]
        [InlineData(1440)]
        [InlineData(2160)]
        public void TheBaseDivisorIsTheFinestThatReachesTheTarget(int onScreenHeight)
        {
            // Missing low is accepted; missing low by more than a step is waste, because every step costs
            // real sharpness for fill rate that was already inside the budget.
            int divisor = SoftwareRenderScale.BaseDivisorFor(onScreenHeight);

            Assert.True(divisor == SoftwareRenderScale.MinDivisor
                || onScreenHeight / (divisor - 1) > SoftwareRenderScale.TargetRenderLines);
        }

        [Fact]
        public void ADisplayAlreadyUnderTheTargetIsRenderedWhole()
        {
            Assert.Equal(SoftwareRenderScale.MinDivisor, SoftwareRenderScale.BaseDivisorFor(480));
        }

        [Fact]
        public void TheBaseDivisorIsTheFloorTheAdaptivePolicyReturnsTo()
        {
            SoftwareRenderScale scale = new();
            scale.SetBaseDivisor(2);

            // Quiet enough to undo any number of steps, but the base is not a step and cannot be undone.
            Feed(scale, FastMs, windows: (SoftwareRenderScale.BaseGoodWindowsForStepUp + 2) * 4);

            Assert.Equal(2, scale.Divisor);
        }

        [Fact]
        public void SlowFramesStepDownFromTheBaseAndStopAtTheAdaptiveLimit()
        {
            SoftwareRenderScale scale = new();
            scale.SetBaseDivisor(2);

            Feed(scale, SlowMs, windows: 40);

            Assert.Equal(2 + SoftwareRenderScale.MaxAdaptiveSteps, scale.Divisor);
            Assert.Equal(scale.MaxDivisor, scale.Divisor);
        }

        [Fact]
        public void AResizeKeepsTheStepsAlreadyTakenButRebasesThem()
        {
            SoftwareRenderScale scale = new();
            scale.SetBaseDivisor(2);
            Feed(scale, SlowMs, windows: 2);
            Assert.Equal(3, scale.Divisor);

            // Moving to a display that needs a coarser base does not undo what the machine already showed
            // it needs; the one step stays on top of the new base.
            scale.SetBaseDivisor(3);

            Assert.Equal(4, scale.Divisor);
        }

        [Fact]
        public void ARebaseDiscardsMeasurementsTakenAtTheOldSize()
        {
            SoftwareRenderScale scale = new();
            scale.SetBaseDivisor(2);
            Feed(scale, SlowMs, windows: 2);
            Assert.Equal(3, scale.Divisor);

            scale.SetBaseDivisor(4);
            // The window straight after the change is spent settling, so a slow one cannot act on frames
            // that were partly drawn at the previous size.
            Feed(scale, SlowMs);

            Assert.Equal(5, scale.Divisor);
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
            (int width, int height) = SoftwareRenderScale.Apply(2, 1, 8);

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

            Assert.Equal(scale.MaxDivisor, scale.Divisor);
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
                Assert.InRange(scale.Divisor, scale.BaseDivisor, scale.MaxDivisor);
                Feed(scale, FastMs, windows: 8);
                Assert.InRange(scale.Divisor, scale.BaseDivisor, scale.MaxDivisor);
            }
        }
    }
}
