using System.Collections.Generic;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class LanternReleaseTests
    {
        [Fact]
        public void RestoreReleasedCandy_RestoresOnlyTheReleasedCandy()
        {
            ConstraintedPoint firstPoint = new();
            ConstraintedPoint secondPoint = new();
            CandyContext first = CapturedCandy(firstPoint);
            CandyContext second = CapturedCandy(secondPoint);
            List<CandyContext> candies = [first, second];

            int restoredIndex = LanternRelease.RestoreReleasedCandy(candies, secondPoint);

            Assert.Equal(1, restoredIndex);
            Assert.True(first.inLantern);
            Assert.False(second.inLantern);
            Assert.True(RGBAColor.RGBAEqual(RGBAColor.transparentRGBA, first.candy.color));
            Assert.True(RGBAColor.RGBAEqual(RGBAColor.solidOpaqueRGBA, second.candy.color));
            Assert.False(second.candy.passTransformationsToChilds);
            Assert.Equal(0.71f, second.candy.scaleX);
            Assert.Equal(0.71f, second.candy.scaleY);
            Assert.Equal(0.71f, second.candyMain.scaleX);
            Assert.Equal(0.71f, second.candyMain.scaleY);
            Assert.Equal(0.71f, second.candyTop.scaleX);
            Assert.Equal(0.71f, second.candyTop.scaleY);
        }

        private static CandyContext CapturedCandy(ConstraintedPoint point)
        {
            return new CandyContext
            {
                point = point,
                candy = CapturedVisual(),
                candyMain = CapturedVisual(),
                candyTop = CapturedVisual(),
                inLantern = true
            };
        }

        private static GameObject CapturedVisual()
        {
            return new GameObject
            {
                color = RGBAColor.transparentRGBA,
                passTransformationsToChilds = true,
                scaleX = 0.3f,
                scaleY = 0.3f
            };
        }
    }
}
