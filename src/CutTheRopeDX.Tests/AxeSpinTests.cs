using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class AxeSpinTests
    {
        [Fact]
        public void RotationStepUsesVelocityLengthDividedByThirty()
        {
            // Time Travel divides by 20 in a world whose units are two thirds of DX's, so the same
            // swing has to divide by 30 here: |(90, 120)| = 150, and 150 / 30 = 5 degrees.
            Vector velocity = new(90f, 120f);

            float step = AxeSpin.RotationStepForVelocity(velocity);

            Assert.Equal(5f, step);
        }

        [Fact]
        public void RotationStepMatchesTheOriginalForTheSameAuthoredSwing()
        {
            // A swing that reads as 100 units/s in Time Travel's world is 150 in DX's; both must
            // turn the blade by the same 5 degrees.
            const float timeTravelSpeed = 100f;
            float dxStep = AxeSpin.RotationStepForVelocity(new Vector(timeTravelSpeed * 1.5f, 0f));

            Assert.Equal(timeTravelSpeed / 20f, dxStep, 4);
        }

        [Fact]
        public void RotationStepClampsToForty()
        {
            Vector velocity = new(2000f, 0f);

            float step = AxeSpin.RotationStepForVelocity(velocity);

            Assert.Equal(40f, step);
        }
    }
}
