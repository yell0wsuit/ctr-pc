using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class AxeSpinTests
    {
        [Fact]
        public void RotationStep_UsesVelocityLengthDividedByTwenty()
        {
            Vector velocity = new(60f, 80f);

            float step = AxeSpin.RotationStepForVelocity(velocity);

            Assert.Equal(5f, step);
        }

        [Fact]
        public void RotationStep_ClampsToForty()
        {
            Vector velocity = new(1000f, 0f);

            float step = AxeSpin.RotationStepForVelocity(velocity);

            Assert.Equal(40f, step);
        }
    }
}
