using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Physics;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class ConstraintedPointTests
    {
        [Theory]
        [InlineData((int)Constraint.CONSTRAINT.DISTANCE)]
        [InlineData((int)Constraint.CONSTRAINT.NOT_MORE_THAN)]
        [InlineData((int)Constraint.CONSTRAINT.NOT_LESS_THAN)]
        public void CoincidentZeroRestConstraintDoesNotInventASeparationDirection(int typeValue)
        {
            Vector position = new(10f, 20f);
            ConstraintedPoint first = new() { pos = position };
            ConstraintedPoint second = new() { pos = position };
            first.AddConstraintwithRestLengthofType(second, 0f, (Constraint.CONSTRAINT)typeValue);

            ConstraintedPoint.SatisfyConstraints(first);

            Assert.Equal(position, first.pos);
            Assert.Equal(position, second.pos);
        }
    }
}
