using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class LevelFlowStateTests
    {
        private const float Frame = 0.016f;

        [Fact]
        public void FreshState_IsPlayingAndAllowsOutcomes()
        {
            LevelFlowState gameplayFlow = new();

            Assert.Equal(RestartPhase.Playing, gameplayFlow.Phase);
            Assert.True(gameplayFlow.CanTriggerOutcome);
            Assert.True(gameplayFlow.CanTriggerTerminalOutcome);
            Assert.True(gameplayFlow.CanReactToCandy());
        }

        [Fact]
        public void FadingOut_BlocksOutcomes()
        {
            LevelFlowState gameplayFlow = new();

            gameplayFlow.BeginRestartDim();

            Assert.Equal(RestartPhase.FadingOut, gameplayFlow.Phase);
            Assert.False(gameplayFlow.CanTriggerOutcome);
        }

        [Fact]
        public void Advance_FadingOut_RequestsSceneSwapThenCompletes()
        {
            LevelFlowState gameplayFlow = new();
            gameplayFlow.BeginRestartDim();

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
        public void Advance_AdvancesAsSoonAsDimIsExhausted()
        {
            // Level-triggered, not edge-triggered: a delta large enough to consume the whole dim
            // must advance the phase in the same call.
            LevelFlowState gameplayFlow = new();
            gameplayFlow.BeginRestartDim();

            Assert.Equal(RestartStep.SwapScene, gameplayFlow.Advance(1f));
        }

        [Fact]
        public void Advance_IsIdempotentOncePlaying()
        {
            LevelFlowState gameplayFlow = new();
            gameplayFlow.BeginRestartDim();
            _ = gameplayFlow.Advance(1f);
            _ = gameplayFlow.Advance(1f);
            Assert.Equal(RestartPhase.Playing, gameplayFlow.Phase);

            Assert.Equal(RestartStep.None, gameplayFlow.Advance(1f));
            Assert.Equal(RestartPhase.Playing, gameplayFlow.Phase);
        }

        [Fact]
        public void Advance_WhilePlaying_DoesNothing()
        {
            LevelFlowState gameplayFlow = new();

            Assert.Equal(RestartStep.None, gameplayFlow.Advance(Frame));
            Assert.Equal(RestartPhase.Playing, gameplayFlow.Phase);
        }

        [Fact]
        public void MarkWon_BlocksFurtherTerminalOutcomes()
        {
            LevelFlowState gameplayFlow = new();

            gameplayFlow.MarkWon();

            Assert.True(gameplayFlow.WonTriggered);
            Assert.True(gameplayFlow.TransitionActive);
            Assert.False(gameplayFlow.CanTriggerTerminalOutcome);
            Assert.False(gameplayFlow.CanReactToCandy());
        }

        [Fact]
        public void MarkLost_BlocksFurtherTerminalOutcomes()
        {
            LevelFlowState gameplayFlow = new();

            gameplayFlow.MarkLost();

            Assert.True(gameplayFlow.LostTriggered);
            Assert.True(gameplayFlow.TransitionActive);
            Assert.False(gameplayFlow.CanTriggerTerminalOutcome);
            Assert.False(gameplayFlow.CanReactToCandy());
        }

        [Fact]
        public void ScheduledLoss_BlocksCandyReactionsBeforeLossFires()
        {
            // Spider and hazard losses wait for their visual animation before MarkLost. The
            // transition gate must close immediately so candy cannot produce a win meanwhile.
            LevelFlowState gameplayFlow = new();

            gameplayFlow.MarkTransitionActive();

            Assert.True(gameplayFlow.TransitionActive);
            Assert.False(gameplayFlow.LostTriggered);
            Assert.False(gameplayFlow.CanReactToCandy());

            gameplayFlow.MarkLost();

            Assert.True(gameplayFlow.LostTriggered);
            Assert.False(gameplayFlow.CanTriggerTerminalOutcome);
        }

        [Fact]
        public void CanReactToCandy_False_WhenTargetAlreadyFed()
        {
            LevelFlowState gameplayFlow = new();

            Assert.False(gameplayFlow.CanReactToCandy(targetAlreadyFed: true));
        }

        [Fact]
        public void BeginRestartDim_AlwaysSetsANonZeroDim()
        {
            // This invariant is what makes a stranded restart unreachable: the phase is never
            // FadingOut with no dim left, so Advance always has something to consume.
            LevelFlowState gameplayFlow = new();

            gameplayFlow.BeginRestartDim();

            Assert.Equal(RestartPhase.FadingOut, gameplayFlow.Phase);
            Assert.True(gameplayFlow.DimTime > 0f);
        }

        [Fact]
        public void Reset_ClearsStrandedRestartPhase()
        {
            LevelFlowState gameplayFlow = new();
            gameplayFlow.BeginRestartDim();
            gameplayFlow.MarkLost();

            gameplayFlow.Reset();

            Assert.Equal(RestartPhase.Playing, gameplayFlow.Phase);
            Assert.False(gameplayFlow.LostTriggered);
            Assert.False(gameplayFlow.TransitionActive);
            Assert.True(gameplayFlow.CanTriggerOutcome);
        }

        [Fact]
        public void ResetOutcome_PreservesRestartPhase()
        {
            // Show() runs in the middle of a restart, between the two dim phases. Clearing the
            // phase there would wipe the in-flight restart.
            LevelFlowState gameplayFlow = new();
            gameplayFlow.BeginRestartDim();
            _ = gameplayFlow.Advance(1f);
            Assert.Equal(RestartPhase.FadingIn, gameplayFlow.Phase);
            gameplayFlow.MarkLost();

            gameplayFlow.ResetOutcome();

            Assert.Equal(RestartPhase.FadingIn, gameplayFlow.Phase);
            Assert.Equal(LevelFlowState.DimDuration, gameplayFlow.DimTime);
            Assert.False(gameplayFlow.LostTriggered);
        }
    }
}
