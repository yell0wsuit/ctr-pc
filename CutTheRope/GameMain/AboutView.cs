using System;
using System.Reflection;

using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
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
        /// <returns>A fully constructed <see cref="MenuView"/> for the About/Credits screen.</returns>
        public MenuView CreateAbout(
            BaseElement background,
            IButtonDelegation buttonDelegate)
        {
            MenuView menuView = new();
            currentContainer = BuildAboutContainer(buttonDelegate);
            autoScrollEnabled = false;
            _ = background.AddChild(currentContainer);
            _ = menuView.AddChild(background);

            Button backButton = MenuController.CreateBackButtonWithDelegateID(buttonDelegate, MenuButtonId.BackToOptions);
            backButton.SetName("backb");
            backButton.x = FrameworkTypes.Canvas.xOffsetScaled;
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
            autoScrollEnabled = true;
        }

        /// <summary>
        /// Disables auto-scrolling for the About/Credits view.
        /// </summary>
        public void DisableAutoScroll()
        {
            autoScrollEnabled = false;
        }

        /// <summary>
        /// Advances auto-scroll if enabled.
        /// </summary>
        /// <returns>
        /// True if auto-scroll was applied this frame; otherwise false.
        /// </returns>
        public bool UpdateAutoScroll()
        {
            if (!autoScrollEnabled || currentContainer == null)
            {
                return false;
            }

            Vector scroll = currentContainer.GetScroll();
            Vector maxScroll = currentContainer.GetMaxScroll();
            scroll.Y += 0.5f;
            scroll.Y = Framework.Helpers.CTRMathHelper.FIT_TO_BOUNDARIES(scroll.Y, 0.0, maxScroll.Y);
            currentContainer.SetScroll(scroll);
            return true;
        }

        /// <summary>
        /// Handles mouse wheel scrolling for the About/Credits content.
        /// </summary>
        /// <param name="scrollDelta">Mouse wheel delta value.</param>
        /// <returns>
        /// True if the scroll was handled by the about container; otherwise false.
        /// </returns>
        public bool HandleMouseWheel(int scrollDelta)
        {
            if (currentContainer == null)
            {
                return false;
            }

            autoScrollEnabled = false;
            currentContainer.HandleMouseWheel(scrollDelta);
            return true;
        }

        private static ScrollableContainer BuildAboutContainer(IButtonDelegation buttonDelegate)
        {
            float containerWidth = 1300f;
            float containerHeight = 1100f;

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

            Text fanworkMain = CreateCenteredTextBlock(BuildFanworkMainText(), containerWidth);
            _ = vBox.AddChild(fanworkMain);

            Button fanworkProjectWebsite = CreateCenteredLinkButton(
                Application.GetString("ABOUT_FANWORK_PROJECT_WEBSITE"),
                MenuButtonId.FanworkProjectWebsite,
                buttonDelegate,
                containerWidth);
            _ = vBox.AddChild(fanworkProjectWebsite);

            Text fanworkProjectNote = CreateCenteredTextBlock(Application.GetString("ABOUT_FANWORK_PROJECT_NOTE"), containerWidth);
            _ = vBox.AddChild(fanworkProjectNote);

            Button fanworkCtrhWebsite = CreateCenteredLinkButton(
                Application.GetString("ABOUT_FANWORK_CTRH_WEBSITE"),
                MenuButtonId.FanworkCtrhWebsite,
                buttonDelegate,
                containerWidth);
            _ = vBox.AddChild(fanworkCtrhWebsite);

            Text fanworkLead = CreateCenteredTextBlock(Application.GetString("ABOUT_FANWORK_LEAD"), containerWidth);
            _ = vBox.AddChild(fanworkLead);

            Text fanworkTeam = CreateCenteredTextBlock(Application.GetString("ABOUT_FANWORK_TEAM"), containerWidth);
            _ = vBox.AddChild(fanworkTeam);

            Text fanworkMembers = CreateCenteredTextBlock(Application.GetString("ABOUT_FANWORK_MEMBERS"), containerWidth);
            _ = vBox.AddChild(fanworkMembers);

            // Original Zeptolab credit section

            Image ZeptolabLogo = Image.Image_createWithResIDQuad(Resources.Img.MenuLogo, 1);
            _ = vBox.AddChild(ZeptolabLogo);

            Text aboutBody = CreateCenteredTextBlock(Application.GetString("ABOUT_TEXT").ToString(), containerWidth);
            _ = vBox.AddChild(aboutBody);

            Image bottomLogo = Image.Image_createWithResIDQuad(Resources.Img.MenuLogo, 2);
            _ = vBox.AddChild(bottomLogo);

            string specialThanksText = Application.GetString("ABOUT_SPECIAL_THANKS");
            Text specialThanks = CreateCenteredTextBlock(specialThanksText, containerWidth);
            _ = vBox.AddChild(specialThanks);

            return container;
        }

        /// <summary>
        /// Creates a centered text block with the standard about font.
        /// </summary>
        /// <param name="text">Text to render in the block.</param>
        /// <param name="width">Maximum width for wrapping.</param>
        /// <returns>Configured <see cref="Text"/> element.</returns>
        private static Text CreateCenteredTextBlock(string text, float width)
        {
            Text block = new Text().InitWithFont(Application.GetFont(Resources.Fnt.SmallFont));
            block.SetAlignment(2);
            block.SetStringandWidth(text, (int)width);
            return block;
        }

        /// <summary>
        /// Creates a centered, clickable text button for URLs or actions.
        /// </summary>
        private static Button CreateCenteredLinkButton(
            string text,
            MenuButtonId buttonId,
            IButtonDelegation buttonDelegate,
            float width)
        {
            Text upText = CreateCenteredTextBlock(text, width);
            Text downText = CreateCenteredTextBlock(text, width);
            downText.color = RGBAColor.MakeRGBA(1f, 1f, 1f, 0.6f);

            Button button = new Button().InitWithUpElementDownElementandID(upText, downText, buttonId);
            button.delegateButtonDelegate = buttonDelegate;
            button.SetTouchIncreaseLeftRightTopBottom(10f, 10f, 10f, 10f);
            return button;
        }

        /// <summary>
        /// Builds the fanwork main text with version substitution.
        /// </summary>
        private static string BuildFanworkMainText()
        {
            string text = Application.GetString("ABOUT_FANWORK_MAIN").ToString();
            string version = GetAssemblyVersion();
            return text.Replace("%versionNo%", version, StringComparison.Ordinal);
        }

        /// <summary>
        /// Gets the executing assembly version string.
        /// </summary>
        private static string GetAssemblyVersion()
        {
            string fullName = Assembly.GetExecutingAssembly().FullName;
            return fullName.Split('=', StringSplitOptions.None)[1].Split(',', StringSplitOptions.None)[0];
        }

        /// <summary>
        /// Scroll container holding the About/Credits content.
        /// </summary>
        private ScrollableContainer currentContainer;

        /// <summary>
        /// Whether auto-scroll is currently enabled.
        /// </summary>
        private bool autoScrollEnabled;
    }
}
