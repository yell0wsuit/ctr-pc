using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers the text wrapper against the shapes real localized strings take. A line break
    /// followed by indenting spaces - which is how the Chinese credits are written - used to be
    /// able to describe a line of negative length and throw where it was measured.
    /// </summary>
    public sealed class TextWrapRobustnessTests
    {
        /// <summary>A line break followed by the indent the credits are written with.</summary>
        private const string Indented = "Cut the Rope\n\n      一款由 ZeptoLab 开发并由 Chillingo 发布的的游戏。\n\n      其它设计：\n      Nataliya Omelyanchuk";

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void AnIndentedLineBreakWrapsAtEveryWidth(bool breakLongWords)
        {
            _ = HeadlessGame.Boot();

            for (float width = 10f; width <= 400f; width += 10f)
            {
                Text block = new Text().InitWithFont(Application.GetFont(Resources.Fnt.SmallFont));
                block.wrapLongWords = breakLongWords;

                block.SetStringandWidth(Indented, width);

                Assert.NotEmpty(block.Lines);
            }
        }

        [Fact]
        public void EveryLineIsPartOfTheTextItWasGiven()
        {
            // An empty line is the one thing a trimmed indent can leave behind; anything else is
            // the wrapper losing its place.
            _ = HeadlessGame.Boot();

            Text block = new Text().InitWithFont(Application.GetFont(Resources.Fnt.SmallFont));
            block.wrapLongWords = true;
            block.SetStringandWidth(Indented, 40f);

            foreach (FormattedString line in block.Lines)
            {
                Assert.True(
                    line.string_.Length == 0 || Indented.Contains(line.string_),
                    $"\"{line.string_}\" is not part of the text");
            }
        }
    }
}
