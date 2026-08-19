using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers the reset confirmation's text column. The text hangs in a group drawn at the content
    /// scale, so a wrap width measured in logical units alone came out that much wider than the
    /// screen once the group had grown.
    /// </summary>
    public sealed class ResetTextLayoutTests
    {
        [Fact]
        public void TheTextColumnFitsTheScreenItIsDrawnOn()
        {
            _ = HeadlessGame.Boot();

            foreach (LayoutSurface surface in LayoutSurfaces.All)
            {
                LayoutSurfaces.WithSurface(surface.Width, surface.Height, () =>
                {
                    CTRRectangle visible = ScreenPresentation.Instance.Snapshot.VisibleBounds;
                    Text text = ResetText();

                    Assert.True(
                        text.width * ContentFit.Scale <= visible.w,
                        $"{surface.Name}: a {text.width * ContentFit.Scale} column on a {visible.w} screen");
                });
            }
        }

        [Fact]
        public void TheDesignShapeKeepsTheAuthoredColumn()
        {
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(2560, 1440, () =>
                Assert.Equal(2560f * 0.95f, ResetText().width, 1f));
        }

        /// <summary>Builds the reset view and returns its confirmation text.</summary>
        /// <returns>The text, wrapped for the current surface.</returns>
        private static Text ResetText()
        {
            MenuController controller = new((CTRRootController)Application.SharedRootController());
            try
            {
                controller.ShowView(MenuController.VIEW_RESET);
                Text text = (Text)controller.GetView(MenuController.VIEW_RESET)
                    .GetChild(1)
                    .GetChild(0);
                Assert.NotNull(text);
                return text;
            }
            finally
            {
                controller.Dispose();
            }
        }
    }
}
