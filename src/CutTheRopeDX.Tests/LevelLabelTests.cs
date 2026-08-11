using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class LevelLabelTests
    {
        [Fact]
        public void ResolveNormalLevelWithoutNameShowsNumbersOverLevelWord()
        {
            LevelLabelText label = LevelLabel.Resolve(false, null, "Level", "1 - 1");

            Assert.Equal("1 - 1", label.Primary);
            Assert.Equal("Level", label.Secondary);
        }

        [Fact]
        public void ResolveNormalLevelWithNameShowsNameOverLevelWordAndNumbers()
        {
            LevelLabelText label = LevelLabel.Resolve(false, "Sugar Rush", "Level", "1 - 1");

            Assert.Equal("Sugar Rush", label.Primary);
            Assert.Equal("Level 1 - 1", label.Secondary);
        }

        [Fact]
        public void ResolveCustomLevelWithNameShowsNameWithoutNumbers()
        {
            LevelLabelText label = LevelLabel.Resolve(true, "My Test Level", "Level", "1 - 1");

            Assert.Equal("My Test Level", label.Primary);
            Assert.Null(label.Secondary);
        }

        [Fact]
        public void ResolveCustomLevelWithoutNameShowsNoLabel()
        {
            LevelLabelText label = LevelLabel.Resolve(true, null, "Level", "1 - 1");

            Assert.Null(label.Primary);
            Assert.Null(label.Secondary);
        }

        [Fact]
        public void ResolveCustomLevelWithBlankNameShowsNoLabel()
        {
            LevelLabelText label = LevelLabel.Resolve(true, "   ", "Level", "1 - 1");

            Assert.Null(label.Primary);
            Assert.Null(label.Secondary);
        }
    }
}
