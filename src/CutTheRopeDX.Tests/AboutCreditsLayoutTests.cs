using System;
using System.Linq;

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
    /// Covers the credits: the column they wrap into, the window they scroll inside, and the
    /// agreement between how tall the stack draws and how far it scrolls.
    /// </summary>
    public sealed class AboutCreditsLayoutTests
    {
        [Fact]
        public void AWordWithNoBreakInItIsBrokenToFitTheColumn()
        {
            // The project URL is one unbroken word. Wrapping on spaces alone left it running off
            // both sides of the credits column, with the middle of the link the only part on
            // screen.
            _ = HeadlessGame.Boot();

            const string url = "https://github.com/yell0wsuit/cuttherope-dx";
            const float column = 100f;

            Text unbroken = new Text().InitWithFont(Application.GetFont(Resources.Fnt.SmallFont));
            unbroken.SetStringandWidth(url, column);

            Text broken = new Text().InitWithFont(Application.GetFont(Resources.Fnt.SmallFont));
            broken.wrapLongWords = true;
            broken.SetStringandWidth(url, column);

            Assert.True(
                broken.height > unbroken.height,
                $"the link still laid out as one line of {unbroken.height}");
        }

        [Theory]
        [InlineData(100f)]
        [InlineData(140f)]
        [InlineData(180f)]
        [InlineData(220f)]
        [InlineData(257f)]
        public void NoLineOfABrokenWordIsWiderThanTheColumn(float column)
        {
            // The break used to land after the character that overflowed rather than before it,
            // so a broken line came out up to one character wider than the column. The credits
            // column is exactly as wide as the container it scrolls in, so that character was
            // drawn past the clip - half of it off each end of a centered line, which is how the
            // project link read on an iPad or an ultrawide.
            _ = HeadlessGame.Boot();

            Text broken = new Text().InitWithFont(Application.GetFont(Resources.Fnt.SmallFont));
            broken.wrapLongWords = true;
            broken.SetStringandWidth("https://github.com/yell0wsuit/cuttherope-dx/", column);

            Assert.True(broken.Lines.Count > 1, $"the link fitted a {column} column unbroken");
            foreach (FormattedString line in broken.Lines)
            {
                Assert.True(
                    line.width <= column,
                    $"a {line.width} line in a {column} column: \"{line.string_}\"");
            }
        }

        [Fact]
        public void ABrokenWordKeepsEveryCharacter()
        {
            // Breaking before the overflowing character has to carry it onto the next line, not
            // drop it.
            _ = HeadlessGame.Boot();

            const string url = "https://github.com/yell0wsuit/cuttherope-dx/";
            Text broken = new Text().InitWithFont(Application.GetFont(Resources.Fnt.SmallFont));
            broken.wrapLongWords = true;
            broken.SetStringandWidth(url, 100f);

            Assert.Equal(url, string.Concat(broken.Lines.Select(line => line.string_)));
        }

        [Fact]
        public void TheCreditsScrollAsFarAsTheyDraw()
        {
            // How far the credits scroll is read off the stack's own height, and every block in it
            // draws at the content scale. Leaving the stack at the height its unscaled blocks
            // measured stopped the reader a third of the way from the end on a phone.
            _ = HeadlessGame.Boot();

            foreach (LayoutSurface surface in LayoutSurfaces.All)
            {
                LayoutSurfaces.WithSurface(surface.Width, surface.Height, () =>
                    WithAboutView(container =>
                    {
                        // The stack itself is the container's own content element, which it only
                        // exposes through the reach it reports: how far it scrolls plus the window
                        // it scrolls inside is the height it believes its content is.
                        float believed = container.GetMaxScroll().Y + container.height;
                        Assert.Equal(DrawnExtent(container), believed, 1f);
                    }));
            }
        }

        [Fact]
        public void TheScrollWindowFollowsTheViewport()
        {
            // A phone screen is two and a half times as tall as the shape the credits were
            // authored on. Held at the authored height, the window read them through a slot in
            // the middle of the screen.
            _ = HeadlessGame.Boot();

            foreach (LayoutSurface surface in LayoutSurfaces.All)
            {
                LayoutSurfaces.WithSurface(surface.Width, surface.Height, () =>
                    WithAboutView(container =>
                    {
                        CTRRectangle visible = ScreenPresentation.Instance.Snapshot.VisibleBounds;
                        Assert.True(
                            container.height > visible.h * 0.75f,
                            $"{surface.Name}: a {container.height} window on a {visible.h} viewport");
                        Assert.True(
                            container.height < visible.h,
                            $"{surface.Name}: the window is not inset from the screen");
                    }));
            }
        }

        [Fact]
        public void ResizingLeavesTheReaderInsideTheCredits()
        {
            // The window grows on a resize, which pulls the end of the credits up the screen. A
            // scroll offset left where it was would sit past that end, on blank card.
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(2560, 1440, () =>
            {
                MenuController controller = new(
                    (CTRRootController)Application.SharedRootController());
                try
                {
                    controller.ShowView(MenuController.VIEW_ABOUT);
                    ScrollableContainer container = Credits(controller);
                    container.SetScroll(container.GetMaxScroll());

                    CtrRenderer.OnSurfaceChanged(720, 1280);
                    controller.RelayoutTree(ScreenPresentation.Instance.Snapshot);

                    container = Credits(controller);
                    Assert.InRange(
                        container.GetScroll().Y,
                        0f,
                        MathF.Max(0f, container.GetMaxScroll().Y));
                }
                finally
                {
                    controller.Dispose();
                }
            });
        }

        /// <summary>
        /// Builds a menu controller, shows the About view and hands its credits container to
        /// <paramref name="body"/>, disposing the controller afterwards either way.
        /// </summary>
        /// <param name="body">What to assert about the container.</param>
        private static void WithAboutView(Action<ScrollableContainer> body)
        {
            MenuController controller = new(
                (CTRRootController)Application.SharedRootController());
            try
            {
                controller.ShowView(MenuController.VIEW_ABOUT);
                body(Credits(controller));
            }
            finally
            {
                controller.Dispose();
            }
        }

        /// <summary>Finds the container the credits scroll inside.</summary>
        /// <param name="controller">Controller owning the About view.</param>
        /// <returns>The credits container.</returns>
        private static ScrollableContainer Credits(MenuController controller)
        {
            ScrollableContainer found = FindScroller(controller.GetView(MenuController.VIEW_ABOUT));
            Assert.NotNull(found);
            return found;
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

        /// <summary>
        /// How far the drawn credits reach below the top of the stack they are in. Each block is
        /// scaled about its own center, so where it draws is not where its unscaled rectangle is.
        /// </summary>
        /// <param name="container">Container whose content to measure.</param>
        /// <returns>The drawn extent in logical units.</returns>
        private static float DrawnExtent(ScrollableContainer container)
        {
            float extent = 0f;
            for (int i = 0; i < container.ChildsCount(); i++)
            {
                BaseElement child = container.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                float top = child.y + (child.height * (1f - child.scaleY) / 2f);
                extent = MathF.Max(extent, top + (child.height * child.scaleY));
            }

            return extent;
        }
    }
}
