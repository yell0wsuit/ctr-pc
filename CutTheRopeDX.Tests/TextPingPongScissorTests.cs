using CutTheRopeDX.Framework.Visual;

using Microsoft.Xna.Framework;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class TextPingPongScissorTests
    {
        [Fact]
        public void CalculatePingPongScissorRectangle_RoundsOutwardAndPreservesVerticalBounds()
        {
            Rectangle previousScissor = new(10, 20, 300, 200);

            Rectangle result = Text.CalculatePingPongScissorRectangle(
                50.25f, 80f, 99.5f, Matrix.Identity, previousScissor);

            Assert.Equal(new Rectangle(50, 20, 100, 200), result);
        }
    }
}
