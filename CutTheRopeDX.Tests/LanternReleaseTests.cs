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
        public void RestoreReleasedCandyRestoresOnlyTheReleasedCandy()
        {
            ConstraintedPoint firstPoint = new();
            ConstraintedPoint secondPoint = new();
            CandyContext first = CapturedCandy(firstPoint);
            CandyContext second = CapturedCandy(secondPoint);
            List<CandyContext> candies = [first, second];

            int restoredIndex = LanternRelease.RestoreReleasedCandy(candies, secondPoint);

            Assert.Equal(1, restoredIndex);
            Assert.True(first.Lifecycle.Attachments.InLantern);
            Assert.False(second.Lifecycle.Attachments.InLantern);
            Assert.True(RGBAColor.RGBAEqual(RGBAColor.transparentRGBA, first.WholeBody.Visual.color));
            Assert.True(RGBAColor.RGBAEqual(RGBAColor.solidOpaqueRGBA, second.WholeBody.Visual.color));
            Assert.False(second.WholeBody.Visual.passTransformationsToChilds);
            Assert.Equal(0.71f, second.WholeBody.Visual.scaleX);
            Assert.Equal(0.71f, second.WholeBody.Visual.scaleY);
            Assert.Equal(0.71f, second.WholeBody.Main.scaleX);
            Assert.Equal(0.71f, second.WholeBody.Main.scaleY);
            Assert.Equal(0.71f, second.WholeBody.Top.scaleX);
            Assert.Equal(0.71f, second.WholeBody.Top.scaleY);
        }

        private static CandyContext CapturedCandy(ConstraintedPoint point)
        {
            CandyBody body = new(
                point,
                CandyBodyRole.Whole,
                CapturedVisual(),
                CapturedVisual(),
                CapturedVisual());

            CandyContext candy = new(body);
            _ = candy.Lifecycle.Attachments.CaptureInLantern();
            return candy;
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
