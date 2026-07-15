using CutTheRopeDX.Framework;

using Xunit;

namespace CutTheRopeDX.Tests
{
    // Spike/bouncer collision widths are table-driven from the original XML quad data rather than
    // read from the live texture atlas: the JSON atlas trim differs from both originals (every
    // frame is +2 px vs the pre-json atlas; the WP7 base assets differ more), so deriving physics
    // from the atlas silently changes collision whenever art is re-packed.
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
                ActivePhysicsConstants.SpikesCollisionLineWidth(rotatable: false, widthIndex));
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
                ActivePhysicsConstants.SpikesCollisionLineWidth(rotatable: true, widthIndex));
            Assert.Equal(expected, result, precision: 3);
        }

        [Theory]
        [InlineData(false, 1, 212f)]
        [InlineData(false, 2, 333f)]
        [InlineData(false, 3, 453f)]
        [InlineData(false, 4, 566f)]
        [InlineData(true, 1, 202f)]
        [InlineData(true, 2, 319f)]
        [InlineData(true, 3, 444f)]
        [InlineData(true, 4, 559f)]
        public void SpikesLineWidth_DesktopUsesXmlQuadWidths(bool rotatable, int widthIndex, float expected)
        {
            float result = WithMobilePhysics(false, () =>
                ActivePhysicsConstants.SpikesCollisionLineWidth(rotatable, widthIndex));
            Assert.Equal(expected, result, precision: 3);
        }

        // WP7 electro zap length = preCut width 267 - 130 = 137 -> x3 = 411.
        // XML quad preCut width = 833, zap = 833 - 400 = 433.
        [Fact]
        public void ElectroSpikesObjectWidth_MobileUsesWp7PreCutWidth()
        {
            float objectWidth = WithMobilePhysics(true, ActivePhysicsConstants.ElectroSpikesCollisionObjectWidth);
            Assert.Equal(801f, objectWidth, precision: 3);
        }

        [Fact]
        public void ElectroSpikesObjectWidth_DesktopUsesXmlPreCutWidth()
        {
            float objectWidth = WithMobilePhysics(false, ActivePhysicsConstants.ElectroSpikesCollisionObjectWidth);
            Assert.Equal(833f, objectWidth, precision: 3);
        }

        // Both originals let the bouncer's collision width follow the current animation quad,
        // so the tables are frame-indexed to keep the per-frame wobble faithful.
        [Theory]
        [InlineData(false, 0, 198f)] // 66 * 3
        [InlineData(false, 2, 210f)] // 70 * 3
        [InlineData(true, 0, 333f)]  // 111 * 3
        [InlineData(true, 2, 354f)]  // 118 * 3
        [InlineData(true, 4, 318f)]  // 106 * 3
        public void BouncerWidth_MobileUsesWp7QuadWidthsPerFrame(bool large, int frameIndex, float expected)
        {
            float result = WithMobilePhysics(true, () =>
                ActivePhysicsConstants.BouncerCollisionWidth(large, frameIndex));
            Assert.Equal(expected, result, precision: 3);
        }

        [Theory]
        [InlineData(false, 0, 194f)]
        [InlineData(false, 2, 204f)]
        [InlineData(false, 4, 194f)]
        [InlineData(true, 0, 302f)]
        [InlineData(true, 2, 322f)]
        [InlineData(true, 4, 302f)]
        public void BouncerWidth_DesktopUsesXmlQuadWidthsPerFrame(bool large, int frameIndex, float expected)
        {
            float result = WithMobilePhysics(false, () =>
                ActivePhysicsConstants.BouncerCollisionWidth(large, frameIndex));
            Assert.Equal(expected, result, precision: 3);
        }

        // Rocket catch-slat bb (0.6 x quad width, 0.05 x quad height of the rocket body quad),
        // pinned from XML quads and expressed center-relative to the rocket object.
        // Mobile: Experiments base quad 10 = 116x58 centered at (91,67) on the 199x134 sheet, x3.
        [Fact]
        public void RocketCatchBox_MobileUsesExperimentsBaseQuad()
        {
            (float w, float h, float ox, float oy) = WithMobilePhysics(true, () => (
                ActivePhysicsConstants.RocketCatchBoxWidth,
                ActivePhysicsConstants.RocketCatchBoxHeight,
                ActivePhysicsConstants.RocketCatchBoxCenterOffsetX,
                ActivePhysicsConstants.RocketCatchBoxCenterOffsetY));
            Assert.Equal(208.8f, w, precision: 3);  // 116 * 0.6 * 3
            Assert.Equal(8.7f, h, precision: 3);    // 58 * 0.05 * 3
            Assert.Equal(-25.5f, ox, precision: 3); // (91 - 99.5) * 3
            Assert.Equal(0f, oy, precision: 3);     // (67 - 67) * 3
        }

        // Desktop: dx atlas quad 10 = 358x179 centered at (288, 208.5) on the 619x418 sheet,
        // frozen as constants so atlas repacks cannot move the hitbox.
        [Fact]
        public void RocketCatchBox_DesktopUsesFrozenDxAtlasQuad()
        {
            (float w, float h, float ox, float oy) = WithMobilePhysics(false, () => (
                ActivePhysicsConstants.RocketCatchBoxWidth,
                ActivePhysicsConstants.RocketCatchBoxHeight,
                ActivePhysicsConstants.RocketCatchBoxCenterOffsetX,
                ActivePhysicsConstants.RocketCatchBoxCenterOffsetY));
            Assert.Equal(214.8f, w, precision: 3);  // 358 * 0.6
            Assert.Equal(8.95f, h, precision: 3);   // 179 * 0.05
            Assert.Equal(-21.5f, ox, precision: 3); // 288 - 309.5
            Assert.Equal(-0.5f, oy, precision: 3);  // 208.5 - 209
        }

        // Defensive: an out-of-range animation frame must clamp into the table, not throw.
        [Fact]
        public void BouncerWidth_ClampsFrameIndexIntoTable()
        {
            float mobile = WithMobilePhysics(true, () =>
                ActivePhysicsConstants.BouncerCollisionWidth(large: false, frameIndex: 9));
            Assert.Equal(198f, mobile, precision: 3); // clamped to last small frame (66 * 3)

            float desktop = WithMobilePhysics(false, () =>
                ActivePhysicsConstants.BouncerCollisionWidth(large: true, frameIndex: -1));
            Assert.Equal(302f, desktop, precision: 3); // clamped to first large frame
        }
    }
}
