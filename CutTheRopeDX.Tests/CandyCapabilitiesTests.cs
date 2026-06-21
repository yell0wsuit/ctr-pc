using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class CandyCapabilitiesTests
    {
        [Fact]
        public void Candy_DefaultCapabilitiesMatchCurrentCandyBehavior()
        {
            CandyCapabilities candy = CandyCapabilities.Candy;

            Assert.True(candy.CanCollectStars);
            Assert.True(candy.CanOpenMouth);
            Assert.True(candy.CanBeEaten);
            Assert.True(candy.CanLoseLevelWhenOffScreen);
            Assert.True(candy.CanBeGrabbedBySpider);
            Assert.True(candy.CanBeGrabbedByMouse);
            Assert.True(candy.CanBeGrabbedByHand);
            Assert.True(candy.CanEnterTransport);
            Assert.True(candy.CanFloatInWater);
            Assert.True(candy.CanBeDraggedBySnail);
        }

        [Fact]
        public void LightBulb_IsPhysicalButNotCandyConsumable()
        {
            CandyCapabilities bulb = CandyCapabilities.LightBulb;

            Assert.False(bulb.CanCollectStars);
            Assert.False(bulb.CanOpenMouth);
            Assert.False(bulb.CanBeEaten);
            Assert.False(bulb.CanLoseLevelWhenOffScreen);
            Assert.False(bulb.CanBeGrabbedBySpider);
            Assert.False(bulb.CanBeGrabbedByMouse);
            Assert.False(bulb.CanBeGrabbedByHand);
            Assert.True(bulb.CanEnterTransport);
            Assert.False(bulb.CanFloatInWater);
            Assert.False(bulb.CanBeDraggedBySnail);
        }

        [Fact]
        public void BoundsTopY_UsesSpecificObjectBoundingBox()
        {
            GameObject body = new()
            {
                drawY = 200f,
                bb = new CTRRectangle(10f, 25f, 30f, 40f)
            };
            CandyContext ctx = new()
            {
                candy = body
            };

            Assert.Equal(225f, GameObject.BoundsTopY(ctx.candy));
        }

        [Fact]
        public void CandyContext_ToView_PreservesCapabilities()
        {
            CandyContext ctx = new()
            {
                point = new ConstraintedPoint
                {
                    pos = new Vector(1f, 2f)
                },
                Capabilities = CandyCapabilities.LightBulb
            };

            CandyView view = ctx.ToView();

            Assert.False(view.Capabilities.CanBeEaten);
            Assert.False(view.Capabilities.CanCollectStars);
        }

        [Fact]
        public void InteractionRotation_UsesCandyMainWhenAvailable()
        {
            CandyContext ctx = new()
            {
                candy = new GameObject
                {
                    rotation = 15f
                },
                candyMain = new GameObject
                {
                    rotation = 45f
                }
            };

            Assert.Equal(45f, ctx.InteractionRotation);
        }

        [Fact]
        public void InteractionRotation_FallsBackToRootObjectRotation()
        {
            CandyContext ctx = new()
            {
                candy = new GameObject
                {
                    rotation = 30f
                }
            };

            Assert.Equal(30f, ctx.InteractionRotation);
        }

        [Fact]
        public void CandyInteraction_GatesLightBulbCandyOnlyInteractions()
        {
            CandyContext bulb = new()
            {
                Capabilities = CandyCapabilities.LightBulb
            };

            Assert.False(CandyInteraction.CanCollectStar(bulb));
            Assert.False(CandyInteraction.CanBeGrabbedByHand(bulb));
            Assert.False(CandyInteraction.CanAttachAnts(bulb));
            Assert.False(CandyInteraction.CanBeBrokenByHazards(bulb));
        }
    }
}
