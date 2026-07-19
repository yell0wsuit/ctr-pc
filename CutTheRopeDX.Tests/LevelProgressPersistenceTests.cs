using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class LevelProgressPersistenceTests
    {
        [Fact]
        public void ShouldPersist_TrueWhenImprovedInNormalPlay()
        {
            Assert.True(LevelProgressPersistence.ShouldPersist(
                customLevelActive: false,
                newValue: 1200,
                storedValue: 900));
        }

        [Fact]
        public void ShouldPersist_FalseWhenNotImprovedInNormalPlay()
        {
            Assert.False(LevelProgressPersistence.ShouldPersist(
                customLevelActive: false,
                newValue: 900,
                storedValue: 900));
        }

        [Fact]
        public void ShouldPersist_FalseInCustomLevelEvenWhenImproved()
        {
            Assert.False(LevelProgressPersistence.ShouldPersist(
                customLevelActive: true,
                newValue: 1200,
                storedValue: 900));
        }

        [Fact]
        public void ShouldPersist_FalseInCustomLevelFromZero()
        {
            Assert.False(LevelProgressPersistence.ShouldPersist(
                customLevelActive: true,
                newValue: 3,
                storedValue: 0));
        }
    }
}
