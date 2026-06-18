using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.GameMain;

using Xunit;

using static CutTheRopeDX.Framework.Helpers.CTRMathHelper;

namespace CutTheRopeDX.Tests
{
    public class BungeeConnectorTests
    {
        private static ConstraintedPoint PointAt(float x, float y, float weight)
        {
            ConstraintedPoint p = new();
            p.SetWeight(weight);
            p.pos = Vect(x, y);
            return p;
        }

        [Fact]
        public void Init_PreservesHeadWeight_WhenHeadPassedIn()
        {
            ConstraintedPoint head = PointAt(100f, 100f, 1f);
            ConstraintedPoint tail = PointAt(100f, 160f, 1f);

            _ = new Bungee().InitWithHeadAtXYTailAtTXTYandLength(
                head, head.pos.X, head.pos.Y, tail, tail.pos.X, tail.pos.Y, 60f);

            Assert.Equal(1f, head.weight);
        }

        [Fact]
        public void Init_SetsAnchorWeight_WhenHeadAutoCreated()
        {
            ConstraintedPoint tail = PointAt(100f, 160f, 1f);

            Bungee bungee = new Bungee().InitWithHeadAtXYTailAtTXTYandLength(
                null, 100f, 100f, tail, tail.pos.X, tail.pos.Y, 60f);

            Assert.Equal(0.02f, bungee.bungeeAnchor.weight);
        }
    }
}
