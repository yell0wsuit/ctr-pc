using CutTheRopeDX.Framework;

using Xunit;

namespace CutTheRopeDX.Tests
{
    // Mobile physics must use the WP7 base-asset quad widths (x3) for collision geometry that
    // the engine derives from texture sizes, because the desktop art is trimmed differently
    // (e.g. obj_spikes_02 is 335 px wide on desktop but 106x3=318 in WP7 units).
    public class TextureCollisionWidthTests
    {
        private static T WithMobilePhysics<T>(bool mobile, System.Func<T> body)
        {
            bool previous = ActivePhysicsConstants.UseMobilePhysicsModel;
            try
            {
                ActivePhysicsConstants.UseMobilePhysicsModel = mobile;
                return body();
            }
            finally
            {
                ActivePhysicsConstants.UseMobilePhysicsModel = previous;
            }
        }

        [Theory]
        [InlineData(1, 204f)] // 68 * 3
        [InlineData(2, 318f)] // 106 * 3
        [InlineData(3, 438f)] // 146 * 3
        [InlineData(4, 543f)] // 181 * 3
        public void SpikesLineWidth_MobileUsesWp7StaticQuadWidths(int widthIndex, float expected)
        {
            float result = WithMobilePhysics(true, () =>
                ActivePhysicsConstants.SpikesCollisionLineWidth(rotatable: false, widthIndex, textureQuadWidth: 999f));
            Assert.Equal(expected, result, precision: 3);
        }

        [Theory]
        [InlineData(1, 204f)] // 68 * 3
        [InlineData(2, 354f)] // 118 * 3
        [InlineData(3, 426f)] // 142 * 3
        [InlineData(4, 534f)] // 178 * 3
        public void SpikesLineWidth_MobileUsesWp7RotatableQuadWidths(int widthIndex, float expected)
        {
            float result = WithMobilePhysics(true, () =>
                ActivePhysicsConstants.SpikesCollisionLineWidth(rotatable: true, widthIndex, textureQuadWidth: 999f));
            Assert.Equal(expected, result, precision: 3);
        }

        [Fact]
        public void SpikesLineWidth_DesktopPassesTextureQuadWidthThrough()
        {
            float result = WithMobilePhysics(false, () =>
                ActivePhysicsConstants.SpikesCollisionLineWidth(rotatable: false, widthIndex: 2, textureQuadWidth: 335f));
            Assert.Equal(335f, result, precision: 3);
        }

        // WP7 electro zap length = preCut width 267 - 130 = 137 -> x3 = 411. The dx electrodes
        // sheet is 833 wide (not 267x3=801), so the object width must be overridden on mobile.
        [Fact]
        public void ElectroSpikesObjectWidth_MobileUsesWp7PreCutWidth()
        {
            float objectWidth = WithMobilePhysics(true, () =>
                ActivePhysicsConstants.ElectroSpikesCollisionObjectWidth(833f));
            Assert.Equal(801f, objectWidth, precision: 3);
        }

        [Fact]
        public void ElectroSpikesObjectWidth_DesktopPassesObjectWidthThrough()
        {
            float objectWidth = WithMobilePhysics(false, () =>
                ActivePhysicsConstants.ElectroSpikesCollisionObjectWidth(833f));
            Assert.Equal(833f, objectWidth, precision: 3);
        }

        // WP7 bouncer collision width follows the current animation quad, so the mobile
        // override is frame-indexed to keep the per-frame wobble 1:1.
        [Theory]
        [InlineData(false, 0, 198f)] // 66 * 3
        [InlineData(false, 2, 210f)] // 70 * 3
        [InlineData(true, 0, 333f)]  // 111 * 3
        [InlineData(true, 2, 354f)]  // 118 * 3
        [InlineData(true, 4, 318f)]  // 106 * 3
        public void BouncerWidth_MobileUsesWp7QuadWidthsPerFrame(bool large, int frameIndex, float expected)
        {
            float result = WithMobilePhysics(true, () =>
                ActivePhysicsConstants.BouncerCollisionWidth(large, frameIndex, objectWidth: 999f));
            Assert.Equal(expected, result, precision: 3);
        }

        [Fact]
        public void BouncerWidth_DesktopPassesObjectWidthThrough()
        {
            float result = WithMobilePhysics(false, () =>
                ActivePhysicsConstants.BouncerCollisionWidth(large: true, frameIndex: 0, objectWidth: 304f));
            Assert.Equal(304f, result, precision: 3);
        }

        // Defensive: an out-of-range animation frame must clamp into the table, not throw.
        [Fact]
        public void BouncerWidth_MobileClampsFrameIndexIntoTable()
        {
            float result = WithMobilePhysics(true, () =>
                ActivePhysicsConstants.BouncerCollisionWidth(large: false, frameIndex: 9, objectWidth: 999f));
            Assert.Equal(198f, result, precision: 3); // clamped to last small frame (66 * 3)
        }
    }
}
