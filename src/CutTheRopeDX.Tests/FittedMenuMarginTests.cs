using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers what the menus keep between themselves and the screen edge. The content scale grows
    /// with a window's departure from the design shape and measured only the shape, so on a very
    /// wide window the main menu's logo was drawn off the top of the screen and its bottom button
    /// off the foot of it, while a near-square one left both flush against the edge.
    /// </summary>
    public sealed class FittedMenuMarginTests
    {
        [Theory]
        [MemberData(nameof(Cases))]
        public void AFittedMenuKeepsItsMarginToTheEdge(
            string surfaceName,
            int width,
            int height,
            int viewId,
            string viewName)
        {
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(width, height, () =>
            {
                MenuController controller = new(
                    (CTRRootController)Application.SharedRootController());
                try
                {
                    controller.ShowView(viewId);
                    controller.Update(0.016f);

                    BaseElement group = FindFittedGroup(controller.GetView(viewId));
                    Assert.NotNull(group);

                    CTRRectangle visible = ScreenPresentation.Instance.Snapshot.VisibleBounds;
                    CTRRectangle drawn = DrawnContent(group);
                    string where = $"{viewName} at {surfaceName}";

                    Assert.True(
                        drawn.x >= FittedContentFit.EdgeMargin - 0.5f,
                        $"{where}: content reaches {drawn.x} from the left edge");
                    Assert.True(
                        drawn.y >= FittedContentFit.EdgeMargin - 0.5f,
                        $"{where}: content reaches {drawn.y} from the top edge");
                    Assert.True(
                        drawn.x + drawn.w <= visible.w - FittedContentFit.EdgeMargin + 0.5f,
                        $"{where}: content reaches {visible.w - drawn.x - drawn.w} from the right edge");
                    Assert.True(
                        drawn.y + drawn.h <= visible.h - FittedContentFit.EdgeMargin + 0.5f,
                        $"{where}: content reaches {visible.h - drawn.y - drawn.h} from the bottom edge");
                }
                finally
                {
                    controller.Dispose();
                }
            });
        }

        [Theory]
        [MemberData(nameof(LayoutSurfaces.Theory), MemberType = typeof(LayoutSurfaces))]
        public void TheLevelGridKeepsItsMarginToTheEdge(string surfaceName, int width, int height)
        {
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(width, height, () =>
            {
                MenuController controller = new(
                    (CTRRootController)Application.SharedRootController());
                try
                {
                    controller.PreLevelSelect();
                    controller.ShowView(MenuController.VIEW_LEVEL_SELECT);
                    controller.Update(0.016f);

                    // A pack too large to fit scrolls instead of being fitted, and a scrolling
                    // grid answers to its container's bounds rather than to this rule.
                    BaseElement group = FindFittedGroup(controller.GetView(MenuController.VIEW_LEVEL_SELECT));
                    Assert.NotNull(group);

                    CTRRectangle visible = ScreenPresentation.Instance.Snapshot.VisibleBounds;
                    CTRRectangle drawn = DrawnContent(group);

                    Assert.True(
                        drawn.y >= FittedContentFit.EdgeMargin - 0.5f,
                        $"the grid at {surfaceName} reaches {drawn.y} from the top edge");
                    Assert.True(
                        drawn.y + drawn.h <= visible.h - FittedContentFit.EdgeMargin + 0.5f,
                        $"the grid at {surfaceName} reaches {visible.h - drawn.y - drawn.h} from the bottom edge");
                }
                finally
                {
                    controller.Dispose();
                }
            });
        }

        /// <summary>Where a fitted group's content is drawn, in logical space.</summary>
        /// <param name="group">The fitted group holding the content.</param>
        /// <returns>The drawn rectangle.</returns>
        private static CTRRectangle DrawnContent(BaseElement group)
        {
            // The inverse of the placement rule: a group is scaled about its own center, and the
            // half-box that takes back out is what puts design coordinate x at origin + x * scale.
            float scale = group.scaleX;
            float originX = group.x + ((group.width >> 1) * (1f - scale));
            float originY = group.y + ((group.height >> 1) * (1f - scale));
            CTRRectangle content = DesignExtent.Measure(group);
            return new CTRRectangle(
                originX + (content.x * scale),
                originY + (content.y * scale),
                content.w * scale,
                content.h * scale);
        }

        /// <summary>Returns the first fitted group in an element tree.</summary>
        /// <param name="element">Element to search from.</param>
        /// <returns>The group, or <see langword="null"/> when the tree holds none.</returns>
        private static BaseElement FindFittedGroup(BaseElement element)
        {
            if (element is FittedGroup group)
            {
                return group;
            }

            foreach (BaseElement child in element.GetChilds().Values)
            {
                BaseElement found = child == null ? null : FindFittedGroup(child);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        public static TheoryData<string, int, int, int, string> Cases()
        {
            TheoryData<string, int, int, int, string> data = [];
            foreach (LayoutSurface surface in LayoutSurfaces.All)
            {
                data.Add(surface.Name, surface.Width, surface.Height, MenuController.VIEW_MAIN_MENU, "MainMenu");
                data.Add(surface.Name, surface.Width, surface.Height, MenuController.VIEW_OPTIONS, "Options");
                data.Add(surface.Name, surface.Width, surface.Height, MenuController.VIEW_RESET, "Reset");
                data.Add(surface.Name, surface.Width, surface.Height, MenuController.VIEW_LANGUAGE_SELECT, "LanguageSelect");
            }
            return data;
        }
    }
}
