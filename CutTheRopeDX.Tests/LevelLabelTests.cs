using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class LevelLabelTests
    {
        [Fact]
        public void Resolve_NormalLevelWithoutName_ShowsNumbersOverLevelWord()
        {
            LevelLabelText label = LevelLabel.Resolve(false, null, "Level", "1 - 1");

            Assert.Equal("1 - 1", label.Primary);
            Assert.Equal("Level", label.Secondary);
        }

        [Fact]
        public void Resolve_NormalLevelWithName_ShowsNameOverLevelWordAndNumbers()
        {
            LevelLabelText label = LevelLabel.Resolve(false, "Sugar Rush", "Level", "1 - 1");

            Assert.Equal("Sugar Rush", label.Primary);
            Assert.Equal("Level 1 - 1", label.Secondary);
        }

        [Fact]
        public void Resolve_CustomLevelWithName_ShowsNameWithoutNumbers()
        {
            LevelLabelText label = LevelLabel.Resolve(true, "My Test Level", "Level", "1 - 1");

            Assert.Equal("My Test Level", label.Primary);
            Assert.Null(label.Secondary);
        }

        [Fact]
        public void Resolve_CustomLevelWithoutName_ShowsNoLabel()
        {
            LevelLabelText label = LevelLabel.Resolve(true, null, "Level", "1 - 1");

            Assert.Null(label.Primary);
            Assert.Null(label.Secondary);
        }

        [Fact]
        public void Resolve_CustomLevelWithBlankName_ShowsNoLabel()
        {
            LevelLabelText label = LevelLabel.Resolve(true, "   ", "Level", "1 - 1");

            Assert.Null(label.Primary);
            Assert.Null(label.Secondary);
        }
    }
}
