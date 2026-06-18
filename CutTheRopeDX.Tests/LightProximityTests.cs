using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class LightProximityTests
    {
        [Fact]
        public void IsWithinLight_TrueWhenPointIsInsideRadius()
        {
            Assert.True(LightProximity.IsWithinLight(new Vector(1f, 1f), new Vector(0f, 0f), 2f));
        }

        [Fact]
        public void IsWithinLight_FalseAtRadiusAndBeyond()
        {
            Assert.False(LightProximity.IsWithinLight(new Vector(5f, 0f), new Vector(0f, 0f), 5f));
            Assert.False(LightProximity.IsWithinLight(new Vector(6f, 0f), new Vector(0f, 0f), 5f));
        }

        [Fact]
        public void IsWithinLight_UsesStrictDiagonalDistance()
        {
            Vector point = new(3f, 4f);
            Vector lightPos = new(0f, 0f);

            Assert.False(LightProximity.IsWithinLight(point, lightPos, 5f));
            Assert.True(LightProximity.IsWithinLight(point, lightPos, 6f));
        }

        [Fact]
        public void LightBulbDefinition_UsesLightBulbCapabilities()
        {
            Assert.True(LightBulbDefinition.EmitsLight);
            Assert.False(LightBulbDefinition.Capabilities.CanBeEaten);
            Assert.False(LightBulbDefinition.Capabilities.CanCollectStars);
        }
    }
}
