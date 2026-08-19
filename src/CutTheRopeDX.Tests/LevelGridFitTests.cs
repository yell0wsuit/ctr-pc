using System;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers the level grid against the chrome in the screen's corners. The grid is centered on
    /// the viewport and the star total and back button sit in opposite corners, so a grid grown to
    /// fill a square window reached both - the star total ended up under a tile, and the tile in
    /// the other corner ended up under the button, where it could not be pressed at all.
    /// </summary>
    public sealed class LevelGridFitTests
    {
        [Fact]
        public void AGridThatClearsTheCornersIsDrawnAtTheScaleItAskedFor()
        {
            // The shape the game was drawn for: the grid is a block in the middle of a wide screen
            // and the corners are nowhere near it.
            CTRRectangle visible = new(0f, 0f, 2560f, 1440f);

            float scale = LevelGridFit.ScaleFor(
                visible,
                1f,
                1130f,
                1235f,
                new CTRRectangle(2450f, 40f, 84f, 55f),
                new CTRRectangle(0f, 1214f, 226f, 226f));

            Assert.Equal(1f, scale, 0.0001);
        }

        [Theory]
        // Square, where the grid fills the window and meets both corners.
        [InlineData(1440, 1440)]
        // A tall window, where standing clear of a corner costs nothing: there is room above and
        // below for the grid to stop short of it.
        [InlineData(1440, 2560)]
        // A wide one, where the room is to the sides instead.
        [InlineData(3413, 1440)]
        public void TheGridAtItsFittedScaleClearsBothCorners(int width, int height)
        {
            CTRRectangle visible = new(0f, 0f, width, height);
            CTRRectangle star = new(visible.w - 114f, 40f, 84f, 55f);
            CTRRectangle button = new(0f, visible.h - 226f, 226f, 226f);
            const float gridWidth = 1130f;
            const float gridHeight = 1235f;

            float scale = LevelGridFit.ScaleFor(visible, 1.5f, gridWidth, gridHeight, star, button);

            CTRRectangle grid = new(
                (visible.w - (gridWidth * scale)) / 2f,
                (visible.h - (gridHeight * scale)) / 2f,
                gridWidth * scale,
                gridHeight * scale);

            Assert.False(Overlaps(grid, star), $"the grid runs under the star total at {width}x{height}");
            Assert.False(Overlaps(grid, button), $"the grid runs under the button at {width}x{height}");
        }

        [Fact]
        public void ATallWindowKeepsMoreOfTheGridThanAShortOneDoes()
        {
            // Clearing a corner needs separation on one axis, not both, so a window with room
            // above and below the grid gives up less than one that has room only to the sides.
            CTRRectangle button = new(0f, 0f, 226f, 226f);

            float tall = LevelGridFit.ScaleFor(
                new CTRRectangle(0f, 0f, 1440f, 2560f),
                1.5f,
                1130f,
                1235f,
                button with { y = 2560f - 226f });
            float square = LevelGridFit.ScaleFor(
                new CTRRectangle(0f, 0f, 1440f, 1440f),
                1.5f,
                1130f,
                1235f,
                button with { y = 1440f - 226f });

            Assert.True(tall > square, $"a tall window gave up more ({tall}) than a square one ({square})");
        }

        [Fact]
        public void TheLevelPickerDrawsItsGridClearOfItsOwnChrome()
        {
            // The same thing measured against the real screen: the grid the picker builds, at the
            // scale the layout pass gives it, against the chrome that picker actually has.
            _ = HeadlessGame.Boot();

            foreach (LayoutSurface surface in LayoutSurfaces.All)
            {
                LayoutSurfaces.WithSurface(surface.Width, surface.Height, () =>
                {
                    MenuController controller = new(
                        (CTRRootController)Application.SharedRootController());
                    try
                    {
                        controller.PreLevelSelect();
                        controller.ShowView(MenuController.VIEW_LEVEL_SELECT);
                        View view = controller.GetView(MenuController.VIEW_LEVEL_SELECT);

                        CTRRectangle visible = ScreenPresentation.Instance.Snapshot.VisibleBounds;
                        BaseElement group = FindFittedGroup(view);
                        BaseElement star = view.GetChildWithName("starText");

                        // Asked for by both names the scenes use: looked up by one alone this came
                        // back null, and the case below skipped itself rather than failing.
                        Button back = (view.GetChildWithName("backb")
                            ?? view.GetChildWithName("backButton")) as Button;
                        Assert.NotNull(star);
                        Assert.NotNull(back);

                        // A pack of more than 25 levels scrolls rather than being fitted. Every
                        // shipped pack is 25, so this holds the line for custom ones.
                        if (group == null)
                        {
                            ScrollableContainer window = FindScroller(view);
                            Assert.NotNull(window);
                            Assert.True(
                                window.y + window.height
                                    <= visible.h - (back.height * back.scaleY) + 0.5f,
                                $"{surface.Name}: the scrolling grid ends under the back button");
                            return;
                        }

                        CTRRectangle grid = GridRect(group, visible);

                        Assert.False(
                            Overlaps(grid, DrawnRect(star, visible, topRight: true)),
                            $"{surface.Name}: the grid runs under the star total");
                        Assert.False(
                            Overlaps(grid, DrawnRect(back, visible, topRight: false)),
                            $"{surface.Name}: the grid runs under the back button");
                    }
                    finally
                    {
                        controller.Dispose();
                    }
                });
            }
        }

        /// <summary>Where the grid inside a fitted group is drawn.</summary>
        /// <param name="group">The fitted group holding the grid.</param>
        /// <param name="visible">The logical region the viewport exposes.</param>
        /// <returns>The grid's drawn rectangle.</returns>
        private static CTRRectangle GridRect(BaseElement group, CTRRectangle visible)
        {
            BaseElement stack = group.GetChild(0);
            float widest = 0f;
            foreach (BaseElement row in stack.GetChilds().Values)
            {
                widest = MathF.Max(widest, row?.width ?? 0f);
            }

            float width = widest * group.scaleX;
            float height = stack.height * group.scaleY;
            return new CTRRectangle(
                (visible.w - width) / 2f,
                (visible.h - height) / 2f,
                width,
                height);
        }

        /// <summary>Where a corner-anchored element is drawn.</summary>
        /// <param name="element">Element to measure.</param>
        /// <param name="visible">The logical region the viewport exposes.</param>
        /// <param name="topRight">Whether it sits in the top-right corner rather than bottom-left.</param>
        /// <returns>The element's drawn rectangle.</returns>
        private static CTRRectangle DrawnRect(BaseElement element, CTRRectangle visible, bool topRight)
        {
            float width = element.width * element.scaleX;
            float height = element.height * element.scaleY;
            return topRight
                ? new CTRRectangle(visible.w - width - 30f, 40f, width, height)
                : new CTRRectangle(0f, visible.h - height, width, height);
        }

        /// <summary>Whether two rectangles share any area.</summary>
        /// <param name="a">First rectangle.</param>
        /// <param name="b">Second rectangle.</param>
        /// <returns><see langword="true"/> when they overlap.</returns>
        private static bool Overlaps(CTRRectangle a, CTRRectangle b)
        {
            return a.x < b.x + b.w && b.x < a.x + a.w && a.y < b.y + b.h && b.y < a.y + a.h;
        }

        /// <summary>Returns the first scrolling container in an element tree.</summary>
        /// <param name="element">Element to search from.</param>
        /// <returns>The container, or <see langword="null"/> when the tree holds none.</returns>
        private static ScrollableContainer FindScroller(BaseElement element)
        {
            if (element is ScrollableContainer scroller)
            {
                return scroller;
            }

            foreach (BaseElement child in element.GetChilds().Values)
            {
                ScrollableContainer found = child == null ? null : FindScroller(child);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
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
    }
}
