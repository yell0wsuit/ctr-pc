using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>
    /// Time Travel axe reach: the blade destroys a candy and cuts a chain at the same 64-unit
    /// distance in the original's world, which is 96 in DX's.
    /// </summary>
    public sealed class AxeReachTests
    {
        /// <summary>The original's blade reach, in its own world units.</summary>
        private const float TimeTravelReach = 64f;

        [Fact]
        public void BladeReachMatchesTheOriginal()
        {
            Assert.Equal(TimeTravelReach * 1.5f, AxeDefinition.HazardCollisionDistance, 4);
            Assert.Equal(TimeTravelReach * 1.5f, AxeDefinition.ChainCutRadius, 4);
        }

        [Fact]
        public void BladeBreaksCandyJustInsideItsReach()
        {
            Assert.True(BreaksCandyAtDistance(AxeDefinition.HazardCollisionDistance - 5f));
        }

        [Fact]
        public void BladeLeavesCandyJustOutsideItsReach()
        {
            Assert.False(BreaksCandyAtDistance(AxeDefinition.HazardCollisionDistance + 5f));
        }

        /// <summary>
        /// Parks the blade <paramref name="distance"/> from the candy for one frame and reports
        /// whether the candy was destroyed.
        /// </summary>
        private static bool BreaksCandyAtDistance(float distance)
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .Axe(60, 400, "first")
                .Build();

            CandyContext candy = scene.Candy();
            CandyContext axe = scene.Candies().Find(c => c.axe != null);
            ConstraintedPoint candyPoint = candy.WholeBody.Point;
            ConstraintedPoint bladePoint = axe.WholeBody.Point;

            Vector at = new(candyPoint.pos.X + distance, candyPoint.pos.Y);
            bladePoint.pos = at;
            bladePoint.prevPos = at;
            bladePoint.v = default;

            HeadlessGame.StepFrames(scene, 1);
            return candy.HasNoWholeBodyInPlay;
        }
    }
}
