using System.Collections.Generic;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Platform;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Pins the constants that drive world simulation to the design size. Level coordinates, map
    /// extent and the physics simulation are authored in one fixed space; only the camera's
    /// visible region responds to the viewport. A use that starts tracking the surface would
    /// change gameplay rather than layout, and would do it silently.
    /// </summary>
    public sealed class WorldSpacePinTests
    {
        [Theory]
        [MemberData(nameof(Surfaces))]
        public void ScreenConstantsDoNotFollowTheSurface(string name, int width, int height)
        {
            // SCREEN_WIDTH is declared as 320f and raised to the design size during application
            // startup, so the game must be booted before these mean anything.
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(width, height, () =>
            {
                Assert.True(
                    FrameworkTypes.SCREEN_WIDTH == 2560f && FrameworkTypes.SCREEN_HEIGHT == 1440f,
                    $"{name}: the world constants followed the surface, "
                        + $"got {FrameworkTypes.SCREEN_WIDTH}x{FrameworkTypes.SCREEN_HEIGHT}");
            });
        }

        [Fact]
        public void TheSurfaceMatrixReallyDoesMoveTheVisibleBounds()
        {
            // Keeps the theory above from being vacuous. If every surface in the matrix produced
            // the same visible bounds, "the screen constants did not move" would be proving
            // nothing at all.
            _ = HeadlessGame.Boot();
            HashSet<string> observed = [];

            foreach (LayoutSurface surface in LayoutSurfaces.All)
            {
                LayoutSurfaces.WithSurface(surface.Width, surface.Height, () =>
                {
                    CTRRectangle bounds = ScreenPresentation.Instance.Snapshot.VisibleBounds;
                    _ = observed.Add($"{bounds.w}x{bounds.h}");
                });
            }

            Assert.True(
                observed.Count > 1,
                $"the surface matrix produced {observed.Count} distinct visible bounds; "
                + "the pin theory cannot prove anything unless the viewport actually varies");
        }

        public static TheoryData<string, int, int> Surfaces()
        {
            return LayoutSurfaces.Theory();
        }
    }
}
