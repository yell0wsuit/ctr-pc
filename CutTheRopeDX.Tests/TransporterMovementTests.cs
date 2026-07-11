using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class TransporterMovementTests
    {
        [Theory]
        [InlineData(30f, 500f, 58f, 2)]
        [InlineData(270f, -500f, 242f, -2)]
        public void Move_NormalizesDeltasSpanningMultipleWraps(
            float position,
            float delta,
            float expectedPosition,
            int expectedCrossings)
        {
            TransporterMovementResult result = TransporterMovement.Move(
                position,
                delta,
                beltLength: 300f,
                transitionDistance: 18f);

            Assert.Equal(expectedPosition, result.Position, 3);
            Assert.Equal(expectedCrossings, result.Crossings);
            Assert.InRange(result.Position, 18f, 282f);
            Assert.InRange(result.DistanceFromEdge, 18f, 150f);
        }

        [Theory]
        [InlineData(30f, 20f, 274f, 1)]
        [InlineData(270f, -20f, 26f, -1)]
        public void Move_PreservesIosSingleWrapMapping(
            float position,
            float delta,
            float expectedPosition,
            int expectedCrossings)
        {
            TransporterMovementResult result = TransporterMovement.Move(
                position,
                delta,
                beltLength: 300f,
                transitionDistance: 18f);

            Assert.Equal(expectedPosition, result.Position, 3);
            Assert.Equal(expectedCrossings, result.Crossings);
        }
    }
}
