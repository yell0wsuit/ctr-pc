using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class GameWinChewingTests
    {
        [Theory]
        [InlineData(0, true)]
        [InlineData(1, true)]
        [InlineData(2, false)]
        [InlineData(3, false)]
        public void ShouldPlayPrimaryChewingOnGameWon_OnlyForLegacySingleTargetWin(int targetCount, bool expected)
        {
            Assert.Equal(expected, GameWinChewing.ShouldPlayPrimaryChewingOnGameWon(targetCount));
        }
    }
}
