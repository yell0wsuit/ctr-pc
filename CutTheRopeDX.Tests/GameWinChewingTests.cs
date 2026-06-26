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

        [Theory]
        [InlineData(2, false, true, true)]
        [InlineData(3, false, true, true)]
        [InlineData(1, false, true, false)]
        [InlineData(2, true, true, false)]
        [InlineData(2, false, false, false)]
        public void ShouldSchedulePostEatSleep_OnlyForFlashMultiTargetNonNightLevels(
            int targetCount,
            bool isNightLevel,
            bool usesFlashXmlAnimations,
            bool expected)
        {
            Assert.Equal(expected, GameWinChewing.ShouldSchedulePostEatSleep(
                targetCount,
                isNightLevel,
                usesFlashXmlAnimations));
        }
    }
}
