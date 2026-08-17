using System.Collections.Generic;
using System.Reflection;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Pins the level-result panel to the viewport it is shown over. Every piece of the panel is
    /// authored in design coordinates, so a viewport of another shape has to carry the whole
    /// composition with it rather than leave it where the design size put it.
    /// </summary>
    public sealed class ResultPanelLayoutTests
    {
        /// <summary>Middle star of the result panel, which the composition is built around.</summary>
        private const string PanelCenterPiece = "star2";

        [Fact]
        public void PanelIsDrawnWhereItWasAuthoredAtTheDesignShape()
        {
            WithPanel(2560, 1440, (panel, visible) =>
            {
                // The authored center of the composition, a shade left of the design center. What
                // matters below is that this offset is preserved, not that it is zero.
                Assert.Equal(1260f, DrawnCenterX(panel, PanelCenterPiece), 0.5);
                Assert.Equal(1280f, visible.w / 2f, 0.5);
            });
        }

        [Theory]
        [MemberData(nameof(Surfaces))]
        public void PanelCentersOnTheViewportAtEveryShape(string name, int width, int height)
        {
            float authoredOffsetX = 0f;
            float authoredOffsetY = 0f;
            WithPanel(2560, 1440, (panel, visible) =>
            {
                authoredOffsetX = DrawnCenterX(panel, PanelCenterPiece) - (visible.w / 2f);
                authoredOffsetY = DrawnCenterY(panel, PanelCenterPiece) - (visible.h / 2f);
            });

            WithPanel(width, height, (panel, visible) =>
            {
                // The panel keeps its authored offset from the middle of the screen, grown by
                // however much the fit draws the composition larger. Anything else means it is
                // still resolving against the design size instead of the window.
                Assert.Equal(
                    authoredOffsetX * FittedScale(),
                    DrawnCenterX(panel, PanelCenterPiece) - (visible.w / 2f),
                    1.0);
                Assert.Equal(
                    authoredOffsetY * FittedScale(),
                    DrawnCenterY(panel, PanelCenterPiece) - (visible.h / 2f),
                    1.0);
                Assert.False(string.IsNullOrEmpty(name));
            });
        }

        [Theory]
        [MemberData(nameof(Surfaces))]
        public void PanelStaysOnScreenAtEveryShape(string name, int width, int height)
        {
            WithPanel(width, height, (panel, visible) =>
            {
                (float left, float right) = DrawnExtentX(panel);

                Assert.True(left >= 0f, $"{name}: panel runs {0f - left} past the left edge");
                Assert.True(
                    right <= visible.w,
                    $"{name}: panel runs {right - visible.w} past the right edge");
            });
        }

        [Theory]
        [MemberData(nameof(Surfaces))]
        public void PanelButtonsTakeAPressWhereTheyAreDrawn(string name, int width, int height)
        {
            WithPanel(width, height, (panel, visible) =>
            {
                // The panel only takes input once a level has been won, and a press has to land on
                // the button the player is looking at rather than the one the design size would
                // have drawn there.
                panel.SetEnabled(true);
                int pressed = 0;

                foreach (KeyValuePair<int, BaseElement> entry in panel.GetChilds())
                {
                    if (entry.Value is not Button button)
                    {
                        continue;
                    }

                    float x = ToDrawnX(panel, button.drawX + (button.width / 2f));
                    float y = ToDrawnY(panel, button.drawY + (button.height / 2f));

                    Assert.True(
                        panel.OnTouchDownXY(x, y),
                        $"{name}: a press at ({x}, {y}) missed the button drawn there");
                    pressed++;
                }

                Assert.Equal(3, pressed);
                Assert.True(visible.w > 0f);
            });
        }

        /// <summary>
        /// Runs <paramref name="body"/> against a result panel laid out for the given surface.
        /// </summary>
        /// <param name="width">Surface width to lay out for.</param>
        /// <param name="height">Surface height to lay out for.</param>
        /// <param name="body">Work to run against the panel and the viewport it was laid out for.</param>
        private static void WithPanel(int width, int height, System.Action<BaseElement, CTRRectangle> body)
        {
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(width, height, () =>
            {
                GameController controller = HeadlessGame.LoadLevelWithController(pack: 1, level: 4);
                try
                {
                    View view = controller.GetView(0);
                    BoxOpenClose box = (BoxOpenClose)view.GetChild(GameView.VIEW_ELEMENT_RESULTS);

                    // Resolving the whole view is what puts every drawX in this subtree in step:
                    // a child's position only means anything once its parents' have been resolved.
                    _ = ElementGeometryWalker.Describe(view);

                    body(box.result, ScreenPresentation.Instance.Snapshot.VisibleBounds);
                }
                finally
                {
                    controller.Dispose();
                }
            });
        }

        /// <summary>
        /// Where a named piece of the panel is actually drawn horizontally, center included: the
        /// group scales about its own center, so a resolved position is only where the piece lands
        /// once that scale has been taken into account.
        /// </summary>
        /// <param name="panel">Element carrying the panel's design-space content.</param>
        /// <param name="pieceName">Name of the piece to measure.</param>
        /// <returns>The drawn horizontal center of that piece, in logical space.</returns>
        private static float DrawnCenterX(BaseElement panel, string pieceName)
        {
            BaseElement piece = panel.GetChildWithName(pieceName);
            Assert.NotNull(piece);

            return ToDrawnX(panel, piece.drawX + (piece.width / 2f));
        }

        /// <summary>
        /// Where a named piece of the panel is actually drawn vertically, center included.
        /// </summary>
        /// <param name="panel">Element carrying the panel's design-space content.</param>
        /// <param name="pieceName">Name of the piece to measure.</param>
        /// <returns>The drawn vertical center of that piece, in logical space.</returns>
        private static float DrawnCenterY(BaseElement panel, string pieceName)
        {
            BaseElement piece = panel.GetChildWithName(pieceName);
            Assert.NotNull(piece);

            return ToDrawnY(panel, piece.drawY + (piece.height / 2f));
        }

        /// <summary>
        /// The horizontal span the whole panel is drawn across, ignoring pieces with no size of
        /// their own - a text label carries its own metrics rather than a laid-out box.
        /// </summary>
        /// <param name="panel">Element carrying the panel's design-space content.</param>
        /// <returns>Left and right edges of the drawn composition, in logical space.</returns>
        private static (float Left, float Right) DrawnExtentX(BaseElement panel)
        {
            float left = float.MaxValue;
            float right = float.MinValue;

            foreach (KeyValuePair<int, BaseElement> entry in panel.GetChilds())
            {
                BaseElement piece = entry.Value;
                if (piece == null || piece.width <= 0)
                {
                    continue;
                }

                left = System.MathF.Min(left, ToDrawnX(panel, piece.drawX));
                right = System.MathF.Max(right, ToDrawnX(panel, piece.drawX + piece.width));
            }

            Assert.True(left < right, "the panel described no drawable piece");
            return (left, right);
        }

        /// <summary>
        /// Puts a resolved horizontal position through the panel group's scale, about the same
        /// point the renderer scales about.
        /// </summary>
        /// <param name="panel">Element carrying the panel's design-space content.</param>
        /// <param name="resolvedX">Resolved position, before the group's scale.</param>
        /// <returns>The drawn position, in logical space.</returns>
        private static float ToDrawnX(BaseElement panel, float resolvedX)
        {
            float center = panel.drawX + (panel.width >> 1);
            return center + ((resolvedX - center) * panel.scaleX);
        }

        /// <summary>
        /// Puts a resolved vertical position through the panel group's scale, about the same point
        /// the renderer scales about.
        /// </summary>
        /// <param name="panel">Element carrying the panel's design-space content.</param>
        /// <param name="resolvedY">Resolved position, before the group's scale.</param>
        /// <returns>The drawn position, in logical space.</returns>
        private static float ToDrawnY(BaseElement panel, float resolvedY)
        {
            float center = panel.drawY + (panel.height >> 1);
            return center + ((resolvedY - center) * panel.scaleY);
        }

        private static float FittedScale()
        {
            return (float)typeof(ViewController)
                .GetProperty("FittedScale", BindingFlags.Static | BindingFlags.NonPublic)
                .GetValue(null);
        }

        public static TheoryData<string, int, int> Surfaces()
        {
            return LayoutSurfaces.Theory();
        }
    }
}
