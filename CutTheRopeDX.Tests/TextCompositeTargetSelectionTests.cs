using CutTheRopeDX.Framework.Visual;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class TextCompositeTargetSelectionTests
    {
        [Fact]
        public void GetNextCompositeTargetIndex_AlternatesConsecutiveTextDraws()
        {
            int first = Text.GetNextCompositeTargetIndex(-1);
            int second = Text.GetNextCompositeTargetIndex(first);
            int third = Text.GetNextCompositeTargetIndex(second);

            Assert.Equal(0, first);
            Assert.Equal(1, second);
            Assert.Equal(0, third);
        }
    }
}
