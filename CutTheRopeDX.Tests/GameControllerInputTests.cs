using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class GameControllerInputTests
    {
        [Fact]
        public void CanPauseFromGameplay_FalseDuringOutcomeTransition()
        {
            Assert.False(GameControllerInput.CanPauseFromGameplay(
                gameplayHudTouchable: true,
                outcomeTransitionActive: true,
                restartDimActive: false));
        }

        [Fact]
        public void CanPauseFromGameplay_TrueWhenOutcomeTransitionInactive()
        {
            Assert.True(GameControllerInput.CanPauseFromGameplay(
                gameplayHudTouchable: true,
                outcomeTransitionActive: false,
                restartDimActive: false));
        }

        [Fact]
        public void CannotPause_WhileRestartDimIsPlaying()
        {
            Assert.False(GameControllerInput.CanPauseFromGameplay(
                gameplayHudTouchable: true,
                outcomeTransitionActive: false,
                restartDimActive: true));
        }

        [Fact]
        public void CanPause_OnceRestartDimHasFinished()
        {
            Assert.True(GameControllerInput.CanPauseFromGameplay(
                gameplayHudTouchable: true,
                outcomeTransitionActive: false,
                restartDimActive: false));
        }

        [Fact]
        public void CanExitResultWithBack_FalseDuringOutcomeTransition()
        {
            Assert.False(GameControllerInput.CanExitResultWithBack(
                resultTouchable: true,
                outcomeTransitionActive: true));
        }

        [Fact]
        public void CanExitResultWithBack_TrueAfterTransition()
        {
            Assert.True(GameControllerInput.CanExitResultWithBack(
                resultTouchable: true,
                outcomeTransitionActive: false));
        }
    }
}
