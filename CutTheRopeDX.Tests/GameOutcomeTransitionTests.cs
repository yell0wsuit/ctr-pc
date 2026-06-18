using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class GameOutcomeTransitionTests
    {
        [Fact]
        public void CanTriggerTerminalOutcome_FalseAfterLossTriggered()
        {
            Assert.False(GameOutcomeTransition.CanTriggerTerminalOutcome(gameWonTriggered: false, gameLostTriggered: true));
        }

        [Fact]
        public void CanTriggerTerminalOutcome_FalseAfterWinTriggered()
        {
            Assert.False(GameOutcomeTransition.CanTriggerTerminalOutcome(gameWonTriggered: true, gameLostTriggered: false));
        }

        [Fact]
        public void CanTriggerTerminalOutcome_TrueBeforeAnyOutcome()
        {
            Assert.True(GameOutcomeTransition.CanTriggerTerminalOutcome(gameWonTriggered: false, gameLostTriggered: false));
        }

        [Fact]
        public void CanConsumeCandy_TrueWhileNoOutcomeTransition()
        {
            Assert.True(GameOutcomeTransition.CanConsumeCandy(outcomeTransitionActive: false));
        }

        [Fact]
        public void CanConsumeCandy_FalseOnceOutcomeTransitionActive()
        {
            Assert.False(GameOutcomeTransition.CanConsumeCandy(outcomeTransitionActive: true));
        }
    }
}
