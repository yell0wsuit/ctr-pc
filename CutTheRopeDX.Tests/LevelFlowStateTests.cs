using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class LevelFlowStateTests
    {
        private const float Frame = 0.016f;

        [Fact]
        public void FreshStateIsPlayingAndAllowsOutcomes()
        {
            LevelFlowState gameplayFlow = new();

            Assert.Equal(RestartPhase.Playing, gameplayFlow.Phase);
            Assert.Equal(LevelOutcomeState.Playing, gameplayFlow.Outcome);
            Assert.True(gameplayFlow.CanTriggerOutcome);
            Assert.True(gameplayFlow.CanRestart);
            Assert.True(gameplayFlow.CanReactToCandy());
        }

        [Fact]
        public void FadingOutBlocksOutcomes()
        {
            LevelFlowState gameplayFlow = new();

            Assert.True(gameplayFlow.TryBeginRestartDim());

            Assert.Equal(RestartPhase.FadingOut, gameplayFlow.Phase);
            Assert.False(gameplayFlow.CanTriggerOutcome);
        }

        [Fact]
        public void FadingOutAtomicallyRejectsEveryOutcomeTransition()
        {
            LevelFlowState winFlow = new();
            LevelFlowState immediateLossFlow = new();
            LevelFlowState scheduledLossFlow = new();
            Assert.True(winFlow.TryBeginRestartDim());
            Assert.True(immediateLossFlow.TryBeginRestartDim());
            Assert.True(scheduledLossFlow.TryBeginRestartDim());

            Assert.False(winFlow.TryBeginWin());
            Assert.False(immediateLossFlow.TryBeginLoss());
            Assert.False(scheduledLossFlow.TryScheduleLoss());
        }

        [Fact]
        public void AdvanceFadingOutRequestsSceneSwapThenCompletes()
        {
            LevelFlowState gameplayFlow = new();
            Assert.True(gameplayFlow.TryBeginRestartDim());

            RestartStep step = RestartStep.None;
            for (int i = 0; i < 60 && step != RestartStep.SwapScene; i++)
            {
                step = gameplayFlow.Advance(Frame);
            }
            Assert.Equal(RestartStep.SwapScene, step);
            Assert.Equal(RestartPhase.FadingIn, gameplayFlow.Phase);

            step = RestartStep.None;
            for (int i = 0; i < 60 && step != RestartStep.Completed; i++)
            {
                step = gameplayFlow.Advance(Frame);
            }
            Assert.Equal(RestartStep.Completed, step);
            Assert.Equal(RestartPhase.Playing, gameplayFlow.Phase);
        }

        [Fact]
        public void AdvanceAdvancesAsSoonAsDimIsExhausted()
        {
            // Level-triggered, not edge-triggered: a delta large enough to consume the whole dim
            // must advance the phase in the same call.
            LevelFlowState gameplayFlow = new();
            Assert.True(gameplayFlow.TryBeginRestartDim());

            Assert.Equal(RestartStep.SwapScene, gameplayFlow.Advance(1f));
        }

        [Fact]
        public void AdvanceIsIdempotentOncePlaying()
        {
            LevelFlowState gameplayFlow = new();
            Assert.True(gameplayFlow.TryBeginRestartDim());
            _ = gameplayFlow.Advance(1f);
            _ = gameplayFlow.Advance(1f);
            Assert.Equal(RestartPhase.Playing, gameplayFlow.Phase);

            Assert.Equal(RestartStep.None, gameplayFlow.Advance(1f));
            Assert.Equal(RestartPhase.Playing, gameplayFlow.Phase);
        }

        [Fact]
        public void AdvanceWhilePlayingDoesNothing()
        {
            LevelFlowState gameplayFlow = new();

            Assert.Equal(RestartStep.None, gameplayFlow.Advance(Frame));
            Assert.Equal(RestartPhase.Playing, gameplayFlow.Phase);
        }

        [Fact]
        public void BeginWinBlocksFurtherTerminalOutcomes()
        {
            LevelFlowState gameplayFlow = new();

            Assert.True(gameplayFlow.TryBeginWin());

            Assert.True(gameplayFlow.WonTriggered);
            Assert.True(gameplayFlow.TransitionActive);
            Assert.False(gameplayFlow.CanReactToCandy());
        }

        [Fact]
        public void BeginLossBlocksFurtherTerminalOutcomes()
        {
            LevelFlowState gameplayFlow = new();

            Assert.True(gameplayFlow.TryBeginLoss());

            Assert.True(gameplayFlow.LostTriggered);
            Assert.True(gameplayFlow.TransitionActive);
            Assert.False(gameplayFlow.CanReactToCandy());
        }

        [Fact]
        public void ScheduledLossBlocksCandyReactionsBeforeLossFires()
        {
            // Spider and hazard losses wait for their visual animation before TryBeginLoss. The
            // transition gate must close immediately so candy cannot produce a win meanwhile.
            LevelFlowState gameplayFlow = new();

            Assert.True(gameplayFlow.TryScheduleLoss());

            Assert.True(gameplayFlow.TransitionActive);
            Assert.False(gameplayFlow.LostTriggered);
            Assert.False(gameplayFlow.CanReactToCandy());

            Assert.True(gameplayFlow.TryBeginLoss());

            Assert.True(gameplayFlow.LostTriggered);
        }

        [Fact]
        public void PendingLossRejectsDuplicateSchedulingAndACompetingWin()
        {
            LevelFlowState gameplayFlow = new();

            Assert.True(gameplayFlow.TryScheduleLoss());

            Assert.Equal(LevelOutcomeState.PendingLoss, gameplayFlow.Outcome);
            Assert.False(gameplayFlow.TryScheduleLoss());
            Assert.False(gameplayFlow.TryBeginWin());
            Assert.Equal(LevelOutcomeState.PendingLoss, gameplayFlow.Outcome);
        }

        [Fact]
        public void NonPlayingOutcomesCannotTriggerAnotherOutcome()
        {
            LevelFlowState pendingLoss = new();
            LevelFlowState winning = new();
            LevelFlowState losing = new();
            LevelFlowState won = new();
            LevelFlowState lost = new();
            Assert.True(pendingLoss.TryScheduleLoss());
            Assert.True(winning.TryBeginWin());
            Assert.True(losing.TryBeginLoss());
            Assert.True(won.TryBeginWin());
            Assert.True(won.CompleteWinTransition());
            Assert.True(lost.TryBeginLoss());
            Assert.True(lost.TryBeginRestartDim());

            Assert.False(pendingLoss.CanTriggerOutcome);
            Assert.False(winning.CanTriggerOutcome);
            Assert.False(losing.CanTriggerOutcome);
            Assert.False(won.CanTriggerOutcome);
            Assert.False(lost.CanTriggerOutcome);
        }

        [Fact]
        public void EveryOutcomePresentationAllowsRestart()
        {
            LevelFlowState pendingLoss = new();
            LevelFlowState winning = new();
            LevelFlowState losing = new();
            Assert.True(pendingLoss.TryScheduleLoss());
            Assert.True(winning.TryBeginWin());
            Assert.True(losing.TryBeginLoss());

            Assert.True(pendingLoss.CanRestart);
            Assert.True(winning.CanRestart);
            Assert.True(losing.CanRestart);
            Assert.True(pendingLoss.TryBeginRestartDim());
            Assert.True(winning.TryBeginRestartDim());
            Assert.True(losing.TryBeginRestartDim());
            Assert.Equal(RestartPhase.FadingOut, pendingLoss.Phase);
            Assert.Equal(RestartPhase.FadingOut, winning.Phase);
            Assert.Equal(RestartPhase.FadingOut, losing.Phase);
        }

        [Fact]
        public void RestartInFlightPreventsAWinFromCompleting()
        {
            LevelFlowState gameplayFlow = new();
            Assert.True(gameplayFlow.TryBeginWin());
            Assert.True(gameplayFlow.TryBeginRestartDim());

            Assert.False(gameplayFlow.CompleteWinTransition());
            Assert.NotEqual(LevelOutcomeState.Won, gameplayFlow.Outcome);
        }

        [Fact]
        public void RestartDuringADimIsRejectedSoAPendingDispatchCannotResetIt()
        {
            LevelFlowState gameplayFlow = new();
            Assert.True(gameplayFlow.TryBeginLoss());
            Assert.True(gameplayFlow.TryBeginRestartDim());
            _ = gameplayFlow.Advance(LevelFlowState.DimDuration / 2f);
            float dimAfterPartialFade = gameplayFlow.DimTime;

            Assert.False(gameplayFlow.CanRestart);
            Assert.False(gameplayFlow.TryBeginRestartDim());
            Assert.Equal(dimAfterPartialFade, gameplayFlow.DimTime);
        }

        [Fact]
        public void PendingLossCanOnlyAdvanceToLosing()
        {
            LevelFlowState gameplayFlow = new();
            Assert.True(gameplayFlow.TryScheduleLoss());

            Assert.True(gameplayFlow.TryBeginLoss());

            Assert.Equal(LevelOutcomeState.Losing, gameplayFlow.Outcome);
            Assert.True(gameplayFlow.LostTriggered);
            Assert.False(gameplayFlow.WonTriggered);
        }

        [Fact]
        public void WinningCannotChangeToLosingAndCompletesAsWon()
        {
            LevelFlowState gameplayFlow = new();
            Assert.True(gameplayFlow.TryBeginWin());

            Assert.False(gameplayFlow.TryBeginLoss());
            Assert.True(gameplayFlow.CompleteWinTransition());

            Assert.Equal(LevelOutcomeState.Won, gameplayFlow.Outcome);
            Assert.True(gameplayFlow.WonTriggered);
            Assert.False(gameplayFlow.LostTriggered);
            Assert.False(gameplayFlow.TransitionActive);
            Assert.False(gameplayFlow.CanReactToCandy());
        }

        [Fact]
        public void LosingCompletesAsLostWhenRestartBegins()
        {
            LevelFlowState gameplayFlow = new();
            Assert.True(gameplayFlow.TryBeginLoss());

            Assert.True(gameplayFlow.TryBeginRestartDim());

            Assert.Equal(LevelOutcomeState.Lost, gameplayFlow.Outcome);
            Assert.True(gameplayFlow.LostTriggered);
            Assert.False(gameplayFlow.WonTriggered);
            Assert.False(gameplayFlow.TransitionActive);
            Assert.False(gameplayFlow.CanReactToCandy());
        }

        [Fact]
        public void CanReactToCandyFalseWhenTargetAlreadyFed()
        {
            LevelFlowState gameplayFlow = new();

            Assert.False(gameplayFlow.CanReactToCandy(targetAlreadyFed: true));
        }

        [Fact]
        public void TryBeginRestartDimAlwaysSetsANonZeroDim()
        {
            // This invariant is what makes a stranded restart unreachable: the phase is never
            // FadingOut with no dim left, so Advance always has something to consume.
            LevelFlowState gameplayFlow = new();

            Assert.True(gameplayFlow.TryBeginRestartDim());

            Assert.Equal(RestartPhase.FadingOut, gameplayFlow.Phase);
            Assert.True(gameplayFlow.DimTime > 0f);
        }

        [Fact]
        public void ResetClearsStrandedRestartPhase()
        {
            LevelFlowState gameplayFlow = new();
            Assert.True(gameplayFlow.TryBeginLoss());
            Assert.True(gameplayFlow.TryBeginRestartDim());

            gameplayFlow.Reset();

            Assert.Equal(RestartPhase.Playing, gameplayFlow.Phase);
            Assert.False(gameplayFlow.LostTriggered);
            Assert.False(gameplayFlow.TransitionActive);
            Assert.True(gameplayFlow.CanTriggerOutcome);
        }

        [Fact]
        public void ResetOutcomePreservesRestartPhase()
        {
            // Show() runs in the middle of a restart, between the two dim phases. Clearing the
            // phase there would wipe the in-flight restart.
            LevelFlowState gameplayFlow = new();
            Assert.True(gameplayFlow.TryBeginRestartDim());
            _ = gameplayFlow.Advance(1f);
            Assert.Equal(RestartPhase.FadingIn, gameplayFlow.Phase);
            Assert.True(gameplayFlow.TryBeginLoss());

            gameplayFlow.ResetOutcome();

            Assert.Equal(RestartPhase.FadingIn, gameplayFlow.Phase);
            Assert.Equal(LevelFlowState.DimDuration, gameplayFlow.DimTime);
            Assert.False(gameplayFlow.LostTriggered);
        }
    }
}
