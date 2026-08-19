using System;

using CutTheRopeDX.Commons;
using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers the level picker's backdrop: the pack's box cover, drawn once and once rotated about
    /// the seam between the halves. The painting is lit about its own middle, so where that middle
    /// lands is what the screen looks like.
    /// </summary>
    public sealed class LevelSelectCoverCenteringTests
    {
        [Theory]
        [MemberData(nameof(LayoutSurfaces.Theory), MemberType = typeof(LayoutSurfaces))]
        public void TheBoxCoverStaysCenteredOnTheViewport(string surfaceName, int width, int height)
        {
            _ = surfaceName;
            _ = HeadlessGame.Boot();

            WithLevelSelect(width, height, (left, right, visible) =>
            {
                CTRRectangle drawn = Union(Drawn(left), Drawn(right));

                // Within a unit rather than exactly: the mirrored half is nudged half a unit down
                // the way the artwork was authored, to hide the seam between the two.
                Assert.Equal(visible.w / 2f, drawn.x + (drawn.w / 2f), 1f);
                Assert.Equal(visible.h / 2f, drawn.y + (drawn.h / 2f), 1f);
            });
        }

        [Theory]
        [MemberData(nameof(LayoutSurfaces.Theory), MemberType = typeof(LayoutSurfaces))]
        public void TheBoxCoverReachesEveryEdge(string surfaceName, int width, int height)
        {
            _ = surfaceName;
            _ = HeadlessGame.Boot();

            WithLevelSelect(width, height, (left, right, visible) =>
            {
                CTRRectangle drawn = Union(Drawn(left), Drawn(right));

                Assert.True(drawn.x <= 0f, $"the cover starts at {drawn.x}");
                Assert.True(drawn.y <= 0f, $"the cover starts at {drawn.y}");
                Assert.True(drawn.x + drawn.w >= visible.w, $"the cover ends at {drawn.x + drawn.w} on a {visible.w} viewport");
                Assert.True(drawn.y + drawn.h >= visible.h, $"the cover ends at {drawn.y + drawn.h} on a {visible.h} viewport");
            });
        }

        /// <summary>
        /// Shows the level picker at the given surface size and hands the two cover halves to
        /// <paramref name="body"/>.
        /// </summary>
        /// <param name="width">Surface width to run at.</param>
        /// <param name="height">Surface height to run at.</param>
        /// <param name="body">Work to run against the laid-out halves.</param>
        private static void WithLevelSelect(int width, int height, Action<Image, Image, CTRRectangle> body)
        {
            LayoutSurfaces.WithSurface(width, height, () =>
            {
                MenuController controller = new(
                    (CTRRootController)Application.SharedRootController());
                try
                {
                    controller.PreLevelSelect();
                    controller.ShowView(MenuController.VIEW_LEVEL_SELECT);

                    Image left = (Image)controller.GetView(MenuController.VIEW_LEVEL_SELECT).GetChild(0);
                    Image right = (Image)left.GetChild(0);
                    BaseElement.CalculateTopLeft(left);
                    BaseElement.CalculateTopLeft(right);

                    body(left, right, ScreenPresentation.Instance.Snapshot.VisibleBounds);
                }
                finally
                {
                    controller.Dispose();
                }
            });
        }

        /// <summary>Where an element scaled about its own center is drawn.</summary>
        /// <param name="element">Element to measure.</param>
        /// <returns>The drawn rectangle.</returns>
        private static CTRRectangle Drawn(BaseElement element)
        {
            return new CTRRectangle(
                element.drawX + (element.width * (1f - element.scaleX) / 2f),
                element.drawY + (element.height * (1f - element.scaleY) / 2f),
                element.width * element.scaleX,
                element.height * element.scaleY);
        }

        /// <summary>The smallest rectangle containing both of the given ones.</summary>
        /// <param name="a">First rectangle.</param>
        /// <param name="b">Second rectangle.</param>
        /// <returns>Their union.</returns>
        private static CTRRectangle Union(CTRRectangle a, CTRRectangle b)
        {
            float x = MathF.Min(a.x, b.x);
            float y = MathF.Min(a.y, b.y);
            return new CTRRectangle(
                x,
                y,
                MathF.Max(a.x + a.w, b.x + b.w) - x,
                MathF.Max(a.y + a.h, b.y + b.h) - y);
        }
    }
}
