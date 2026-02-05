using System;
using System.Reflection;

using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Builds and manages the Credits menu view and its scrolling behavior.
    /// </summary>
    internal sealed class AboutView
    {
        /// <summary>
        /// Creates the Credits view and attaches it to the provided background element.
        /// </summary>
        /// <param name="background">Background element that will host the about content.</param>
        /// <param name="buttonDelegate">Delegate used for handling the back button.</param>
        /// <returns>A fully constructed <see cref="MenuView"/> for the Credits screen.</returns>
        public MenuView CreateAbout(
            BaseElement background,
            IButtonDelegation buttonDelegate)
        {
            MenuView menuView = new();
            currentContainer = BuildAboutContainer();
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
        /// Disables auto-scrolling for the Credits view.
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
        /// Handles mouse wheel scrolling for the Credits content.
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

        private static ScrollableContainer BuildAboutContainer()
        {
            string aboutText = BuildAboutText();
            float containerWidth = 1300f;
            float containerHeight = 1100f;

            VBox vBox = new VBox().InitWithOffsetAlignWidth(0f, 2, containerWidth);
            BaseElement spacer = new()
            {
                width = (int)containerWidth,
                height = 100
            };
            _ = vBox.AddChild(spacer);

            Image topLogo = Image.Image_createWithResIDQuad(Resources.Img.MenuLogo, 1);
            _ = vBox.AddChild(topLogo);

            Text aboutBody = new Text().InitWithFont(Application.GetFont(Resources.Fnt.SmallFont));
            aboutBody.SetAlignment(2);
            aboutBody.SetStringandWidth(aboutText, (int)containerWidth);

            ScrollableContainer container = new ScrollableContainer().InitWithWidthHeightContainer(containerWidth, containerHeight, vBox);
            container.anchor = container.parentAnchor = 18;

            _ = vBox.AddChild(aboutBody);

            Image bottomLogo = Image.Image_createWithResIDQuad(Resources.Img.MenuLogo, 2);
            _ = vBox.AddChild(bottomLogo);

            string specialThanksText = Application.GetString("ABOUT_SPECIAL_THANKS");
            Text specialThanks = new Text().InitWithFont(Application.GetFont(Resources.Fnt.SmallFont));
            specialThanks.SetAlignment(2);
            specialThanks.SetStringandWidth(specialThanksText, containerWidth);
            _ = vBox.AddChild(specialThanks);

            return container;
        }

        /// <summary>
        /// Builds the localized about text, including the current assembly version.
        /// </summary>
        private static string BuildAboutText()
        {
            string text = Application.GetString("ABOUT_TEXT").ToString();
            string[] separator = ["%versionNo%"];
            string[] array = text.Split(separator, StringSplitOptions.None);
            for (int i = 0; i < array.Length; i++)
            {
                if (i == 0)
                {
                    text = "";
                }
                if (i == array.Length - 1)
                {
                    string fullName = Assembly.GetExecutingAssembly().FullName;
                    text += fullName.Split('=', StringSplitOptions.None)[1].Split(',', StringSplitOptions.None)[0];
                    text += " ";
                }
                text += array[i];
            }
            return text;
        }

        /// <summary>
        /// Scroll container holding the Credits content.
        /// </summary>
        private ScrollableContainer currentContainer;

        /// <summary>
        /// Whether auto-scroll is currently enabled.
        /// </summary>
        private bool autoScrollEnabled;
    }
}
