using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

using CutTheRope.Commons;
using CutTheRope.Desktop;
using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Media;
using CutTheRope.Framework.Visual;
using CutTheRope.Helpers;

using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace CutTheRope.GameMain
{
    internal sealed class MenuController : ViewController, IButtonDelegation, IMovieMgrDelegate, IScrollableContainerProtocol, ITimelineDelegate
    {
        public static Button CreateButtonWithTextIDDelegate(string str, ButtonId bid, IButtonDelegation d)
        {
            Image image = Image.Image_createWithResIDQuad(Resources.Img.MenuButtonDefault, 0);
            Image image2 = Image.Image_createWithResIDQuad(Resources.Img.MenuButtonDefault, 1);
            FontGeneric font = Application.GetFont(Resources.Fnt.BigFont);
            Text text = new Text().InitWithFont(font);
            text.SetString(str);
            Text text2 = new Text().InitWithFont(font);
            text2.SetString(str);
            text.anchor = text.parentAnchor = 18;
            text2.anchor = text2.parentAnchor = 18;
            _ = image.AddChild(text);
            _ = image2.AddChild(text2);
            Button button = new Button().InitWithUpElementDownElementandID(image, image2, bid);
            button.SetTouchIncreaseLeftRightTopBottom(15.0, 15.0, 15.0, 15.0);
            button.delegateButtonDelegate = d;
            return button;
        }

        public static Button CreateShortButtonWithTextIDDelegate(string str, ButtonId bid, IButtonDelegation d)
        {
            Image image = Image.Image_createWithResIDQuad(Resources.Img.MenuButtonShort, 1);
            Image image2 = Image.Image_createWithResIDQuad(Resources.Img.MenuButtonShort, 0);
            FontGeneric font = Application.GetFont(Resources.Fnt.BigFont);
            Text text = new Text().InitWithFont(font);
            text.SetString(str);
            Text text2 = new Text().InitWithFont(font);
            text2.SetString(str);
            text.anchor = text.parentAnchor = 18;
            text2.anchor = text2.parentAnchor = 18;
            _ = image.AddChild(text);
            _ = image2.AddChild(text2);
            Button button = new Button().InitWithUpElementDownElementandID(image, image2, bid);
            button.SetTouchIncreaseLeftRightTopBottom(15.0, 15.0, 15.0, 15.0);
            button.delegateButtonDelegate = d;
            return button;
        }

        public static ToggleButton CreateToggleButtonWithText1Text2IDDelegate(string str1, string str2, ButtonId bid, IButtonDelegation d)
        {
            Image image = Image.Image_createWithResIDQuad(Resources.Img.MenuButtonDefault, 0);
            Image image2 = Image.Image_createWithResIDQuad(Resources.Img.MenuButtonDefault, 1);
            Image image3 = Image.Image_createWithResIDQuad(Resources.Img.MenuButtonDefault, 0);
            Image image4 = Image.Image_createWithResIDQuad(Resources.Img.MenuButtonDefault, 1);
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
            _ = image.AddChild(text);
            _ = image2.AddChild(text2);
            _ = image3.AddChild(text3);
            _ = image4.AddChild(text4);
            ToggleButton toggleButton = new ToggleButton().InitWithUpElement1DownElement1UpElement2DownElement2andID(image, image2, image3, image4, bid);
            toggleButton.SetTouchIncreaseLeftRightTopBottom(10.0, 10.0, 10.0, 10.0);
            toggleButton.delegateButtonDelegate = d;
            return toggleButton;
        }

        public static Button CreateBackButtonWithDelegateID(IButtonDelegation d, ButtonId bid)
        {
            Button button = CreateButtonWithImageQuad1Quad2IDDelegate(Resources.Img.MenuExtraButtons, 0, 1, bid, d);
            button.anchor = button.parentAnchor = 33;
            return button;
        }

        public static Button CreateButtonWithImageIDDelegate(string resourceName, ButtonId bid, IButtonDelegation d)
        {
            CTRTexture2D texture = Application.GetTexture(resourceName);
            Image up = Image.Image_create(texture);
            Image image = Image.Image_create(texture);
            image.scaleX = 1.2f;
            image.scaleY = 1.2f;
            Button button = new Button().InitWithUpElementDownElementandID(up, image, bid);
            button.SetTouchIncreaseLeftRightTopBottom(10.0, 10.0, 10.0, 10.0);
            button.delegateButtonDelegate = d;
            return button;
        }

        public static Button CreateButton2WithImageQuad1Quad2IDDelegate(string resourceName, int q1, int q2, ButtonId bid, IButtonDelegation d)
        {
            Image up = Image.Image_createWithResIDQuad(resourceName, q1);
            Image image = Image.Image_createWithResIDQuad(resourceName, q2);
            Vector relativeQuadOffset = Image.GetRelativeQuadOffset(resourceName, q2, q1);
            image.x -= relativeQuadOffset.x;
            image.y -= relativeQuadOffset.y;
            Button button = new Button().InitWithUpElementDownElementandID(up, image, bid);
            button.delegateButtonDelegate = d;
            return button;
        }

        public static Button CreateButtonWithImageQuad1Quad2IDDelegate(string resourceName, int q1, int q2, int bid, IButtonDelegation d)
        {
            Image image = Image.Image_createWithResIDQuad(resourceName, q1);
            Image image2 = Image.Image_createWithResIDQuad(resourceName, q2);
            image.DoRestoreCutTransparency();
            image2.DoRestoreCutTransparency();
            Button button = new Button().InitWithUpElementDownElementandID(image, image2, bid);
            button.delegateButtonDelegate = d;
            CTRTexture2D texture = Application.GetTexture(resourceName);
            button.ForceTouchRect(MakeRectangle(texture.quadOffsets[q1].x, texture.quadOffsets[q1].y, texture.quadRects[q1].w, texture.quadRects[q1].h));
            return button;
        }

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
                    backgroundResource = Resources.Img.MenuBgrXmasSecondary;
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
                        backgroundSecondaryQuad = 0;
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
                int selectedCandySkin = Preferences.GetIntForKey(CTRPreferences.PREFS_SELECTED_CANDY);
                Image candyUp = Image.Image_createWithResIDQuad(Resources.Img.MenuLogoNew, selectedCandySkin);
                Image candyDown = Image.Image_createWithResIDQuad(Resources.Img.MenuLogoNew, selectedCandySkin);
                candyDown.scaleX = candyDown.scaleY = 0.95f;  // Slight press feedback
                Button candyButton = new Button().InitWithUpElementDownElementandID(candyUp, candyDown, MenuButtonId.CandySelect);
                candyButton.SetName("logoCandyButton");
                candyButton.delegateButtonDelegate = this;
                candyButton.anchor = candyButton.parentAnchor = 10;  // Top-center of logo
                candyButton.x = 147f;  // Offset right from center
                candyButton.y = 510f;  // Offset down from top of logo
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
                    glowTimeline.AddKeyFrame(KeyFrame.MakeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                    glowTimeline.AddKeyFrame(KeyFrame.MakeScale(0.85, 0.85, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.3));
                    glowTimeline.AddKeyFrame(KeyFrame.MakeScale(1.15, 1.15, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.3));
                    glowTimeline.AddKeyFrame(KeyFrame.MakeScale(0.85, 0.85, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.3));
                    glowTimeline.AddKeyFrame(KeyFrame.MakeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.3));
                    glowTimeline.AddKeyFrame(KeyFrame.MakeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1.0));  // Pause
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
                    handTimeline.AddKeyFrame(KeyFrame.MakePos(200, 70, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.0));
                    handTimeline.AddKeyFrame(KeyFrame.MakePos(180, 70, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.6));  // Move LEFT (toward candy)
                    handTimeline.AddKeyFrame(KeyFrame.MakePos(200, 70, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.6));
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
                timeline.AddKeyFrame(KeyFrame.MakeRotation(45.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.AddKeyFrame(KeyFrame.MakeRotation(405.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 75.0));
                timeline.SetTimelineLoopType(Timeline.LoopType.TIMELINE_REPLAY);
                _ = image4.AddTimeline(timeline);
                image4.PlayTimeline(0);
                _ = baseElement.AddChild(image4);
            }
            return baseElement;
        }

        public BaseElement CreateBackgroundWithLogo(bool l)
        {
            return CreateBackgroundWithLogowithShadow(l, true);
        }

        public static Image CreateAudioElementForQuadwithCrosspressediconOffset(int q, bool b, bool p, Vector offset)
        {
            int num = p ? 1 : 0;
            Image image = Image.Image_createWithResIDQuad(Resources.Img.MenuOptions, num);
            Image image2 = Image.Image_createWithResIDQuad(Resources.Img.MenuOptions, q);
            Image.SetElementPositionWithRelativeQuadOffset(image2, Resources.Img.MenuOptions, num, q);
            image2.parentAnchor = image2.anchor = 9;
            image2.x += offset.x;
            image2.y += offset.y;
            _ = image.AddChild(image2);
            if (b)
            {
                image2.color = RGBAColor.MakeRGBA(0.5f, 0.5f, 0.5f, 0.5f);
                Image image3 = Image.Image_createWithResIDQuad(Resources.Img.MenuOptions, 4);
                image3.parentAnchor = image3.anchor = 9;
                Image.SetElementPositionWithRelativeQuadOffset(image3, Resources.Img.MenuOptions, num, 4);
                _ = image.AddChild(image3);
            }
            return image;
        }

        public static ToggleButton CreateAudioButtonWithQuadDelegateIDiconOffset(int q, IButtonDelegation delegateValue, ButtonId bid, Vector offset)
        {
            Image u = CreateAudioElementForQuadwithCrosspressediconOffset(q, false, false, offset);
            Image d = CreateAudioElementForQuadwithCrosspressediconOffset(q, false, true, offset);
            Image u2 = CreateAudioElementForQuadwithCrosspressediconOffset(q, true, false, offset);
            Image d2 = CreateAudioElementForQuadwithCrosspressediconOffset(q, true, true, offset);
            ToggleButton toggleButton = new ToggleButton().InitWithUpElement1DownElement1UpElement2DownElement2andID(u, d, u2, d2, bid);
            toggleButton.delegateButtonDelegate = delegateValue;
            return toggleButton;
        }

        public static Button CreateLanguageButtonWithIDDelegate(ButtonId bid, IButtonDelegation d)
        {
            string @string = Application.SharedAppSettings().GetString(8);
            int q = 7;
            if (@string.IsEqualToString("ru"))
            {
                q = 4;
            }
            else if (@string.IsEqualToString("de"))
            {
                q = 5;
            }
            else if (@string.IsEqualToString("fr"))
            {
                q = 6;
            }
            string string2 = Application.GetString(STR_MENU_LANGUAGE);
            Image image = Image.Image_createWithResIDQuad(Resources.Img.MenuButtonDefault, 0);
            Image image2 = Image.Image_createWithResIDQuad(Resources.Img.MenuButtonDefault, 1);
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
            text.width += (int)(image3.width + RTPD(10.0));
            text2.width += (int)(image4.width + RTPD(10.0));
            Button button = new Button().InitWithUpElementDownElementandID(image, image2, bid);
            button.SetTouchIncreaseLeftRightTopBottom(15.0, 15.0, 15.0, 15.0);
            button.delegateButtonDelegate = d;
            return button;
        }

        public static BaseElement CreateElementWithResIdquad(int resId, int quad)
        {
            return resId != -1 && quad != -1 ? Image.Image_createWithResIDQuad(resId, quad) : new BaseElement();
        }

        public static ToggleButton CreateToggleButtonWithResquadquad2buttonIDdelegate(string resourceName, int quad, int quad2, int bId, IButtonDelegation delegateValue)
        {
            int res = ResourceNameTranslator.ToResourceId(resourceName);
            BaseElement baseElement = CreateElementWithResIdquad(res, quad);
            BaseElement baseElement2 = CreateElementWithResIdquad(res, quad);
            BaseElement baseElement3 = CreateElementWithResIdquad(res, quad2);
            BaseElement baseElement4 = CreateElementWithResIdquad(res, quad2);
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

        public static ToggleButton CreateToggleButtonWithResquadquad2buttonIDdelegate(int res, int quad, int quad2, int bId, IButtonDelegation delegateValue)
        {
            BaseElement baseElement = CreateElementWithResIdquad(res, quad);
            BaseElement baseElement2 = CreateElementWithResIdquad(res, quad);
            BaseElement baseElement3 = CreateElementWithResIdquad(res, quad2);
            BaseElement baseElement4 = CreateElementWithResIdquad(res, quad2);
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

        public static BaseElement CreateControlButtontitleAnchortextbuttonIDdelegate(int q, int tq, string str, int bId, IButtonDelegation delegateValue)
        {
            Image image = Image.Image_createWithResIDQuad(Resources.Img.MenuOptions, q);
            Text text = Text.CreateWithFontandString(Resources.Fnt.SmallFont, str);
            text.parentAnchor = 9;
            text.anchor = 18;
            text.scaleX = text.scaleY = 0.75f;
            _ = image.AddChild(text);
            Image.SetElementPositionWithRelativeQuadOffset(text, Resources.Img.MenuOptions, q, tq);
            if (bId != -1)
            {
                ToggleButton toggleButton = CreateToggleButtonWithResquadquad2buttonIDdelegate(Resources.Img.MenuOptions, -1, 8, bId, delegateValue);
                toggleButton.SetName("button");
                toggleButton.parentAnchor = 9;
                Image.SetElementPositionWithRelativeQuadOffset(toggleButton, Resources.Img.MenuOptions, q, 8);
                _ = image.AddChild(toggleButton);
                int num = (image.width / 2) - (toggleButton.width / 2);
                toggleButton.SetTouchIncreaseLeftRightTopBottom(num, num, image.height * 0.85, 0.0);
            }
            else
            {
                Image image2 = Image.Image_createWithResIDQuad(Resources.Img.MenuOptions, 7);
                image2.parentAnchor = 9;
                Image.SetElementPositionWithRelativeQuadOffset(image2, Resources.Img.MenuOptions, q, 7);
                _ = image.AddChild(image2);
            }
            return image;
        }

        public static Image CreateBlankScoresButtonWithIconpressed(int quad, bool pressed)
        {
            Image image3 = Image.Image_createWithResIDQuad(Resources.Img.MenuButtonAchivCup, pressed ? 1 : 0);
            Image image2 = Image.Image_createWithResIDQuad(Resources.Img.MenuButtonAchivCup, quad);
            _ = image3.AddChild(image2);
            image2.parentAnchor = 9;
            Image.SetElementPositionWithRelativeQuadOffset(image2, 59, 0, quad);
            return image3;
        }

        public static Button CreateScoresButtonWithIconbuttonIDdelegate(int quad, int bId, IButtonDelegation delegateValue)
        {
            Image up = CreateBlankScoresButtonWithIconpressed(quad, false);
            Image image = CreateBlankScoresButtonWithIconpressed(quad, true);
            Image.SetElementPositionWithRelativeQuadOffset(image, 59, 0, 1);
            Button button = new Button().InitWithUpElementDownElementandID(up, image, bId);
            button.delegateButtonDelegate = delegateValue;
            return button;
        }

        public void CreateMainMenu()
        {
            MenuView menuView = new();
            BaseElement baseElement = CreateBackgroundWithLogo(true);
            VBox vBox = new VBox().InitWithOffsetAlignWidth(5.0, 2, SCREEN_WIDTH);
            vBox.anchor = vBox.parentAnchor = 34;
            vBox.y = -85f;
            Button c = CreateButtonWithTextIDDelegate(Application.GetString(STR_MENU_PLAY), MenuButtonId.Play, this);
            _ = vBox.AddChild(c);
            Button c2 = CreateButtonWithTextIDDelegate(Application.GetString(STR_MENU_OPTIONS), MenuButtonId.Options, this);
            _ = vBox.AddChild(c2);
            Button c3 = CreateButtonWithTextIDDelegate(Application.GetString(STR_MENU_QUIT_BUTTON), MenuButtonId.ShowQuitPopup, this);
            _ = vBox.AddChild(c3);
            _ = baseElement.AddChild(vBox);
            bool flag = Application.GetString(STR_MENU_FACEBOOK_BUTTON).Length() > 0;
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
                button.x -= texture.preCutSize.x;
                button.y -= texture.preCutSize.y;
                _ = baseElement2.AddChild(button);
                Button button2 = CreateButton2WithImageQuad1Quad2IDDelegate(Resources.Img.MenuExtraButtons, 2, 2, MenuButtonId.OpenFacebook, this);
                button2.anchor = 9;
                button2.parentAnchor = 36;
                Image.SetElementPositionWithQuadOffset(button2, Resources.Img.MenuExtraButtons, 2);
                button2.x -= texture.preCutSize.x;
                button2.y -= texture.preCutSize.y;
                if (flag)
                {
                    _ = baseElement2.AddChild(button2);
                }
                Image image = Image.Image_createWithResIDQuad(Resources.Img.MenuExtraButtonsEn, 0);
                image.anchor = 9;
                image.parentAnchor = 36;
                Image.SetElementPositionWithQuadOffset(image, Resources.Img.MenuExtraButtonsEn, 0);
                image.x -= texture.preCutSize.x;
                image.y -= texture.preCutSize.y;
                _ = baseElement2.AddChild(image);
            }
            _ = menuView.AddChild(baseElement);
            AttachSnowfallOverlay(menuView);
            AddViewwithID(menuView, 0);
        }

        public void CreateOptions()
        {
            MenuView menuView = new();
            BaseElement baseElement = CreateBackgroundWithLogowithShadow(false, false);
            _ = menuView.AddChild(baseElement);
            BaseElement baseElement2 = CreateControlButtontitleAnchortextbuttonIDdelegate(5, 10, Application.GetString(STR_MENU_DRAG_TO_CUT), -1, null);
            BaseElement baseElement3 = CreateControlButtontitleAnchortextbuttonIDdelegate(6, 9, Application.GetString(STR_MENU_CLICK_TO_CUT), MenuButtonId.ToggleClickToCut, this);
            HBox hBox = new HBox().InitWithOffsetAlignHeight(RTPD(80.0), 16, MAX(baseElement2.height, baseElement3.height));
            hBox.parentAnchor = hBox.anchor = 18;
            _ = hBox.AddChild(baseElement2);
            _ = hBox.AddChild(baseElement3);
            _ = menuView.AddChild(hBox);
            Image image = Image.Image_createWithResIDQuad(Resources.Img.MenuBgrShadow, 0);
            image.anchor = image.parentAnchor = 18;
            image.scaleX = image.scaleY = 2f;
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(3);
            timeline.AddKeyFrame(KeyFrame.MakeRotation(45.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.AddKeyFrame(KeyFrame.MakeRotation(405.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 75.0));
            timeline.SetTimelineLoopType(Timeline.LoopType.TIMELINE_REPLAY);
            _ = image.AddTimeline(timeline);
            image.PlayTimeline(0);
            _ = menuView.AddChild(image);
            VBox vBox = new VBox().InitWithOffsetAlignWidth(5f, 2, SCREEN_WIDTH);
            vBox.anchor = vBox.parentAnchor = 18;
            Vector offset = VectSub(Image.GetQuadCenter(Resources.Img.MenuOptions, 0), Image.GetQuadOffset(Resources.Img.MenuOptions, 12));
            ToggleButton toggleButton = CreateAudioButtonWithQuadDelegateIDiconOffset(3, this, MenuButtonId.ToggleMusic, vectZero);
            ToggleButton toggleButton2 = CreateAudioButtonWithQuadDelegateIDiconOffset(2, this, MenuButtonId.ToggleSound, offset);
            HBox hBox2 = new HBox().InitWithOffsetAlignHeight(-10f, 16, toggleButton.height);
            _ = hBox2.AddChild(toggleButton2);
            _ = hBox2.AddChild(toggleButton);
            _ = vBox.AddChild(hBox2);
            Button c = CreateLanguageButtonWithIDDelegate(MenuButtonId.Language, this);
            _ = vBox.AddChild(c);
            Button c2 = CreateButtonWithTextIDDelegate(Application.GetString(STR_MENU_RESET), MenuButtonId.ShowReset, this);
            _ = vBox.AddChild(c2);
            Button c3 = CreateButtonWithTextIDDelegate(Application.GetString(STR_MENU_CREDITS), MenuButtonId.ShowCredits, this);
            _ = vBox.AddChild(c3);
            _ = baseElement.AddChild(vBox);
            hBox.y = (vBox.height / 2) + 10;
            vBox.y = -(float)hBox.height / 2;
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

        public void CreateReset()
        {
            MenuView menuView = new();
            BaseElement baseElement = CreateBackgroundWithLogo(false);
            Text text = new Text().InitWithFont(Application.GetFont(Resources.Fnt.BigFont));
            text.SetAlignment(2);
            text.SetStringandWidth(Application.GetString(STR_MENU_RESET_TEXT), Global.ScreenSizeManager.CurrentSize.Width * 0.95);
            text.anchor = text.parentAnchor = 18;
            _ = baseElement.AddChild(text);
            text.y = -200f;
            Button button = CreateButtonWithTextIDDelegate(Application.GetString(STR_MENU_YES), MenuButtonId.ConfirmResetYes, this);
            button.anchor = button.parentAnchor = 34;
            button.y = -540f;
            Button button2 = CreateButtonWithTextIDDelegate(Application.GetString(STR_MENU_NO), MenuButtonId.ConfirmResetNo, this);
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

        public void CreateAbout()
        {
            MenuView menuView = new();
            BaseElement baseElement = CreateBackgroundWithLogo(false);
            string text = Application.GetString(STR_MENU_ABOUT_TEXT).ToString();
            string[] separator = ["%@"];
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
            float num = 1300f;
            float h = 1100f;
            VBox vBox = new VBox().InitWithOffsetAlignWidth(0f, 2, num);
            BaseElement baseElement2 = new()
            {
                width = (int)num,
                height = 100
            };
            _ = vBox.AddChild(baseElement2);
            Image c = Image.Image_createWithResIDQuad(Resources.Img.MenuLogo, 1);
            _ = vBox.AddChild(c);
            Text text2 = new Text().InitWithFont(Application.GetFont(Resources.Fnt.SmallFont));
            text2.SetAlignment(2);
            text2.SetStringandWidth(text, (int)num);
            aboutContainer = new ScrollableContainer().InitWithWidthHeightContainer(num, h, vBox);
            aboutContainer.anchor = aboutContainer.parentAnchor = 18;
            _ = vBox.AddChild(text2);
            Image c2 = Image.Image_createWithResIDQuad(Resources.Img.MenuLogo, 2);
            _ = vBox.AddChild(c2);
            string @string = Application.GetString(STR_MENU_ABOUT_SPECIAL_THANKS);
            Text text3 = new Text().InitWithFont(Application.GetFont(Resources.Fnt.SmallFont));
            text3.SetAlignment(2);
            text3.SetStringandWidth(@string, num);
            _ = vBox.AddChild(text3);
            _ = baseElement.AddChild(aboutContainer);
            _ = menuView.AddChild(baseElement);
            Button button = CreateBackButtonWithDelegateID(this, MenuButtonId.BackToOptions);
            button.SetName("backb");
            button.x = Canvas.xOffsetScaled;
            _ = menuView.AddChild(button);
            AttachSnowfallOverlay(menuView);
            AddViewwithID(menuView, 3);
        }

        public void CreateCandySelection()
        {
            MenuView menuView = CandySelectionView.CreateCandySelection(this, out candyContainer);
            AttachSnowfallOverlay(menuView);
            AddViewwithID(menuView, VIEW_CANDY_SELECT);
        }

        public static HBox CreateTextWithStar(string t)
        {
            HBox hbox = new HBox().InitWithOffsetAlignHeight(0.0, 16, (double)RTD(50.0));
            Text text = new Text().InitWithFont(Application.GetFont(Resources.Fnt.BigFont));
            text.SetString(t);
            text.scaleX = text.scaleY = 0.7f;
            text.rotationCenterX = -(float)text.width / 2;
            text.width = (int)(text.width * 0.7f);
            _ = hbox.AddChild(text);
            Image c = Image.Image_createWithResIDQuad(Resources.Img.MenuPackSelection, 3);
            _ = hbox.AddChild(c);
            return hbox;
        }

        public static float GetBoxWidth()
        {
            return Image.GetQuadSize(Resources.Img.MenuPackSelection, 4).x + (Image.GetQuadOffset(Resources.Img.MenuPackSelection, 4).x * 2f);
        }

        public static float GetPackOffset()
        {
            float num = SCREEN_WIDTH + (Canvas.xOffset * 2);
            float boxWidth = GetBoxWidth();
            return boxWidth * 3f > num - 200f ? boxWidth / 2f : 0f;
        }

        // Explicit mapping from pack index to quad index
        // MenuPackSelection quads 4-10 are packs 1-7 (Cardboard, Fabric, Foil, Valentine, Cosmic, Toy, Gift)
        private static readonly int[] PackSelection1QuadMap =
        [
            4,  // pack 1
            5,  // pack 2
            6,  // pack 3
            7,  // pack 4
            8,  // pack 5
            9,  // pack 6
            10, // pack 7
        ];

        // MenuPackSelection2 quads 0-6 are packs 8-14
        private static readonly int[] PackSelection2QuadMap =
        [
            0, // pack 8
            1, // pack 9
            2, // pack 10
            3, // pack 11
            4, // pack 12
            5, // pack 13
            6, // pack 14
            7, // coming soon box
        ];

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
            int totalStars = CTRPreferences.GetTotalStars();
            if (n > 0 && n < CTRPreferences.GetPacksCount() && CTRPreferences.GetUnlockedForPackLevel(n, 0) == UNLOCKEDSTATE.LOCKED && totalStars >= CTRPreferences.PackUnlockStars(n))
            {
                CTRPreferences.SetUnlockedForPackLevel(UNLOCKEDSTATE.JUSTUNLOCKED, n, 0);
            }
            string resourceName;
            int q;

            // Determine resource and quad based on pack index
            switch (n)
            {
                case var _ when n == CTRPreferences.GetPacksCount():
                    // "Coming soon" box - always use the last quad from MenuPackSelection2
                    resourceName = Resources.Img.MenuPackSelection2;
                    q = PackSelection2QuadMap[^1];
                    break;

                case <= 6:
                    // Packs 0-6 use MenuPackSelection
                    resourceName = Resources.Img.MenuPackSelection;
                    q = n >= 0 && n < PackSelection1QuadMap.Length ? PackSelection1QuadMap[n] : 4; // fallback to pack 1
                    break;

                default:
                    // Packs 7+ use MenuPackSelection2
                    int index = n - 7;
                    if (index >= 0 && index < PackSelection2QuadMap.Length - 1) // -1 to exclude "coming soon" quad
                    {
                        resourceName = Resources.Img.MenuPackSelection2;
                        q = PackSelection2QuadMap[index];
                    }
                    else
                    {
                        // fallback to pack 1 from MenuPackSelection
                        resourceName = Resources.Img.MenuPackSelection;
                        q = 4;
                    }
                    break;
            }
            string nsstring;
            if (n == CTRPreferences.GetPacksCount())
            {
                nsstring = Application.GetString(STR_MENU_BOX_SOON_LABEL);
            }
            else
            {
                string text3 = (n + 1).ToString(CultureInfo.InvariantCulture);
                string text4 = ". ";
                string boxLabelId = n switch
                {
                    0 => STR_MENU_BOX1_LABEL,
                    1 => STR_MENU_BOX2_LABEL,
                    2 => STR_MENU_BOX3_LABEL,
                    3 => STR_MENU_BOX4_LABEL,
                    4 => STR_MENU_BOX5_LABEL,
                    5 => STR_MENU_BOX6_LABEL,
                    6 => STR_MENU_BOX7_LABEL,
                    7 => STR_MENU_BOX8_LABEL,
                    8 => STR_MENU_BOX9_LABEL,
                    9 => STR_MENU_BOX10_LABEL,
                    10 => STR_MENU_BOX11_LABEL,
                    11 => STR_MENU_BOX12_LABEL,
                    12 => STR_MENU_BOX13_LABEL,
                    13 => STR_MENU_BOX14_LABEL,
                    _ => STR_MENU_BOX1_LABEL,
                };
                string @string = Application.GetString(boxLabelId);
                nsstring = text3 + text4 + (@string?.ToString());
            }
            string nSString = nsstring;
            UNLOCKEDSTATE unlockedForPackLevel = CTRPreferences.GetUnlockedForPackLevel(n, 0);
            bool flag = unlockedForPackLevel == UNLOCKEDSTATE.LOCKED && n != CTRPreferences.GetPacksCount();
            touchBaseElement.bid = n != CTRPreferences.GetPacksCount() ? MenuButtonId.ForPack(n) : new MenuButtonId(-1);
            Image image = Image.Image_createWithResIDQuad(resourceName, q);
            image.DoRestoreCutTransparency();
            image.anchor = image.parentAnchor = 9;
            if (flag)
            {
                _ = baseElement.AddChild(image);
                int num = CTRPreferences.PackUnlockStars(n);
                Image image2 = Image.Image_createWithResIDQuad(Resources.Img.MenuPackSelection, 2);
                image2.DoRestoreCutTransparency();
                image2.anchor = image2.parentAnchor = 9;
                _ = image.AddChild(image2);
                HBox hBox = CreateTextWithStar(num.ToString(CultureInfo.InvariantCulture));
                hBox.anchor = hBox.parentAnchor = 18;
                hBox.y = 110f;
                _ = image2.AddChild(hBox);
                Text text = new Text().InitWithFont(Application.GetFont(Resources.Fnt.SmallFont));
                string newString = Application.GetString(STR_MENU_UNLOCK_HINT).ToString().Replace("%d", num.ToString(CultureInfo.InvariantCulture));
                text.SetAlignment(2);
                text.anchor = 10;
                text.parentAnchor = 34;
                text.SetStringandWidth(newString, 600f);
                text.y = -60f;
                text.scaleX = text.scaleY = 0.7f;
                text.rotationCenterY = -(float)text.height / 2;
                _ = touchBaseElement.AddChild(text);
            }
            else
            {
                if (n != CTRPreferences.GetPacksCount())
                {
                    int q2 = 0;
                    int q3 = 1;
                    MonsterSlot monsterSlot = MonsterSlot.MonsterSlot_createWithResIDQuad(Resources.Img.MenuPackSelection, q2);
                    monsterSlot.c = c;
                    monsterSlot.DoRestoreCutTransparency();
                    monsterSlot.anchor = 9;
                    monsterSlot.parentAnchor = 9;
                    monsterSlot.y = image.y;
                    _ = baseElement.AddChild(monsterSlot);
                    Image image3 = Image.Image_createWithResIDQuad(Resources.Img.MenuPackSelection, q3);
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
                if (unlockedForPackLevel == UNLOCKEDSTATE.JUSTUNLOCKED)
                {
                    Image image4 = Image.Image_createWithResIDQuad(Resources.Img.MenuPackSelection, 2);
                    image4.SetName("lockHideMe");
                    image4.DoRestoreCutTransparency();
                    image4.anchor = image4.parentAnchor = 9;
                    _ = baseElement.AddChild(image4);
                    Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(3);
                    timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                    timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1.5));
                    timeline.AddKeyFrame(KeyFrame.MakeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                    timeline.AddKeyFrame(KeyFrame.MakeScale(2.0, 2.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1.5));
                    _ = image4.AddTimeline(timeline);
                }
            }
            Text text2 = new Text().InitWithFont(Application.GetFont(Resources.Fnt.BigFont));
            text2.anchor = text2.parentAnchor = 10;
            text2.scaleX = text2.scaleY = 0.75f;
            if (LANGUAGE is Language.LANGDE or Language.LANGEN)
            {
                text2.scaleX = 0.7f;
            }
            text2.SetAlignment(2);
            if (n != CTRPreferences.GetPacksCount())
            {
                text2.SetString(nSString);
            }
            else
            {
                text2.SetStringandWidth(nSString, 656.0);
            }
            text2.y = 140f;
            _ = image.AddChild(text2);
            Timeline timeline2 = new Timeline().InitWithMaxKeyFramesOnTrack(4);
            timeline2.AddKeyFrame(KeyFrame.MakeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline2.AddKeyFrame(KeyFrame.MakeScale(0.95, 1.05, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.15));
            timeline2.AddKeyFrame(KeyFrame.MakeScale(1.05, 0.95, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.2));
            timeline2.AddKeyFrame(KeyFrame.MakeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.25));
            _ = baseElement.AddTimeline(timeline2);
            baseElement.height = touchBaseElement.height = image.height;
            baseElement.width = touchBaseElement.width = image.width;
            return touchBaseElement;
        }

        public void CreatePackSelect()
        {
            MenuView menuView = new();
            BaseElement baseElement = CreateBackgroundWithLogo(false);
            string text = Application.GetString(STR_MENU_TOTAL_STARS).ToString();
            text = text.Replace("%d", "");
            HBox hBox = CreateTextWithStar(text + CTRPreferences.GetTotalStars().ToString(CultureInfo.InvariantCulture));
            hBox.x = -30f - Canvas.xOffsetScaled;
            hBox.y = 40f;
            hBox.SetName("text");
            HBox hBox2 = new HBox().InitWithOffsetAlignHeight(-20f, 16, SCREEN_HEIGHT);
            float num = SCREEN_WIDTH + (Canvas.xOffset * 2);
            float boxWidth = GetBoxWidth();
            float num2 = boxWidth * 3f;
            if (num2 > num - 200f)
            {
                num2 = boxWidth * 2f;
            }
            packContainer = new ScrollableContainer().InitWithWidthHeightContainer(num2, SCREEN_HEIGHT, hBox2);
            packContainer.minAutoScrollToSpointLength = RTD(5.0);
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
            CTRTexture2D texture = Application.GetTexture(Resources.Img.MenuPackSelection);
            BaseElement baseElement2 = new()
            {
                width = (int)texture.preCutSize.x,
                height = (int)texture.preCutSize.y
            };
            _ = hBox2.AddChild(baseElement2);
            float num3 = 0f + GetPackOffset();
            for (int i = 0; i < CTRPreferences.GetPacksCount() + 1; i++)
            {
                TouchBaseElement touchBaseElement = (TouchBaseElement)CreatePackElementforContainer(i, packContainer);
                boxes[i] = touchBaseElement;
                _ = hBox2.AddChild(touchBaseElement);
                touchBaseElement.x -= 0f;
                touchBaseElement.y -= 0f;
                _ = packContainer.AddScrollPointAtXY((double)num3, 0.0);
                touchBaseElement.bbc = MakeRectangle(0f, 0f, -20f, 0f);
                num3 += touchBaseElement.width + -20f;
            }
            hBox2.width += 1000;
            Image image = Image.Image_createWithResIDQuad(Resources.Img.MenuPackSelection, 11);
            image.anchor = 17;
            image.y += SCREEN_HEIGHT / 2f;
            image.x = packContainer.x - 2f;
            _ = baseElement.AddChild(image);
            Image image2 = Image.Image_createWithResIDQuad(Resources.Img.MenuPackSelection, 11);
            image2.anchor = 20;
            image2.y += SCREEN_HEIGHT / 2f;
            image2.x = packContainer.x + packContainer.width + 2f;
            _ = baseElement.AddChild(image2);
            image2.scaleX = image2.scaleY = -1f;
            _ = baseElement.AddChild(packContainer);
            Image image3 = Image.Image_createWithResIDQuad(Resources.Img.MenuPackSelection, 12);
            image3.anchor = 20;
            image3.y += SCREEN_HEIGHT / 2f;
            image3.x = packContainer.x + 3f;
            _ = baseElement.AddChild(image3);
            Image image4 = Image.Image_createWithResIDQuad(Resources.Img.MenuPackSelection, 12);
            image4.anchor = 17;
            image4.y += SCREEN_HEIGHT / 2f;
            image4.x = packContainer.x + packContainer.width - 3f;
            image4.scaleX = image4.scaleY = -1f;
            _ = baseElement.AddChild(image4);
            prevb = CreateButton2WithImageQuad1Quad2IDDelegate(Resources.Img.MenuPackSelection, 13, 14, MenuButtonId.PreviousPack, this);
            prevb.parentAnchor = 17;
            prevb.anchor = 20;
            prevb.x = packContainer.x - 40f;
            _ = baseElement.AddChild(prevb);
            nextb = CreateButton2WithImageQuad1Quad2IDDelegate(Resources.Img.MenuPackSelection, 13, 14, MenuButtonId.NextPack, this);
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
            int lastPack = CTRPreferences.GetLastPack();
            packContainer.PlaceToScrollPoint(lastPack);
            ScrollableContainerchangedTargetScrollPoint(packContainer, lastPack);
        }

        public static void CreateLeaderboards()
        {
        }

        public static void CreateAchievements()
        {
        }

        public void ShowCantUnlockPopup()
        {
            popUpMenu.ShowCantUnlockPopup();
        }

        public void ShowGameFinishedPopup()
        {
            popUpMenu.ShowGameFinishedPopup();
        }

        public void ShowYesNoPopup(string str, MenuButtonId buttonYesId, MenuButtonId buttonNoId)
        {
            ep = popUpMenu.ShowYesNoPopup(str, buttonYesId, buttonNoId);
        }

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

        public void ScrollableContainerchangedTargetScrollPoint(ScrollableContainer e, int i)
        {
            currentPack = i;
            pack = i;
            CTRPreferences.SetLastPack(i);
        }

        public BaseElement CreateButtonForLevelPack(int l, int p)
        {
            bool flag = CTRPreferences.GetUnlockedForPackLevel(p, l) == UNLOCKEDSTATE.LOCKED;
            int starsForPackLevel = CTRPreferences.GetStarsForPackLevel(p, l);
            TouchBaseElement touchBaseElement = new()
            {
                bbc = MakeRectangle(5.0, 0.0, -10.0, 0.0),
                delegateValue = this
            };
            Image image;
            if (flag)
            {
                touchBaseElement.bid = new MenuButtonId(-1);
                image = Image.Image_createWithResIDQuad(Resources.Img.MenuLevelSelection, 1);
                image.DoRestoreCutTransparency();
            }
            else
            {
                touchBaseElement.bid = MenuButtonId.ForLevel(l);
                image = Image.Image_createWithResIDQuad(Resources.Img.MenuLevelSelection, 0);
                image.DoRestoreCutTransparency();
                Text text = new Text().InitWithFont(Application.GetFont(Resources.Fnt.BigFont));
                string @string = (l + 1).ToString(CultureInfo.InvariantCulture);
                text.SetString(@string);
                text.anchor = text.parentAnchor = 18;
                text.y -= 5f;
                _ = image.AddChild(text);
                Image image2 = Image.Image_createWithResIDQuad(Resources.Img.MenuLevelSelection, 2 + starsForPackLevel);
                image2.DoRestoreCutTransparency();
                image2.anchor = image2.parentAnchor = 9;
                _ = image.AddChild(image2);
            }
            image.anchor = image.parentAnchor = 18;
            _ = touchBaseElement.AddChild(image);
            touchBaseElement.SetSizeToChildsBounds();
            return touchBaseElement;
        }

        public void CreateLevelSelect()
        {
            float num = 0.3f;
            MenuView menuView = new();
            string boxCover = PackConfig.GetBoxCoverOrDefault(pack);
            Image image = Image.Image_createWithResIDQuad(boxCover, 0);
            Image image2 = Image.Image_createWithResIDQuad(boxCover, 0);
            Vector quadSize = Image.GetQuadSize(boxCover, 0);
            float x = (SCREEN_WIDTH / 2f) - quadSize.x;
            image.x = x;
            image2.x = SCREEN_WIDTH / 2f;
            image2.rotation = 180f;
            image2.y -= 0.5f;
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(3);
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.MakeRGBA(0.85, 0.85, 0.85, 1.0), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, num));
            _ = image.AddTimeline(timeline);
            image.SetName("levelsBack");
            _ = image.AddChild(image2);
            _ = menuView.AddChild(image);
            Image image3 = Image.Image_createWithResIDQuad(Resources.Img.MenuLoading, 0);
            Image image4 = Image.Image_createWithResIDQuad(Resources.Img.MenuLoading, 1);
            image3.x = Image.GetQuadOffset(Resources.Img.MenuLoading, 0).x;
            image3.y = 80f;
            image4.x = Image.GetQuadOffset(Resources.Img.MenuLoading, 1).x;
            image4.y = 80f;
            _ = menuView.AddChild(image3);
            _ = menuView.AddChild(image4);
            Image image5 = Image.Image_createWithResIDQuad(Resources.Img.MenuBgrShadow, 0);
            image5.SetName("shadow");
            image5.anchor = image5.parentAnchor = 18;
            image5.scaleX = image5.scaleY = 2f;
            Timeline timeline2 = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline2.AddKeyFrame(KeyFrame.MakeScale(2.0, 2.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline2.AddKeyFrame(KeyFrame.MakeScale(5.0, 5.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, (double)num));
            timeline2.delegateTimelineDelegate = this;
            _ = image5.AddTimeline(timeline2);
            Timeline timeline3 = new Timeline().InitWithMaxKeyFramesOnTrack(3);
            timeline3.AddKeyFrame(KeyFrame.MakeRotation(45.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline3.AddKeyFrame(KeyFrame.MakeRotation(405.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 75.0));
            timeline3.SetTimelineLoopType(Timeline.LoopType.TIMELINE_REPLAY);
            _ = image5.AddTimeline(timeline3);
            image5.PlayTimeline(1);
            _ = menuView.AddChild(image5);
            HBox hBox = CreateTextWithStar(CTRPreferences.GetTotalStarsInPack(pack).ToString(CultureInfo.InvariantCulture) + "/" + (CTRPreferences.GetLevelsInPackCount(pack) * 3).ToString(CultureInfo.InvariantCulture));
            hBox.x = -20f;
            hBox.y = 20f;
            float of = 55f;
            float of2 = 10f;
            float h = 202.79999f;
            VBox vBox = new VBox().InitWithOffsetAlignWidth(of, 2, SCREEN_WIDTH);
            vBox.SetName("levelsBox");
            vBox.x = 0f;
            vBox.y = 110f;
            int levelsInPack = CTRPreferences.GetLevelsInPackCount(pack);
            int columnsPerRow = 5;
            int num3 = 0;
            for (int i = 0; i < levelsInPack; i += columnsPerRow)
            {
                HBox hBox2 = new HBox().InitWithOffsetAlignHeight(of2, 16, h);
                for (int j = 0; j < columnsPerRow && num3 < levelsInPack; j++)
                {
                    _ = hBox2.AddChild(CreateButtonForLevelPack(num3++, pack));
                }
                _ = vBox.AddChild(hBox2);
            }
            Timeline timeline4 = new Timeline().InitWithMaxKeyFramesOnTrack(3);
            timeline4.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline4.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, num));
            _ = vBox.AddTimeline(timeline4);
            hBox.anchor = hBox.parentAnchor = 12;
            hBox.SetName("starText");
            hBox.x = -(float)Canvas.xOffsetScaled;
            Timeline timeline5 = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline5.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline5.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, num));
            _ = hBox.AddTimeline(timeline5);
            _ = menuView.AddChild(hBox);
            _ = menuView.AddChild(vBox);
            Button button = CreateBackButtonWithDelegateID(this, MenuButtonId.PackSelect);
            button.SetName("backButton");
            Timeline timeline6 = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline6.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline6.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, num));
            _ = button.AddTimeline(timeline6);
            button.x = Canvas.xOffsetScaled;
            _ = menuView.AddChild(button);
            AttachSnowfallOverlay(menuView);
            AddViewwithID(menuView, 6);
        }

        public MenuController(ViewController parent)
            : base(parent)
        {
            ddMainMenu = new DelayedDispatcher();
            ddPackSelect = new DelayedDispatcher();
            popUpMenu = new PopUpMenu(this);
            CreateMainMenu();
            CreateOptions();
            CreateReset();
            CreateAbout();
            CreateCandySelection();
            CreateMovieView();
            CreatePackSelect();
            CreateLeaderboards();
            CreateAchievements();
            MapPickerController c = new(this);
            AddChildwithID(c, 0);
        }

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

        public override void Activate()
        {
            showNextPackStatus = false;
            base.Activate();
            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
            pack = cTRRootController.GetPack();
            if (viewToShow == 6)
            {
                currentPack = pack;
                PreLevelSelect();
            }
            ShowView(viewToShow);
            CTRSoundMgr.StopMusic();
            if (SpecialEvents.IsXmas)
            {
                CTRSoundMgr.PlayMusic(Resources.Snd.MenuMusicXmas);
            }
            else
            {
                CTRSoundMgr.PlayMusic(Resources.Snd.MenuMusic);
            }
        }

        public void ShowNextPack()
        {
            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
            int num = cTRRootController.GetPack();
            if (num < CTRPreferences.GetPacksCount() - 1)
            {
                packContainer.delegateScrollableContainerProtocol = this;
                packContainer.MoveToScrollPointmoveMultiplier(num + 1, 0.8);
                showNextPackStatus = true;
                return;
            }
            replayingIntroMovie = false;
            packContainer.PlaceToScrollPoint(cTRRootController.GetPack() + 1);
            CTRSoundMgr.StopMusic();
            Application.SharedMovieMgr().delegateMovieMgrDelegate = this;
            Application.SharedMovieMgr().PlayURL("outro", !Preferences.GetBooleanForKey("MUSIC_ON") && !Preferences.GetBooleanForKey("SOUND_ON"));
        }

        public override void OnChildDeactivated(int n)
        {
            base.OnChildDeactivated(n);
            ((CTRRootController)Application.SharedRootController()).SetSurvival(false);
            Deactivate();
        }

        public void MoviePlaybackFinished(string url)
        {
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
                    CTRSoundMgr.PlayMusic(Resources.Snd.MenuMusicXmas);
                }
                else
                {
                    CTRSoundMgr.PlayMusic(Resources.Snd.MenuMusic);
                }
            }
            if (CTRPreferences.ShouldPlayLevelScroll())
            {
                packContainer.PlaceToScrollPoint(CTRPreferences.GetPacksCount() - 1);
                packContainer.MoveToScrollPointmoveMultiplier(0, 0.6);
                CTRPreferences.DisablePlayLevelScroll();
            }
            else
            {
                packContainer.PlaceToScrollPoint(CTRPreferences.GetLastPack());
            }
            ShowView(5);
            if (url != null && url.HasSuffix("outro"))
            {
                packContainer.MoveToScrollPointmoveMultiplier(CTRPreferences.GetPacksCount(), 0.8);
                ShowGameFinishedPopup();
            }
        }

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

        public void TimelineFinished(Timeline t)
        {
            CTRSoundMgr.StopMusic();
            CTRRootController ctrrootController = (CTRRootController)Application.SharedRootController();
            ctrrootController.SetPack(pack);
            ctrrootController.SetLevel(level);
            Application.SharedRootController().SetViewTransition(-1);
            ((MapPickerController)GetChild(0)).SetAutoLoadMap(LevelsList.LEVEL_NAMES[pack, level]);
            if (pack == 0 && level == 0 && CTRPreferences.GetScoreForPackLevel(0, 0) != 0)
            {
                replayingIntroMovie = true;
                ShowView(7);
                CTRSoundMgr.StopMusic();
                Application.SharedMovieMgr().delegateMovieMgrDelegate = this;
                Application.SharedMovieMgr().PlayURL("intro", !Preferences.GetBooleanForKey("MUSIC_ON") && !Preferences.GetBooleanForKey("SOUND_ON"));
                return;
            }
            ActivateChild(0);
        }

        public void RecreateOptions()
        {
            DeleteView(1);
            CreateOptions();
        }

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

            switch (n)
            {
                case var id when id == MenuButtonId.Play:
                    {
                        for (int i = 0; i < CTRPreferences.GetPacksCount(); i++)
                        {
                            GameController.CheckForBoxPerfect(i);
                        }
                        replayingIntroMovie = false;
                        if (CTRPreferences.GetScoreForPackLevel(0, 0) == 0)
                        {
                            ShowView(7);
                            CTRSoundMgr.StopMusic();
                            Application.SharedMovieMgr().delegateMovieMgrDelegate = this;
                            Application.SharedMovieMgr().PlayURL("intro", !Preferences.GetBooleanForKey("MUSIC_ON") && !Preferences.GetBooleanForKey("SOUND_ON"));
                            return;
                        }
                        MoviePlaybackFinished(null);
                        return;
                    }
                case var id when id == MenuButtonId.Options:
                    ShowView(1);
                    return;
                case var id when id == MenuButtonId.PlayPack0:
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
                        ctrrootController.SetPack(pack);
                        Deactivate();
                        return;
                    }
                case var id when id == MenuButtonId.OpenFullVersion:
                    CTRRootController.OpenFullVersionPage();
                    return;
                case var id when id == MenuButtonId.ToggleSound:
                    Preferences.SetBooleanForKey(!Preferences.GetBooleanForKey("SOUND_ON"), "SOUND_ON", true);
                    return;
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
                            CTRSoundMgr.PlayMusic(Resources.Snd.MenuMusicXmas);
                        }
                        else
                        {
                            CTRSoundMgr.PlayMusic(Resources.Snd.MenuMusic);
                        }
                        return;
                    }
                case var id when id == MenuButtonId.ShowCredits:
                    aboutContainer.SetScroll(Vect(0f, 0f));
                    aboutAutoScroll = true;
                    ShowView(3);
                    return;
                case var id when id == MenuButtonId.ShowReset:
                    ShowView(4);
                    return;
                case var id when id == MenuButtonId.Leaderboards || id == MenuButtonId.LeaderboardsAchievementsUnused || id == MenuButtonId.MoreGamesUnused:
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
                    ShowView(5);
                    packContainer.MoveToScrollPointmoveMultiplier(pack, 0.8);
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
                    AndroidAPI.OpenUrl("http://twitter.com/zeptolab");
                    return;
                case var id when id == MenuButtonId.OpenFacebook:
                    AndroidAPI.OpenUrl("http://www.facebook.com/cuttherope");
                    return;
                case var id when id == MenuButtonId.NextPack:
                    {
                        int num2 = currentPack;
                        int num3 = scrollPacksLeft + 1;
                        scrollPacksLeft = num3;
                        int sp2 = FixScrollPoint(num2 + num3 - scrollPacksRight);
                        packContainer.MoveToScrollPointmoveMultiplier(sp2, 0.8);
                        bScrolling = true;
                        return;
                    }
                case var id when id == MenuButtonId.PreviousPack:
                    {
                        int num4 = currentPack;
                        int num3 = scrollPacksRight + 1;
                        scrollPacksRight = num3;
                        int sp3 = FixScrollPoint(num4 - num3 + scrollPacksLeft);
                        packContainer.MoveToScrollPointmoveMultiplier(sp3, 0.8);
                        bScrolling = true;
                        break;
                    }
                case var id when id == MenuButtonId.Language:
                    {
                        string @string = Application.SharedAppSettings().GetString(8);
                        string[] array3 =
                        [
                    "en",
                    "ru",
                    "de",
                    "fr"
                        ];
                        int num = array3.Length;
                        bool flag4 = false;
                        for (int j = 0; j < num; j++)
                        {
                            if (@string.IsEqualToString(array3[j]))
                            {
                                string nSString = array3[(j + 1) % num];
                                Application.SharedAppSettings().SetString(8, nSString);
                                Preferences.SetStringForKey(nSString.ToString(), "PREFS_LOCALE", true);
                                flag4 = true;
                                break;
                            }
                        }
                        if (!flag4)
                        {
                            Application.SharedAppSettings().SetString(8, array3[1]);
                            Preferences.SetStringForKey(array3[1].ToString(), "PREFS_LOCALE", true);
                        }
                        CTRResourceMgr ctrresourceMgr2 = Application.SharedResourceMgr();
                        ctrresourceMgr2.FreePack(PackLocalizationMenu);
                        ctrresourceMgr2.ClearCachedResources();
                        ctrresourceMgr2.InitLoading();
                        ctrresourceMgr2.LoadPack(PackLocalizationMenu);
                        ctrresourceMgr2.LoadImmediately();
                        DeleteView(5);
                        CreatePackSelect();
                        DeleteView(0);
                        CreateMainMenu();
                        DeleteView(4);
                        CreateReset();
                        DeleteView(3);
                        CreateAbout();
                        DeleteView(VIEW_CANDY_SELECT);
                        CreateCandySelection();
                        CreateLeaderboards();
                        ddMainMenu.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(Selector_recreateOptions), null, 0.01);
                        ((CTRRootController)Application.SharedRootController()).RecreateLoadingController();
                        return;
                    }
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
                        return;
                    }
                    break;
                case var id when id == MenuButtonId.ShowQuitPopup:
                    ShowYesNoPopup(Application.GetString(STR_MENU_QUIT), MenuButtonId.QuitGame, MenuButtonId.ClosePopup);
                    return;
                case var id when id == MenuButtonId.CandySelect:
                    // Switch to candy selection mode or open candy selection view
                    if (activeViewID == VIEW_CANDY_SELECT)
                    {
                        // Already in candy select view, switch to candy mode
                        CandySelectionView.SwitchToMode(false);
                    }
                    else
                    {
                        // Open candy selection view
                        Preferences.SetBooleanForKey(true, "PREFS_CANDY_WAS_CHANGED", true);
                        ShowView(VIEW_CANDY_SELECT);
                    }
                    return;
                case var id when id == MenuButtonId.RopeSelect:
                    // Switch to rope selection mode
                    CandySelectionView.SwitchToMode(true);
                    return;
                case var id when id == MenuButtonId.BackFromCandySelect:
                    // Return to main menu from candy selection
                    // Recreate the main menu to reflect the new candy selection
                    DeleteView(0);
                    CreateMainMenu();
                    ShowView(0);
                    return;
                default:
                    // Handle candy slot selection buttons
                    if (n.IsCandySlotButton())
                    {
                        int selectedCandyIndex = n.GetCandyIndex();

                        // Save the selected candy skin to preferences
                        Preferences.SetIntForKey(selectedCandyIndex, CTRPreferences.PREFS_SELECTED_CANDY, true);

                        // Update candy slot buttons to show new equipped state
                        CandySelectionView.UpdateCandySlotButtons(selectedCandyIndex);
                        return;
                    }

                    // Handle rope slot selection buttons
                    if (n.IsRopeSlotButton())
                    {
                        int selectedRopeIndex = n.GetRopeIndex();

                        // Save the selected rope skin to preferences
                        Preferences.SetIntForKey(selectedRopeIndex, CTRPreferences.PREFS_SELECTED_ROPE, true);

                        // Update rope slot buttons to show new equipped state (reuse candy logic)
                        CandySelectionView.UpdateCandySlotButtons(selectedRopeIndex);
                        return;
                    }
                    // Handle pack selection buttons dynamically
                    if (n.IsPackButton())
                    {
                        int targetPack = n.GetPackIndex();
                        if (pack != targetPack)
                        {
                            packContainer.MoveToScrollPointmoveMultiplier(targetPack, 0.8);
                            return;
                        }
                        CTRPreferences.SetLastPack(pack);
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

        void IButtonDelegation.OnButtonPressed(ButtonId buttonId)
        {
            OnButtonPressed(MenuButtonId.FromButtonId(buttonId));
        }

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

        public override void Update(float delta)
        {
            base.Update(delta);
            MovieMgr movieMgr = Application.SharedMovieMgr();
            if (movieMgr.IsPlaying())
            {
                movieMgr.Update();
                return;
            }
            if (activeViewID == 3 && aboutAutoScroll)
            {
                Vector scroll = aboutContainer.GetScroll();
                Vector maxScroll = aboutContainer.GetMaxScroll();
                scroll.y += 0.5f;
                scroll.y = FIT_TO_BOUNDARIES(scroll.y, 0.0, maxScroll.y);
                aboutContainer.SetScroll(scroll);
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

        /// <summary>
        /// Handles mouse wheel scrolling for menu views with scrollable content.
        /// </summary>
        /// <param name="scrollDelta">The mouse wheel scroll delta from the input system.</param>
        /// <returns>True if the scroll was handled by this controller, false otherwise.</returns>
        /// <remarks>
        /// Currently handles scrolling for:
        /// <list type="bullet">
        /// <item>
        ///      <description>About/Credits view (activeViewID == 3): Forwards to aboutContainer and disables auto-scroll</description>
        /// </item>
        /// </list>
        /// To add scrolling support for additional views:
        /// <list type="bullet">
        ///  <item>
        ///      <description>Store a reference to the view's ScrollableContainer field.</description>
        ///  </item>
        ///  <item>
        ///       <description>Add a new <c>if</c> block checking <c>activeViewID</c>.</description>
        ///   </item>
        ///   <item>
        ///      <description>Call <c>container.HandleMouseWheel(scrollDelta)</c> and return <c>true</c>.</description>
        ///  </item>
        /// </list>
        /// </remarks>
        public override bool HandleMouseWheel(int scrollDelta)
        {
            // Handle scroll wheel for candy selection view
            if (activeViewID == VIEW_CANDY_SELECT && candyContainer != null)
            {
                candyContainer.HandleMouseWheel(scrollDelta);
                return true;
            }

            // Handle scroll wheel for about/credits view (activeViewID == 3)
            if (activeViewID == 3 && aboutContainer != null)
            {
                aboutAutoScroll = false; // Disable auto-scroll when user manually scrolls
                aboutContainer.HandleMouseWheel(scrollDelta);
                return true;
            }

            // Not handled by this controller, allow base class to handle
            return base.HandleMouseWheel(scrollDelta);
        }

        public override bool TouchesBeganwithEvent(IList<TouchLocation> touches)
        {
            bool flag = base.TouchesBeganwithEvent(touches);
            if (activeViewID == 3 && aboutAutoScroll)
            {
                aboutAutoScroll = false;
            }
            return flag;
        }

        public void TimelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
        {
        }

        public override void FullscreenToggled(bool isFullscreen)
        {
            DeleteView(5);
            CreatePackSelect();
            BaseElement childWithName = GetView(0).GetChild(0).GetChildWithName("container");
            if (childWithName != null)
            {
                childWithName.x = -(float)Canvas.xOffsetScaled;
            }
            BaseElement childWithName2 = GetView(5).GetChild(0).GetChildWithName("text");
            if (childWithName2 != null)
            {
                childWithName2.x = -20f - Canvas.xOffsetScaled;
            }
            for (int i = 0; i < 10; i++)
            {
                View view3 = GetView(i);
                if (view3 != null)
                {
                    BaseElement childWithName3 = view3.GetChildWithName("backb");
                    if (childWithName3 != null)
                    {
                        childWithName3.x = Canvas.xOffsetScaled;
                    }
                }
            }
            BaseElement view4 = GetView(6);
            if (view4 != null)
            {
                view4.GetChildWithName("backButton").x = Canvas.xOffsetScaled;
                view4.GetChildWithName("starText").x = -(float)Canvas.xOffsetScaled;
            }
        }

        public void Selector_recreateOptions(FrameworkTypes param)
        {
            RecreateOptions();
        }

        public override bool BackButtonPressed()
        {
            int num = activeViewID;
            if (num == 0)
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
            switch (num)
            {
                case 1:
                    OnButtonPressed(MenuButtonId.BackFromOptions);
                    break;
                case 3:
                case 4:
                    OnButtonPressed(MenuButtonId.BackToOptions);
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

        public const int VIEW_MAIN_MENU = 0;

        public const int VIEW_OPTIONS = 1;

        public const int VIEW_HELP = 2;

        public const int VIEW_ABOUT = 3;

        public const int VIEW_RESET = 4;

        public const int VIEW_PACK_SELECT = 5;

        public const int VIEW_LEVEL_SELECT = 6;

        public const int VIEW_MOVIE = 7;

        public const int VIEW_LEADERBOARDS = 8;

        public const int VIEW_ACHIEVEMENTS = 9;

        public const int VIEW_CANDY_SELECT = 10;
        public DelayedDispatcher ddMainMenu;

        public DelayedDispatcher ddPackSelect;

        private readonly PopUpMenu popUpMenu;
        private ScrollableContainer aboutContainer;

        private ScrollableContainer candyContainer;

        private ScrollableContainer packContainer;

        private readonly BaseElement[] boxes = new BaseElement[CTRPreferences.GetPacksCount() + 1];

        private bool showNextPackStatus;

        private bool aboutAutoScroll;

        private bool replayingIntroMovie;

        private int currentPack;

        private int scrollPacksLeft;

        private int scrollPacksRight;

        private bool bScrolling;

        private Button nextb;

        private Button prevb;

        private int pack;

        private int level;

        public int viewToShow;

        private Popup ep;

        private static readonly string[] PackLocalizationMenu = [Resources.Img.MenuExtraButtonsEn];

        public sealed class TouchBaseElement : BaseElement
        {
            public override bool OnTouchDownXY(float tx, float ty)
            {
                _ = base.OnTouchDownXY(tx, ty);
                CTRRectangle r = MakeRectangle(drawX + bbc.x, drawY + bbc.y, width + bbc.w, height + bbc.h);
                CTRRectangle rectangle = RectInRectIntersection(MakeRectangle(0.0, 0.0, SCREEN_WIDTH, SCREEN_HEIGHT), r);
                if (PointInRect(tx, ty, r.x, r.y, r.w, r.h) && rectangle.w > r.w / 2.0)
                {
                    delegateValue.OnButtonPressed(bid);
                    return true;
                }
                return false;
            }

            public MenuButtonId bid;

            public CTRRectangle bbc;

            public IButtonDelegation delegateValue;
        }

        public sealed class MonsterSlot : Image
        {
            public static MonsterSlot MonsterSlot_create(CTRTexture2D t)
            {
                return (MonsterSlot)new MonsterSlot().InitWithTexture(t);
            }

            public static MonsterSlot MonsterSlot_createWithResID(int r)
            {
                return MonsterSlot_create(Application.GetTexture(ResourceNameTranslator.TranslateLegacyId(r)));
            }

            public static MonsterSlot MonsterSlot_createWithResID(string resourceName)
            {
                return MonsterSlot_create(Application.GetTexture(resourceName));
            }

            public static MonsterSlot MonsterSlot_createWithResIDQuad(int r, int q)
            {
                MonsterSlot monsterSlot = MonsterSlot_create(Application.GetTexture(ResourceNameTranslator.TranslateLegacyId(r)));
                monsterSlot.SetDrawQuad(q);
                return monsterSlot;
            }

            public static MonsterSlot MonsterSlot_createWithResIDQuad(string resourceName, int q)
            {
                MonsterSlot monsterSlot = MonsterSlot_create(Application.GetTexture(resourceName));
                monsterSlot.SetDrawQuad(q);
                return monsterSlot;
            }

            public override void Draw()
            {
                PreDraw();
                if (quadToDraw == -1)
                {
                    GLDrawer.DrawImage(texture, drawX, drawY);
                }
                else
                {
                    DrawQuad(quadToDraw);
                }
                float num = c.GetScroll().x;
                Vector preCutSize = Application.GetTexture(Resources.Img.MenuPackSelection).preCutSize;
                if (num >= s && num < e)
                {
                    num -= preCutSize.x + -20f;
                    float num2 = num - ((s + e) / 2f);
                    OpenGL.SetScissorRectangle(250.0 - (double)num2, 0.0, 200.0, SCREEN_HEIGHT);
                    PostDraw();
                    OpenGL.SetScissorRectangle(c.drawX, c.drawY, c.width, c.height);
                }
            }

            public ScrollableContainer c;

            public float s;

            public float e;
        }
    }
}
