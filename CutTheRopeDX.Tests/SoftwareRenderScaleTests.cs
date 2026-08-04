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

        private const double SlowMs = SoftwareRenderScale.StepDownAboveMs + 5d;

        private const double FastMs = SoftwareRenderScale.StepUpBelowMs - 2d;

        private const double BudgetMs = (SoftwareRenderScale.StepDownAboveMs + SoftwareRenderScale.StepUpBelowMs) / 2d;

        [Fact]
        public void StartsOnTheTallestRung()
        {
            SoftwareRenderScale scale = new();

            Assert.Equal(0, scale.Step);
            Assert.Equal(SoftwareRenderScale.RenderLineLadder[0], scale.TargetLines);
        }

        [Fact]
        public void TheLadderRunsFromTallestToShortest()
        {
            int[] ladder = SoftwareRenderScale.RenderLineLadder;

            for (int rung = 1; rung < ladder.Length; rung++)
            {
                Assert.True(ladder[rung] < ladder[rung - 1]);
            }
        }

        [Fact]
        public void TheFirstRungIsTheSweetSpot()
        {
            // 540 lines is the height the picture was judged on; the rungs below it are reserves.
            Assert.Equal(540, SoftwareRenderScale.RenderLineLadder[0]);
        }

        [Fact]
        public void FittingA768LineDisplayGivesTheTargetHeight()
        {
            (int width, int height) = SoftwareRenderScale.Apply(1366, 768, 540);

            Assert.Equal(540, height);
            Assert.Equal(960, width);
        }

        [Fact]
        public void FittingKeepsTheAspectRatio()
        {
            (int width, int height) = SoftwareRenderScale.Apply(1366, 768, 540);

            // The render target is stretched back over the whole on-screen rect, so a drifted aspect ratio
            // would show as a distorted picture rather than a smaller one.
            Assert.Equal(1366d / 768d, width / (double)height, precision: 2);
        }

        [Theory]
        [InlineData(1920, 1080, 960, 540)]
        [InlineData(3840, 2160, 1920, 1080)]
        public void DisplaysThatDivideEvenlyKeepAWholeUpscale(int onScreenWidth, int onScreenHeight, int expectedWidth, int expectedHeight)
        {
            // These are the displays where the blit still lands on the point-sampled path for free, so the
            // fit has to come out exactly rather than a pixel off.
            (int width, int height) = SoftwareRenderScale.Apply(onScreenWidth, onScreenHeight, onScreenHeight / 2);

            Assert.Equal(expectedWidth, width);
            Assert.Equal(expectedHeight, height);
        }

        [Fact]
        public void ADisplayAlreadyUnderTheTargetIsRenderedWhole()
        {
            (int width, int height) = SoftwareRenderScale.Apply(854, 480, 540);

            Assert.Equal(854, width);
            Assert.Equal(480, height);
        }

        [Fact]
        public void FittingNeverProducesADegenerateSize()
        {
            (int width, int height) = SoftwareRenderScale.Apply(2, 4000, 1);

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

            Assert.Equal(0, scale.Step);
        }

        [Fact]
        public void OneSlowWindowStepsDownOnce()
        {
            SoftwareRenderScale scale = new();

            Feed(scale, SlowMs);

            Assert.Equal(1, scale.Step);
        }

        [Fact]
        public void TheWindowAfterAChangeIsSpentSettling()
        {
            SoftwareRenderScale scale = new();

            // The window straddling the change measures frames drawn at both sizes, so it must not act on
            // them: two slow windows are one step down, not two.
            Feed(scale, SlowMs, windows: 2);

            Assert.Equal(1, scale.Step);
        }

        [Fact]
        public void SustainedSlowLoadWalksToTheShortestRungAndStops()
        {
            SoftwareRenderScale scale = new();

            Feed(scale, SlowMs, windows: 40);

            Assert.Equal(SoftwareRenderScale.MaxStep, scale.Step);
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

            Assert.Equal(0, scale.Step);
        }

        [Fact]
        public void AQuietRunTooShortToQualifyDoesNotStepUp()
        {
            SoftwareRenderScale scale = new();
            Feed(scale, SlowMs);
            Assert.Equal(1, scale.Step);

            // One window settles the change; the rest are quiet but stop short of the required run.
            Feed(scale, FastMs, windows: SoftwareRenderScale.BaseGoodWindowsForStepUp);

            Assert.Equal(1, scale.Step);
        }

        [Fact]
        public void ASustainedQuietRunStepsBackUp()
        {
            SoftwareRenderScale scale = new();
            Feed(scale, SlowMs);

            Feed(scale, FastMs, windows: SoftwareRenderScale.BaseGoodWindowsForStepUp + 1);

            Assert.Equal(0, scale.Step);
        }

        [Fact]
        public void TheTallestRungIsTheCeilingTheAdaptivePolicyReturnsTo()
        {
            SoftwareRenderScale scale = new();

            // Quiet enough to undo any number of steps, but there is nothing above the first rung.
            Feed(scale, FastMs, windows: (SoftwareRenderScale.BaseGoodWindowsForStepUp + 2) * 4);

            Assert.Equal(0, scale.Step);
        }

        [Fact]
        public void FramesInsideTheBudgetBreakTheQuietRun()
        {
            SoftwareRenderScale scale = new();
            Feed(scale, SlowMs);

            // Frame times between the two thresholds are the rate the step was chosen for. They are not
            // evidence that a shorter scene has become unnecessary, so the run has to start over.
            for (int repeat = 0; repeat < 5; repeat++)
            {
                Feed(scale, FastMs, windows: SoftwareRenderScale.BaseGoodWindowsForStepUp - 1);
                Feed(scale, BudgetMs);
            }

            Assert.Equal(1, scale.Step);
        }

        [Fact]
        public void AStepUpThatHadToBeUndoneMakesTheNextOneHarderToEarn()
        {
            SoftwareRenderScale scale = new();
            Feed(scale, SlowMs);
            Feed(scale, FastMs, windows: SoftwareRenderScale.BaseGoodWindowsForStepUp + 1);
            Assert.Equal(0, scale.Step);

            // Stepping up made the frame slow again, which says the quiet windows were measuring a cheaper
            // scene rather than a machine that had caught up.
            Feed(scale, SlowMs, windows: 2);
            Assert.Equal(1, scale.Step);

            // The run that earned the first step up must now no longer be enough.
            Feed(scale, FastMs, windows: SoftwareRenderScale.BaseGoodWindowsForStepUp + 1);
            Assert.Equal(1, scale.Step);

            Feed(scale, FastMs, windows: SoftwareRenderScale.BaseGoodWindowsForStepUp + 1);
            Assert.Equal(0, scale.Step);
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

            Assert.Equal(0, scale.Step);
        }

        [Fact]
        public void ResetDropsTheQuietRun()
        {
            SoftwareRenderScale scale = new();
            Feed(scale, SlowMs);
            Feed(scale, FastMs, windows: SoftwareRenderScale.BaseGoodWindowsForStepUp);

            scale.Reset();
            Feed(scale, FastMs);

            Assert.Equal(1, scale.Step);
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

            Assert.Equal(0, scale.Step);
        }

        [Fact]
        public void TheStepStaysOnTheLadder()
        {
            SoftwareRenderScale scale = new();

            for (int repeat = 0; repeat < 10; repeat++)
            {
                Feed(scale, SlowMs, windows: 8);
                Assert.InRange(scale.Step, 0, SoftwareRenderScale.MaxStep);
                Feed(scale, FastMs, windows: 8);
                Assert.InRange(scale.Step, 0, SoftwareRenderScale.MaxStep);
            }
        }
    }
}
