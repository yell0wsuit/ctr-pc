using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.GameMain;

using Xunit;

using static CutTheRopeDX.Framework.Helpers.CTRMathHelper;

namespace CutTheRopeDX.Tests
{
    public class ConveyorAutoHookWrapTests
    {
        [Fact]
        public void DidMoveToOtherSide_MovesAutoAttachedRopeWithHook()
        {
            ConstraintedPoint candy = new()
            {
                pos = Vect(100f, 160f),
            };
            Bungee rope = new Bungee().InitWithHeadAtXYTailAtTXTYandLength(
                null, 100f, 100f, candy, candy.pos.X, candy.pos.Y, 60f);
            Grab autoHook = new()
            {
                x = 200f,
                y = 100f,
            };
            autoHook.SetRope(rope);

            Assert.Equal(-1, autoHook.candyNumber);

            autoHook.DidMoveToOtherSide();

            Assert.Equal(200f, rope.bungeeAnchor.pos.X);
            Assert.Equal(200f, candy.pos.X);
        }
    }
}
