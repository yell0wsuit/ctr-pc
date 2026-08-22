using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>Mobile-reference candy entry boundaries for bamboo tubes.</summary>
    public sealed class MobileBambooTubeCollisionTests
    {
        [Theory]
        [InlineData(24f, true)]
        [InlineData(25f, false)]
        public void EntryUsesStrictTwentyFiveUnitRadiusFromCandyCenter(float lateralOffset, bool expected)
        {
            GameScene scene = Scenario.New()
                .Design("useMobilePhysics", "true")
                .Candy(160, 100)
                .BambooTube(160, 220, TubeMouth.CatchesFalling)
                .OmNom(20, 460)
                .Build();
            CandyBody body = scene.Candy().WholeBody;
            BambooTube tube = scene.BambooTubes()[0];
            Vector towardCenter = TubeGeometry.CentreDirection(TubeMouth.CatchesFalling);
            float halfBody = tube.bb.w * 0.5f;
            Vector entryHole = new(
                tube.x - (towardCenter.X * halfBody),
                tube.y - (towardCenter.Y * halfBody));
            Vector position = new(
                entryHole.X + (lateralOffset * Scenario.Scale),
                entryHole.Y);

            body.Point.pos = position;
            body.Point.prevPos = new Vector(
                position.X - (towardCenter.X * Scenario.Scale),
                position.Y - (towardCenter.Y * Scenario.Scale));

            Assert.Equal(expected, tube.TryCatchCandy(body.Point));
        }
    }
}
