using CutTheRopeDX.Framework.Visual;

using Xunit;

namespace CutTheRopeDX.Desktop.Tests
{
    public class TextCompositeTargetSelectionTests
    {
        [Fact]
        public void GetNextCompositeTargetIndexAlternatesConsecutiveTextDraws()
        {
            int first = FontStashFont.GetNextCompositeTargetIndex(-1);
            int second = FontStashFont.GetNextCompositeTargetIndex(first);
            int third = FontStashFont.GetNextCompositeTargetIndex(second);

            Assert.Equal(0, first);
            Assert.Equal(1, second);
            Assert.Equal(0, third);
        }
    }
}
