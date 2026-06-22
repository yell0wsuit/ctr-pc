using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class CandyCollisionTests
    {
        [Fact]
        public void ShouldParticipate_FalseWhenCandyIsInLantern()
        {
            Assert.False(CandyCollision.ShouldParticipate(noCandy: false, inLantern: true));
        }

        [Fact]
        public void ShouldParticipate_TrueForUneatenCandyOutsideLantern()
        {
            Assert.True(CandyCollision.ShouldParticipate(noCandy: false, inLantern: false));
            Assert.False(CandyCollision.ShouldParticipate(noCandy: true, inLantern: false));
        }

        [Fact]
        public void ShouldParticipate_TrueForBubbledCandy()
        {
            // A bubbled (or ghost-bubbled) body still collides; bubble state is not an exclusion.
            Assert.True(CandyCollision.ShouldParticipate(noCandy: false, inLantern: false));
        }

        [Fact]
        public void PairDistance_UsesExplicitAdditiveRadii()
        {
            CandyContext a = new() { collisionRadius = 32f };
            CandyContext b = new() { collisionRadius = 32f };

            Assert.Equal(64f, CandyCollision.PairDistance(a, b));
        }

        [Fact]
        public void PairDistance_UsesDesktopCandyBodyRatioForNormalCandy()
        {
            bool previous = ActivePhysicsConstants.UseMobilePhysicsModel;
            try
            {
                ActivePhysicsConstants.UseMobilePhysicsModel = false;
                CandyContext a = new();
                CandyContext b = new();

                Assert.Equal(102.4f, CandyCollision.PairDistance(a, b), precision: 3);
            }
            finally
            {
                ActivePhysicsConstants.UseMobilePhysicsModel = previous;
            }
        }

        [Fact]
        public void PairDistance_UsesMobileCandyBodyRatioForNormalCandy()
        {
            bool previous = ActivePhysicsConstants.UseMobilePhysicsModel;
            try
            {
                ActivePhysicsConstants.UseMobilePhysicsModel = true;
                CandyContext a = new();
                CandyContext b = new();

                Assert.Equal(96f, CandyCollision.PairDistance(a, b), precision: 3);
            }
            finally
            {
                ActivePhysicsConstants.UseMobilePhysicsModel = previous;
            }
        }

        [Fact]
        public void PairDistance_UsesLargestAbsoluteOverride()
        {
            CandyContext candy = new() { collisionRadius = 32f };
            CandyContext bulb = new() { collisionDistanceOverride = 94.5f };

            Assert.Equal(94.5f, CandyCollision.PairDistance(candy, bulb));
            Assert.Equal(94.5f, CandyCollision.PairDistance(bulb, bulb));
        }

        [Fact]
        public void ShouldUseHtmlModel_TrueOnlyForDesktopCandyToCandy()
        {
            CandyContext candy = new();
            CandyContext other = new()
            {
                Capabilities = CandyCapabilities.LightBulb,
                collisionDistanceOverride = 94.5f
            };

            Assert.True(CandyCollision.ShouldUseHtmlModel(candy, new CandyContext(), useMobilePhysicsModel: false));
            Assert.False(CandyCollision.ShouldUseHtmlModel(candy, new CandyContext(), useMobilePhysicsModel: true));
            Assert.False(CandyCollision.ShouldUseHtmlModel(candy, other, useMobilePhysicsModel: false));
            Assert.False(CandyCollision.ShouldUseHtmlModel(other, candy, useMobilePhysicsModel: false));
        }

        [Fact]
        public void ShouldHtmlNudge_TrueWhenWithinNineTenthsBodyWidthAndClosing()
        {
            // body width 100 -> trigger threshold 90; distance 80 <= 90 and 80 < previous 100 (closing in)
            Assert.True(CandyCollision.ShouldHtmlNudge(distance: 80f, previousDistance: 100f, candyBodyWidth: 100f));
        }

        [Fact]
        public void ShouldHtmlNudge_FalseWhenSeparating()
        {
            // within threshold (80 <= 90) but distance grew vs last frame (80 >= 70) -> not closing in
            Assert.False(CandyCollision.ShouldHtmlNudge(distance: 80f, previousDistance: 70f, candyBodyWidth: 100f));
        }

        [Fact]
        public void ShouldHtmlNudge_FalseBeyondTriggerWidth()
        {
            // 91 > 0.9 * 100 = 90
            Assert.False(CandyCollision.ShouldHtmlNudge(distance: 91f, previousDistance: 100f, candyBodyWidth: 100f));
        }

        [Fact]
        public void ShouldHtmlNudge_FiresNearSurfaceTouchUsingBodyWidth()
        {
            // Regression: the HTML trigger is 0.9 × candy bounding-box WIDTH (M.lm.N = 112), i.e. ~the
            // surface-touch distance — NOT 0.9 × radius (~46), which let candies overlap to near-center.
            const float bodyWidth = 112f; // GetCandyBoundingBox().w (PC) == HTML M.lm width
            const float radius = 51.2f;   // DefaultCandyCollisionRadius (the previous, buggy arg)
            const float nearTouch = 100f; // approaching; surfaces about to meet (prev was larger)

            Assert.True(CandyCollision.ShouldHtmlNudge(nearTouch, previousDistance: 110f, candyBodyWidth: bodyWidth));
            Assert.False(CandyCollision.ShouldHtmlNudge(nearTouch, previousDistance: 110f, candyBodyWidth: radius));
        }

        [Fact]
        public void HtmlNudgeImpulse_IsEqualAndOppositeReverseVelocityScaled()
        {
            // a moved +2 in x last frame (prev 98 -> pos 100); b moved -2 in x (prev 122 -> pos 120): closing in.
            ConstraintedPoint a = new()
            {
                pos = new Vector(100f, 100f),
                prevPos = new Vector(98f, 100f)
            };
            ConstraintedPoint b = new()
            {
                pos = new Vector(120f, 100f),
                prevPos = new Vector(122f, 100f)
            };

            // aRevX = (98-100)*62.5 = -125 ; bRevX = (122-120)*62.5 = +125 ; impulseA.X = -125 - 125 = -250
            Vector impulse = CandyCollision.HtmlNudgeImpulse(a, b);
            Assert.Equal(-250f, impulse.X, precision: 3);
            Assert.Equal(0f, impulse.Y, precision: 3);
        }
    }
}
