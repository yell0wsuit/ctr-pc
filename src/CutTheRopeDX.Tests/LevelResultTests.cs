using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class LevelResultTests
    {
        [Theory]
        [InlineData(30f, 0, 0f, 0, 0)]
        [InlineData(40f, 3, 0f, 3000, 3000)]
        [InlineData(12.5f, 2, 1750f, 2000, 3750)]
        [InlineData(29.875f, 2, 12.5f, 2000, 2013)]
        public void CalculatePreservesPreciseTimeBonusUntilFinalCeiling(
            float elapsedTime,
            int stars,
            float expectedTimeBonus,
            int expectedStarBonus,
            int expectedFinalScore)
        {
            LevelResult result = LevelResultCalculator.Calculate(elapsedTime, stars);

            Assert.Equal(elapsedTime, result.ElapsedTime);
            Assert.Equal(stars, result.StarsCollected);
            Assert.Equal(expectedTimeBonus, result.TimeBonus);
            Assert.Equal(expectedStarBonus, result.StarBonus);
            Assert.Equal(expectedFinalScore, result.FinalScore);
        }
    }
}
