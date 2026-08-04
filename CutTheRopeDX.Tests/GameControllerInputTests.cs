using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class GameControllerInputTests
    {
        [Fact]
        public void CanPauseFromGameplayFalseDuringOutcomeTransition()
        {
            Assert.False(GameControllerInput.CanPauseFromGameplay(
                gameplayHudTouchable: true,
                outcomeTransitionActive: true,
                restartDimActive: false));
        }

        [Fact]
        public void CanPauseFromGameplayTrueWhenOutcomeTransitionInactive()
        {
            Assert.True(GameControllerInput.CanPauseFromGameplay(
                gameplayHudTouchable: true,
                outcomeTransitionActive: false,
                restartDimActive: false));
        }

        [Fact]
        public void CannotPauseWhileRestartDimIsPlaying()
        {
            Assert.False(GameControllerInput.CanPauseFromGameplay(
                gameplayHudTouchable: true,
                outcomeTransitionActive: false,
                restartDimActive: true));
        }

        [Fact]
        public void CanPauseOnceRestartDimHasFinished()
        {
            Assert.True(GameControllerInput.CanPauseFromGameplay(
                gameplayHudTouchable: true,
                outcomeTransitionActive: false,
                restartDimActive: false));
        }

        [Fact]
        public void CanExitResultWithBackFalseDuringOutcomeTransition()
        {
            Assert.False(GameControllerInput.CanExitResultWithBack(
                resultTouchable: true,
                outcomeTransitionActive: true));
        }

        [Fact]
        public void CanExitResultWithBackTrueAfterTransition()
        {
            Assert.True(GameControllerInput.CanExitResultWithBack(
                resultTouchable: true,
                outcomeTransitionActive: false));
        }
    }
}
