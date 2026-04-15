using System;
using System.Collections.Generic;
using System.Globalization;

using CutTheRopeDX.Commons;
using CutTheRopeDX.Desktop;
using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Media;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.Helpers;

using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Main menu controller that builds menu views, handles menu buttons, and coordinates transitions into gameplay.
    /// </summary>
    internal sealed class MenuController : ViewController, IButtonDelegation, IMovieMgrDelegate, IScrollableContainerProtocol, ITimelineDelegate
    {
        /// <summary>
        /// Creates a full-width text button using the standard menu button quads.
        /// </summary>
        /// <param name="str">Button label text.</param>
        /// <param name="bid">Button identifier assigned to the created button.</param>
        /// <param name="d">Button delegate that receives press events.</param>
        /// <returns>The configured menu button.</returns>
        public static Button CreateButtonWithTextIDDelegate(string str, ButtonId bid, IButtonDelegation d)
        {
            Image image = Image.Image_createWithResIDQuad(Resources.Img.MenuButtons, 0);
            Image image2 = Image.Image_createWithResIDQuad(Resources.Img.MenuButtons, 1);
            FontGeneric font = Application.GetFont(Resources.Fnt.BigFont);
            Text text = new Text().InitWithFont(font);
            text.SetString(str);
            Text text2 = new Text().InitWithFont(font);
            text2.SetString(str);
            text.anchor = text.parentAnchor = 18;
            text2.anchor = text2.parentAnchor = 18;
            text.pingPongEnabled = true;
            text2.pingPongEnabled = true;
            _ = image.AddChild(text);
            _ = image2.AddChild(text2);
            Button button = new Button().InitWithUpElementDownElementandID(image, image2, bid);
            button.SetTouchIncreaseLeftRightTopBottom(15, 15, 15, 15);
            button.delegateButtonDelegate = d;
            return button;
        }

        /// <summary>
        /// Creates a shorter text button, optionally rendering it in the selected state.
        /// </summary>
        /// <param name="str">Button label text.</param>
        /// <param name="bid">Button identifier assigned to the created button.</param>
        /// <param name="d">Button delegate that receives press events.</param>
        /// <param name="selected">Whether the button should render with its selected-state quads.</param>
        /// <returns>The configured short menu button.</returns>
        public static Button CreateShortButtonWithTextIDDelegate(string str, ButtonId bid, IButtonDelegation d, bool selected = false)
        {
            // When selected, swap quads so the "down" look is the default state
            int upQuad = selected ? 2 : 3;
            int downQuad = selected ? 3 : 2;
            Image image = Image.Image_createWithResIDQuad(Resources.Img.MenuButtons, upQuad);
            Image image2 = Image.Image_createWithResIDQuad(Resources.Img.MenuButtons, downQuad);
            FontGeneric font = Application.GetFont(Resources.Fnt.BigFont);
            Text text = new Text().InitWithFont(font);
            text.SetString(str);
            Text text2 = new Text().InitWithFont(font);
            text2.SetString(str);
            text.anchor = text.parentAnchor = 18;
            text2.anchor = text2.parentAnchor = 18;
            text.pingPongEnabled = true;
            text2.pingPongEnabled = true;
            _ = image.AddChild(text);
            _ = image2.AddChild(text2);
            Button button = new Button().InitWithUpElementDownElementandID(image, image2, bid);
            button.SetTouchIncreaseLeftRightTopBottom(15, 15, 15, 15);
            button.delegateButtonDelegate = d;
            return button;
        }

        /// <summary>
        /// Creates a two-state text toggle button using the standard menu button quads.
        /// </summary>
        /// <param name="str1">Label text for the first toggle state.</param>
        /// <param name="str2">Label text for the second toggle state.</param>
        /// <param name="bid">Button identifier assigned to the created toggle.</param>
        /// <param name="d">Button delegate that receives press events.</param>
        /// <returns>The configured toggle button.</returns>
        public static ToggleButton CreateToggleButtonWithText1Text2IDDelegate(string str1, string str2, ButtonId bid, IButtonDelegation d)
        {
            Image image = Image.Image_createWithResIDQuad(Resources.Img.MenuButtons, 0);
            Image image2 = Image.Image_createWithResIDQuad(Resources.Img.MenuButtons, 1);
            Image image3 = Image.Image_createWithResIDQuad(Resources.Img.MenuButtons, 0);
            Image image4 = Image.Image_createWithResIDQuad(Resources.Img.MenuButtons, 1);
            FontGeneric font = Application.GetFont(Resources.Fnt.BigFont);
            Text text = new Text().InitWithFont(font);
            text.SetString(str1);
            Text text2 = new Text().InitWithFont(font);
            text2.SetString(str1);
            Text text3 = new Text().InitWithFont(font);
            text3.SetString(str2);
            Text text4 = new Text().InitWithFont(font);
            text4.SetString(str2);
            text.anchor = text.parentAnchor = 18;
            text2.anchor = text2.parentAnchor = 18;
            text3.anchor = text3.parentAnchor = 18;
            text4.anchor = text4.parentAnchor = 18;
            text.pingPongEnabled = true;
            text2.pingPongEnabled = true;
            text3.pingPongEnabled = true;
            text4.pingPongEnabled = true;
            _ = image.AddChild(text);
            _ = image2.AddChild(text2);
            _ = image3.AddChild(text3);
            _ = image4.AddChild(text4);
            ToggleButton toggleButton = new ToggleButton().InitWithUpElement1DownElement1UpElement2DownElement2andID(image, image2, image3, image4, bid);
            toggleButton.SetTouchIncreaseLeftRightTopBottom(10, 10, 10, 10);
            toggleButton.delegateButtonDelegate = d;
            return toggleButton;
        }

        /// <summary>
        /// Creates the standard back button.
        /// </summary>
        /// <param name="d">Button delegate that receives press events.</param>
        /// <param name="bid">Button identifier assigned to the back button.</param>
        /// <returns>The configured back button.</returns>
        public static Button CreateBackButtonWithDelegateID(IButtonDelegation d, ButtonId bid)
        {
            Button button = CreateButtonWithImageQuad1Quad2IDDelegate(Resources.Img.MenuExtraButtons, 0, 1, bid, d);
            button.anchor = button.parentAnchor = 33;
            return button;
        }

        /// <summary>
        /// Creates an image button from a texture resource.
        /// </summary>
        /// <param name="resourceName">Texture resource name for the button image.</param>
        /// <param name="bid">Button identifier assigned to the created button.</param>
        /// <param name="d">Button delegate that receives press events.</param>
        /// <returns>The configured image button.</returns>
        public static Button CreateButtonWithImageIDDelegate(string resourceName, ButtonId bid, IButtonDelegation d)
        {
            CTRTexture2D texture = Application.GetTexture(resourceName);
            Image up = Image.Image_create(texture);
            Image image = Image.Image_create(texture);
            image.scaleX = 1.2f;
            image.scaleY = 1.2f;
            Button button = new Button().InitWithUpElementDownElementandID(up, image, bid);
            button.SetTouchIncreaseLeftRightTopBottom(10, 10, 10, 10);
            button.delegateButtonDelegate = d;
            return button;
        }

        /// <summary>
        /// Creates an image button whose down-state quad is position-adjusted relative to the up-state quad.
        /// </summary>
        /// <param name="resourceName">Texture resource name containing both quads.</param>
        /// <param name="q1">Up-state quad index.</param>
        /// <param name="q2">Down-state quad index.</param>
        /// <param name="bid">Button identifier assigned to the created button.</param>
        /// <param name="d">Button delegate that receives press events.</param>
        /// <returns>The configured image button.</returns>
        public static Button CreateButton2WithImageQuad1Quad2IDDelegate(string resourceName, int q1, int q2, ButtonId bid, IButtonDelegation d)
        {
            Image up = Image.Image_createWithResIDQuad(resourceName, q1);
            Image image = Image.Image_createWithResIDQuad(resourceName, q2);
            Vector relativeQuadOffset = Image.GetRelativeQuadOffset(resourceName, q2, q1);
            image.x -= relativeQuadOffset.X;
            image.y -= relativeQuadOffset.Y;
            Button button = new Button().InitWithUpElementDownElementandID(up, image, bid);
            button.delegateButtonDelegate = d;
            return button;
        }

        /// <summary>
        /// Creates an image button from explicit up and down quad indices.
        /// </summary>
        /// <param name="resourceName">Texture resource name containing both quads.</param>
        /// <param name="q1">Up-state quad index.</param>
        /// <param name="q2">Down-state quad index.</param>
        /// <param name="bid">Button identifier assigned to the created button.</param>
        /// <param name="d">Button delegate that receives press events.</param>
        /// <returns>The configured image button.</returns>
        public static Button CreateButtonWithImageQuad1Quad2IDDelegate(string resourceName, int q1, int q2, ButtonId bid, IButtonDelegation d)
        {
            Image image = Image.Image_createWithResIDQuad(resourceName, q1);
            Image image2 = Image.Image_createWithResIDQuad(resourceName, q2);
            Button button = new Button().InitWithUpElementDownElementandID(image, image2, bid);
            button.delegateButtonDelegate = d;
            CTRTexture2D texture = Application.GetTexture(resourceName);
            button.ForceTouchRect(MakeRectangle(texture.quadOffsets[q1].X, texture.quadOffsets[q1].Y, texture.quadRects[q1].w, texture.quadRects[q1].h));
            return button;
        }

        /// <summary>
        /// Creates an image button that reuses one quad for both button states.
        /// </summary>
        /// <param name="resourceName">Texture resource name containing the quad.</param>
        /// <param name="quad">Quad index used by the button states.</param>
        /// <param name="bid">Raw button identifier assigned to the created button.</param>
        /// <param name="d">Button delegate that receives press events.</param>
        /// <returns>The configured image button.</returns>
        public static Button CreateButtonWithImageQuadIDDelegate(string resourceName, int quad, int bid, IButtonDelegation d)
        {
            Image up = Image.Image_createWithResIDQuad(resourceName, quad);
            up.color.AlphaChannel = 0.6f;
            Image down = Image.Image_createWithResIDQuad(resourceName, quad);
            Button button = new Button().InitWithUpElementDownElementandID(up, down, bid);
            button.delegateButtonDelegate = d;
            CTRTexture2D texture = Application.GetTexture(resourceName);
            button.ForceTouchRect(MakeRectangle(texture.quadOffsets[quad].X, texture.quadOffsets[quad].Y, texture.quadRects[quad].w, texture.quadRects[quad].h));
            return button;
        }

        /// <summary>
        /// Creates the menu background with optional logo and rotating shadow layer.
        /// </summary>
        /// <param name="l">Whether to include the menu logo and logo candy button.</param>
        /// <param name="s">Whether to include the rotating shadow layer.</param>
        /// <returns>The configured background element.</returns>
        public BaseElement CreateBackgroundWithLogowithShadow(bool l, bool s)
        {
            BaseElement baseElement = new()
            {
                width = (int)SCREEN_WIDTH,
                height = (int)SCREEN_HEIGHT
            };

            // Select secondary background based on special events
            string backgroundResource;
            int backgroundQuad;
            switch (true)
            {
                case var _ when SpecialEvents.IsXmas:
                    backgroundResource = Resources.Img.MenuBgrXmas;
                    backgroundQuad = 0;
                    break;
                default:
                    backgroundResource = Resources.Img.MenuBgr;
                    backgroundQuad = 0;
                    break;
            }

            Image image = Image.Image_createWithResIDQuad(backgroundResource, backgroundQuad);
            image.anchor = image.parentAnchor = 34;
            image.scaleX = image.scaleY = 1.25f;
            image.rotationCenterY = image.height / 2;
            image.passTransformationsToChilds = false;
            _ = baseElement.AddChild(image);
            if (l)
            {
                // Select main background based on special events
                string backgroundSecondaryResource;
                int backgroundSecondaryQuad;
                switch (true)
                {
                    case var _ when SpecialEvents.IsXmas:
                        backgroundSecondaryResource = Resources.Img.MenuBgrXmas;
                        backgroundSecondaryQuad = 1;
                        break;
                    default:
                        backgroundSecondaryResource = Resources.Img.MenuBgr;
                        backgroundSecondaryQuad = 1;
                        break;
                }

                Image image2 = Image.Image_createWithResIDQuad(backgroundSecondaryResource, backgroundSecondaryQuad);
                image2.anchor = image2.parentAnchor = 34;
                image2.scaleX = image2.scaleY = 1.25f;
                image2.passTransformationsToChilds = false;
                image2.rotationCenterY = image2.height / 2;
                _ = image.AddChild(image2);

                // Add event-specific decorations to logo -- layer bottom
                switch (true)
                {
                    case var _ when SpecialEvents.IsXmas:
                        // Hat background layer (behind the logo) - add to baseElement before logo
                        Image hatBackground = Image.Image_createWithResIDQuad(Resources.Img.MenuLogoXmasHat, 0);
                        hatBackground.anchor = 9;  // Top-left of the hat sprite
                        hatBackground.parentAnchor = 9;  // Relative to top-left of base (no positioning limits)
                        hatBackground.x = 965f;  // Adjust horizontal position (positive = right)
                        hatBackground.y = 71f;  // Adjust vertical position (positive = down)
                        _ = baseElement.AddChild(hatBackground);
                        break;
                    default:
                        break;
                }

                // Main logo
                Image image3 = Image.Image_createWithResIDQuad(Resources.Img.MenuLogoNew, 52);
                image3.anchor = 10;
                image3.parentAnchor = 10;
                image3.y = 55f;
                _ = baseElement.AddChild(image3);

                // Candy on rope (positioned under the logo)
                // Get selected candy skin from preferences (0-50 for candy_01 to candy_51)
                int selectedCandySkin = Preferences.GetIntForKey("PREFS_SELECTED_CANDY");
                Image candyUp = Image.Image_createWithResIDQuad(Resources.Img.MenuLogoNew, selectedCandySkin);
                Image candyDown = Image.Image_createWithResIDQuad(Resources.Img.MenuLogoNew, selectedCandySkin);
                candyDown.scaleX = candyDown.scaleY = 0.95f;  // Slight press feedback
                Button candyButton = new Button().InitWithUpElementDownElementandID(candyUp, candyDown, MenuButtonId.CandySelect);
                candyButton.SetName("logoCandyButton");
                candyButton.delegateButtonDelegate = this;
                candyButton.anchor = candyButton.parentAnchor = 10;  // Top-center of logo
                candyButton.x = 143f;  // Offset right from center
                candyButton.y = 490f;  // Offset down from top of logo
                candyButton.SetTouchIncreaseLeftRightTopBottom(40f, 40f, 40f, 40f);
                _ = image3.AddChild(candyButton);

                // Check if tutorial has been completed
                bool showCandyTutorial = !Preferences.GetBooleanForKey("PREFS_CANDY_WAS_CHANGED");

                if (showCandyTutorial)
                {
                    // Glow effect - pulsing animation (shrink/expand rapidly, pause, repeat)
                    /*
                    Image glowImage = Image.Image_createWithResIDQuad(Resources.Img.CandySelectionFx, 0);
                    glowImage.x = -25f;
                    glowImage.y = -25f;
                    Timeline glowTimeline = new Timeline().InitWithMaxKeyFramesOnTrack(6);
                    // Rapid pulse: normal -> shrink -> expand -> shrink -> normal, then pause
                    glowTimeline.AddKeyFrame(KeyFrame.MakeScale(1, 1, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                    glowTimeline.AddKeyFrame(KeyFrame.MakeScale(0.85, 0.85, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.3));
                    glowTimeline.AddKeyFrame(KeyFrame.MakeScale(1.15, 1.15, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.3));
                    glowTimeline.AddKeyFrame(KeyFrame.MakeScale(0.85, 0.85, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.3));
                    glowTimeline.AddKeyFrame(KeyFrame.MakeScale(1, 1, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.3));
                    glowTimeline.AddKeyFrame(KeyFrame.MakeScale(1, 1, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1));  // Pause
                    glowTimeline.SetTimelineLoopType(Timeline.LoopType.TIMELINE_REPLAY);
                    _ = glowImage.AddTimeline(glowTimeline);
                    glowImage.PlayTimeline(0);
                    _ = candyButton.AddChild(glowImage);
                    */

                    // Pointing hand indicator
                    Image handImage = Image.Image_createWithResIDQuad(Resources.Img.CandySelectionFx, 1);
                    // Hand pointing animation - horizontal jabbing/pointing motion
                    // Keep y constant for horizontal movement only
                    Timeline handTimeline = new Timeline().InitWithMaxKeyFramesOnTrack(3);
                    handTimeline.AddKeyFrame(KeyFrame.MakePos(200, 70, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0));
                    handTimeline.AddKeyFrame(KeyFrame.MakePos(180, 70, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.6f));  // Move LEFT (toward candy)
                    handTimeline.AddKeyFrame(KeyFrame.MakePos(200, 70, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.6f));
                    handTimeline.SetTimelineLoopType(Timeline.LoopType.TIMELINE_REPLAY);
                    _ = handImage.AddTimeline(handTimeline);
                    handImage.PlayTimeline(0);
                    _ = candyButton.AddChild(handImage);
                }

                // Add event-specific decorations to logo -- layer top
                switch (true)
                {
                    case var _ when SpecialEvents.IsXmas:
                        // Hat foreground layer (on top of the text logo)
                        Image hatForeground = Image.Image_createWithResIDQuad(Resources.Img.MenuLogoXmasHat, 1);
                        hatForeground.anchor = 9;  // Top-left of the hat sprite
                        hatForeground.parentAnchor = 9;  // Relative to top-left of logo
                        hatForeground.x = 30f;  // Adjust horizontal position (positive = right)
                        hatForeground.y = -80f;  // Adjust vertical position (positive = down)
                        _ = image3.AddChild(hatForeground);
                        break;
                    default:
                        break;
                }

                _ = baseElement.AddChild(image3);
            }
            if (s)
            {
                Image image4 = Image.Image_createWithResIDQuad(Resources.Img.MenuBgrShadow, 0);
                image4.anchor = image4.parentAnchor = 18;
                image4.scaleX = image4.scaleY = 2f;
                Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(3);
                timeline.AddKeyFrame(KeyFrame.MakeRotation(45, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                timeline.AddKeyFrame(KeyFrame.MakeRotation(405, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 75));
                timeline.SetTimelineLoopType(Timeline.LoopType.TIMELINE_REPLAY);
                _ = image4.AddTimeline(timeline);
                image4.PlayTimeline(0);
                _ = baseElement.AddChild(image4);
            }
            return baseElement;
        }

        /// <summary>
        /// Creates the menu background with an optional logo and the default shadow layer.
        /// </summary>
        /// <param name="l">Whether to include the menu logo and logo candy button.</param>
        /// <returns>The configured background element.</returns>
        public BaseElement CreateBackgroundWithLogo(bool l)
        {
            return CreateBackgroundWithLogowithShadow(l, true);
        }

        /// <summary>
        /// Creates an audio option element for a specific icon quad and toggle state.
        /// </summary>
        /// <param name="q">Audio icon quad index.</param>
        /// <param name="b">Whether to draw the disabled cross overlay.</param>
        /// <param name="p">Whether to use the pressed-state background quad.</param>
        /// <returns>The configured audio option image.</returns>
        public static Image CreateAudioElementForQuadwithCrosspressediconOffset(int q, bool b, bool p)
        {
            int pressedStateQuad = p ? 1 : 0;
            Image image = Image.Image_createWithResIDQuad(Resources.Img.MenuOptions, pressedStateQuad);
            Image image2 = Image.Image_createWithResIDQuad(Resources.Img.MenuOptions, q);
            image2.parentAnchor = image2.anchor = 9;
            image2.x = (image.width - image2.width) / 2f;
            image2.y = (image.height - image2.height) / 2f;
            _ = image.AddChild(image2);
            if (b)
            {
                image2.color = RGBAColor.MakeRGBA(0.5f, 0.5f, 0.5f, 0.5f);
                Image image3 = Image.Image_createWithResIDQuad(Resources.Img.MenuOptions, 4);
                image3.parentAnchor = image3.anchor = 9;
                image3.x = image2.x + image2.width - (image3.width / 2f);
                image3.y = image2.y + image2.height - image3.height;
                _ = image.AddChild(image3);
            }
            return image;
        }

        /// <summary>
        /// Creates a toggle button for an audio option.
        /// </summary>
        /// <param name="q">Audio icon quad index.</param>
        /// <param name="delegateValue">Button delegate that receives press events.</param>
        /// <param name="bid">Button identifier assigned to the created toggle.</param>
        /// <returns>The configured audio toggle button.</returns>
        public static ToggleButton CreateAudioButtonWithQuadDelegateIDiconOffset(int q, IButtonDelegation delegateValue, ButtonId bid)
        {
            Image u = CreateAudioElementForQuadwithCrosspressediconOffset(q, false, false);
            Image d = CreateAudioElementForQuadwithCrosspressediconOffset(q, false, true);
            Image u2 = CreateAudioElementForQuadwithCrosspressediconOffset(q, true, false);
            Image d2 = CreateAudioElementForQuadwithCrosspressediconOffset(q, true, true);
            ToggleButton toggleButton = new ToggleButton().InitWithUpElement1DownElement1UpElement2DownElement2andID(u, d, u2, d2, bid);
            toggleButton.delegateButtonDelegate = delegateValue;
            return toggleButton;
        }

        /*public static Button CreateLanguageButtonWithIDDelegate(ButtonId bid, IButtonDelegation d)
        {
            int q = LanguageHelper.GetLanguageFlagQuadIndex();
            string string2 = Application.GetString("LANGUAGE");
            Image image = Image.Image_createWithResIDQuad(Resources.Img.MenuButtons, 0);
            Image image2 = Image.Image_createWithResIDQuad(Resources.Img.MenuButtons, 1);
            FontGeneric font = Application.GetFont(Resources.Fnt.BigFont);
            Text text = new Text().InitWithFont(font);
            text.SetString(string2);
            Text text2 = new Text().InitWithFont(font);
            text2.SetString(string2);
            text.anchor = text.parentAnchor = 18;
            text2.anchor = text2.parentAnchor = 18;
            _ = image.AddChild(text);
            _ = image2.AddChild(text2);
            Image image3 = Image.Image_createWithResIDQuad(Resources.Img.MenuExtraButtons, q);
            Image image4 = Image.Image_createWithResIDQuad(Resources.Img.MenuExtraButtons, q);
            image4.parentAnchor = image3.parentAnchor = 20;
            image4.anchor = image3.anchor = 20;
            _ = text.AddChild(image3);
            _ = text2.AddChild(image4);
            text.width += (int)(image3.width + RTPD(10));
            text2.width += (int)(image4.width + RTPD(10));
            Button button = new Button().InitWithUpElementDownElementandID(image, image2, bid);
            button.SetTouchIncreaseLeftRightTopBottom(15, 15, 15, 15);
            button.delegateButtonDelegate = d;
            return button;
        }*/

        /// <summary>
        /// Creates an image element for a resource quad, or an empty element when no valid quad is provided.
        /// </summary>
        /// <param name="resourceName">Texture resource name.</param>
        /// <param name="quad">Quad index to create, or -1 for an empty element.</param>
        /// <returns>The created image or empty element.</returns>
        public static BaseElement CreateElementWithResIdquad(string resourceName, int quad)
        {
            return !string.IsNullOrEmpty(resourceName) && quad != -1 ? Image.Image_createWithResIDQuad(resourceName, quad) : new BaseElement();
        }

        /// <summary>
        /// Creates a toggle button from two resource quads.
        /// </summary>
        /// <param name="resourceName">Texture resource name containing both quads.</param>
        /// <param name="quad">First toggle-state quad index.</param>
        /// <param name="quad2">Second toggle-state quad index.</param>
        /// <param name="bId">Raw button identifier assigned to the created toggle.</param>
        /// <param name="delegateValue">Button delegate that receives press events.</param>
        /// <returns>The configured toggle button.</returns>
        public static ToggleButton CreateToggleButtonWithResquadquad2buttonIDdelegate(string resourceName, int quad, int quad2, int bId, IButtonDelegation delegateValue)
        {
            BaseElement baseElement = CreateElementWithResIdquad(resourceName, quad);
            BaseElement baseElement2 = CreateElementWithResIdquad(resourceName, quad);
            BaseElement baseElement3 = CreateElementWithResIdquad(resourceName, quad2);
            BaseElement baseElement4 = CreateElementWithResIdquad(resourceName, quad2);
            int width = MAX(baseElement.width, baseElement3.width);
            int height = MAX(baseElement.height, baseElement3.height);
            baseElement.width = baseElement2.width = width;
            baseElement.height = baseElement2.height = height;
            baseElement3.width = baseElement4.width = width;
            baseElement3.height = baseElement4.height = height;
            baseElement2.scaleX = baseElement2.scaleY = baseElement4.scaleX = baseElement4.scaleY = 1.2f;
            ToggleButton toggleButton = new ToggleButton().InitWithUpElement1DownElement1UpElement2DownElement2andID(baseElement, baseElement2, baseElement3, baseElement4, bId);
            toggleButton.delegateButtonDelegate = delegateValue;
            return toggleButton;
        }

        /// <summary>
        /// Adds the seasonal snowfall overlay to a menu view when the event is enabled.
        /// </summary>
        /// <param name="menuView">View that should receive the overlay.</param>
        private static void AttachSnowfallOverlay(View menuView)
        {
            SnowfallOverlay overlay = SnowfallOverlay.CreateIfEnabled();
            if (overlay != null)
            {
                overlay.anchor = overlay.parentAnchor = 9;
                overlay.Start();
                _ = menuView.AddChild(overlay);
            }
        }

        /// <summary>
        /// Creates a control illustration with a label and either a check toggle or fixed check mark.
        /// </summary>
        /// <param name="q">Illustration quad index in the menu options texture.</param>
        /// <param name="str">Label text shown under the illustration.</param>
        /// <param name="bId">Raw button identifier for the toggle, or -1 for a non-interactive check mark.</param>
        /// <param name="delegateValue">Button delegate that receives toggle events.</param>
        /// <returns>The configured control option element.</returns>
        public static BaseElement CreateControlButtontitleAnchortextbuttonIDdelegate(int q, string str, int bId, IButtonDelegation delegateValue)
        {
            Image image = Image.Image_createWithResIDQuad(Resources.Img.MenuOptions, q);
            int illustrationHeight = image.height;
            image.height = illustrationHeight + 140;
            Text text = Text.CreateWithFontandString(Resources.Fnt.SmallFont, str);
            text.parentAnchor = 9;
            text.anchor = 18;
            text.scaleX = text.scaleY = 0.75f;
            text.SetAlignment(2);
            _ = image.AddChild(text);
            text.x = image.width / 2f;
            FontGeneric font = Application.GetFont(Resources.Fnt.SmallFont);
            float singleLineHeight = (font.FontHeight() + font.GetTopSpacing()) * text.scaleY;
            float extraTextHeight = (text.height * text.scaleY) - singleLineHeight;
            text.y = illustrationHeight + 37 + (extraTextHeight > 0 ? 10 : 0);
            if (extraTextHeight > 0)
            {
                image.height = illustrationHeight + 140 + (int)extraTextHeight;
            }
            float checkY = illustrationHeight + 75 + extraTextHeight;
            if (bId != -1)
            {
                ToggleButton toggleButton = CreateToggleButtonWithResquadquad2buttonIDdelegate(Resources.Img.MenuOptions, -1, 8, bId, delegateValue);
                toggleButton.SetName("button");
                toggleButton.parentAnchor = 9;
                toggleButton.x = (image.width - toggleButton.width) / 2f;
                toggleButton.y = checkY;
                Image checkBg = Image.Image_createWithResIDQuad(Resources.Img.MenuOptions, 9);
                checkBg.parentAnchor = 9;
                checkBg.x = ((image.width - checkBg.width) / 2f) - 10;
                checkBg.y = toggleButton.y + toggleButton.height - checkBg.height;
                _ = image.AddChild(checkBg);
                _ = image.AddChild(toggleButton);
                int horizontalTouchPadding = (image.width / 2) - (toggleButton.width / 2);
                toggleButton.SetTouchIncreaseLeftRightTopBottom(horizontalTouchPadding, horizontalTouchPadding, image.height * 0.85f, 0);
            }
            else
            {
                Image image2 = Image.Image_createWithResIDQuad(Resources.Img.MenuOptions, 7);
                image2.parentAnchor = 9;
                image2.x = (image.width - image2.width) / 2f;
                image2.y = checkY;
                Image checkBg = Image.Image_createWithResIDQuad(Resources.Img.MenuOptions, 10);
                checkBg.parentAnchor = 9;
                checkBg.x = ((image.width - checkBg.width) / 2f) - 10;
                checkBg.y = image2.y + image2.height - checkBg.height;
                _ = image.AddChild(checkBg);
                _ = image.AddChild(image2);
            }
            return image;
        }

        /// <summary>
        /// Creates the visual element for an achievement or leaderboard button state.
        /// </summary>
        /// <param name="quad">Icon quad index.</param>
        /// <param name="pressed">Whether to use the pressed-state button background.</param>
        /// <returns>The configured scores button image.</returns>
        public static Image CreateBlankScoresButtonWithIconpressed(int quad, bool pressed)
        {
            Image image3 = Image.Image_createWithResIDQuad(Resources.Img.MenuButtonAchivCup, pressed ? 1 : 0);
            Image image2 = Image.Image_createWithResIDQuad(Resources.Img.MenuButtonAchivCup, quad);
            _ = image3.AddChild(image2);
            image2.parentAnchor = 9;
            Image.SetElementPositionWithRelativeQuadOffset(image2, Resources.Img.MenuButtonAchivCup, 0, quad);
            return image3;
        }

        /// <summary>
        /// Creates an achievement or leaderboard button.
        /// </summary>
        /// <param name="quad">Icon quad index.</param>
        /// <param name="bId">Raw button identifier assigned to the created button.</param>
        /// <param name="delegateValue">Button delegate that receives press events.</param>
        /// <returns>The configured scores button.</returns>
        public static Button CreateScoresButtonWithIconbuttonIDdelegate(int quad, int bId, IButtonDelegation delegateValue)
        {
            Image up = CreateBlankScoresButtonWithIconpressed(quad, false);
            Image image = CreateBlankScoresButtonWithIconpressed(quad, true);
            Image.SetElementPositionWithRelativeQuadOffset(image, Resources.Img.MenuButtonAchivCup, 0, 1);
            Button button = new Button().InitWithUpElementDownElementandID(up, image, bId);
            button.delegateButtonDelegate = delegateValue;
            return button;
        }

        /// <summary>
        /// Builds the main menu view.
        /// </summary>
        public void CreateMainMenu()
        {
            MenuView menuView = new();
            BaseElement baseElement = CreateBackgroundWithLogo(true);
            VBox vBox = new VBox().InitWithOffsetAlignWidth(5, 2, SCREEN_WIDTH);
            vBox.anchor = vBox.parentAnchor = 34;
            vBox.y = -85f;
            Button c = CreateButtonWithTextIDDelegate(Application.GetString("PLAY"), MenuButtonId.Play, this);
            _ = vBox.AddChild(c);
            Button c2 = CreateButtonWithTextIDDelegate(Application.GetString("OPTIONS"), MenuButtonId.Options, this);
            _ = vBox.AddChild(c2);
            Button c3 = CreateButtonWithTextIDDelegate(Application.GetString("QUIT_BUTTON"), MenuButtonId.ShowQuitPopup, this);
            _ = vBox.AddChild(c3);
            _ = baseElement.AddChild(vBox);
            bool flag = Application.GetString("FACEBOOK_BUTTON").Length > 0;
            if (flag)
            {
                BaseElement baseElement2 = new();
                baseElement2.SetName("container");
                baseElement2.parentAnchor = baseElement2.anchor = 18;
                baseElement2.width = baseElement.width;
                baseElement2.height = baseElement.height;
                baseElement2.x -= Canvas.xOffsetScaled;
                _ = baseElement.AddChild(baseElement2);
                CTRTexture2D texture = Application.GetTexture(Resources.Img.MenuExtraButtons);
                Button button = CreateButton2WithImageQuad1Quad2IDDelegate(Resources.Img.MenuExtraButtons, 3, 3, MenuButtonId.OpenTwitter, this);
                button.anchor = 9;
                button.parentAnchor = 36;
                Image.SetElementPositionWithQuadOffset(button, Resources.Img.MenuExtraButtons, 3);
                button.x -= texture.preCutSize.X;
                button.y -= texture.preCutSize.Y;
                _ = baseElement2.AddChild(button);
                Button button2 = CreateButton2WithImageQuad1Quad2IDDelegate(Resources.Img.MenuExtraButtons, 2, 2, MenuButtonId.OpenFacebook, this);
                button2.anchor = 9;
                button2.parentAnchor = 36;
                Image.SetElementPositionWithQuadOffset(button2, Resources.Img.MenuExtraButtons, 2);
                button2.x -= texture.preCutSize.X;
                button2.y -= texture.preCutSize.Y;
                if (flag)
                {
                    _ = baseElement2.AddChild(button2);
                }
                Image image = Image.Image_createWithResIDQuad(Resources.Img.MenuExtraButtonsEn, 0);
                image.anchor = 9;
                image.parentAnchor = 36;
                Image.SetElementPositionWithQuadOffset(image, Resources.Img.MenuExtraButtonsEn, 0);
                image.x -= texture.preCutSize.X;
                image.y -= texture.preCutSize.Y;
                _ = baseElement2.AddChild(image);
            }
            _ = menuView.AddChild(baseElement);
            AttachSnowfallOverlay(menuView);
            AddViewwithID(menuView, 0);
        }

        /// <summary>
        /// Builds the options view.
        /// </summary>
        public void CreateOptions()
        {
            MenuView menuView = new();
            BaseElement baseElement = CreateBackgroundWithLogowithShadow(false, false);
            _ = menuView.AddChild(baseElement);
            BaseElement baseElement2 = CreateControlButtontitleAnchortextbuttonIDdelegate(5, Application.GetString("DRAG_TO_CUT"), -1, null);
            BaseElement baseElement3 = CreateControlButtontitleAnchortextbuttonIDdelegate(6, Application.GetString("CLICK_TO_CUT"), MenuButtonId.ToggleClickToCut, this);
            HBox hBox = new HBox().InitWithOffsetAlignHeight(RTPD(80), 16, MAX(baseElement2.height, baseElement3.height));
            hBox.parentAnchor = hBox.anchor = 18;
            _ = hBox.AddChild(baseElement2);
            _ = hBox.AddChild(baseElement3);
            _ = menuView.AddChild(hBox);
            Image image = Image.Image_createWithResIDQuad(Resources.Img.MenuBgrShadow, 0);
            image.anchor = image.parentAnchor = 18;
            image.scaleX = image.scaleY = 2f;
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(3);
            timeline.AddKeyFrame(KeyFrame.MakeRotation(45, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline.AddKeyFrame(KeyFrame.MakeRotation(405, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 75));
            timeline.SetTimelineLoopType(Timeline.LoopType.TIMELINE_REPLAY);
            _ = image.AddTimeline(timeline);
            image.PlayTimeline(0);
            _ = menuView.AddChild(image);
            VBox vBox = new VBox().InitWithOffsetAlignWidth(5f, 2, SCREEN_WIDTH);
            vBox.anchor = vBox.parentAnchor = 18;
            ToggleButton toggleButton = CreateAudioButtonWithQuadDelegateIDiconOffset(3, this, MenuButtonId.ToggleMusic);
            ToggleButton toggleButton2 = CreateAudioButtonWithQuadDelegateIDiconOffset(2, this, MenuButtonId.ToggleSound);
            HBox hBox2 = new HBox().InitWithOffsetAlignHeight(-10f, 16, toggleButton.height);
            _ = hBox2.AddChild(toggleButton2);
            _ = hBox2.AddChild(toggleButton);
            _ = vBox.AddChild(hBox2);
            Button langBtn = CreateButtonWithTextIDDelegate(Application.GetString("LANGUAGE"), MenuButtonId.ShowLanguage, this);
            _ = vBox.AddChild(langBtn);
            Button c2 = CreateButtonWithTextIDDelegate(Application.GetString("RESET"), MenuButtonId.ShowReset, this);
            _ = vBox.AddChild(c2);
            Button c3 = CreateButtonWithTextIDDelegate(Application.GetString("CREDITS"), MenuButtonId.ShowCredits, this);
            _ = vBox.AddChild(c3);
            _ = baseElement.AddChild(vBox);
            hBox.y = (vBox.height / 2) + 10;
            vBox.y = -hBox.height / 2;
            bool flag4 = Preferences.GetBooleanForKey("SOUND_ON");
            bool flag2 = Preferences.GetBooleanForKey("MUSIC_ON");
            bool flag3 = Preferences.GetBooleanForKey("PREFS_CLICK_TO_CUT");
            if (!flag4)
            {
                toggleButton2.Toggle();
            }
            if (!flag2)
            {
                toggleButton.Toggle();
            }
            ToggleButton toggleButton3 = (ToggleButton)baseElement3.GetChildWithName("button");
            if (flag3 && toggleButton3 != null)
            {
                toggleButton3.Toggle();
            }
            Button button = CreateBackButtonWithDelegateID(this, MenuButtonId.BackFromOptions);
            button.SetName("backb");
            button.x = Canvas.xOffsetScaled;
            _ = menuView.AddChild(button);
            AttachSnowfallOverlay(menuView);
            AddViewwithID(menuView, 1);
        }

        /// <summary>
        /// Builds the reset-confirmation view.
        /// </summary>
        public void CreateReset()
        {
            MenuView menuView = new();
            BaseElement baseElement = CreateBackgroundWithLogo(false);
            Text text = new Text().InitWithFont(Application.GetFont(Resources.Fnt.BigFont));
            text.SetAlignment(2);
            text.SetStringandWidth(Application.GetString("RESET_TEXT"), Global.ScreenSizeManager.CurrentSize.Width * 0.95f);
            text.anchor = text.parentAnchor = 18;
            _ = baseElement.AddChild(text);
            text.y = -200f;
            Button button = CreateButtonWithTextIDDelegate(Application.GetString("YES"), MenuButtonId.ConfirmResetYes, this);
            button.anchor = button.parentAnchor = 34;
            button.y = -540f;
            Button button2 = CreateButtonWithTextIDDelegate(Application.GetString("NO"), MenuButtonId.ConfirmResetNo, this);
            button2.anchor = button2.parentAnchor = 34;
            button2.y = -320f;
            _ = baseElement.AddChild(button);
            _ = baseElement.AddChild(button2);
            _ = menuView.AddChild(baseElement);
            Button button3 = CreateBackButtonWithDelegateID(this, MenuButtonId.BackToOptions);
            button3.SetName("backb");
            button3.x = Canvas.xOffsetScaled;
            _ = menuView.AddChild(button3);
            AttachSnowfallOverlay(menuView);
            AddViewwithID(menuView, 4);
        }

        /// <summary>
        /// Builds the language selection view from the available UI language list.
        /// </summary>
        public void CreateLanguageSelection()
        {
            MenuView menuView = new();
            BaseElement baseElement = CreateBackgroundWithLogo(false);

            IReadOnlyList<string> langCodes = LanguageHelper.UiLanguageCodes;
            string currentLocale = LanguageHelper.CurrentCode;
            int columns = 3;

            // Build rows using VBox of HBoxes (same pattern as options menu)
            VBox vBox = new VBox().InitWithOffsetAlignWidth(5f, 2, SCREEN_WIDTH);
            vBox.anchor = vBox.parentAnchor = 18;

            for (int i = 0; i < langCodes.Count; i += columns)
            {
                string firstName = LanguageHelper.GetLanguageDisplayName(langCodes[i]);
                bool firstSelected = langCodes[i] == currentLocale;
                Button firstButton = CreateShortButtonWithTextIDDelegate(firstName, MenuButtonId.ForLanguage(i), this, firstSelected);
                HBox hBox = new HBox().InitWithOffsetAlignHeight(-10f, 16, firstButton.height);
                _ = hBox.AddChild(firstButton);

                for (int j = 1; j < columns && i + j < langCodes.Count; j++)
                {
                    string name = LanguageHelper.GetLanguageDisplayName(langCodes[i + j]);
                    bool selected = langCodes[i + j] == currentLocale;
                    Button button = CreateShortButtonWithTextIDDelegate(name, MenuButtonId.ForLanguage(i + j), this, selected);
                    _ = hBox.AddChild(button);
                }

                _ = vBox.AddChild(hBox);
            }

            _ = baseElement.AddChild(vBox);
            _ = menuView.AddChild(baseElement);
            Button backButton = CreateBackButtonWithDelegateID(this, MenuButtonId.BackFromLanguage);
            backButton.SetName("backb");
            backButton.x = Canvas.xOffsetScaled;
            _ = menuView.AddChild(backButton);
            AttachSnowfallOverlay(menuView);
            AddViewwithID(menuView, VIEW_LANGUAGE_SELECT);
        }

        /// <summary>
        /// Builds the movie playback view.
        /// </summary>
        public void CreateMovieView()
        {
            MovieView movieView = new();
            RectangleElement rectangleElement = new()
            {
                width = (int)SCREEN_WIDTH,
                height = (int)SCREEN_HEIGHT,
                color = RGBAColor.blackRGBA
            };
            _ = movieView.AddChild(rectangleElement);
            AttachSnowfallOverlay(movieView);
            AddViewwithID(movieView, 7);
        }

        /// <summary>
        /// Builds the about and credits view.
        /// </summary>
        public void CreateAbout()
        {
            BaseElement background = CreateBackgroundWithLogo(false);
            aboutView = new AboutView();
            MenuView menuView = aboutView.CreateAbout(background, this);
            AttachSnowfallOverlay(menuView);
            AddViewwithID(menuView, 3);
        }

        /// <summary>
        /// Builds the candy, rope, Om Nom, and trace skin selection view.
        /// </summary>
        public void CreateCandySelection()
        {
            MenuView menuView = CandySelectionView.CreateCandySelection(this, out candyContainer);
            AttachSnowfallOverlay(menuView);
            AddViewwithID(menuView, VIEW_CANDY_SELECT);
        }

        /// <summary>
        /// Creates a horizontal star-count label.
        /// </summary>
        /// <param name="t">Text shown before the star icon.</param>
        /// <returns>The configured label row.</returns>
        public static HBox CreateTextWithStar(string t)
        {
            HBox hbox = new HBox().InitWithOffsetAlignHeight(0, 16, RTD(50));
            Text text = new Text().InitWithFont(Application.GetFont(Resources.Fnt.BigFont));
            text.SetString(t);
            text.scaleX = text.scaleY = 0.7f;
            text.rotationCenterX = -text.width / 2;
            text.width = (int)(text.width * 0.7f);
            _ = hbox.AddChild(text);
            Image c = Image.Image_createWithResIDQuad(Resources.Img.MenuPackUI, 3);
            _ = hbox.AddChild(c);
            return hbox;
        }

        /// <summary>
        /// Gets the rendered width of a pack box in the pack picker.
        /// </summary>
        /// <returns>The box width including its quad offset padding.</returns>
        public static float GetBoxWidth()
        {
            PackDefinition firstPack = PackConfig.Packs[0];
            return Image.GetQuadSize(firstPack.PackSpritesheet, firstPack.PackQuadIndex).X + (Image.GetQuadOffset(firstPack.PackSpritesheet, firstPack.PackQuadIndex).X * 2f);
        }

        /// <summary>
        /// Gets the horizontal pack offset used when the screen can only show two boxes comfortably.
        /// </summary>
        /// <returns>The pack offset in screen units.</returns>
        public static float GetPackOffset()
        {
            float availableScreenWidth = SCREEN_WIDTH + (Canvas.xOffset * 2);
            float boxWidth = GetBoxWidth();
            return boxWidth * 3f > availableScreenWidth - 200f ? boxWidth / 2f : 0f;
        }

        /// <summary>
        /// Creates one pack tile for the pack selection scroll container.
        /// </summary>
        /// <param name="n">Displayed pack index.</param>
        /// <param name="c">Scrollable container that owns the pack tile.</param>
        /// <returns>The configured pack tile element.</returns>
        public BaseElement CreatePackElementforContainer(int n, ScrollableContainer c)
        {
            TouchBaseElement touchBaseElement = new()
            {
                delegateValue = this
            };
            BaseElement baseElement = new();
            baseElement.SetName("boxContainer");
            baseElement.anchor = baseElement.parentAnchor = 12;
            _ = touchBaseElement.AddChild(baseElement);
            int totalStars = CTRPreferences.GetTotalStarsInBox(CTRPreferences.GetBoxForPack(n));
            if (n > 0 && n < CTRPreferences.GetPacksCount() && CTRPreferences.GetUnlockedForPackLevel(n, 0) == UNLOCKEDSTATE.LOCKED && totalStars >= CTRPreferences.PackUnlockStars(n))
            {
                CTRPreferences.SetUnlockedForPackLevel(UNLOCKEDSTATE.JUSTUNLOCKED, n, 0);
            }
            // Resolve pack config index: for display index == packsCount, use the coming soon entry
            int packConfigIndex = n < CTRPreferences.GetPacksCount() ? n : PackConfig.GetComingSoonPackIndex();
            bool isComingSoon = n >= CTRPreferences.GetPacksCount();
            PackDefinition packDef = PackConfig.Packs[packConfigIndex];
            string resourceName = packDef.PackSpritesheet;
            int q = packDef.PackQuadIndex;

            // Fallback to first pack's box sprite if quad index is out of range
            int quadCount = Application.GetTexture(resourceName).quadRects?.Length ?? 0;
            if (q < 0 || q >= quadCount)
            {
                resourceName = PackConfig.Packs[0].PackSpritesheet;
                q = PackConfig.Packs[0].PackQuadIndex;
            }
            string boxPackStrings;
            if (isComingSoon)
            {
                boxPackStrings = Application.GetString("BOX_SOON_LABEL");
            }
            else
            {
                string boxPackNameString = Application.GetString(PackConfig.GetPackName(n));
                boxPackStrings = $"{n + 1}. {boxPackNameString}";
            }
            string packTitle = boxPackStrings;
            UNLOCKEDSTATE unlockedForPackLevel = CTRPreferences.GetUnlockedForPackLevel(n, 0);
            bool flag = unlockedForPackLevel == UNLOCKEDSTATE.LOCKED && !isComingSoon;
            touchBaseElement.bid = !isComingSoon ? MenuButtonId.ForPack(n) : new MenuButtonId(-1);
            Image image = Image.Image_createWithResIDQuad(resourceName, q);
            image.DoRestoreCutTransparency();
            image.anchor = image.parentAnchor = 9;
            if (flag)
            {
                _ = baseElement.AddChild(image);
                int requiredStars = CTRPreferences.PackUnlockStars(n);
                Image image2 = Image.Image_createWithResIDQuad(Resources.Img.MenuPackUI, 2);
                image2.DoRestoreCutTransparency();
                image2.anchor = image2.parentAnchor = 9;
                _ = image.AddChild(image2);
                HBox hBox = CreateTextWithStar(requiredStars.ToString(CultureInfo.InvariantCulture));
                hBox.anchor = hBox.parentAnchor = 18;
                hBox.y = 110f;
                _ = image2.AddChild(hBox);
                Text text = new Text().InitWithFont(Application.GetFont(Resources.Fnt.SmallFont));
                string newString = Application.GetString("UNLOCK_HINT").ToString().Replace("%d", requiredStars.ToString(CultureInfo.InvariantCulture));
                text.SetAlignment(2);
                text.anchor = 10;
                text.parentAnchor = 34;
                text.SetStringandWidth(newString, 600f);
                text.y = -60f;
                text.scaleX = text.scaleY = 0.7f;
                text.rotationCenterY = -text.height / 2;
                _ = touchBaseElement.AddChild(text);
            }
            else
            {
                if (!isComingSoon)
                {
                    // drawing om nom and the background behind him in the box
                    int q3 = 1;
                    MonsterSlot monsterSlot = MonsterSlot.Create(PackConfig.GetBoxHoleBgColor(n));
                    monsterSlot.c = c;
                    monsterSlot.anchor = 9;
                    monsterSlot.parentAnchor = 9;
                    monsterSlot.y = image.y;
                    _ = baseElement.AddChild(monsterSlot);
                    Image image3 = Image.Image_createWithResIDQuad(Resources.Img.MenuPackUI, q3);
                    image3.DoRestoreCutTransparency();
                    image3.anchor = 17;
                    monsterSlot.s = (image.width * (n - 1)) + (-20f * n) + packContainer.x + 50f;
                    monsterSlot.e = monsterSlot.s + 1200f;
                    image3.x = packContainer.x - 0f + monsterSlot.width + -20f - GetPackOffset();
                    image3.y = packContainer.y + (SCREEN_HEIGHT / 2f);
                    image3.parentAnchor = -1;
                    _ = monsterSlot.AddChild(image3);
                }
                _ = baseElement.AddChild(image);
                if (CTRPreferences.IsPackPerfect(n) & !isComingSoon)
                {
                    // Create perfect pack badge
                    Image packPerfect = Image.Image_createWithResIDQuad(Resources.Img.MenuPackUI, 8);
                    packPerfect.parentAnchor = packPerfect.anchor = 33; // bottom-left
                    packPerfect.x = 100f;
                    packPerfect.y = -100f;
                    _ = image.AddChild(packPerfect);
                }
                if (unlockedForPackLevel == UNLOCKEDSTATE.JUSTUNLOCKED && !isComingSoon)
                {
                    Image image4 = Image.Image_createWithResIDQuad(Resources.Img.MenuPackUI, 2);
                    image4.SetName("lockHideMe");
                    image4.DoRestoreCutTransparency();
                    image4.anchor = image4.parentAnchor = 9;
                    _ = baseElement.AddChild(image4);
                    Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(3);
                    timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                    timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1.5f));
                    timeline.AddKeyFrame(KeyFrame.MakeScale(1, 1, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                    timeline.AddKeyFrame(KeyFrame.MakeScale(2, 2, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1.5f));
                    _ = image4.AddTimeline(timeline);
                }
            }
            // Add box label if defined in pack config
            string boxLabelTextKey = PackConfig.GetBoxLabelText(n);
            if (!string.IsNullOrEmpty(boxLabelTextKey))
            {
                Image boxLabel = Image.Image_createWithResIDQuad(Resources.Img.MenuPackUI, 9);
                boxLabel.parentAnchor = boxLabel.anchor = 36; // bottom-right
                boxLabel.x = -90f;
                boxLabel.y = -90f;
                _ = image.AddChild(boxLabel);

                // Add rotated text centered on the label (like button text)
                Text labelText = new Text().InitWithFont(Application.GetFont(Resources.Fnt.BigFont));
                labelText.SetString(Application.GetString(boxLabelTextKey));
                labelText.scaleX = labelText.scaleY = 0.35f;
                labelText.SetAlignment(1); // Center alignment
                labelText.anchor = labelText.parentAnchor = 18; // center-center like buttons
                labelText.rotation = -16f;
                _ = boxLabel.AddChild(labelText);
            }
            Text text2 = new Text().InitWithFont(Application.GetFont(Resources.Fnt.BigFont));
            text2.anchor = text2.parentAnchor = 10;
            text2.scaleX = text2.scaleY = 0.75f;
            if (LanguageHelper.IsCurrentAny(Language.LANGDE, Language.LANGEN))
            {
                text2.scaleX = 0.7f;
            }
            text2.SetAlignment(2);
            if (!isComingSoon)
            {
                text2.SetString(packTitle);
            }
            else
            {
                text2.SetStringandWidth(packTitle, 656);
            }
            text2.y = 140f;
            _ = image.AddChild(text2);
            Timeline timeline2 = new Timeline().InitWithMaxKeyFramesOnTrack(4);
            timeline2.AddKeyFrame(KeyFrame.MakeScale(1, 1, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline2.AddKeyFrame(KeyFrame.MakeScale(0.95f, 1.05f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.15f));
            timeline2.AddKeyFrame(KeyFrame.MakeScale(1.05f, 0.95f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.2f));
            timeline2.AddKeyFrame(KeyFrame.MakeScale(1, 1, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.25f));
            _ = baseElement.AddTimeline(timeline2);
            baseElement.height = touchBaseElement.height = image.height;
            baseElement.width = touchBaseElement.width = image.width;
            return touchBaseElement;
        }

        /// <summary>
        /// Builds the pack selection view.
        /// </summary>
        public void CreatePackSelect()
        {
            MenuView menuView = new();
            BaseElement baseElement = CreateBackgroundWithLogo(false);
            string text = Application.GetString("TOTAL_STARS").ToString();
            text = text.Replace("%d", "");
            HBox hBox = CreateTextWithStar(text + CTRPreferences.GetTotalStars().ToString(CultureInfo.InvariantCulture));
            hBox.x = -30f - Canvas.xOffsetScaled;
            hBox.y = 40f;
            hBox.SetName("text");
            HBox hBox2 = new HBox().InitWithOffsetAlignHeight(-20f, 16, SCREEN_HEIGHT);
            float availableScreenWidth = SCREEN_WIDTH + (Canvas.xOffset * 2);
            float boxWidth = GetBoxWidth();
            float containerWidth = boxWidth * 3f;
            if (containerWidth > availableScreenWidth - 200f)
            {
                containerWidth = boxWidth * 2f;
            }
            packContainer = new ScrollableContainer().InitWithWidthHeightContainer(containerWidth, SCREEN_HEIGHT, hBox2);
            packContainer.minAutoScrollToSpointLength = RTD(5);
            packContainer.shouldBounceHorizontally = true;
            packContainer.resetScrollOnShow = false;
            packContainer.dontHandleTouchDownsHandledByChilds = true;
            packContainer.dontHandleTouchMovesHandledByChilds = true;
            packContainer.dontHandleTouchUpsHandledByChilds = true;
            packContainer.TurnScrollPointsOnWithCapacity(CTRPreferences.GetPacksCount() + 2);
            packContainer.delegateScrollableContainerProtocol = this;
            packContainer.x = (SCREEN_WIDTH / 2f) - (packContainer.width / 2);
            hBox.anchor = hBox.parentAnchor = 12;
            _ = baseElement.AddChild(hBox);
            CTRTexture2D texture = Application.GetTexture(Resources.Img.MenuPackUI);
            BaseElement baseElement2 = new()
            {
                width = (int)texture.preCutSize.X,
                height = (int)texture.preCutSize.Y
            };
            _ = hBox2.AddChild(baseElement2);
            float scrollPointX = 0f + GetPackOffset();
            int displayCount = CTRPreferences.GetPacksCount() + (PackConfig.GetComingSoonPackIndex() >= 0 ? 1 : 0);
            for (int i = 0; i < displayCount; i++)
            {
                TouchBaseElement touchBaseElement = (TouchBaseElement)CreatePackElementforContainer(i, packContainer);
                boxes[i] = touchBaseElement;
                _ = hBox2.AddChild(touchBaseElement);
                touchBaseElement.x -= 0f;
                touchBaseElement.y -= 0f;
                _ = packContainer.AddScrollPointAtXY(scrollPointX, 0f);
                touchBaseElement.bbc = MakeRectangle(0f, 0f, -20f, 0f);
                scrollPointX += touchBaseElement.width + -20f;
            }
            hBox2.width += 1000;
            Image image = Image.Image_createWithResIDQuad(Resources.Img.MenuPackUI, 4);
            image.anchor = 17;
            image.y += SCREEN_HEIGHT / 2f;
            image.x = packContainer.x - 2f;
            _ = baseElement.AddChild(image);
            Image image2 = Image.Image_createWithResIDQuad(Resources.Img.MenuPackUI, 4);
            image2.anchor = 20;
            image2.y += SCREEN_HEIGHT / 2f;
            image2.x = packContainer.x + packContainer.width + 2f;
            _ = baseElement.AddChild(image2);
            image2.scaleX = image2.scaleY = -1f;
            _ = baseElement.AddChild(packContainer);
            Image image3 = Image.Image_createWithResIDQuad(Resources.Img.MenuPackUI, 5);
            image3.anchor = 20;
            image3.y += SCREEN_HEIGHT / 2f;
            image3.x = packContainer.x + 3f;
            _ = baseElement.AddChild(image3);
            Image image4 = Image.Image_createWithResIDQuad(Resources.Img.MenuPackUI, 5);
            image4.anchor = 17;
            image4.y += SCREEN_HEIGHT / 2f;
            image4.x = packContainer.x + packContainer.width - 3f;
            image4.scaleX = image4.scaleY = -1f;
            _ = baseElement.AddChild(image4);
            prevb = CreateButton2WithImageQuad1Quad2IDDelegate(Resources.Img.MenuPackUI, 6, 7, MenuButtonId.PreviousPack, this);
            prevb.parentAnchor = 17;
            prevb.anchor = 20;
            prevb.x = packContainer.x - 40f;
            _ = baseElement.AddChild(prevb);
            nextb = CreateButton2WithImageQuad1Quad2IDDelegate(Resources.Img.MenuPackUI, 6, 7, MenuButtonId.NextPack, this);
            nextb.anchor = nextb.parentAnchor = 17;
            nextb.x = packContainer.x + packContainer.width + 40f;
            nextb.scaleX = -1f;
            _ = baseElement.AddChild(nextb);
            _ = menuView.AddChild(baseElement);
            Button button = CreateBackButtonWithDelegateID(this, MenuButtonId.BackFromPackSelect);
            button.SetName("backb");
            button.x = Canvas.xOffsetScaled;
            _ = menuView.AddChild(button);
            AttachSnowfallOverlay(menuView);
            AddViewwithID(menuView, 5);
            int lastPack = CTRPreferences.GetLastBox();
            ((CTRRootController)Application.SharedRootController()).SetBox(CTRPreferences.GetLastGamePack());
            packContainer.PlaceToScrollPoint(lastPack);
            ScrollableContainerchangedTargetScrollPoint(packContainer, lastPack);
        }

        /// <summary>
        /// Creates the leaderboards view placeholder.
        /// </summary>
        public static void CreateLeaderboards()
        {
        }

        /// <summary>
        /// Creates the achievements view placeholder.
        /// </summary>
        public static void CreateAchievements()
        {
        }

        /// <summary>
        /// Shows the popup used when the selected pack cannot be unlocked.
        /// </summary>
        public void ShowCantUnlockPopup()
        {
            popUpMenu.ShowCantUnlockPopup();
        }

        /// <summary>
        /// Shows the popup displayed after the game is completed.
        /// </summary>
        public void ShowGameFinishedPopup()
        {
            popUpMenu.ShowGameFinishedPopup();
        }

        /// <summary>
        /// Shows a generic yes/no popup.
        /// </summary>
        /// <param name="str">Popup text.</param>
        /// <param name="buttonYesId">Button identifier for the yes action.</param>
        /// <param name="buttonNoId">Button identifier for the no action.</param>
        public void ShowYesNoPopup(string str, MenuButtonId buttonYesId, MenuButtonId buttonNoId)
        {
            ep = popUpMenu.ShowYesNoPopup(str, buttonYesId, buttonNoId);
        }

        /// <summary>
        /// Handles a pack scroll container reaching a target scroll point.
        /// </summary>
        /// <param name="e">Scrollable container that reached the point.</param>
        /// <param name="i">Reached scroll point index.</param>
        public void ScrollableContainerreachedScrollPoint(ScrollableContainer e, int i)
        {
            currentPack = i;
            pack = i;
            scrollPacksLeft = 0;
            scrollPacksRight = 0;
            bScrolling = false;
            if (prevb.IsEnabled())
            {
                prevb.SetState(Button.BUTTON_STATE.BUTTON_UP);
            }
            if (nextb.IsEnabled())
            {
                nextb.SetState(Button.BUTTON_STATE.BUTTON_UP);
            }
            if (i == CTRPreferences.GetPacksCount())
            {
                return;
            }
            boxes[i].GetChildWithName("boxContainer").PlayTimeline(0);
            UNLOCKEDSTATE unlockedForPackLevel = CTRPreferences.GetUnlockedForPackLevel(i, 0);
            BaseElement childWithName = boxes[i].GetChildWithName("lockHideMe");
            if (childWithName != null && unlockedForPackLevel == UNLOCKEDSTATE.JUSTUNLOCKED)
            {
                CTRPreferences.SetUnlockedForPackLevel(UNLOCKEDSTATE.UNLOCKED, i, 0);
                childWithName.PlayTimeline(0);
            }
            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
            if (showNextPackStatus && i == cTRRootController.GetPack() + 1)
            {
                showNextPackStatus = false;
                if (unlockedForPackLevel == UNLOCKEDSTATE.LOCKED)
                {
                    ShowCantUnlockPopup();
                }
            }
        }

        /// <summary>
        /// Handles the pack scroll container changing its target scroll point.
        /// </summary>
        /// <param name="e">Scrollable container whose target changed.</param>
        /// <param name="i">Target scroll point index.</param>
        public void ScrollableContainerchangedTargetScrollPoint(ScrollableContainer e, int i)
        {
            currentPack = i;
            pack = i;
            CTRPreferences.SetLastBox(i);
            CTRPreferences.SetLastGamePack(CTRPreferences.GetBoxForPack(i));
        }

        /// <summary>
        /// Creates one level selection button for a pack.
        /// </summary>
        /// <param name="l">Level index within the pack.</param>
        /// <param name="p">Pack index.</param>
        /// <returns>The configured level button element.</returns>
        public BaseElement CreateButtonForLevelPack(int l, int p)
        {
            bool flag = CTRPreferences.GetUnlockedForPackLevel(p, l) == UNLOCKEDSTATE.LOCKED;
            int starsForPackLevel = CTRPreferences.GetStarsForPackLevel(p, l);
            TouchBaseElement touchBaseElement = new()
            {
                bbc = MakeRectangle(5f, 0f, -10f, 0f),
                delegateValue = this
            };
            Image image;
            if (flag)
            {
                touchBaseElement.bid = new MenuButtonId(-1);
                image = Image.Image_createWithResIDQuad(Resources.Img.MenuLevelUi, 1);
                image.DoRestoreCutTransparency();
            }
            else
            {
                touchBaseElement.bid = MenuButtonId.ForLevel(l);
                image = Image.Image_createWithResIDQuad(Resources.Img.MenuLevelUi, 0);
                image.DoRestoreCutTransparency();
                Text text = new Text().InitWithFont(Application.GetFont(Resources.Fnt.BigFont));
                string @string = (l + 1).ToString(CultureInfo.InvariantCulture);
                text.SetString(@string);
                text.anchor = text.parentAnchor = 18;
                text.y -= 5f;
                _ = image.AddChild(text);
                Image image2 = Image.Image_createWithResIDQuad(Resources.Img.MenuLevelUi, 2 + starsForPackLevel);
                image2.DoRestoreCutTransparency();
                image2.anchor = image2.parentAnchor = 9;
                _ = image.AddChild(image2);
            }
            image.anchor = image.parentAnchor = 18;
            _ = touchBaseElement.AddChild(image);
            touchBaseElement.SetSizeToChildsBounds();
            return touchBaseElement;
        }

        /// <summary>
        /// Builds the level selection view for the current pack.
        /// </summary>
        public void CreateLevelSelect()
        {
            float transitionDuration = 0.3f;
            MenuView menuView = new();
            string boxCover = PackConfig.GetBoxCoverOrDefault(pack);
            Image image = Image.Image_createWithResIDQuad(boxCover, 0);
            Image image2 = Image.Image_createWithResIDQuad(boxCover, 0);
            Vector quadSize = Image.GetQuadSize(boxCover, 0);
            float x = (SCREEN_WIDTH / 2f) - quadSize.X;
            image.x = x;
            image2.x = SCREEN_WIDTH / 2f;
            image2.rotation = 180f;
            image2.y -= 0.5f;
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(3);
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.MakeRGBA(0.85f, 0.85f, 0.85f, 1), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, transitionDuration));
            _ = image.AddTimeline(timeline);
            image.SetName("levelsBack");
            _ = image.AddChild(image2);
            _ = menuView.AddChild(image);
            Image image3 = Image.Image_createWithResIDQuad(Resources.Img.MenuLevelUi, 6);
            Image image4 = Image.Image_createWithResIDQuad(Resources.Img.MenuLevelUi, 7);
            image3.x = Image.GetQuadOffset(Resources.Img.MenuLevelUi, 6).X;
            image3.y = 80f;
            image4.x = Image.GetQuadOffset(Resources.Img.MenuLevelUi, 7).X;
            image4.y = 80f;
            _ = menuView.AddChild(image3);
            _ = menuView.AddChild(image4);
            Image image5 = Image.Image_createWithResIDQuad(Resources.Img.MenuBgrShadow, 0);
            image5.SetName("shadow");
            image5.anchor = image5.parentAnchor = 18;
            image5.scaleX = image5.scaleY = 2f;
            Timeline timeline2 = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline2.AddKeyFrame(KeyFrame.MakeScale(2, 2, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline2.AddKeyFrame(KeyFrame.MakeScale(5, 5, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, transitionDuration));
            timeline2.delegateTimelineDelegate = this;
            _ = image5.AddTimeline(timeline2);
            Timeline timeline3 = new Timeline().InitWithMaxKeyFramesOnTrack(3);
            timeline3.AddKeyFrame(KeyFrame.MakeRotation(45, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline3.AddKeyFrame(KeyFrame.MakeRotation(405, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 75));
            timeline3.SetTimelineLoopType(Timeline.LoopType.TIMELINE_REPLAY);
            _ = image5.AddTimeline(timeline3);
            image5.PlayTimeline(1);
            _ = menuView.AddChild(image5);
            HBox hBox = CreateTextWithStar(CTRPreferences.GetTotalStarsInPack(pack).ToString(CultureInfo.InvariantCulture) + "/" + (CTRPreferences.GetLevelsInPackCount(pack) * 3).ToString(CultureInfo.InvariantCulture));

            hBox.x = -30f;
            hBox.y = 40f;
            int levelsInPack = CTRPreferences.GetLevelsInPackCount(pack);
            int columnsPerRow;
            float horizontalSpacing;
            float buttonScale;

            switch (levelsInPack)
            {
                case <= 9:
                    columnsPerRow = 3;
                    horizontalSpacing = 100f;
                    buttonScale = 1.25f;
                    break;
                case <= 12:
                    columnsPerRow = 4;
                    horizontalSpacing = 60f;
                    buttonScale = 1.25f;
                    break;
                default:
                    columnsPerRow = 5;
                    horizontalSpacing = 10f;
                    buttonScale = 1f;
                    break;
            }

            float verticalSpacing = 55f;
            float rowHeight = 203f * buttonScale;
            VBox vBox = new VBox().InitWithOffsetAlignWidth(verticalSpacing, 2, SCREEN_WIDTH);
            vBox.SetName("levelsBox");
            vBox.x = 0f;
            int levelIndex = 0;
            for (int i = 0; i < levelsInPack; i += columnsPerRow)
            {
                HBox hBox2 = new HBox().InitWithOffsetAlignHeight(horizontalSpacing, 16, rowHeight);
                for (int j = 0; j < columnsPerRow && levelIndex < levelsInPack; j++)
                {
                    BaseElement levelButton = CreateButtonForLevelPack(levelIndex++, pack);
                    if (buttonScale != 1f)
                    {
                        levelButton.scaleX = buttonScale;
                        levelButton.scaleY = buttonScale;
                        levelButton.width = (int)(levelButton.width * buttonScale);
                        levelButton.height = (int)(levelButton.height * buttonScale);
                    }
                    _ = hBox2.AddChild(levelButton);
                }
                _ = vBox.AddChild(hBox2);
            }
            float levelsTopY = 110f;
            float availableHeight = SCREEN_HEIGHT - levelsTopY;
            BaseElement levelsElement;
            if (levelsInPack > 25)
            {
                vBox.y = 0f;
                vBox.height += (int)levelsTopY - 15;
                levelContainer = new ScrollableContainer().InitWithWidthHeightContainer(SCREEN_WIDTH, availableHeight, vBox);
                levelContainer.shouldBounceVertically = true;
                levelContainer.y = levelsTopY;
                levelsElement = levelContainer;
            }
            else
            {
                levelContainer = null;
                vBox.y = (SCREEN_HEIGHT - vBox.height) / 2f;
                levelsElement = vBox;
            }
            Timeline timeline4 = new Timeline().InitWithMaxKeyFramesOnTrack(3);
            timeline4.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline4.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, transitionDuration));
            _ = levelsElement.AddTimeline(timeline4);
            hBox.anchor = hBox.parentAnchor = 12;
            hBox.SetName("starText");
            hBox.x = -30f - Canvas.xOffsetScaled;
            Timeline timeline5 = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline5.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline5.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, transitionDuration));
            _ = hBox.AddTimeline(timeline5);
            _ = menuView.AddChild(hBox);
            _ = menuView.AddChild(levelsElement);
            Button button = CreateBackButtonWithDelegateID(this, MenuButtonId.PackSelect);
            button.SetName("backButton");
            Timeline timeline6 = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline6.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline6.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, transitionDuration));
            _ = button.AddTimeline(timeline6);
            button.x = Canvas.xOffsetScaled;
            _ = menuView.AddChild(button);
            AttachSnowfallOverlay(menuView);
            AddViewwithID(menuView, 6);
        }

        /// <summary>
        /// Initializes the menu controller and creates its child views.
        /// </summary>
        /// <param name="parent">Parent view controller.</param>
        public MenuController(ViewController parent)
            : base(parent)
        {
            ddMainMenu = new DelayedDispatcher();
            ddPackSelect = new DelayedDispatcher();
            popUpMenu = new PopUpMenu(this);
            CreateMainMenu();
            CreateOptions();
            CreateReset();
            CreateLanguageSelection();
            CreateAbout();
            CreateCandySelection();
            CreateMovieView();
            CreatePackSelect();
            CreateLeaderboards();
            CreateAchievements();
            MapPickerController c = new(this);
            AddChildwithID(c, 0);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (ddMainMenu != null)
                {
                    ddMainMenu.CancelAllDispatches();
                    ddMainMenu.Dispose();
                    ddMainMenu = null;
                }
                if (ddPackSelect != null)
                {
                    ddPackSelect.CancelAllDispatches();
                    ddPackSelect.Dispose();
                    ddPackSelect = null;
                }
            }
            base.Dispose(disposing);
        }

        /// <inheritdoc />
        public override void Activate()
        {
            showNextPackStatus = false;
            base.Activate();
            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
            pack = cTRRootController.GetPack();
            if (IsSinglePack && viewToShow == VIEW_PACK_SELECT)
            {
                pack = 0;
                currentPack = 0;
                PreLevelSelect();
                viewToShow = VIEW_LEVEL_SELECT;
            }
            if (viewToShow == VIEW_LEVEL_SELECT)
            {
                currentPack = pack;
                PreLevelSelect();
            }
            ShowView(viewToShow);
            CTRSoundMgr.StopMusic();
            if (SpecialEvents.IsXmas)
            {
                CTRSoundMgr.PlayMusic(Resources.Music.MenuMusicXmas);
            }
            else
            {
                CTRSoundMgr.PlayMusic(Resources.Music.MenuMusic);
            }
        }

        /// <summary>
        /// Advances from the current pack to the next pack or outro flow.
        /// </summary>
        public void ShowNextPack()
        {
            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
            int currentPackIndex = cTRRootController.GetPack();
            if (currentPackIndex < CTRPreferences.GetPacksCount() - 1)
            {
                packContainer.delegateScrollableContainerProtocol = this;
                packContainer.MoveToScrollPointmoveMultiplier(currentPackIndex + 1, 0.8f);
                showNextPackStatus = true;
                return;
            }
            replayingIntroMovie = false;
            if (PackConfig.OutroVideo != null)
            {
                packContainer.PlaceToScrollPoint(cTRRootController.GetPack() + 1);
                CTRSoundMgr.StopMusic();
                Application.SharedMovieMgr().delegateMovieMgrDelegate = this;
                Application.SharedMovieMgr().PlayURL(PackConfig.OutroVideo, !Preferences.GetBooleanForKey("MUSIC_ON") && !Preferences.GetBooleanForKey("SOUND_ON"));
            }
            else
            {
                MoviePlaybackFinished(null);
            }
        }

        /// <inheritdoc />
        public override void OnChildDeactivated(int n)
        {
            base.OnChildDeactivated(n);
            ((CTRRootController)Application.SharedRootController()).SetSurvival(false);
            Deactivate();
        }

        /// <summary>
        /// Handles completion of intro or outro movie playback.
        /// </summary>
        /// <param name="url">Completed movie URL, or <see langword="null"/> when entering the menu flow directly.</param>
        public void MoviePlaybackFinished(string url)
        {
            bool isOutro = url != null && url == PackConfig.OutroVideo;
            if (replayingIntroMovie)
            {
                replayingIntroMovie = false;
                ActivateChild(0);
                return;
            }
            if (url != null)
            {
                if (SpecialEvents.IsXmas)
                {
                    CTRSoundMgr.PlayMusic(Resources.Music.MenuMusicXmas);
                }
                else
                {
                    CTRSoundMgr.PlayMusic(Resources.Music.MenuMusic);
                }
            }
            if (IsSinglePack)
            {
                pack = 0;
                currentPack = 0;
                CTRPreferences.SetLastBox(0);
                CTRPreferences.SetLastGamePack(CTRPreferences.GetBoxForPack(0));
                PreLevelSelect();
                ShowView(VIEW_LEVEL_SELECT);
                if (isOutro)
                {
                    ShowGameFinishedPopup();
                }
                return;
            }
            if (!isOutro && CTRPreferences.ShouldPlayLevelScroll())
            {
                packContainer.PlaceToScrollPoint(CTRPreferences.GetPacksCount() - 1);
                packContainer.MoveToScrollPointmoveMultiplier(0, 0.6f);
                CTRPreferences.DisablePlayLevelScroll();
            }
            else
            {
                packContainer.PlaceToScrollPoint(CTRPreferences.GetLastBox());
            }
            ShowView(5);
            if (isOutro)
            {
                packContainer.PlaceToScrollPoint(CTRPreferences.GetPacksCount() - 1);
                ShowGameFinishedPopup();
            }
        }

        /// <summary>
        /// Loads pack cover resources and recreates the level selection view before it is shown.
        /// </summary>
        public void PreLevelSelect()
        {
            CTRResourceMgr cTRResourceMgr = Application.SharedResourceMgr();
            string[] array = PackConfig.GetBoxCovers(pack);
            cTRResourceMgr.InitLoading();
            cTRResourceMgr.LoadPack(array);
            cTRResourceMgr.LoadImmediately();
            if (GetView(6) != null)
            {
                DeleteView(6);
            }
            CreateLevelSelect();
        }

        /// <inheritdoc />
        public void TimelineFinished(Timeline t)
        {
            CTRSoundMgr.StopMusic();
            CTRRootController ctrrootController = (CTRRootController)Application.SharedRootController();
            ctrrootController.SetBox(CTRPreferences.GetBoxForPack(pack));
            ctrrootController.SetPack(pack);
            ctrrootController.SetLevel(level);
            Application.SharedRootController().SetViewTransition(-1);
            ((MapPickerController)GetChild(0)).SetAutoLoadMap(LevelsList.LEVEL_NAMES[pack, level]);
            if (pack == 0 && level == 0 && CTRPreferences.GetScoreForPackLevel(0, 0) != 0 && PackConfig.IntroVideo != null)
            {
                replayingIntroMovie = true;
                ShowView(7);
                CTRSoundMgr.StopMusic();
                Application.SharedMovieMgr().delegateMovieMgrDelegate = this;
                Application.SharedMovieMgr().PlayURL(PackConfig.IntroVideo, !Preferences.GetBooleanForKey("MUSIC_ON") && !Preferences.GetBooleanForKey("SOUND_ON"));
                return;
            }
            ActivateChild(0);
        }

        /// <summary>
        /// Recreates the options view after localized resources or settings change.
        /// </summary>
        public void RecreateOptions()
        {
            DeleteView(1);
            CreateOptions();
        }

        /// <summary>
        /// Handles a typed menu button press.
        /// </summary>
        /// <param name="n">Menu button identifier that was pressed.</param>
        public void OnButtonPressed(MenuButtonId n)
        {
            if (n.Value != -1)
            {
                CTRSoundMgr.PlaySound(Resources.Snd.Tap);
            }

            if (n.IsLevelButton())
            {
                level = n.GetLevelIndex();
                ActiveView().GetChildWithName("levelsBox").PlayTimeline(0);
                ActiveView().GetChildWithName("shadow").PlayTimeline(0);
                ActiveView().GetChildWithName("levelsBack").PlayTimeline(0);
                ActiveView().GetChildWithName("starText").PlayTimeline(0);
                ActiveView().GetChildWithName("backButton").PlayTimeline(0);
                return;
            }

            if (n.IsLanguageSelectButton())
            {
                int langIndex = n.GetLanguageSelectIndex();
                IReadOnlyList<string> langCodes = LanguageHelper.UiLanguageCodes;
                if (langIndex >= 0 && langIndex < langCodes.Count)
                {
                    string newLocale = langCodes[langIndex];
                    Application.SharedAppSettings().SetString((int)ApplicationSettings.AppSettings.APP_SETTING_LOCALE, newLocale);
                    Preferences.SetStringForKey(newLocale, "PREFS_LOCALE", true);
                    CTRResourceMgr ctrresourceMgr2 = Application.SharedResourceMgr();
                    ctrresourceMgr2.FreePack(PackLocalizationMenu);
                    ctrresourceMgr2.ClearCachedFonts();
                    ctrresourceMgr2.InitLoading();
                    ctrresourceMgr2.LoadPack(PackLocalizationMenu);
                    ctrresourceMgr2.LoadImmediately();
                    DeleteView(VIEW_PACK_SELECT);
                    CreatePackSelect();
                    DeleteView(VIEW_MAIN_MENU);
                    CreateMainMenu();
                    DeleteView(VIEW_RESET);
                    CreateReset();
                    DeleteView(VIEW_LANGUAGE_SELECT);
                    CreateLanguageSelection();
                    DeleteView(VIEW_ABOUT);
                    CreateAbout();
                    DeleteView(VIEW_CANDY_SELECT);
                    CreateCandySelection();
                    CreateLeaderboards();
                    ddMainMenu.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(Selector_recreateOptions), null, 0.01f);
                    ((CTRRootController)Application.SharedRootController()).RecreateLoadingController();
                    ShowView(VIEW_OPTIONS);
                }
                return;
            }

            switch (n)
            {
                case var id when id == MenuButtonId.Play:
                    {
                        for (int i = 0; i < CTRPreferences.GetPacksCount(); i++)
                        {
                            GameController.CheckForBoxPerfect(i);
                        }
                        replayingIntroMovie = false;
                        if (CTRPreferences.GetScoreForPackLevel(0, 0) == 0 && PackConfig.IntroVideo != null)
                        {
                            ShowView(7);
                            CTRSoundMgr.StopMusic();
                            Application.SharedMovieMgr().delegateMovieMgrDelegate = this;
                            Application.SharedMovieMgr().PlayURL(PackConfig.IntroVideo, !Preferences.GetBooleanForKey("MUSIC_ON") && !Preferences.GetBooleanForKey("SOUND_ON"));
                            return;
                        }
                        MoviePlaybackFinished(null);
                        return;
                    }
                case var id when id == MenuButtonId.Options:
                    ShowView(1);
                    return;
                case var id when id == MenuButtonId.PlayPack0:
                    ((CTRRootController)Application.SharedRootController()).SetBox(CTRPreferences.GetBoxForPack(0));
                    ((CTRRootController)Application.SharedRootController()).SetPack(0);
                    PreLevelSelect();
                    Application.SharedRootController().SetViewTransition(-1);
                    ((MapPickerController)GetChild(0)).SetNormalMode();
                    ActivateChild(0);
                    return;
                case var id when id == MenuButtonId.SurvivalMode:
                    {
                        CTRSoundMgr.StopMusic();
                        pack = 0;
                        Application.SharedRootController().SetViewTransition(-1);
                        CTRRootController ctrrootController = (CTRRootController)Application.SharedRootController();
                        CTRResourceMgr ctrresourceMgr = Application.SharedResourceMgr();
                        ctrresourceMgr.InitLoading();
                        ctrresourceMgr.LoadPack(PackConfig.GetBoxCovers(pack));
                        ctrresourceMgr.LoadImmediately();
                        ctrrootController.SetSurvival(true);
                        ctrrootController.SetBox(CTRPreferences.GetBoxForPack(pack));
                        ctrrootController.SetPack(pack);
                        Deactivate();
                        return;
                    }
                case var id when id == MenuButtonId.OpenFullVersion:
                    CTRRootController.OpenFullVersionPage();
                    return;
                case var id when id == MenuButtonId.ToggleSound:
                    {
                        bool soundWasOn = Preferences.GetBooleanForKey("SOUND_ON");
                        Preferences.SetBooleanForKey(!soundWasOn, "SOUND_ON", true);
                        if (soundWasOn)
                        {
                            CTRSoundMgr.SuspendSoundEffects();
                        }
                        else
                        {
                            CTRSoundMgr.RestoreSoundEffects();
                        }
                        return;
                    }
                case var id when id == MenuButtonId.ToggleMusic:
                    {
                        bool flag6 = Preferences.GetBooleanForKey("MUSIC_ON");
                        Preferences.SetBooleanForKey(!flag6, "MUSIC_ON", true);
                        if (flag6)
                        {
                            CTRSoundMgr.StopMusic();
                            return;
                        }
                        if (SpecialEvents.IsXmas)
                        {
                            CTRSoundMgr.PlayMusic(Resources.Music.MenuMusicXmas);
                        }
                        else
                        {
                            CTRSoundMgr.PlayMusic(Resources.Music.MenuMusic);
                        }
                        return;
                    }
                case var id when id == MenuButtonId.ShowCredits:
                    aboutView?.ResetAndEnableAutoScroll();
                    ShowView(3);
                    return;
                case var id when id == MenuButtonId.ShowReset:
                    ShowView(4);
                    return;
                case var id when id == MenuButtonId.Leaderboards:
                    break;
                case var id when id == MenuButtonId.BackToOptions:
                    ShowView(1);
                    return;
                case var id when id == MenuButtonId.ToggleClickToCut:
                    {
                        bool flag7 = Preferences.GetBooleanForKey("PREFS_CLICK_TO_CUT");
                        Preferences.SetBooleanForKey(!flag7, "PREFS_CLICK_TO_CUT", true);
                        return;
                    }
                case var id when id == MenuButtonId.PackSelect:
                    if (IsSinglePack)
                    {
                        ShowView(VIEW_MAIN_MENU);
                        return;
                    }
                    ShowView(5);
                    packContainer.MoveToScrollPointmoveMultiplier(pack, 0.8f);
                    return;
                case var id when id == MenuButtonId.ConfirmResetYes:
                    {
                        CTRPreferences ctrpreferences = Application.SharedPreferences();
                        CTRPreferences.ResetToDefaults();
                        Preferences.RequestSave();
                        DeleteView(5);
                        CreatePackSelect();
                        ShowView(1);
                        return;
                    }
                case var id when id == MenuButtonId.ConfirmResetNo:
                    ShowView(1);
                    return;
                case var id when id == MenuButtonId.PopupOk:
                    ((Popup)ActiveView().GetChildWithName("popup")).HidePopup();
                    return;
                case var id when id == MenuButtonId.OpenTwitter:
                    OpenUrl("http://twitter.com/zeptolab");
                    return;
                case var id when id == MenuButtonId.OpenFacebook:
                    OpenUrl("http://www.facebook.com/cuttherope");
                    return;
                case var id when id == MenuButtonId.FanworkProjectWebsite:
                    OpenUrl(Application.GetString("ABOUT_FANWORK_PROJECT_WEBSITE").ToString());
                    return;
                case var id when id == MenuButtonId.FanworkCtrhWebsite:
                    OpenUrl(Application.GetString("ABOUT_FANWORK_CTRH_WEBSITE").ToString());
                    return;
                case var id when id == MenuButtonId.NextPack:
                    {
                        int currentPackIndex = currentPack;
                        int leftScrollCount = scrollPacksLeft + 1;
                        scrollPacksLeft = leftScrollCount;
                        int sp2 = FixScrollPoint(currentPackIndex + leftScrollCount - scrollPacksRight);
                        packContainer.MoveToScrollPointmoveMultiplier(sp2, 0.8f);
                        bScrolling = true;
                        return;
                    }
                case var id when id == MenuButtonId.PreviousPack:
                    {
                        int currentPackIndex = currentPack;
                        int rightScrollCount = scrollPacksRight + 1;
                        scrollPacksRight = rightScrollCount;
                        int sp3 = FixScrollPoint(currentPackIndex - rightScrollCount + scrollPacksLeft);
                        packContainer.MoveToScrollPointmoveMultiplier(sp3, 0.8f);
                        bScrolling = true;
                        break;
                    }
                case var id when id == MenuButtonId.ShowLanguage:
                    ShowView(VIEW_LANGUAGE_SELECT);
                    return;
                case var id when id == MenuButtonId.BackFromLanguage:
                    ShowView(VIEW_OPTIONS);
                    return;
                case var id when id == MenuButtonId.BackFromPackSelect || id == MenuButtonId.BackFromOptions || id == MenuButtonId.BackFromLeaderboards || id == MenuButtonId.BackFromAchievements:
                    {
                        string[] array4 =
                        [
                    "BS",
                    "OP",
                    "LB",
                    "AC"
                        ];
                        string[] array5 = new string[4];
                        array5[0] = "BS_BACK_PRESSED";
                        array5[1] = "OP_BACK_PRESSED";
                        string nsstring = array4[n.Value - MenuButtonId.BackFromPackSelect.Value];
                        string nsstring2 = array5[n.Value - MenuButtonId.BackFromPackSelect.Value];
                        ShowView(0);
                        return;
                    }
                case var id when id == MenuButtonId.QuitGame:
                    Global.XnaGame.Exit();
                    return;
                case var id when id == MenuButtonId.ClosePopup:
                    if (ep != null)
                    {
                        ep.HidePopup();
                        ep = null;
                        updateReleaseUrl = null;
                        return;
                    }
                    break;
                case var id when id == MenuButtonId.UpdateDownload:
                    if (!string.IsNullOrWhiteSpace(updateReleaseUrl))
                    {
                        OpenUrl(updateReleaseUrl);
                    }
                    if (ep != null)
                    {
                        ep.HidePopup();
                        ep = null;
                        updateReleaseUrl = null;
                    }
                    return;
                case var id when id == MenuButtonId.ShowQuitPopup:
                    ShowYesNoPopup(Application.GetString("QUIT"), MenuButtonId.QuitGame, MenuButtonId.ClosePopup);
                    return;
                case var id when id == MenuButtonId.BackFromCandySelect:
                    // Return to main menu from candy selection
                    // Recreate the main menu to reflect the new candy selection
                    DeleteView(0);
                    CreateMainMenu();
                    ShowView(0);
                    return;
                default:
                    MenuSkinSelectionAction skinAction = MenuSkinSelectionActionResolver.Resolve(n);
                    if (skinAction.Kind == MenuSkinSelectionActionKind.SwitchMode)
                    {
                        if (activeViewID == VIEW_CANDY_SELECT)
                        {
                            switch (skinAction.Mode)
                            {
                                case SkinSelectionMode.Candy:
                                    CandySelectionView.SwitchToCandyMode();
                                    break;
                                case SkinSelectionMode.Rope:
                                    CandySelectionView.SwitchToRopeMode();
                                    break;
                                case SkinSelectionMode.OmNom:
                                    CandySelectionView.SwitchToOmNomMode();
                                    break;
                                case SkinSelectionMode.Trace:
                                    CandySelectionView.SwitchToTraceMode();
                                    break;
                                default:
                                    throw new InvalidOperationException($"Unhandled {nameof(SkinSelectionMode)}: {skinAction.Mode}.");
                            }
                        }
                        else if (skinAction.Mode == SkinSelectionMode.Candy)
                        {
                            Preferences.SetBooleanForKey(true, "PREFS_CANDY_WAS_CHANGED", true);
                            ShowView(VIEW_CANDY_SELECT);
                        }

                        return;
                    }

                    if (skinAction.Kind == MenuSkinSelectionActionKind.SelectSlot
                        && skinAction.PreferenceKey != null
                        && skinAction.SelectedIndex.HasValue)
                    {
                        int selectedIndex = skinAction.SelectedIndex.Value;
                        Preferences.SetIntForKey(selectedIndex, skinAction.PreferenceKey, true);

                        switch (skinAction.Mode)
                        {
                            case SkinSelectionMode.Candy:
                            case SkinSelectionMode.Rope:
                            case SkinSelectionMode.Trace:
                                CandySelectionView.UpdateCandySlotButtons(selectedIndex);
                                break;
                            case SkinSelectionMode.OmNom:
                                CandySelectionView.SelectOmNomSlot(selectedIndex);
                                break;
                            default:
                                throw new InvalidOperationException($"Unhandled {nameof(SkinSelectionMode)}: {skinAction.Mode}.");
                        }

                        return;
                    }

                    // Handle pack selection buttons dynamically
                    if (n.IsPackButton())
                    {
                        int targetPack = n.GetPackIndex();
                        if (pack != targetPack)
                        {
                            packContainer.MoveToScrollPointmoveMultiplier(targetPack, 0.8f);
                            return;
                        }
                        CTRPreferences.SetLastBox(pack);
                        CTRPreferences.SetLastGamePack(CTRPreferences.GetBoxForPack(pack));
                        bool flag5 = CTRPreferences.GetUnlockedForPackLevel(targetPack, 0) == UNLOCKEDSTATE.LOCKED && targetPack != CTRPreferences.GetPacksCount();
                        if (targetPack != CTRPreferences.GetPacksCount() && !flag5)
                        {
                            PreLevelSelect();
                            ShowView(6);
                            return;
                        }
                    }
                    return;
            }
        }

        /// <inheritdoc />
        void IButtonDelegation.OnButtonPressed(ButtonId buttonId)
        {
            OnButtonPressed(MenuButtonId.FromButtonId(buttonId));
        }

        /// <summary>
        /// Clamps a requested pack scroll point to the available scroll point range.
        /// </summary>
        /// <param name="moveToPack">Requested pack scroll point.</param>
        /// <returns>The clamped scroll point.</returns>
        private int FixScrollPoint(int moveToPack)
        {
            if (moveToPack >= packContainer.GetTotalScrollPoints())
            {
                moveToPack = packContainer.GetTotalScrollPoints() - 1;
            }
            else if (moveToPack < 0)
            {
                moveToPack = 0;
            }
            return moveToPack;
        }

        /// <inheritdoc />
        public override void Update(float delta)
        {
            base.Update(delta);
            MovieMgr movieMgr = Application.SharedMovieMgr();
            if (movieMgr.IsPlaying())
            {
                movieMgr.Update();
                return;
            }
            TryShowOutdatedWindowsPopup();
            TryShowUpdatePopup();
            if (activeViewID == VIEW_ABOUT && aboutView != null && aboutView.UpdateAutoScroll())
            {
                return;
            }
            if (activeViewID == VIEW_CANDY_SELECT)
            {
                CandySelectionView.Update(delta);
                return;
            }
            if (activeViewID == 5 && ddPackSelect != null)
            {
                ddPackSelect.Update(delta);
                if (Global.XnaGame.IsKeyPressed(Keys.Left))
                {
                    OnButtonPressed(MenuButtonId.PreviousPack);
                    return;
                }
                if (Global.XnaGame.IsKeyPressed(Keys.Right))
                {
                    OnButtonPressed(MenuButtonId.NextPack);
                    return;
                }
                if ((Global.XnaGame.IsKeyPressed(Keys.Space) || Global.XnaGame.IsKeyPressed(Keys.Enter)) && !bScrolling)
                {
                    OnButtonPressed(MenuButtonId.ForPack(currentPack));
                    return;
                }
            }
            else
            {
                if (activeViewID == 0 && ddMainMenu != null)
                {
                    ddMainMenu.Update(delta);
                    return;
                }
                if (activeViewID == 1 && ddMainMenu != null)
                {
                    ddMainMenu.Update(delta);
                }
            }
        }

        /// <inheritdoc />
        public override bool HandleMouseWheel(int scrollDelta)
        {
            // Give popup scrollable content first chance to handle mouse wheel.
            if (ActiveView() != null)
            {
                BaseElement popupElement = ActiveView().GetChildWithName("popup");
                if (popupElement is Popup popup && popup.HandleMouseWheel(scrollDelta))
                {
                    return true;
                }
            }

            // Handle scroll wheel for level selection view
            if (activeViewID == VIEW_LEVEL_SELECT && levelContainer != null)
            {
                levelContainer.HandleMouseWheel(scrollDelta);
                return true;
            }

            // Handle scroll wheel for candy selection view
            if (activeViewID == VIEW_CANDY_SELECT && candyContainer != null)
            {
                candyContainer.HandleMouseWheel(scrollDelta);
                return true;
            }

            // Handle scroll wheel for about/credits view (activeViewID == VIEW_ABOUT)
            if (activeViewID == VIEW_ABOUT && aboutView != null && aboutView.HandleMouseWheel(scrollDelta))
            {
                return true;
            }

            // Not handled by this controller, allow base class to handle
            return base.HandleMouseWheel(scrollDelta);
        }

        /// <inheritdoc />
        public override bool TouchesBeganwithEvent(IList<TouchLocation> touches)
        {
            bool flag = base.TouchesBeganwithEvent(touches);
            if (activeViewID == VIEW_ABOUT)
            {
                aboutView?.DisableAutoScroll();
            }
            return flag;
        }

        /// <inheritdoc />
        public void TimelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
        {
        }

        /// <inheritdoc />
        public override void FullscreenToggled(bool isFullscreen)
        {
            DeleteView(5);
            CreatePackSelect();
            BaseElement childWithName = GetView(0).GetChild(0).GetChildWithName("container");
            _ = (childWithName?.x = -Canvas.xOffsetScaled);
            BaseElement childWithName2 = GetView(5).GetChild(0).GetChildWithName("text");
            _ = (childWithName2?.x = -20f - Canvas.xOffsetScaled);
            for (int i = 0; i < 10; i++)
            {
                View view3 = GetView(i);
                if (view3 != null)
                {
                    BaseElement childWithName3 = view3.GetChildWithName("backb");
                    _ = (childWithName3?.x = Canvas.xOffsetScaled);
                }
            }
            BaseElement view4 = GetView(6);
            if (view4 != null)
            {
                view4.GetChildWithName("backButton").x = Canvas.xOffsetScaled;
                view4.GetChildWithName("starText").x = -Canvas.xOffsetScaled;
            }
        }

        /// <summary>
        /// Delayed-dispatcher callback that recreates the options view.
        /// </summary>
        /// <param name="param">Unused dispatcher parameter.</param>
        public void Selector_recreateOptions(FrameworkTypes param)
        {
            RecreateOptions();
        }

        /// <inheritdoc />
        public override bool BackButtonPressed()
        {
            int currentViewId = activeViewID;
            if (currentViewId == 0)
            {
                if (ep != null)
                {
                    OnButtonPressed(MenuButtonId.ClosePopup);
                }
                else
                {
                    OnButtonPressed(MenuButtonId.ShowQuitPopup);
                }
            }
            switch (currentViewId)
            {
                case 1:
                    OnButtonPressed(MenuButtonId.BackFromOptions);
                    break;
                case 3:
                case 4:
                    OnButtonPressed(MenuButtonId.BackToOptions);
                    break;
                case VIEW_LANGUAGE_SELECT:
                    OnButtonPressed(MenuButtonId.BackFromLanguage);
                    break;
                case 5:
                    OnButtonPressed(MenuButtonId.BackFromPackSelect);
                    break;
                case 6:
                    OnButtonPressed(MenuButtonId.PackSelect);
                    break;
                case VIEW_CANDY_SELECT:
                    OnButtonPressed(MenuButtonId.BackFromCandySelect);
                    break;
                default:
                    break;
            }
            return true;
        }

        /// <summary>
        /// Shows the outdated Windows popup once when the main menu is active and the platform needs it.
        /// </summary>
        private void TryShowOutdatedWindowsPopup()
        {
            if (outdatedWindowsPopupShown)
            {
                return;
            }

            if (activeViewID != VIEW_MAIN_MENU)
            {
                return;
            }

            if (!WindowsVersionChecker.IsOutdatedWindows())
            {
                return;
            }

            outdatedWindowsPopupShown = true;
            WindowsVersionChecker.ShowOutdatedWindowsPopup(popUpMenu.builder);
        }

        /// <summary>
        /// Shows the update-available popup once when update information is ready on the main menu.
        /// </summary>
        private void TryShowUpdatePopup()
        {
            if (updatePopupShown)
            {
                return;
            }

            if (activeViewID != VIEW_MAIN_MENU)
            {
                return;
            }

            if (!CTRPreferences.IsUpdateCheckEnabled())
            {
                return;
            }

            if (!UpdateChecker.TryConsumeUpdate(out UpdateChecker.UpdateInfo info))
            {
                return;
            }

            updatePopupShown = true;
            updateReleaseUrl = info.ReleaseUrl;
            ep = popUpMenu.ShowUpdateAvailablePopup(info.CurrentVersion, info.LatestVersion, MenuButtonId.UpdateDownload, MenuButtonId.ClosePopup);
        }

        /// <summary>Main menu view identifier.</summary>
        public const int VIEW_MAIN_MENU = 0;

        /// <summary>Options view identifier.</summary>
        public const int VIEW_OPTIONS = 1;

        /// <summary>Help view identifier.</summary>
        public const int VIEW_HELP = 2;

        /// <summary>About and credits view identifier.</summary>
        public const int VIEW_ABOUT = 3;

        /// <summary>Reset-confirmation view identifier.</summary>
        public const int VIEW_RESET = 4;

        /// <summary>Pack selection view identifier.</summary>
        public const int VIEW_PACK_SELECT = 5;

        /// <summary>Level selection view identifier.</summary>
        public const int VIEW_LEVEL_SELECT = 6;

        /// <summary>Whether the game is configured with a single playable pack.</summary>
        private static bool IsSinglePack => CTRPreferences.GetPacksCount() == 1;

        /// <summary>Movie playback view identifier.</summary>
        public const int VIEW_MOVIE = 7;

        /// <summary>Leaderboards view identifier.</summary>
        public const int VIEW_LEADERBOARDS = 8;

        /// <summary>Achievements view identifier.</summary>
        public const int VIEW_ACHIEVEMENTS = 9;

        /// <summary>Candy and skin selection view identifier.</summary>
        public const int VIEW_CANDY_SELECT = 10;

        /// <summary>Language selection view identifier.</summary>
        public const int VIEW_LANGUAGE_SELECT = 11;

        /// <summary>Delayed dispatcher used by the main menu and options view.</summary>
        public DelayedDispatcher ddMainMenu;

        /// <summary>Delayed dispatcher used by the pack selection view.</summary>
        public DelayedDispatcher ddPackSelect;

        /// <summary>Popup menu helper used for menu popups.</summary>
        private readonly PopUpMenu popUpMenu;

        /// <summary>Scrollable container for candy and skin selection.</summary>
        private ScrollableContainer candyContainer;

        /// <summary>Scrollable container for pack selection.</summary>
        private ScrollableContainer packContainer;

        /// <summary>Scrollable container for level selection when a pack has many levels.</summary>
        private ScrollableContainer levelContainer;

        /// <summary>Pack box elements shown in the pack selection container.</summary>
        private readonly BaseElement[] boxes = new BaseElement[CTRPreferences.GetPacksCount() + 1];

        /// <summary>Whether to show the next-pack unlock status after scrolling.</summary>
        private bool showNextPackStatus;

        /// <summary>Whether the intro movie is being replayed before entering gameplay.</summary>
        private bool replayingIntroMovie;

        /// <summary>Currently selected pack index in the pack selection view.</summary>
        private int currentPack;

        /// <summary>Pending leftward pack scroll count from repeated next-button presses.</summary>
        private int scrollPacksLeft;

        /// <summary>Pending rightward pack scroll count from repeated previous-button presses.</summary>
        private int scrollPacksRight;

        /// <summary>Whether the pack container is currently auto-scrolling.</summary>
        private bool bScrolling;

        /// <summary>Next-pack arrow button.</summary>
        private Button nextb;

        /// <summary>Previous-pack arrow button.</summary>
        private Button prevb;

        /// <summary>Current pack index used for level selection and gameplay launch.</summary>
        private int pack;

        /// <summary>Current level index used for gameplay launch.</summary>
        private int level;

        /// <summary>View identifier to show when the menu controller is activated.</summary>
        public int viewToShow;

        /// <summary>Currently displayed popup, if any.</summary>
        private Popup ep;

        /// <summary>Release URL opened by the update popup download button.</summary>
        private string updateReleaseUrl;

        /// <summary>Whether the update-available popup has already been shown.</summary>
        private bool updatePopupShown;

        /// <summary>Whether the outdated Windows popup has already been shown.</summary>
        private bool outdatedWindowsPopupShown;

        /// <summary>Localized menu resource pack reloaded when the UI language changes.</summary>
        private static readonly string[] PackLocalizationMenu = [Resources.Img.MenuExtraButtonsEn];

        /// <summary>About view instance used for credits auto-scroll handling.</summary>
        private AboutView aboutView;

        /// <summary>
        /// Touchable wrapper element that sends menu button IDs through the button delegate.
        /// </summary>
        public sealed class TouchBaseElement : BaseElement
        {
            /// <inheritdoc />
            public override bool OnTouchDownXY(float tx, float ty)
            {
                _ = base.OnTouchDownXY(tx, ty);
                CTRRectangle r = MakeRectangle(drawX + bbc.x, drawY + bbc.y, width + bbc.w, height + bbc.h);
                CTRRectangle rectangle = RectInRectIntersection(MakeRectangle(0f, 0f, SCREEN_WIDTH, SCREEN_HEIGHT), r);
                if (PointInRect(tx, ty, r.x, r.y, r.w, r.h) && rectangle.w > r.w / 2)
                {
                    delegateValue.OnButtonPressed(bid);
                    return true;
                }
                return false;
            }

            /// <summary>Menu button identifier emitted when this element is touched.</summary>
            public MenuButtonId bid;

            /// <summary>Touch rectangle adjustment applied relative to the element bounds.</summary>
            public CTRRectangle bbc;

            /// <summary>Delegate that receives touch activation events.</summary>
            public IButtonDelegation delegateValue;
        }

        /// <summary>
        /// Draws a colored rectangle background for Om Nom in the pack selection menu.
        /// Uses scissor clipping to reveal Om Nom as the box scrolls into view.
        /// </summary>
        public sealed class MonsterSlot : ColorRect
        {
            /// <summary>
            /// Quad index for the box background in MenuPackUI texture.
            /// </summary>
            private const int QuadIndex = 0;

            /// <summary>
            /// Creates a new <see cref="MonsterSlot"/> with the specified background color.
            /// Dimensions and positioning are derived from the MenuPackUI texture.
            /// </summary>
            /// <param name="color">Background fill color for the slot.</param>
            /// <returns>A new <see cref="MonsterSlot"/> instance.</returns>
            public static MonsterSlot Create(RGBAColor color)
            {
                CTRTexture2D texture = Application.GetTexture(Resources.Img.MenuPackUI);
                MonsterSlot slot = new()
                {
                    width = (int)texture.preCutSize.X,
                    height = (int)texture.preCutSize.Y,
                    FillColor = color,
                    quadOffset = texture.quadOffsets[QuadIndex],
                    quadSize = Vect(texture.quadRects[QuadIndex].w, texture.quadRects[QuadIndex].h)
                };
                return slot;
            }

            /// <inheritdoc/>
            public override void Draw()
            {
                PreDraw();
                // Draw the colored rectangle at the quad offset position
                DrawHelper.DrawSolidRectWOBorder(
                    drawX + quadOffset.X,
                    drawY + quadOffset.Y,
                    quadSize.X,
                    quadSize.Y,
                    FillColor);

                // Apply scissor clipping to reveal Om Nom during scroll animation
                float scrollX = c.GetScroll().X;
                Vector preCutSize = Application.GetTexture(Resources.Img.MenuPackUI).preCutSize;
                if (scrollX >= s && scrollX < e)
                {
                    scrollX -= preCutSize.X + -20f;
                    float clipOffsetX = scrollX - ((s + e) / 2f);
                    Renderer.SetScissor(250f - clipOffsetX, 0f, 200f, SCREEN_HEIGHT);
                    PostDraw();
                    Renderer.SetScissor(c.drawX, c.drawY, c.width, c.height);
                }
            }

            /// <summary>Reference to the scrollable container for scroll position.</summary>
            public ScrollableContainer c;

            /// <summary>Start scroll position for scissor clipping.</summary>
            public float s;

            /// <summary>End scroll position for scissor clipping.</summary>
            public float e;

            /// <summary>Offset within the preCut area where the quad is drawn.</summary>
            private Vector quadOffset;

            /// <summary>Actual size of the colored rectangle to draw.</summary>
            private Vector quadSize;
        }
    }
}
