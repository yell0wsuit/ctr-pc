using System;
using System.Reflection;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Builds and manages the About/Credits menu view and its scrolling behavior.
    /// </summary>
    internal sealed class AboutView
    {
        /// <summary>
        /// Creates the About/Credits view and attaches it to the provided background element.
        /// </summary>
        /// <param name="background">Background element that will host the about content.</param>
        /// <param name="buttonDelegate">Delegate used for handling the back button.</param>
        /// <param name="scale">
        /// Uniform scale to grow the credits text by, matching the boost menu content gets on a
        /// narrow viewport. Baked in at construction, because it sets the wrap width every text
        /// block was measured at and therefore the height of the stack they form; the viewport it
        /// was baked for is recorded in <see cref="BuiltForScale"/> so a later one can be noticed.
        /// </param>
        /// <returns>A fully constructed <see cref="MenuView"/> for the About/Credits screen.</returns>
        public MenuView CreateAbout(
            BaseElement background,
            IButtonDelegation buttonDelegate,
            float scale)
        {
            MenuView menuView = new();
            currentContainer = BuildAboutContainer(buttonDelegate, scale);
            BuiltForScale = scale;
            AutoScrollEnabled = false;
            _ = background.AddChild(currentContainer);
            _ = menuView.AddChild(background);

            backButton = MenuController.CreateBackButtonWithDelegateID(buttonDelegate, MenuButtonId.BackToOptions);
            backButton.SetName("backb");
            _ = menuView.AddChild(backButton);

            return menuView;
        }

        /// <summary>
        /// Resets scroll position to the top and enables auto-scrolling.
        /// </summary>
        public void ResetAndEnableAutoScroll()
        {
            if (currentContainer == null)
            {
                return;
            }

            currentContainer.SetScroll(new Vector(0f, 0f));
            AutoScrollEnabled = true;
        }

        /// <summary>
        /// Disables auto-scrolling for the About/Credits view.
        /// </summary>
        public void DisableAutoScroll()
        {
            AutoScrollEnabled = false;
        }

        /// <summary>
        /// Advances auto-scroll if enabled.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if auto-scroll was applied this frame; otherwise <see langword="false"/>.
        /// </returns>
        public bool UpdateAutoScroll()
        {
            if (!AutoScrollEnabled || currentContainer == null)
            {
                return false;
            }

            Vector scroll = currentContainer.GetScroll();
            Vector maxScroll = currentContainer.GetMaxScroll();
            scroll.Y += 0.5f;
            scroll.Y = Framework.Helpers.CTRMathHelper.FIT_TO_BOUNDARIES(scroll.Y, 0f, maxScroll.Y);
            currentContainer.SetScroll(scroll);
            return true;
        }

        /// <summary>
        /// Handles mouse wheel scrolling for the About/Credits content.
        /// </summary>
        /// <param name="scrollDelta">Mouse wheel delta value.</param>
        /// <returns>
        /// <see langword="true"/> if the scroll was handled by the about container; otherwise <see langword="false"/>.
        /// </returns>
        public bool HandleMouseWheel(int scrollDelta)
        {
            if (currentContainer == null)
            {
                return false;
            }

            AutoScrollEnabled = false;
            currentContainer.HandleMouseWheel(scrollDelta);
            return true;
        }

        /// <summary>
        /// Builds the scrollable About/Credits content container.
        /// </summary>
        /// <param name="buttonDelegate">Button delegate used by controls embedded in the about content.</param>
        /// <param name="scale">Uniform scale to grow the credits content by.</param>
        /// <returns>The configured scrollable content container.</returns>
        private ScrollableContainer BuildAboutContainer(IButtonDelegation buttonDelegate, float scale)
        {
            float containerWidth = ContainerWidth;
            float containerHeight = WindowHeight(ScreenPresentation.Instance.Snapshot.VisibleBounds);

            // VBox stacks all credit elements vertically within a fixed width.
            VBox vBox = new VBox().InitWithOffsetAlignWidth(0f, 2, containerWidth);

            // Scrollable container clips and scrolls the VBox content.
            ScrollableContainer container = new ScrollableContainer().InitWithWidthHeightContainer(containerWidth, containerHeight, vBox);
            container.anchor = container.parentAnchor = 18;

            // Top spacer to offset the first elements from the container's top edge.
            BaseElement spacer = new()
            {
                width = (int)containerWidth,
                height = 100
            };
            _ = vBox.AddChild(spacer);

            // Fan work credit section

            Image topLogo = Image.Image_createWithResID(Resources.Img.CutTheRopeDXLogo);
            _ = vBox.AddChild(topLogo);

            Text fanworkMain = CreateCenteredTextBlock(BuildFanworkMainText(), containerWidth, scale);
            _ = vBox.AddChild(fanworkMain);

            Button fanworkProjectWebsite = CreateCenteredLinkButton(
                Application.GetString("ABOUT_FANWORK_PROJECT_WEBSITE"),
                MenuButtonId.FanworkProjectWebsite,
                buttonDelegate,
                containerWidth,
                scale);
            _ = vBox.AddChild(fanworkProjectWebsite);

            Text fanworkProjectNote = CreateCenteredTextBlock(Application.GetString("ABOUT_FANWORK_PROJECT_NOTE"), containerWidth, scale);
            _ = vBox.AddChild(fanworkProjectNote);

            Button fanworkCtrhWebsite = CreateCenteredLinkButton(
                Application.GetString("ABOUT_FANWORK_CTRH_WEBSITE"),
                MenuButtonId.FanworkCtrhWebsite,
                buttonDelegate,
                containerWidth,
                scale);
            _ = vBox.AddChild(fanworkCtrhWebsite);

            Text fanworkLead = CreateCenteredTextBlock(Application.GetString("ABOUT_FANWORK_LEAD"), containerWidth, scale);
            _ = vBox.AddChild(fanworkLead);

            Text fanworkTeam = CreateCenteredTextBlock(Application.GetString("ABOUT_FANWORK_TEAM"), containerWidth, scale);
            _ = vBox.AddChild(fanworkTeam);

            Text fanworkMembers = CreateCenteredTextBlock(Application.GetString("ABOUT_FANWORK_MEMBERS"), containerWidth, scale);
            _ = vBox.AddChild(fanworkMembers);

            // Original Zeptolab credit section

            Image ZeptolabLogo = Image.Image_createWithResIDQuad(Resources.Img.MenuLogo, 1);
            _ = vBox.AddChild(ZeptolabLogo);

            string aboutText = ResolveVersionPlaceholder(
                Application.GetString("ABOUT_TEXT").ToString());
            Text aboutBody = CreateCenteredTextBlock(aboutText, containerWidth, scale);
            _ = vBox.AddChild(aboutBody);

            Image bottomLogo = Image.Image_createWithResIDQuad(Resources.Img.MenuLogo, 2);
            _ = vBox.AddChild(bottomLogo);

            string specialThanksText = Application.GetString("ABOUT_SPECIAL_THANKS");
            Text specialThanks = CreateCenteredTextBlock(specialThanksText, containerWidth, scale);
            _ = vBox.AddChild(specialThanks);

            GrowFromTop(vBox, scale);
            credits = vBox;
            creditsExtent = vBox.height;
            return container;
        }

        /// <summary>
        /// Grows every direct child of an already-laid-out VBox by <paramref name="scale"/>,
        /// keeping the stack contiguous from its own top edge.
        /// </summary>
        /// <remarks>
        /// The scale is applied here, once, to each top-level child - not also to a child's own
        /// nested children (a button's up/down text), which would double it - and each child's Y
        /// is corrected the same way <see cref="ViewController.PlaceFittedGroup(BaseElement)"/>
        /// corrects a top-left anchor: <see cref="BaseElement"/> always scales about its own
        /// center, so growing a stack of elements in place needs each one's distance from the
        /// stack's top corrected for that, not just multiplied through. The container this VBox
        /// scrolls inside is left untouched - its own clip rect is computed from the render
        /// backend's single global scale (see the level-select grid's cap, which hit the same
        /// wall with <c>ScrollableContainer</c>'s scissor), so this VBox stays sized to what it
        /// was already built for and only its own content reads bigger inside it.
        /// </remarks>
        /// <param name="vBox">The already-populated box to grow.</param>
        /// <param name="scale">Uniform scale to grow every child by.</param>
        private static void GrowFromTop(VBox vBox, float scale)
        {
            foreach (BaseElement child in vBox.GetChilds().Values)
            {
                if (child == null)
                {
                    continue;
                }

                child.y = LayoutMath.CornerAnchoredOffset(child.y, child.height, scale, farEdge: false);
                child.scaleX = child.scaleY = scale;
            }

            // The box was measured from its children's authored heights, and every one of them
            // now draws taller than that. How far the credits scroll is read off this height, so
            // leaving it behind stops the reader short of the end - by the whole of the growth,
            // which on a phone is the last third of the credits.
            vBox.height = (int)MathF.Round(vBox.height * scale);
        }

        /// <summary>
        /// Sizes the scrolling window to the viewport, so a tall screen reads more of the credits
        /// at once rather than through the slot a landscape one has room for.
        /// </summary>
        /// <remarks>
        /// Applied on every layout pass rather than only when the view is built again: the window
        /// is a clip rectangle, so unlike the wrap width behind
        /// <see cref="BuiltForScale"/> it costs nothing to change and needs nothing re-measured.
        /// The scroll offset is pulled back inside what the resized window can reach, because a
        /// window that just grew leaves the reader past the end of the credits otherwise.
        /// </remarks>
        /// <param name="snapshot">The viewport to lay out against.</param>
        public void ResizeWindow(ViewportLayoutSnapshot snapshot)
        {
            if (currentContainer == null)
            {
                return;
            }

            CTRRectangle visible = snapshot.VisibleBounds;

            currentContainer.width = (int)ContainerWidth;
            currentContainer.height = (int)WindowHeight(visible);

            // The button in the corner is drawn over the bottom of the window, and on a viewport
            // narrow enough for the credits column to reach that corner it covers the last line
            // read. The stack is told it is that much taller than it draws, which is scroll the
            // reader can spend to bring the end out from under the button.
            _ = (credits?.height = creditsExtent + (int)MathF.Round(ChromeReservation(snapshot)));

            Vector scroll = currentContainer.GetScroll();
            scroll.Y = Framework.Helpers.CTRMathHelper.FIT_TO_BOUNDARIES(
                scroll.Y,
                0f,
                currentContainer.GetMaxScroll().Y);
            currentContainer.SetScroll(scroll);
        }

        /// <summary>
        /// How much room at the end of the credits the button in the corner needs, so the last
        /// line can be scrolled out from under it.
        /// </summary>
        /// <remarks>
        /// Asked of <see cref="HudMetrics"/> rather than measured off the button, because the pass
        /// that sizes the button runs after this one - a scene rebuilt in between would be read
        /// while its replacement button was still at its authored size.
        /// </remarks>
        /// <param name="snapshot">The viewport to lay out against.</param>
        /// <returns>The room to reserve, in the stack's own units.</returns>
        private float ChromeReservation(ViewportLayoutSnapshot snapshot)
        {
            return backButton == null
                ? 0f
                : HudMetrics.RoomFor(
                    snapshot,
                    backButton.width,
                    backButton.height,
                    HudMetrics.IsTouchHost).Height;
        }

        /// <summary>
        /// The height of the window the credits scroll inside, on a given viewport.
        /// </summary>
        /// <param name="visible">The logical region the viewport exposes.</param>
        /// <returns>The window height in logical units.</returns>
        private static float WindowHeight(CTRRectangle visible)
        {
            return visible.h - (WindowInset * 2f);
        }

        /// <summary>Width of the column the credits are laid out in.</summary>
        private const float ContainerWidth = 1300f;

        /// <summary>
        /// Distance the scrolling window keeps from the top and bottom of the screen. Chosen so
        /// the window is the height the credits were authored at on the design shape, where the
        /// viewport is exactly the short side tall.
        /// </summary>
        private const float WindowInset = 170f;

        /// <summary>
        /// Creates a centered text block with the standard about font.
        /// </summary>
        /// <param name="text">Text to render in the block.</param>
        /// <param name="width">Maximum width for wrapping, at scale one.</param>
        /// <param name="scale">
        /// Uniform scale the block (or its containing button) will be grown by. The wrap width is
        /// shrunk by the same factor so the rendered line, once scaled, still fits within
        /// <paramref name="width"/> instead of running past it.
        /// </param>
        /// <returns>Configured <see cref="Text"/> element.</returns>
        private static Text CreateCenteredTextBlock(string text, float width, float scale)
        {
            Text block = new Text().InitWithFont(Application.GetFont(Resources.Fnt.SmallFont));
            block.SetAlignment(2);

            // Broken mid-word where a word has no break in it, which is what a credits URL is:
            // wrapping on spaces alone left it running off both sides of the column.
            block.wrapLongWords = true;
            block.SetStringandWidth(text, (int)(width / scale));
            return block;
        }

        /// <summary>
        /// Creates a centered, clickable text button for URLs or actions.
        /// </summary>
        /// <param name="text">Button label text.</param>
        /// <param name="buttonId">Identifier assigned to the button.</param>
        /// <param name="buttonDelegate">Delegate that handles button events.</param>
        /// <param name="width">Maximum width used for text layout, at scale one.</param>
        /// <param name="scale">Uniform scale the button will be grown by.</param>
        /// <returns>A configured centered link button.</returns>
        private static Button CreateCenteredLinkButton(
            string text,
            MenuButtonId buttonId,
            IButtonDelegation buttonDelegate,
            float width,
            float scale)
        {
            Text upText = CreateCenteredTextBlock(text, width, scale);
            Text downText = CreateCenteredTextBlock(text, width, scale);
            downText.color = RGBAColor.MakeRGBA(1f, 1f, 1f, 0.6f);

            Button button = new Button().InitWithUpElementDownElementandID(upText, downText, buttonId);
            button.delegateButtonDelegate = buttonDelegate;
            button.SetTouchIncreaseLeftRightTopBottom(10f, 10f, 10f, 10f);
            return button;
        }

        /// <summary>
        /// Title line shown in place of the versioned one on hosts that carry no version.
        /// </summary>
        private const string WebEditionTitle = "Cut the Rope: DX - Web Edition";

        /// <summary>
        /// Builds the fanwork main text with version substitution.
        /// </summary>
        /// <returns>The localized fanwork body text with version placeholders resolved.</returns>
        private static string BuildFanworkMainText()
        {
            return ResolveVersionPlaceholder(Application.GetString("ABOUT_FANWORK_MAIN").ToString());
        }

        /// <summary>
        /// Gets the executing assembly version string.
        /// </summary>
        /// <returns>The current assembly version string.</returns>
        private static string GetAssemblyVersion()
        {
            string fullName = Assembly.GetExecutingAssembly().FullName;
            return fullName.Split('=', StringSplitOptions.None)[1].Split(',', StringSplitOptions.None)[0];
        }

        /// <summary>
        /// Resolves the <c>%versionNo%</c> placeholder in an About string.
        /// </summary>
        /// <param name="text">Localized text that may carry the placeholder.</param>
        /// <returns>The text with the placeholder resolved for the running host.</returns>
        private static string ResolveVersionPlaceholder(string text)
        {
            if (!OperatingSystem.IsBrowser())
            {
                return text.Replace("%versionNo%", GetAssemblyVersion(), StringComparison.Ordinal);
            }

            string[] lines = text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("%versionNo%", StringComparison.Ordinal))
                {
                    lines[i] = WebEditionTitle;
                }
            }
            return string.Join('\n', lines);
        }

        /// <summary>
        /// The content scale this view was built at, so a layout pass can tell whether the
        /// viewport has since changed shape enough to need it built again.
        /// </summary>
        public float BuiltForScale { get; private set; }

        /// <summary>
        /// Gets or sets how far the credits are scrolled, so a rebuild can put the reader back
        /// where they were rather than snapping them to the top.
        /// </summary>
        public Vector ScrollOffset
        {
            get => currentContainer?.GetScroll() ?? new Vector(0f, 0f);
            set => currentContainer?.SetScroll(value);
        }

        /// <summary>
        /// Gets or sets whether the credits are scrolling themselves, carried across a rebuild for
        /// the same reason as <see cref="ScrollOffset"/>.
        /// </summary>
        public bool AutoScrollEnabled { get; set; }

        /// <summary>
        /// Scroll container holding the About/Credits content.
        /// </summary>
        private ScrollableContainer currentContainer;

        /// <summary>The stack of credits blocks the container scrolls.</summary>
        private VBox credits;

        /// <summary>
        /// How far the credits reach on their own, before any room is reserved at the end for the
        /// button drawn over the bottom of the window.
        /// </summary>
        private int creditsExtent;

        /// <summary>The button back to the options menu, drawn in the corner over the credits.</summary>
        private Button backButton;
    }
}
