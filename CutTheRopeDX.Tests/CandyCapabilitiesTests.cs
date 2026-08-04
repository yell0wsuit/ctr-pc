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
        public void CandyDefaultCapabilitiesMatchCurrentCandyBehavior()
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
        public void LightBulbIsPhysicalButNotCandyConsumable()
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

        private static CandyContext Context(
            GameObject visual = null,
            GameObject main = null,
            GameObject top = null,
            ConstraintedPoint point = null)
        {
            return new CandyContext(
                new CandyBody(point ?? new ConstraintedPoint(), CandyBodyRole.Whole, visual, main, top));
        }

        [Fact]
        public void BoundsTopYUsesSpecificObjectBoundingBox()
        {
            GameObject body = new()
            {
                drawY = 200f,
                bb = new CTRRectangle(10f, 25f, 30f, 40f)
            };
            CandyContext ctx = Context(visual: body);

            Assert.Equal(225f, GameObject.BoundsTopY(ctx.WholeBody.Visual));
        }

        [Fact]
        public void HandCatchVisualsUsesSingleRootForGenericCandyLikeObject()
        {
            GameObject body = new();
            CandyContext ctx = Context(visual: body, main: body);

            List<BaseElement> visuals = ctx.HandCatchVisuals();

            _ = Assert.Single(visuals);
            Assert.Same(body, visuals[0]);
            Assert.Equal(0.9f, ctx.HandCatchScale);
        }

        [Fact]
        public void HandCatchVisualsUsesAllDistinctCandyParts()
        {
            GameObject root = new();
            GameObject main = new();
            GameObject top = new();
            CandyContext ctx = Context(visual: root, main: main, top: top);

            List<BaseElement> visuals = ctx.HandCatchVisuals();

            Assert.Equal(3, visuals.Count);
            Assert.Same(root, visuals[0]);
            Assert.Same(main, visuals[1]);
            Assert.Same(top, visuals[2]);
            Assert.Equal(0.71f, ctx.HandCatchScale);
        }

        [Fact]
        public void TransformChildVisualsIsEmptyForGenericCandyLikeObject()
        {
            GameObject body = new();
            CandyContext ctx = Context(visual: body, main: body);

            Assert.Empty(ctx.TransformChildVisuals());
        }

        [Fact]
        public void TransformChildVisualsUsesDistinctCandyChildParts()
        {
            GameObject root = new();
            GameObject main = new();
            GameObject top = new();
            CandyContext ctx = Context(visual: root, main: main, top: top);

            List<BaseElement> visuals = ctx.TransformChildVisuals();

            Assert.Equal(2, visuals.Count);
            Assert.Same(main, visuals[0]);
            Assert.Same(top, visuals[1]);
        }

        [Fact]
        public void AxeIsPhysicalHazardButNotCandyConsumable()
        {
            CandyCapabilities axe = CandyCapabilities.Axe;

            Assert.False(axe.CanCollectStars);
            Assert.False(axe.CanOpenMouth);
            Assert.False(axe.CanBeEaten);
            Assert.False(axe.CanLoseLevelWhenOffScreen);
            Assert.False(axe.CanBeGrabbedBySpider);
            Assert.False(axe.CanBeGrabbedByMouse);
            Assert.False(axe.CanBeGrabbedByHand);
            Assert.False(axe.CanEnterLantern);
            Assert.True(axe.CanEnterTransport);
            Assert.True(axe.CanBindRocket);
            Assert.False(axe.CanAttachAnts);
            Assert.False(axe.CanCollideWithCandyBodies);
            Assert.False(axe.CanBeBrokenByHazards);
            Assert.False(axe.CanFloatInWater);
            Assert.False(axe.CanBeDraggedBySnail);
        }

        [Fact]
        public void CandyBodyToViewPreservesItsOwnersCapabilities()
        {
            CandyContext ctx = Context(point: new ConstraintedPoint { pos = new Vector(1f, 2f) });
            ctx.Capabilities = CandyCapabilities.LightBulb;

            CandyView view = ctx.WholeBody.ToView();

            Assert.False(view.Capabilities.CanBeEaten);
            Assert.False(view.Capabilities.CanCollectStars);
        }

        [Fact]
        public void InteractionRotationUsesCandyMainWhenAvailable()
        {
            CandyContext ctx = Context(
                visual: new GameObject { rotation = 15f },
                main: new GameObject { rotation = 45f });

            Assert.Equal(45f, ctx.InteractionRotation);
        }

        [Fact]
        public void InteractionRotationFallsBackToRootObjectRotation()
        {
            CandyContext ctx = Context(visual: new GameObject { rotation = 30f });

            Assert.Equal(30f, ctx.InteractionRotation);
        }

        [Fact]
        public void InteractionRotationIsZeroForNonRotatingBodies()
        {
            CandyContext ctx = Context(
                visual: new GameObject { rotation = 30f },
                main: new GameObject { rotation = 45f });
            ctx.Capabilities = CandyCapabilities.LightBulb;

            Assert.Equal(0f, ctx.InteractionRotation);
        }
    }
}
