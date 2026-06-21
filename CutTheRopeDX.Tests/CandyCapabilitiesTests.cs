using System.Collections.Generic;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.Framework.Visual;
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
            Assert.True(candy.CanBindRocket);
            Assert.True(candy.CanAttachAnts);
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
            Assert.True(bulb.CanBeGrabbedByMouse);
            Assert.True(bulb.CanBindRocket);
            Assert.True(bulb.CanAttachAnts);
            Assert.True(bulb.CanBeGrabbedByHand);
            Assert.True(bulb.CanEnterTransport);
            Assert.True(bulb.CanFloatInWater);
            Assert.True(bulb.CanBeDraggedBySnail);
            Assert.False(bulb.CanRotateWithRopes);
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
        public void HandCatchVisuals_UsesSingleRootForGenericCandyLikeObject()
        {
            GameObject body = new();
            CandyContext ctx = new()
            {
                candy = body,
                candyMain = body
            };

            List<BaseElement> visuals = ctx.HandCatchVisuals();

            _ = Assert.Single(visuals);
            Assert.Same(body, visuals[0]);
            Assert.Equal(0.9f, ctx.HandCatchScale);
        }

        [Fact]
        public void HandCatchVisuals_UsesAllDistinctCandyParts()
        {
            GameObject root = new();
            GameObject main = new();
            GameObject top = new();
            CandyContext ctx = new()
            {
                candy = root,
                candyMain = main,
                candyTop = top
            };

            List<BaseElement> visuals = ctx.HandCatchVisuals();

            Assert.Equal(3, visuals.Count);
            Assert.Same(root, visuals[0]);
            Assert.Same(main, visuals[1]);
            Assert.Same(top, visuals[2]);
            Assert.Equal(0.71f, ctx.HandCatchScale);
        }

        [Fact]
        public void TransformChildVisuals_IsEmptyForGenericCandyLikeObject()
        {
            GameObject body = new();
            CandyContext ctx = new()
            {
                candy = body,
                candyMain = body
            };

            Assert.Empty(ctx.TransformChildVisuals());
        }

        [Fact]
        public void TransformChildVisuals_UsesDistinctCandyChildParts()
        {
            GameObject root = new();
            GameObject main = new();
            GameObject top = new();
            CandyContext ctx = new()
            {
                candy = root,
                candyMain = main,
                candyTop = top
            };

            List<BaseElement> visuals = ctx.TransformChildVisuals();

            Assert.Equal(2, visuals.Count);
            Assert.Same(main, visuals[0]);
            Assert.Same(top, visuals[1]);
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
        public void InteractionRotation_IsZeroForNonRotatingBodies()
        {
            CandyContext ctx = new()
            {
                Capabilities = CandyCapabilities.LightBulb,
                candy = new GameObject
                {
                    rotation = 30f
                },
                candyMain = new GameObject
                {
                    rotation = 45f
                }
            };

            Assert.Equal(0f, ctx.InteractionRotation);
        }
    }
}
