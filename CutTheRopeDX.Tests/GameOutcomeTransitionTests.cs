using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class GameOutcomeTransitionTests
    {
        [Fact]
        public void CanTriggerWin_FalseAfterLossTriggered()
        {
            Assert.False(GameOutcomeTransition.CanTriggerWin(gameWonTriggered: false, gameLostTriggered: true));
        }

        [Fact]
        public void CanTriggerWin_FalseAfterWinTriggered()
        {
            Assert.False(GameOutcomeTransition.CanTriggerWin(gameWonTriggered: true, gameLostTriggered: false));
        }

        [Fact]
        public void CanTriggerWin_TrueBeforeAnyOutcome()
        {
            Assert.True(GameOutcomeTransition.CanTriggerWin(gameWonTriggered: false, gameLostTriggered: false));
        }
    }
}
