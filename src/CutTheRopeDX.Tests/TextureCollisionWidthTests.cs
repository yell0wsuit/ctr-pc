using System;

using CutTheRopeDX.Framework;
using CutTheRopeDX.GameMain;
using CutTheRopeDX.Tests.Interactions;

using Xunit;

namespace CutTheRopeDX.Tests
{
    // Spike/bouncer collision widths are table-driven from the original XML quad data rather than
    // read from the live texture atlas: the JSON atlas trim differs from both originals (every
    // frame is +2 px vs the pre-json atlas; the WP7 base assets differ more), so deriving physics
    // from the atlas silently changes collision whenever art is re-packed.
    public class TextureCollisionWidthTests
    {
        private static T WithMobilePhysics<T>(bool mobile, Func<T> body)
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

        private static T WithRocketModel<T>(bool mobile, bool timeTravel, Func<T> body)
        {
            bool previous = ActivePhysicsConstants.UseTimeTravelRocketModel;
            try
            {
                ActivePhysicsConstants.UseTimeTravelRocketModel = timeTravel;
                return WithMobilePhysics(mobile, body);
            }
            finally
            {
                ActivePhysicsConstants.UseTimeTravelRocketModel = previous;
            }
        }

        [Theory]
        [InlineData(1, 204f)] // 68 * 3
        [InlineData(2, 318f)] // 106 * 3
        [InlineData(3, 438f)] // 146 * 3
        [InlineData(4, 543f)] // 181 * 3
        public void SpikesLineWidthMobileUsesWp7StaticQuadWidths(int widthIndex, float expected)
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
        public void SpikesLineWidthMobileUsesWp7RotatableQuadWidths(int widthIndex, float expected)
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
        public void SpikesLineWidthDesktopUsesXmlQuadWidths(bool rotatable, int widthIndex, float expected)
        {
            float result = WithMobilePhysics(false, () =>
                ActivePhysicsConstants.SpikesCollisionLineWidth(rotatable, widthIndex));
            Assert.Equal(expected, result, precision: 3);
        }

        // WP7 electro zap length = preCut width 267 - 130 = 137 -> x3 = 411.
        // XML quad preCut width = 833, zap = 833 - 400 = 433.
        [Fact]
        public void ElectroSpikesObjectWidthMobileUsesWp7PreCutWidth()
        {
            float objectWidth = WithMobilePhysics(true, ActivePhysicsConstants.ElectroSpikesCollisionObjectWidth);
            Assert.Equal(801f, objectWidth, precision: 3);
        }

        [Fact]
        public void ElectroSpikesObjectWidthDesktopUsesXmlPreCutWidth()
        {
            float objectWidth = WithMobilePhysics(false, ActivePhysicsConstants.ElectroSpikesCollisionObjectWidth);
            Assert.Equal(833f, objectWidth, precision: 3);
        }

        // Both originals set the bouncer's collision width from the initial sprite (quad 0) and
        // never advance it with the bounce animation, so only the first quad width is used.
        // Mobile first normalizes the high-resolution iOS quad into authored level space,
        // then applies the native 20-unit logical end-cap.
        [Theory]
        [InlineData(false, 140f)] // small quad 0: ((100 / 1.5) - 20) * 3
        [InlineData(true, 240f)]  // large quad 0: ((150 / 1.5) - 20) * 3
        public void BouncerWidthMobileUsesIosFirstQuadWidth(bool large, float expected)
        {
            float result = WithMobilePhysics(true, () =>
                ActivePhysicsConstants.BouncerCollisionWidth(large));
            Assert.Equal(expected, result, precision: 3);
        }

        [Fact]
        public void BouncerCandyBoxAndBandMobileUseNativeLogicalUnits()
        {
            (float radius, float height) = WithMobilePhysics(true, () => (
                ActivePhysicsConstants.BouncerCollisionRadius,
                ActivePhysicsConstants.BouncerHeight));

            Assert.Equal(60f, radius, precision: 3); // 20 * 3
            Assert.Equal(15f, height, precision: 3); // 5 * 3
        }

        [Theory]
        [InlineData(false, 194f)] // small quad 0
        [InlineData(true, 302f)]  // large quad 0
        public void BouncerWidthDesktopUsesXmlFirstQuadWidth(bool large, float expected)
        {
            float result = WithMobilePhysics(false, () =>
                ActivePhysicsConstants.BouncerCollisionWidth(large));
            Assert.Equal(expected, result, precision: 3);
        }

        // Rocket catch-slat bb (0.65 x quad width, 0.05 x quad height of the rocket body quad),
        // pinned from XML quads and expressed center-relative to the rocket object.
        // Experiments base quad 10 = 116x58 centered at (91,67) on the 199x134 sheet, x3.
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void RocketCatchBoxExperimentsUsesBaseQuad(bool mobilePhysics)
        {
            (float w, float h, float ox, float oy) = WithRocketModel(mobilePhysics, timeTravel: false, () => (
                ActivePhysicsConstants.RocketCatchBoxWidth,
                ActivePhysicsConstants.RocketCatchBoxHeight,
                ActivePhysicsConstants.RocketCatchBoxCenterOffsetX,
                ActivePhysicsConstants.RocketCatchBoxCenterOffsetY));
            Assert.Equal(226.2f, w, precision: 3);  // 116 * 0.65 * 3
            Assert.Equal(8.7f, h, precision: 3);    // 58 * 0.05 * 3
            Assert.Equal(-25.5f, ox, precision: 3); // (91 - 99.5) * 3
            Assert.Equal(0f, oy, precision: 3);     // (67 - 67) * 3
        }

        // Time Travel resource 0x8A quad 10 is 358x179 in the DX sheet, centered at
        // (288,208.5) in its restored 619x418 source frame.
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void RocketCatchBoxTimeTravelUsesItsOwnQuad(bool mobilePhysics)
        {
            (float w, float h, float ox, float oy) = WithRocketModel(mobilePhysics, timeTravel: true, () => (
                ActivePhysicsConstants.RocketCatchBoxWidth,
                ActivePhysicsConstants.RocketCatchBoxHeight,
                ActivePhysicsConstants.RocketCatchBoxCenterOffsetX,
                ActivePhysicsConstants.RocketCatchBoxCenterOffsetY));
            Assert.Equal(232.7f, w, precision: 3);  // 358 * 0.65
            Assert.Equal(8.95f, h, precision: 3);   // 179 * 0.05
            Assert.Equal(-21.5f, ox, precision: 3); // 288 - 619/2
            Assert.Equal(-0.5f, oy, precision: 3);  // 208.5 - 418/2
        }

        [Fact]
        public void RocketVariantsUseTheirNativeScale()
        {
            GameScene experiments = Scenario.New()
                .Candy(60, 100)
                .OmNom(160, 440)
                .Rocket(220, 200)
                .Build();
            GameScene timeTravel = Scenario.New()
                .Design("useTimeTravelRocketPhysics", "true")
                .Candy(60, 100)
                .OmNom(160, 440)
                .Rocket(220, 200)
                .Build();

            Assert.Equal(0.7f, Assert.Single(experiments.Rockets()).scaleX);
            Assert.Equal(0.71f, Assert.Single(timeTravel.Rockets()).scaleX);
        }
    }
}
