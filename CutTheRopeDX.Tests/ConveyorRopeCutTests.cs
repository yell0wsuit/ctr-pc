using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// When a grab wraps around a conveyor edge, the other ropes on the SAME candy are cut. Multi-candy
    /// identity is the rope's tail (candy physics point), not a legacy candy index — the loader hands
    /// every candy-bound grab the number 0, so matching on it would cut other candies' ropes too.
    /// </summary>
    public class ConveyorRopeCutTests
    {
        [Fact]
        public void ShouldCutTrueForAnotherUncutRopeOnTheSameCandy()
        {
            ConstraintedPoint candy = new();
            Assert.True(ConveyorRopeCut.ShouldCut(
                ropeTail: candy, wrappedCandyPoint: candy, isWrappedGrab: false, ropeUncut: true));
        }

        [Fact]
        public void ShouldCutFalseForARopeOnADifferentCandy()
        {
            // The multi-candy isolation invariant: wrapping a grab on candy A must not cut candy B's rope.
            ConstraintedPoint candyA = new();
            ConstraintedPoint candyB = new();
            Assert.False(ConveyorRopeCut.ShouldCut(
                ropeTail: candyB, wrappedCandyPoint: candyA, isWrappedGrab: false, ropeUncut: true));
        }

        [Fact]
        public void ShouldCutFalseForTheWrappedGrabItself()
        {
            // The wrapped grab keeps its own rope; only its siblings are cut.
            ConstraintedPoint candy = new();
            Assert.False(ConveyorRopeCut.ShouldCut(
                ropeTail: candy, wrappedCandyPoint: candy, isWrappedGrab: true, ropeUncut: true));
        }

        [Fact]
        public void ShouldCutFalseWhenRopeAlreadyCut()
        {
            ConstraintedPoint candy = new();
            Assert.False(ConveyorRopeCut.ShouldCut(
                ropeTail: candy, wrappedCandyPoint: candy, isWrappedGrab: false, ropeUncut: false));
        }

        [Fact]
        public void ShouldCutFalseWhenGrabHasNoRope()
        {
            ConstraintedPoint candy = new();
            Assert.False(ConveyorRopeCut.ShouldCut(
                ropeTail: null, wrappedCandyPoint: candy, isWrappedGrab: false, ropeUncut: true));
        }
    }
}
