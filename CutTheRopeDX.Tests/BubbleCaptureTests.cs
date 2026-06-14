using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class BubbleCaptureTests
    {
        [Fact]
        public void Captures_TrueWhenCandyCenterInsideSquare()
        {
            // bubble at (100,100), radius 40 -> square [60,140) x [60,140)
            Assert.True(BubbleCapture.Captures(new Vector(100, 100), new Vector(100, 100), 40f));
            Assert.True(BubbleCapture.Captures(new Vector(139.9f, 60f), new Vector(100, 100), 40f));
        }

        [Fact]
        public void Captures_MatchesPointInRectAsymmetricBounds()
        {
            // low edge inclusive (>=), high edge exclusive (<), per PointInRect.
            Assert.True(BubbleCapture.Captures(new Vector(60f, 100f), new Vector(100, 100), 40f));   // x == cx (>=)
            Assert.False(BubbleCapture.Captures(new Vector(140f, 100f), new Vector(100, 100), 40f));  // x == cx+w (excluded)
        }

        [Fact]
        public void Captures_FalseWhenOutside()
        {
            Assert.False(BubbleCapture.Captures(new Vector(200, 100), new Vector(100, 100), 40f));
            Assert.False(BubbleCapture.Captures(new Vector(100, 200), new Vector(100, 100), 40f));
        }
    }
}
