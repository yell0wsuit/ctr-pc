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
        public void TheCreditsScrollAsFarAsTheyDrawPlusTheRoomTheButtonNeeds()
        {
            // How far the credits scroll is read off the stack's own height, and every block in it
            // draws at the content scale. Leaving the stack at the height its unscaled blocks
            // measured stopped the reader a third of the way from the end on a phone. The stack is
            // told it is taller still by the room the corner button needs, which is scroll spent
            // bringing the last line out from under it.
            _ = HeadlessGame.Boot();

            foreach (LayoutSurface surface in LayoutSurfaces.All)
            {
                LayoutSurfaces.WithSurface(surface.Width, surface.Height, () =>
                    WithAboutView((container, back) =>
                    {
                        float believed = container.GetMaxScroll().Y + container.height;
                        float reserved = believed - DrawnExtent(container);

                        Assert.True(
                            reserved >= back.height * back.scaleY,
                            $"{surface.Name}: {reserved} reserved for a {back.height * back.scaleY} button");
                    }));
            }
        }

        [Fact]
        public void TheLastLineOfTheCreditsClearsTheButtonInTheCorner()
        {
            // Scrolled to the end, the last line has to come to rest above the button rather than
            // behind it - which is what it did on a phone, where the credits column is wide enough
            // to reach the corner the button sits in.
            _ = HeadlessGame.Boot();

            foreach (LayoutSurface surface in LayoutSurfaces.All)
            {
                LayoutSurfaces.WithSurface(surface.Width, surface.Height, () =>
                    WithAboutView((container, back) =>
                    {
                        CTRRectangle visible = ScreenPresentation.Instance.Snapshot.VisibleBounds;
                        float reserved = container.GetMaxScroll().Y + container.height - DrawnExtent(container);

                        // The window is centered, so its bottom edge is half the slack below the
                        // screen's. At the end of the scroll the last line sits the reservation
                        // above that edge, and the button rises its drawn height from the corner.
                        float windowBottom = (visible.h + container.height) / 2f;
                        float lastLineBottom = windowBottom - reserved;
                        float buttonTop = visible.h - (back.height * back.scaleY);

                        Assert.True(
                            lastLineBottom <= buttonTop,
                            $"{surface.Name}: the credits end at {lastLineBottom}, under a button "
                            + $"whose top is at {buttonTop}");
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
                    WithAboutView((container, _) =>
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
        /// <param name="body">What to assert about the container and the button over it.</param>
        private static void WithAboutView(Action<ScrollableContainer, Button> body)
        {
            MenuController controller = new(
                (CTRRootController)Application.SharedRootController());
            try
            {
                controller.ShowView(MenuController.VIEW_ABOUT);
                Button back = controller.GetView(MenuController.VIEW_ABOUT)
                    .GetChildWithName("backb") as Button;
                Assert.NotNull(back);
                body(Credits(controller), back);
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
