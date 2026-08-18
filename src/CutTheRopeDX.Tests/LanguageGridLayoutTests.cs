using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers how many language buttons the picker puts in a row. The buttons are composed in
    /// design space and drawn at the content scale, so a row of three is wider than a phone screen
    /// once the group has grown - which ran the outer two columns off both edges.
    /// </summary>
    public sealed class LanguageGridLayoutTests
    {
        /// <summary>Authored width of a language button.</summary>
        private const float ButtonWidth = 421f;

        [Theory]
        [InlineData(2560, 1440, 3)]
        [InlineData(1280, 720, 3)]
        [InlineData(1024, 768, 3)]
        [InlineData(1000, 1000, 3)]
        [InlineData(2560, 1080, 3)]
        [InlineData(720, 1280, 2)]
        [InlineData(400, 1280, 2)]
        [InlineData(320, 480, 2)]
        public void TheRowIsAsManyButtonsWideAsTheScreenHasRoomFor(int width, int height, int expected)
        {
            ViewportLayoutSnapshot snapshot = ViewportLayout.Compute(width, height);

            Assert.Equal(
                expected,
                LanguageGridLayout.ColumnsFor(
                    snapshot.VisibleBounds,
                    ContentFit.ScaleForAspect(snapshot.Aspect),
                    ButtonWidth));
        }

        [Fact]
        public void ARowOfButtonsFitsTheScreenItIsDrawnOn()
        {
            foreach (LayoutSurface surface in LayoutSurfaces.All)
            {
                ViewportLayoutSnapshot snapshot = ViewportLayout.Compute(surface.Width, surface.Height);
                float scale = ContentFit.ScaleForAspect(snapshot.Aspect);
                int columns = LanguageGridLayout.ColumnsFor(
                    snapshot.VisibleBounds,
                    scale,
                    ButtonWidth);

                float drawnRow = (ButtonWidth + ((columns - 1) * (ButtonWidth + LanguageGridLayout.ButtonSpacing))) * scale;

                Assert.True(
                    drawnRow <= snapshot.VisibleBounds.w,
                    $"{surface.Name}: a {drawnRow} row of {columns} on a {snapshot.VisibleBounds.w} screen");
            }
        }

        [Fact]
        public void ThePickerIsBuiltWithTheNumberOfColumnsTheRuleGives()
        {
            // Ties the constants above to the artwork, and the rule to the view that reads it.
            _ = HeadlessGame.Boot();

            foreach (LayoutSurface surface in LayoutSurfaces.All)
            {
                LayoutSurfaces.WithSurface(surface.Width, surface.Height, () =>
                {
                    MenuController controller = new(
                        (CTRRootController)Application.SharedRootController());
                    try
                    {
                        controller.ShowView(MenuController.VIEW_LANGUAGE_SELECT);
                        CTRRectangle visible = ScreenPresentation.Instance.Snapshot.VisibleBounds;
                        BaseElement row = FirstRow(controller);

                        Assert.Equal(MenuController.LanguageColumns(), row.ChildsCount());
                        Assert.True(
                            row.width * ContentFit.Scale <= visible.w,
                            $"{surface.Name}: a {row.width * ContentFit.Scale} row on a {visible.w} screen");
                    }
                    finally
                    {
                        controller.Dispose();
                    }
                });
            }
        }

        /// <summary>The first row of language buttons in the picker.</summary>
        /// <param name="controller">Controller owning the picker.</param>
        /// <returns>The row.</returns>
        private static BaseElement FirstRow(MenuController controller)
        {
            BaseElement group = controller.GetView(MenuController.VIEW_LANGUAGE_SELECT).GetChild(1);
            BaseElement stack = group.GetChild(0);
            BaseElement row = stack.GetChild(0);
            Assert.NotNull(row);
            return row;
        }
    }
}
