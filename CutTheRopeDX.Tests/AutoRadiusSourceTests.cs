using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class AutoRadiusSourceTests
    {
        [Fact]
        public void PreAttachedNeverAttaches()
        {
            PreAttachedSource source = new();

            Assert.False(source.CanAttach);
        }

        [Fact]
        public void AutoRadiusCanAttachUntilItsRadiusIsSpent()
        {
            AutoRadiusSource source = new(100f, new Vector(0f, 0f));

            Assert.True(source.CanAttach);
        }

        [Fact]
        public void AutoRadiusInRangeUsesRadiusPlusCandyGrabPadding()
        {
            AutoRadiusSource source = new(100f, new Vector(0f, 0f));
            float padding = ActivePhysicsConstants.CandyGrabPadding;

            Assert.True(source.InRange(new Vector(0f, 0f), new Vector(100f + padding - 1f, 0f)));
            Assert.False(source.InRange(new Vector(0f, 0f), new Vector(100f + padding + 1f, 0f)));
        }

        [Fact]
        public void AutoRadiusFadeRunsToZeroThenStopsAttaching()
        {
            AutoRadiusSource source = new(100f, new Vector(0f, 0f));
            source.BeginFade();

            Assert.True(source.IsFading);

            // 1.5 alpha per second; one second of frames is more than enough.
            for (int i = 0; i < 60; i++)
            {
                source.Update(0.016f);
            }

            Assert.False(source.IsFading);
            Assert.Equal(-1f, source.Radius);
            Assert.False(source.CanAttach);
        }

        [Fact]
        public void AutoRadiusOnAnchorMovedRecalculatesTheCircle()
        {
            AutoRadiusSource source = new(100f, new Vector(0f, 0f));
            float firstX = source.Vertices[0];

            source.OnAnchorMoved(new Vector(500f, 500f));

            Assert.NotEqual(firstX, source.Vertices[0]);
        }
    }
}
