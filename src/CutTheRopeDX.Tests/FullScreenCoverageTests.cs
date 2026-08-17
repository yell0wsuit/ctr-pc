using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers elements whose job is to span the whole screen. If one of these keeps the design
    /// size on a resized window, the uncovered remainder shows through as a gap or an unclickable
    /// dead band.
    /// </summary>
    public sealed class FullScreenCoverageTests
    {
        [Theory]
        [MemberData(nameof(Surfaces))]
        public void TheRootViewSpansTheVisibleBounds(string name, int width, int height)
        {
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(width, height, () =>
            {
                CTRRectangle visible = ScreenPresentation.Instance.Snapshot.VisibleBounds;
                View view = new();

                Assert.True(
                    (int)visible.w == view.width && (int)visible.h == view.height,
                    $"{name}: expected {(int)visible.w}x{(int)visible.h}, got {view.width}x{view.height}");
            });
        }

        public static TheoryData<string, int, int> Surfaces()
        {
            return LayoutSurfaces.Theory();
        }
    }
}
