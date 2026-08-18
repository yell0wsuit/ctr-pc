using System.Reflection;

using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers the in-game HUD's scale. The star row reads the content scale from the same place
    /// menus do, so no caller can hand it a value that disagrees with the rest of the frame.
    /// </summary>
    /// <remarks>
    /// Every case runs inside <see cref="LayoutSurfaces.WithSurface"/>. The surface size is
    /// process-wide and the suite runs serially, so a case that set one directly would leave it
    /// behind for every test after it - including the gameplay ones, which frame the world
    /// against it.
    /// </remarks>
    public sealed class HudScaleTests
    {
        [Fact]
        public void TheStarRowIsScaledForTheCurrentViewport()
        {
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(720, 1280, () =>
            {
                GameScene scene = HeadlessGame.LoadLevel(0, 0);

                scene.RelayoutHud();

                Assert.Equal(ContentFit.Scale, FirstStar(scene).scaleX, 0.0001);
            });
        }

        [Fact]
        public void TogglingFullscreenDoesNotResetTheStarRowToUnscaled()
        {
            // The HUD scale used to arrive as an argument with a default of one, and the
            // fullscreen path was the caller that took the default: every toggle on a portrait
            // window snapped the star row back to its authored size until the next resize.
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(720, 1280, () =>
            {
                GameScene scene = HeadlessGame.LoadLevel(0, 0);
                scene.RelayoutHud();
                float beforeToggle = FirstStar(scene).scaleX;
                Assert.True(beforeToggle > 1f, "the fixture viewport should boost the HUD above one");

                scene.FullscreenToggled(true);

                Assert.Equal(beforeToggle, FirstStar(scene).scaleX, 0.0001);
            });
        }

        [Fact]
        public void TheStarRowGrowsFromTheTopLeftCorner()
        {
            // Each icon is center-anchored, so scaling the row means multiplying each icon's
            // position through. An icon whose position did not scale would drift off the corner.
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(720, 1280, () =>
            {
                GameScene scene = HeadlessGame.LoadLevel(0, 0);

                scene.RelayoutHud();

                BaseElement star = FirstStar(scene);
                Assert.Equal(star.width / 2f * ContentFit.Scale, star.x, 0.01);
                Assert.Equal(star.height / 2f * ContentFit.Scale, star.y, 0.01);
            });
        }

        private static BaseElement FirstStar(GameScene scene)
        {
            FieldInfo field = typeof(GameScene).GetField(
                "hudStar",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            BaseElement[] stars = (BaseElement[])field.GetValue(scene);
            Assert.NotNull(stars);
            return stars[0];
        }
    }
}
