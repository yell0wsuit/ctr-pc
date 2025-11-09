using CutTheRope.ctr_commons;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.media;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using CutTheRope.windows;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace CutTheRope.game
{
    internal class MenuController : ViewController, ButtonDelegate, MovieMgrDelegate, ScrollableContainerProtocol, TimelineDelegate
    {
        public static Button createButtonWithTextIDDelegate(NSString str, int bid, ButtonDelegate d)
        {
            Image image = Image.Image_createWithResIDQuad(2, 0);
            Image image2 = Image.Image_createWithResIDQuad(2, 1);
            FontGeneric font = Application.getFont(3);
            Text text = new Text().initWithFont(font);
            text.setString(str);
            Text text2 = new Text().initWithFont(font);
            text2.setString(str);
            text.anchor = text.parentAnchor = 18;
            text2.anchor = text2.parentAnchor = 18;
            image.addChild(text);
            image2.addChild(text2);
            Button button = new Button().initWithUpElementDownElementandID(image, image2, bid);
            button.setTouchIncreaseLeftRightTopBottom(15.0, 15.0, 15.0, 15.0);
            button.delegateButtonDelegate = d;
            return button;
        }

        public static Button createShortButtonWithTextIDDelegate(NSString str, int bid, ButtonDelegate d)
        {
            Image image = Image.Image_createWithResIDQuad(61, 1);
            Image image2 = Image.Image_createWithResIDQuad(61, 0);
            FontGeneric font = Application.getFont(3);
            Text text = new Text().initWithFont(font);
            text.setString(str);
            Text text2 = new Text().initWithFont(font);
            text2.setString(str);
            text.anchor = text.parentAnchor = 18;
            text2.anchor = text2.parentAnchor = 18;
            image.addChild(text);
            image2.addChild(text2);
            Button button = new Button().initWithUpElementDownElementandID(image, image2, bid);
            button.setTouchIncreaseLeftRightTopBottom(15.0, 15.0, 15.0, 15.0);
            button.delegateButtonDelegate = d;
            return button;
        }

        public static ToggleButton createToggleButtonWithText1Text2IDDelegate(NSString str1, NSString str2, int bid, ButtonDelegate d)
        {
            Image image = Image.Image_createWithResIDQuad(2, 0);
            Image image2 = Image.Image_createWithResIDQuad(2, 1);
            Image image3 = Image.Image_createWithResIDQuad(2, 0);
            Image image4 = Image.Image_createWithResIDQuad(2, 1);
            FontGeneric font = Application.getFont(3);
            Text text = new Text().initWithFont(font);
            text.setString(str1);
            Text text2 = new Text().initWithFont(font);
            text2.setString(str1);
            Text text3 = new Text().initWithFont(font);
            text3.setString(str2);
            Text text4 = new Text().initWithFont(font);
            text4.setString(str2);
            text.anchor = text.parentAnchor = 18;
            text2.anchor = text2.parentAnchor = 18;
            text3.anchor = text3.parentAnchor = 18;
            text4.anchor = text4.parentAnchor = 18;
            image.addChild(text);
            image2.addChild(text2);
            image3.addChild(text3);
            image4.addChild(text4);
            ToggleButton toggleButton = new ToggleButton().initWithUpElement1DownElement1UpElement2DownElement2andID(image, image2, image3, image4, bid);
            toggleButton.setTouchIncreaseLeftRightTopBottom(10.0, 10.0, 10.0, 10.0);
            toggleButton.delegateButtonDelegate = d;
            return toggleButton;
        }

        public static Button createBackButtonWithDelegateID(ButtonDelegate d, int bid)
        {
            Button button = createButtonWithImageQuad1Quad2IDDelegate(54, 0, 1, bid, d);
            button.anchor = button.parentAnchor = 33;
            return button;
        }

        public static Button createButtonWithImageIDDelegate(int resID, int bid, ButtonDelegate d)
        {
            CTRTexture2D texture = Application.getTexture(resID);
            Image up = Image.Image_create(texture);
            Image image = Image.Image_create(texture);
            image.scaleX = 1.2f;
            image.scaleY = 1.2f;
            Button button = new Button().initWithUpElementDownElementandID(up, image, bid);
            button.setTouchIncreaseLeftRightTopBottom(10.0, 10.0, 10.0, 10.0);
            button.delegateButtonDelegate = d;
            return button;
        }

        public static Button createButton2WithImageQuad1Quad2IDDelegate(int res, int q1, int q2, int bid, ButtonDelegate d)
        {
            Image up = Image.Image_createWithResIDQuad(res, q1);
            Image image = Image.Image_createWithResIDQuad(res, q2);
            Vector relativeQuadOffset = Image.getRelativeQuadOffset(res, q2, q1);
            image.x -= relativeQuadOffset.x;
            image.y -= relativeQuadOffset.y;
            Button button = new Button().initWithUpElementDownElementandID(up, image, bid);
            button.delegateButtonDelegate = d;
            return button;
        }

        public static Button createButtonWithImageQuad1Quad2IDDelegate(int res, int q1, int q2, int bid, ButtonDelegate d)
        {
            Image image = Image.Image_createWithResIDQuad(res, q1);
            Image image2 = Image.Image_createWithResIDQuad(res, q2);
            image.doRestoreCutTransparency();
            image2.doRestoreCutTransparency();
            Button button = new Button().initWithUpElementDownElementandID(image, image2, bid);
            button.delegateButtonDelegate = d;
            CTRTexture2D texture = Application.getTexture(res);
            button.forceTouchRect(MakeRectangle(texture.quadOffsets[q1].x, texture.quadOffsets[q1].y, texture.quadRects[q1].w, texture.quadRects[q1].h));
            return button;
        }

        public static BaseElement createBackgroundWithLogowithShadow(bool l, bool s)
        {
            BaseElement baseElement = (BaseElement)new BaseElement().init();
            baseElement.width = (int)SCREEN_WIDTH;
            baseElement.height = (int)SCREEN_HEIGHT;
            Image image = Image.Image_createWithResIDQuad(48, 0);
            image.anchor = image.parentAnchor = 34;
            image.scaleX = image.scaleY = 1.25f;
            image.rotationCenterY = (float)(image.height / 2);
            image.passTransformationsToChilds = false;
            baseElement.addChild(image);
            if (l)
            {
                Image image2 = Image.Image_createWithResIDQuad(48, 1);
                image2.anchor = image2.parentAnchor = 34;
                image2.scaleX = image2.scaleY = 1.25f;
                image2.passTransformationsToChilds = false;
                image2.rotationCenterY = (float)(image2.height / 2);
                image.addChild(image2);
                Image image3 = Image.Image_createWithResIDQuad(50, 0);
                image3.anchor = 10;
                image3.parentAnchor = 10;
                image3.y = 55f;
                baseElement.addChild(image3);
            }
            if (s)
            {
                Image image4 = Image.Image_createWithResIDQuad(60, 0);
                image4.anchor = image4.parentAnchor = 18;
                image4.scaleX = image4.scaleY = 2f;
                Timeline timeline = new Timeline().initWithMaxKeyFramesOnTrack(3);
                timeline.addKeyFrame(KeyFrame.makeRotation(45.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makeRotation(405.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 75.0));
                timeline.setTimelineLoopType(Timeline.LoopType.TIMELINE_REPLAY);
                image4.addTimeline(timeline);
                image4.playTimeline(0);
                baseElement.addChild(image4);
            }
            return baseElement;
        }

        public static BaseElement createBackgroundWithLogo(bool l)
        {
            return createBackgroundWithLogowithShadow(l, true);
        }

        public static Image createAudioElementForQuadwithCrosspressediconOffset(int q, bool b, bool p, Vector offset)
        {
            int num = p ? 1 : 0;
            Image image = Image.Image_createWithResIDQuad(8, num);
            Image image2 = Image.Image_createWithResIDQuad(8, q);
            Image.setElementPositionWithRelativeQuadOffset(image2, 8, num, q);
            image2.parentAnchor = image2.anchor = 9;
            image2.x += offset.x;
            image2.y += offset.y;
            image.addChild(image2);
            if (b)
            {
                image2.color = RGBAColor.MakeRGBA(0.5f, 0.5f, 0.5f, 0.5f);
                Image image3 = Image.Image_createWithResIDQuad(8, 4);
                image3.parentAnchor = image3.anchor = 9;
                Image.setElementPositionWithRelativeQuadOffset(image3, 8, num, 4);
                image.addChild(image3);
            }
            return image;
        }

        public static ToggleButton createAudioButtonWithQuadDelegateIDiconOffset(int q, ButtonDelegate delegateValue, int bid, Vector offset)
        {
            Image u = createAudioElementForQuadwithCrosspressediconOffset(q, false, false, offset);
            Image d = createAudioElementForQuadwithCrosspressediconOffset(q, false, true, offset);
            Image u2 = createAudioElementForQuadwithCrosspressediconOffset(q, true, false, offset);
            Image d2 = createAudioElementForQuadwithCrosspressediconOffset(q, true, true, offset);
            ToggleButton toggleButton = new ToggleButton().initWithUpElement1DownElement1UpElement2DownElement2andID(u, d, u2, d2, bid);
            toggleButton.delegateButtonDelegate = delegateValue;
            return toggleButton;
        }

        public static Button createLanguageButtonWithIDDelegate(int bid, ButtonDelegate d)
        {
            NSString @string = Application.sharedAppSettings().getString(8);
            int q = 7;
            if (@string.isEqualToString("ru"))
            {
                q = 4;
            }
            else if (@string.isEqualToString("de"))
            {
                q = 5;
            }
            else if (@string.isEqualToString("fr"))
            {
                q = 6;
            }
            NSString string2 = Application.getString(655370);
            Image image = Image.Image_createWithResIDQuad(2, 0);
            Image image2 = Image.Image_createWithResIDQuad(2, 1);
            FontGeneric font = Application.getFont(3);
            Text text = new Text().initWithFont(font);
            text.setString(string2);
            Text text2 = new Text().initWithFont(font);
            text2.setString(string2);
            text.anchor = text.parentAnchor = 18;
            text2.anchor = text2.parentAnchor = 18;
            image.addChild(text);
            image2.addChild(text2);
            Image image3 = Image.Image_createWithResIDQuad(54, q);
            Image image4 = Image.Image_createWithResIDQuad(54, q);
            image4.parentAnchor = image3.parentAnchor = 20;
            image4.anchor = image3.anchor = 20;
            text.addChild(image3);
            text2.addChild(image4);
            text.width += (int)((float)image3.width + RTPD(10.0));
            text2.width += (int)((float)image4.width + RTPD(10.0));
            Button button = new Button().initWithUpElementDownElementandID(image, image2, bid);
            button.setTouchIncreaseLeftRightTopBottom(15.0, 15.0, 15.0, 15.0);
            button.delegateButtonDelegate = d;
            return button;
        }

        public static BaseElement createElementWithResIdquad(int resId, int quad)
        {
            if (resId != -1 && quad != -1)
            {
                return Image.Image_createWithResIDQuad(resId, quad);
            }
            return (BaseElement)new BaseElement().init();
        }

        public static ToggleButton createToggleButtonWithResquadquad2buttonIDdelegate(int res, int quad, int quad2, int bId, ButtonDelegate delegateValue)
        {
            BaseElement baseElement = createElementWithResIdquad(res, quad);
            BaseElement baseElement2 = createElementWithResIdquad(res, quad);
            BaseElement baseElement3 = createElementWithResIdquad(res, quad2);
            BaseElement baseElement4 = createElementWithResIdquad(res, quad2);
            int width = MAX(baseElement.width, baseElement3.width);
            int height = MAX(baseElement.height, baseElement3.height);
            baseElement.width = baseElement2.width = width;
            baseElement.height = baseElement2.height = height;
            baseElement3.width = baseElement4.width = width;
            baseElement3.height = baseElement4.height = height;
            baseElement2.scaleX = baseElement2.scaleY = baseElement4.scaleX = baseElement4.scaleY = 1.2f;
            ToggleButton toggleButton = new ToggleButton().initWithUpElement1DownElement1UpElement2DownElement2andID(baseElement, baseElement2, baseElement3, baseElement4, bId);
            toggleButton.delegateButtonDelegate = delegateValue;
            return toggleButton;
        }

        public static BaseElement createControlButtontitleAnchortextbuttonIDdelegate(int q, int tq, NSString str, int bId, ButtonDelegate delegateValue)
        {
            Image image = Image.Image_createWithResIDQuad(8, q);
            Text text = Text.createWithFontandString(4, str);
            text.parentAnchor = 9;
            text.anchor = 18;
            text.scaleX = text.scaleY = 0.75f;
            image.addChild(text);
            Image.setElementPositionWithRelativeQuadOffset(text, 8, q, tq);
            if (bId != -1)
            {
                ToggleButton toggleButton = createToggleButtonWithResquadquad2buttonIDdelegate(8, -1, 8, bId, delegateValue);
                toggleButton.setName("button");
                toggleButton.parentAnchor = 9;
                Image.setElementPositionWithRelativeQuadOffset(toggleButton, 8, q, 8);
                image.addChild(toggleButton);
                int num = image.width / 2 - toggleButton.width / 2;
                toggleButton.setTouchIncreaseLeftRightTopBottom((double)num, (double)num, (double)image.height * 0.85, 0.0);
            }
            else
            {
                Image image2 = Image.Image_createWithResIDQuad(8, 7);
                image2.parentAnchor = 9;
                Image.setElementPositionWithRelativeQuadOffset(image2, 8, q, 7);
                image.addChild(image2);
            }
            return image;
        }

        public static Image createBlankScoresButtonWithIconpressed(int quad, bool pressed)
        {
            Image image3 = Image.Image_createWithResIDQuad(59, pressed ? 1 : 0);
            Image image2 = Image.Image_createWithResIDQuad(59, quad);
            image3.addChild(image2);
            image2.parentAnchor = 9;
            Image.setElementPositionWithRelativeQuadOffset(image2, 59, 0, quad);
            return image3;
        }

        public static Button createScoresButtonWithIconbuttonIDdelegate(int quad, int bId, ButtonDelegate delegateValue)
        {
            Image up = createBlankScoresButtonWithIconpressed(quad, false);
            Image image = createBlankScoresButtonWithIconpressed(quad, true);
            Image.setElementPositionWithRelativeQuadOffset(image, 59, 0, 1);
            Button button = new Button().initWithUpElementDownElementandID(up, image, bId);
            button.delegateButtonDelegate = delegateValue;
            return button;
        }

        public virtual void createMainMenu()
        {
            MenuView menuView = (MenuView)new MenuView().initFullscreen();
            BaseElement baseElement = createBackgroundWithLogo(true);
            VBox vBox = new VBox().initWithOffsetAlignWidth(5.0, 2, (double)SCREEN_WIDTH);
            vBox.anchor = vBox.parentAnchor = 34;
            vBox.y = -85f;
            Button c = createButtonWithTextIDDelegate(Application.getString(655360), 0, this);
            vBox.addChild(c);
            Button c2 = createButtonWithTextIDDelegate(Application.getString(655361), 1, this);
            vBox.addChild(c2);
            Button c3 = createButtonWithTextIDDelegate(Application.getString(655506), 41, this);
            vBox.addChild(c3);
            baseElement.addChild(vBox);
            bool flag = Application.getString(655507).length() > 0;
            if (flag)
            {
                BaseElement baseElement2 = (BaseElement)new BaseElement().init();
                baseElement2.setName("container");
                baseElement2.parentAnchor = baseElement2.anchor = 18;
                baseElement2.width = baseElement.width;
                baseElement2.height = baseElement.height;
                baseElement2.x -= (float)canvas.xOffsetScaled;
                baseElement.addChild(baseElement2);
                CTRTexture2D texture = Application.getTexture(54);
                Button button = createButton2WithImageQuad1Quad2IDDelegate(54, 3, 3, 16, this);
                button.anchor = 9;
                button.parentAnchor = 36;
                Image.setElementPositionWithQuadOffset(button, 54, 3);
                button.x -= texture.preCutSize.x;
                button.y -= texture.preCutSize.y;
                baseElement2.addChild(button);
                Button button2 = createButton2WithImageQuad1Quad2IDDelegate(54, 2, 2, 17, this);
                button2.anchor = 9;
                button2.parentAnchor = 36;
                Image.setElementPositionWithQuadOffset(button2, 54, 2);
                button2.x -= texture.preCutSize.x;
                button2.y -= texture.preCutSize.y;
                if (flag)
                {
                    baseElement2.addChild(button2);
                }
                Image image = Image.Image_createWithResIDQuad(149, 0);
                image.anchor = 9;
                image.parentAnchor = 36;
                Image.setElementPositionWithQuadOffset(image, 149, 0);
                image.x -= texture.preCutSize.x;
                image.y -= texture.preCutSize.y;
                baseElement2.addChild(image);
            }
            menuView.addChild(baseElement);
            addViewwithID(menuView, 0);
        }

        public virtual void createOptions()
        {
            MenuView menuView = (MenuView)new MenuView().initFullscreen();
            BaseElement baseElement = createBackgroundWithLogowithShadow(false, false);
            menuView.addChild(baseElement);
            BaseElement baseElement2 = createControlButtontitleAnchortextbuttonIDdelegate(5, 10, Application.getString(655423), -1, null);
            BaseElement baseElement3 = createControlButtontitleAnchortextbuttonIDdelegate(6, 9, Application.getString(655422), 11, this);
            HBox hBox = new HBox().initWithOffsetAlignHeight(RTPD(80.0), 16, (float)MAX(baseElement2.height, baseElement3.height));
            hBox.parentAnchor = hBox.anchor = 18;
            hBox.addChild(baseElement2);
            hBox.addChild(baseElement3);
            menuView.addChild(hBox);
            Image image = Image.Image_createWithResIDQuad(60, 0);
            image.anchor = image.parentAnchor = 18;
            image.scaleX = image.scaleY = 2f;
            Timeline timeline = new Timeline().initWithMaxKeyFramesOnTrack(3);
            timeline.addKeyFrame(KeyFrame.makeRotation(45.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.addKeyFrame(KeyFrame.makeRotation(405.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 75.0));
            timeline.setTimelineLoopType(Timeline.LoopType.TIMELINE_REPLAY);
            image.addTimeline(timeline);
            image.playTimeline(0);
            menuView.addChild(image);
            VBox vBox = new VBox().initWithOffsetAlignWidth(5f, 2, SCREEN_WIDTH);
            vBox.anchor = vBox.parentAnchor = 18;
            Vector offset = vectSub(Image.getQuadCenter(8, 0), Image.getQuadOffset(8, 12));
            ToggleButton toggleButton = createAudioButtonWithQuadDelegateIDiconOffset(3, this, 6, vectZero);
            ToggleButton toggleButton2 = createAudioButtonWithQuadDelegateIDiconOffset(2, this, 5, offset);
            HBox hBox2 = new HBox().initWithOffsetAlignHeight(-10f, 16, (float)toggleButton.height);
            hBox2.addChild(toggleButton2);
            hBox2.addChild(toggleButton);
            vBox.addChild(hBox2);
            Button c = createLanguageButtonWithIDDelegate(22, this);
            vBox.addChild(c);
            Button c2 = createButtonWithTextIDDelegate(Application.getString(655364), 8, this);
            vBox.addChild(c2);
            Button c3 = createButtonWithTextIDDelegate(Application.getString(655365), 7, this);
            vBox.addChild(c3);
            baseElement.addChild(vBox);
            hBox.y = (float)(vBox.height / 2 + 10);
            vBox.y = (float)(-(float)hBox.height / 2);
            bool flag4 = Preferences._getBooleanForKey("SOUND_ON");
            bool flag2 = Preferences._getBooleanForKey("MUSIC_ON");
            bool flag3 = Preferences._getBooleanForKey("PREFS_CLICK_TO_CUT");
            if (!flag4)
            {
                toggleButton2.toggle();
            }
            if (!flag2)
            {
                toggleButton.toggle();
            }
            ToggleButton toggleButton3 = (ToggleButton)baseElement3.getChildWithName("button");
            if (flag3 && toggleButton3 != null)
            {
                toggleButton3.toggle();
            }
            Button button = createBackButtonWithDelegateID(this, 36);
            button.setName("backb");
            button.x = (float)canvas.xOffsetScaled;
            menuView.addChild(button);
            addViewwithID(menuView, 1);
        }

        public virtual void createReset()
        {
            MenuView menuView = (MenuView)new MenuView().initFullscreen();
            BaseElement baseElement = createBackgroundWithLogo(false);
            Text text = new Text().initWithFont(Application.getFont(3));
            text.setAlignment(2);
            text.setStringandWidth(Application.getString(655371), (double)Global.ScreenSizeManager.CurrentSize.Width * 0.95);
            text.anchor = text.parentAnchor = 18;
            baseElement.addChild(text);
            text.y = -200f;
            Button button = createButtonWithTextIDDelegate(Application.getString(655382), 13, this);
            button.anchor = button.parentAnchor = 34;
            button.y = -540f;
            Button button2 = createButtonWithTextIDDelegate(Application.getString(655383), 14, this);
            button2.anchor = button2.parentAnchor = 34;
            button2.y = -320f;
            baseElement.addChild(button);
            baseElement.addChild(button2);
            menuView.addChild(baseElement);
            Button button3 = createBackButtonWithDelegateID(this, 10);
            button3.setName("backb");
            button3.x = (float)canvas.xOffsetScaled;
            menuView.addChild(button3);
            addViewwithID(menuView, 4);
        }

        public virtual void createMovieView()
        {
            MovieView movieView = (MovieView)new MovieView().initFullscreen();
            RectangleElement rectangleElement = (RectangleElement)new RectangleElement().init();
            rectangleElement.width = (int)SCREEN_WIDTH;
            rectangleElement.height = (int)SCREEN_HEIGHT;
            rectangleElement.color = RGBAColor.blackRGBA;
            movieView.addChild(rectangleElement);
            addViewwithID(movieView, 7);
        }

        public virtual void createAbout()
        {
            MenuView menuView = (MenuView)new MenuView().initFullscreen();
            BaseElement baseElement = createBackgroundWithLogo(false);
            string text = Application.getString(655416).ToString();
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
            VBox vBox = new VBox().initWithOffsetAlignWidth(0f, 2, num);
            BaseElement baseElement2 = (BaseElement)new BaseElement().init();
            baseElement2.width = (int)num;
            baseElement2.height = 100;
            vBox.addChild(baseElement2);
            Image c = Image.Image_createWithResIDQuad(50, 1);
            vBox.addChild(c);
            Text text2 = new Text().initWithFont(Application.getFont(4));
            text2.setAlignment(2);
            text2.setStringandWidth(NSS(text), (float)(int)num);
            aboutContainer = new ScrollableContainer().initWithWidthHeightContainer(num, h, vBox);
            aboutContainer.anchor = aboutContainer.parentAnchor = 18;
            vBox.addChild(text2);
            Image c2 = Image.Image_createWithResIDQuad(50, 2);
            vBox.addChild(c2);
            NSString @string = Application.getString(655417);
            Text text3 = new Text().initWithFont(Application.getFont(4));
            text3.setAlignment(2);
            text3.setStringandWidth(@string, num);
            vBox.addChild(text3);
            baseElement.addChild(aboutContainer);
            menuView.addChild(baseElement);
            Button button = createBackButtonWithDelegateID(this, 10);
            button.setName("backb");
            button.x = (float)canvas.xOffsetScaled;
            menuView.addChild(button);
            addViewwithID(menuView, 3);
        }

        public virtual HBox createTextWithStar(NSString t)
        {
            HBox hbox = new HBox().initWithOffsetAlignHeight(0.0, 16, (double)RTD(50.0));
            Text text = new Text().initWithFont(Application.getFont(3));
            text.setString(t);
            text.scaleX = text.scaleY = 0.7f;
            text.rotationCenterX = (float)(-(float)text.width / 2);
            text.width = (int)((float)text.width * 0.7f);
            hbox.addChild(text);
            Image c = Image.Image_createWithResIDQuad(52, 3);
            hbox.addChild(c);
            return hbox;
        }

        public virtual float getBoxWidth()
        {
            return Image.getQuadSize(52, 4).x + Image.getQuadOffset(52, 4).x * 2f;
        }

        public virtual float getPackOffset()
        {
            float num = SCREEN_WIDTH + (float)(canvas.xOffset * 2);
            float boxWidth = getBoxWidth();
            if (boxWidth * 3f > num - 200f)
            {
                return boxWidth / 2f;
            }
            return 0f;
        }

        public virtual BaseElement createPackElementforContainer(int n, ScrollableContainer c)
        {
            TouchBaseElement touchBaseElement = (TouchBaseElement)new TouchBaseElement().init();
            touchBaseElement.delegateValue = this;
            BaseElement baseElement = (BaseElement)new BaseElement().init();
            baseElement.setName("boxContainer");
            baseElement.anchor = baseElement.parentAnchor = 12;
            touchBaseElement.addChild(baseElement);
            int totalStars = CTRPreferences.getTotalStars();
            if (n > 0 && n < CTRPreferences.getPacksCount() && CTRPreferences.getUnlockedForPackLevel(n, 0) == UNLOCKED_STATE.UNLOCKED_STATE_LOCKED && totalStars >= CTRPreferences.packUnlockStars(n))
            {
                CTRPreferences.setUnlockedForPackLevel(UNLOCKED_STATE.UNLOCKED_STATE_JUST_UNLOCKED, n, 0);
            }
            int r = 52;
            int q = 4 + n;
            if (n > 6)
            {
                r = 53;
                q = n - 6;
            }
            NSString nsstring;
            if (n == CTRPreferences.getPacksCount())
            {
                nsstring = Application.getString(655415);
            }
            else
            {
                string text3 = (n + 1).ToString();
                string text4 = ". ";
                NSString @string = Application.getString(655404 + n);
                nsstring = NSS(text3 + text4 + (@string?.ToString()));
            }
            NSString nSString = nsstring;
            UNLOCKED_STATE unlockedForPackLevel = CTRPreferences.getUnlockedForPackLevel(n, 0);
            bool flag = unlockedForPackLevel == UNLOCKED_STATE.UNLOCKED_STATE_LOCKED && n != CTRPreferences.getPacksCount();
            touchBaseElement.bid = 23 + n;
            Image image = Image.Image_createWithResIDQuad(r, q);
            image.doRestoreCutTransparency();
            image.anchor = image.parentAnchor = 9;
            if (flag)
            {
                baseElement.addChild(image);
                int num = CTRPreferences.packUnlockStars(n);
                Image image2 = Image.Image_createWithResIDQuad(52, 2);
                image2.doRestoreCutTransparency();
                image2.anchor = image2.parentAnchor = 9;
                image.addChild(image2);
                HBox hBox = createTextWithStar(NSS(num.ToString()));
                hBox.anchor = hBox.parentAnchor = 18;
                hBox.y = 110f;
                image2.addChild(hBox);
                Text text = new Text().initWithFont(Application.getFont(4));
                NSString newString = NSS(Application.getString(655390).ToString().Replace("%d", num.ToString()));
                text.setAlignment(2);
                text.anchor = 10;
                text.parentAnchor = 34;
                text.setStringandWidth(newString, 700f);
                text.y = -70f;
                text.scaleX = text.scaleY = 0.7f;
                text.rotationCenterY = (float)(-(float)text.height / 2);
                touchBaseElement.addChild(text);
            }
            else
            {
                if (n != CTRPreferences.getPacksCount())
                {
                    int q2 = 0;
                    int q3 = 1;
                    MonsterSlot monsterSlot = MonsterSlot.MonsterSlot_createWithResIDQuad(52, q2);
                    monsterSlot.c = c;
                    monsterSlot.doRestoreCutTransparency();
                    monsterSlot.anchor = 9;
                    monsterSlot.parentAnchor = 9;
                    monsterSlot.y = image.y;
                    baseElement.addChild(monsterSlot);
                    Image image3 = Image.Image_createWithResIDQuad(52, q3);
                    image3.doRestoreCutTransparency();
                    image3.anchor = 17;
                    monsterSlot.s = (float)(image.width * (n - 1)) + -20f * (float)n + packContainer.x + 50f;
                    monsterSlot.e = monsterSlot.s + 1200f;
                    image3.x = packContainer.x - 0f + (float)monsterSlot.width + -20f - getPackOffset();
                    image3.y = packContainer.y + SCREEN_HEIGHT / 2f;
                    image3.parentAnchor = -1;
                    monsterSlot.addChild(image3);
                }
                baseElement.addChild(image);
                if (unlockedForPackLevel == UNLOCKED_STATE.UNLOCKED_STATE_JUST_UNLOCKED)
                {
                    Image image4 = Image.Image_createWithResIDQuad(52, 2);
                    image4.setName("lockHideMe");
                    image4.doRestoreCutTransparency();
                    image4.anchor = image4.parentAnchor = 9;
                    baseElement.addChild(image4);
                    Timeline timeline = new Timeline().initWithMaxKeyFramesOnTrack(3);
                    timeline.addKeyFrame(KeyFrame.makeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                    timeline.addKeyFrame(KeyFrame.makeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1.5));
                    timeline.addKeyFrame(KeyFrame.makeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                    timeline.addKeyFrame(KeyFrame.makeScale(2.0, 2.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1.5));
                    image4.addTimeline(timeline);
                }
            }
            Text text2 = new Text().initWithFont(Application.getFont(3));
            text2.anchor = text2.parentAnchor = 10;
            text2.scaleX = text2.scaleY = 0.75f;
            if (LANGUAGE == Language.LANG_DE || LANGUAGE == Language.LANG_EN)
            {
                text2.scaleX = 0.7f;
            }
            text2.setAlignment(2);
            if (n != CTRPreferences.getPacksCount())
            {
                text2.setString(nSString);
            }
            else
            {
                text2.setStringandWidth(nSString, 656.0);
            }
            text2.y = 120f;
            image.addChild(text2);
            Timeline timeline2 = new Timeline().initWithMaxKeyFramesOnTrack(4);
            timeline2.addKeyFrame(KeyFrame.makeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline2.addKeyFrame(KeyFrame.makeScale(0.95, 1.05, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.15));
            timeline2.addKeyFrame(KeyFrame.makeScale(1.05, 0.95, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.2));
            timeline2.addKeyFrame(KeyFrame.makeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.25));
            baseElement.addTimeline(timeline2);
            baseElement.height = touchBaseElement.height = image.height;
            baseElement.width = touchBaseElement.width = image.width;
            return touchBaseElement;
        }

        public virtual void createPackSelect()
        {
            MenuView menuView = (MenuView)new MenuView().initFullscreen();
            BaseElement baseElement = createBackgroundWithLogo(false);
            string text = Application.getString(655388).ToString();
            text = text.Replace("%d", "");
            HBox hBox = createTextWithStar(NSS(text + CTRPreferences.getTotalStars().ToString()));
            hBox.x = -30f - (float)canvas.xOffsetScaled;
            hBox.y = 40f;
            hBox.setName("text");
            HBox hBox2 = new HBox().initWithOffsetAlignHeight(-20f, 16, SCREEN_HEIGHT);
            float num = SCREEN_WIDTH + (float)(canvas.xOffset * 2);
            float boxWidth = getBoxWidth();
            float num2 = boxWidth * 3f;
            if (num2 > num - 200f)
            {
                num2 = boxWidth * 2f;
            }
            packContainer = new ScrollableContainer().initWithWidthHeightContainer(num2, SCREEN_HEIGHT, hBox2);
            packContainer.minAutoScrollToSpointLength = RTD(5.0);
            packContainer.shouldBounceHorizontally = true;
            packContainer.resetScrollOnShow = false;
            packContainer.dontHandleTouchDownsHandledByChilds = true;
            packContainer.dontHandleTouchMovesHandledByChilds = true;
            packContainer.dontHandleTouchUpsHandledByChilds = true;
            packContainer.turnScrollPointsOnWithCapacity(CTRPreferences.getPacksCount() + 2);
            packContainer.delegateScrollableContainerProtocol = this;
            packContainer.x = SCREEN_WIDTH / 2f - (float)(packContainer.width / 2);
            hBox.anchor = hBox.parentAnchor = 12;
            baseElement.addChild(hBox);
            CTRTexture2D texture = Application.getTexture(52);
            BaseElement baseElement2 = (BaseElement)new BaseElement().init();
            baseElement2.width = (int)texture.preCutSize.x;
            baseElement2.height = (int)texture.preCutSize.y;
            hBox2.addChild(baseElement2);
            float num3 = 0f + getPackOffset();
            for (int i = 0; i < CTRPreferences.getPacksCount() + 1; i++)
            {
                TouchBaseElement touchBaseElement = (TouchBaseElement)createPackElementforContainer(i, packContainer);
                boxes[i] = touchBaseElement;
                hBox2.addChild(touchBaseElement);
                touchBaseElement.x -= 0f;
                touchBaseElement.y -= 0f;
                packContainer.addScrollPointAtXY((double)num3, 0.0);
                touchBaseElement.bbc = MakeRectangle(0f, 0f, -20f, 0f);
                num3 += (float)touchBaseElement.width + -20f;
            }
            hBox2.width += 1000;
            Image image = Image.Image_createWithResIDQuad(52, 11);
            image.anchor = 17;
            image.y += SCREEN_HEIGHT / 2f;
            image.x = packContainer.x - 2f;
            baseElement.addChild(image);
            Image image2 = Image.Image_createWithResIDQuad(52, 11);
            image2.anchor = 20;
            image2.y += SCREEN_HEIGHT / 2f;
            image2.x = packContainer.x + (float)packContainer.width + 2f;
            baseElement.addChild(image2);
            image2.scaleX = image2.scaleY = -1f;
            baseElement.addChild(packContainer);
            Image image3 = Image.Image_createWithResIDQuad(52, 12);
            image3.anchor = 20;
            image3.y += SCREEN_HEIGHT / 2f;
            image3.x = packContainer.x + 3f;
            baseElement.addChild(image3);
            Image image4 = Image.Image_createWithResIDQuad(52, 12);
            image4.anchor = 17;
            image4.y += SCREEN_HEIGHT / 2f;
            image4.x = packContainer.x + (float)packContainer.width - 3f;
            image4.scaleX = image4.scaleY = -1f;
            baseElement.addChild(image4);
            prevb = createButton2WithImageQuad1Quad2IDDelegate(52, 13, 14, 21, this);
            prevb.parentAnchor = 17;
            prevb.anchor = 20;
            prevb.x = packContainer.x - 40f;
            baseElement.addChild(prevb);
            nextb = createButton2WithImageQuad1Quad2IDDelegate(52, 13, 14, 20, this);
            nextb.anchor = nextb.parentAnchor = 17;
            nextb.x = packContainer.x + (float)packContainer.width + 40f;
            nextb.scaleX = -1f;
            baseElement.addChild(nextb);
            menuView.addChild(baseElement);
            addViewwithID(menuView, 5);
            Button button = createBackButtonWithDelegateID(this, 35);
            button.setName("backb");
            button.x = (float)canvas.xOffsetScaled;
            menuView.addChild(button);
            int lastPack = CTRPreferences.getLastPack();
            packContainer.placeToScrollPoint(lastPack);
            scrollableContainerchangedTargetScrollPoint(packContainer, lastPack);
        }

        public virtual void createLeaderboards()
        {
        }

        public virtual void createAchievements()
        {
        }

        public virtual void showCantUnlockPopup()
        {
            CTRRootController cTRRootController = (CTRRootController)Application.sharedRootController();
            Popup popup = (Popup)new Popup().init();
            popup.setName("popup");
            Image image = Image.Image_createWithResIDQuad(49, 0);
            image.doRestoreCutTransparency();
            popup.addChild(image);
            int num = 20;
            image.scaleX = 1.3f;
            Text text = new Text().initWithFont(Application.getFont(3));
            text.setAlignment(2);
            text.setString(Application.getString(655391));
            text.anchor = 18;
            Image.setElementPositionWithQuadOffset(text, 49, 1);
            text.y -= (float)num;
            popup.addChild(text);
            Text text2 = new Text().initWithFont(Application.getFont(3));
            text2.setAlignment(2);
            text2.setString(Application.getString(655392));
            text2.anchor = 18;
            Image.setElementPositionWithQuadOffset(text2, 49, 2);
            popup.addChild(text2);
            text2.y -= (float)num;
            Text text3 = new Text().initWithFont(Application.getFont(4));
            text3.setAlignment(2);
            text3.setStringandWidth(Application.getString(655393), 600f);
            text3.anchor = 18;
            Image.setElementPositionWithQuadOffset(text3, 49, 3);
            text3.y += 50f;
            popup.addChild(text3);
            int totalStars = CTRPreferences.getTotalStars();
            HBox hBox = createTextWithStar(NSS((CTRPreferences.packUnlockStars(cTRRootController.getPack() + 1) - totalStars).ToString()));
            hBox.anchor = 18;
            Image.setElementPositionWithQuadOffset(hBox, 49, 5);
            hBox.y -= (float)num;
            popup.addChild(hBox);
            Button button = createButtonWithTextIDDelegate(Application.getString(655389), 15, this);
            button.anchor = 18;
            Image.setElementPositionWithQuadOffset(button, 49, 4);
            popup.addChild(button);
            popup.showPopup();
            activeView().addChild(popup);
        }

        public virtual void showGameFinishedPopup()
        {
            Popup popup = (Popup)new Popup().init();
            popup.setName("popup");
            Image image = Image.Image_createWithResIDQuad(49, 0);
            image.doRestoreCutTransparency();
            popup.addChild(image);
            Text text = new Text().initWithFont(Application.getFont(3));
            text.setAlignment(2);
            text.setStringandWidth(Application.getString(655394), 600.0);
            text.anchor = 18;
            Image.setElementPositionWithQuadOffset(text, 49, 2);
            text.y -= 170f;
            image.addChild(text);
            Text text2 = new Text().initWithFont(Application.getFont(4));
            text2.setAlignment(2);
            text2.setStringandWidth(Application.getString(655395), 700.0);
            text2.anchor = 18;
            Image.setElementPositionWithQuadOffset(text2, 49, 3);
            text2.y += 30f;
            image.addChild(text2);
            Button button = createButtonWithTextIDDelegate(Application.getString(655389), 15, this);
            button.anchor = 18;
            Image.setElementPositionWithQuadOffset(button, 49, 4);
            image.addChild(button);
            popup.showPopup();
            activeView().addChild(popup);
        }

        public virtual void showYesNoPopup(NSString str, int buttonYesId, int buttonNoId)
        {
            Popup popup = (Popup)new Popup().init();
            popup.setName("popup");
            Image image = Image.Image_createWithResIDQuad(49, 0);
            image.doRestoreCutTransparency();
            popup.addChild(image);
            Text text = new Text().initWithFont(Application.getFont(3));
            text.setAlignment(2);
            text.setStringandWidth(str, 680.0);
            text.anchor = 18;
            Image.setElementPositionWithQuadOffset(text, 49, 2);
            text.y -= 120f;
            image.addChild(text);
            Button button = createButtonWithTextIDDelegate(Application.getString(655382), buttonYesId, this);
            button.anchor = 18;
            Image.setElementPositionWithQuadOffset(button, 49, 4);
            button.y -= (float)button.height;
            image.addChild(button);
            Button button2 = createButtonWithTextIDDelegate(Application.getString(655383), buttonNoId, this);
            button2.anchor = 18;
            Image.setElementPositionWithQuadOffset(button2, 49, 4);
            image.addChild(button2);
            popup.showPopup();
            ep = popup;
            activeView().addChild(popup);
        }

        public virtual void scrollableContainerreachedScrollPoint(ScrollableContainer e, int i)
        {
            currentPack = i;
            pack = i;
            scrollPacksLeft = 0;
            scrollPacksRight = 0;
            bScrolling = false;
            if (prevb.isEnabled())
            {
                prevb.setState(Button.BUTTON_STATE.BUTTON_UP);
            }
            if (nextb.isEnabled())
            {
                nextb.setState(Button.BUTTON_STATE.BUTTON_UP);
            }
            if (i == CTRPreferences.getPacksCount())
            {
                return;
            }
            boxes[i].getChildWithName("boxContainer").playTimeline(0);
            UNLOCKED_STATE unlockedForPackLevel = CTRPreferences.getUnlockedForPackLevel(i, 0);
            BaseElement childWithName = boxes[i].getChildWithName("lockHideMe");
            if (childWithName != null && unlockedForPackLevel == UNLOCKED_STATE.UNLOCKED_STATE_JUST_UNLOCKED)
            {
                CTRPreferences.setUnlockedForPackLevel(UNLOCKED_STATE.UNLOCKED_STATE_UNLOCKED, i, 0);
                childWithName.playTimeline(0);
            }
            CTRRootController cTRRootController = (CTRRootController)Application.sharedRootController();
            if (showNextPackStatus && i == cTRRootController.getPack() + 1)
            {
                showNextPackStatus = false;
                if (unlockedForPackLevel == UNLOCKED_STATE.UNLOCKED_STATE_LOCKED)
                {
                    showCantUnlockPopup();
                }
            }
        }

        public virtual void scrollableContainerchangedTargetScrollPoint(ScrollableContainer e, int i)
        {
            currentPack = i;
            pack = i;
            CTRPreferences.setLastPack(i);
        }

        public virtual BaseElement createButtonForLevelPack(int l, int p)
        {
            bool flag = CTRPreferences.getUnlockedForPackLevel(p, l) == UNLOCKED_STATE.UNLOCKED_STATE_LOCKED;
            int starsForPackLevel = CTRPreferences.getStarsForPackLevel(p, l);
            TouchBaseElement touchBaseElement = (TouchBaseElement)new TouchBaseElement().init();
            touchBaseElement.bbc = MakeRectangle(5.0, 0.0, -10.0, 0.0);
            touchBaseElement.delegateValue = this;
            Image image;
            if (flag)
            {
                touchBaseElement.bid = -1;
                image = Image.Image_createWithResIDQuad(51, 1);
                image.doRestoreCutTransparency();
            }
            else
            {
                touchBaseElement.bid = 1000 + l;
                image = Image.Image_createWithResIDQuad(51, 0);
                image.doRestoreCutTransparency();
                Text text = new Text().initWithFont(Application.getFont(3));
                NSString @string = NSS((l + 1).ToString());
                text.setString(@string);
                text.anchor = text.parentAnchor = 18;
                text.y -= 5f;
                image.addChild(text);
                Image image2 = Image.Image_createWithResIDQuad(51, 2 + starsForPackLevel);
                image2.doRestoreCutTransparency();
                image2.anchor = image2.parentAnchor = 9;
                image.addChild(image2);
            }
            image.anchor = image.parentAnchor = 18;
            touchBaseElement.addChild(image);
            touchBaseElement.setSizeToChildsBounds();
            return touchBaseElement;
        }

        public virtual void createLevelSelect()
        {
            float num = 0.3f;
            MenuView menuView = (MenuView)new MenuView().initFullscreen();
            int num4 = 126 + pack;
            Image image = Image.Image_createWithResIDQuad(num4, 0);
            Image image2 = Image.Image_createWithResIDQuad(num4, 0);
            Vector quadSize = Image.getQuadSize(num4, 0);
            float x = SCREEN_WIDTH / 2f - quadSize.x;
            image.x = x;
            image2.x = SCREEN_WIDTH / 2f;
            image2.rotation = 180f;
            image2.y -= 0.5f;
            Timeline timeline = new Timeline().initWithMaxKeyFramesOnTrack(3);
            timeline.addKeyFrame(KeyFrame.makeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.addKeyFrame(KeyFrame.makeColor(RGBAColor.MakeRGBA(0.85, 0.85, 0.85, 1.0), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, num));
            image.addTimeline(timeline);
            image.setName("levelsBack");
            image.addChild(image2);
            menuView.addChild(image);
            Image image3 = Image.Image_createWithResIDQuad(5, 0);
            Image image4 = Image.Image_createWithResIDQuad(5, 1);
            image3.x = Image.getQuadOffset(5, 0).x;
            image3.y = 80f;
            image4.x = Image.getQuadOffset(5, 1).x;
            image4.y = 80f;
            menuView.addChild(image3);
            menuView.addChild(image4);
            Image image5 = Image.Image_createWithResIDQuad(60, 0);
            image5.setName("shadow");
            image5.anchor = image5.parentAnchor = 18;
            image5.scaleX = image5.scaleY = 2f;
            Timeline timeline2 = new Timeline().initWithMaxKeyFramesOnTrack(2);
            timeline2.addKeyFrame(KeyFrame.makeScale(2.0, 2.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline2.addKeyFrame(KeyFrame.makeScale(5.0, 5.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, (double)num));
            timeline2.delegateTimelineDelegate = this;
            image5.addTimeline(timeline2);
            Timeline timeline3 = new Timeline().initWithMaxKeyFramesOnTrack(3);
            timeline3.addKeyFrame(KeyFrame.makeRotation(45.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline3.addKeyFrame(KeyFrame.makeRotation(405.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 75.0));
            timeline3.setTimelineLoopType(Timeline.LoopType.TIMELINE_REPLAY);
            image5.addTimeline(timeline3);
            image5.playTimeline(1);
            menuView.addChild(image5);
            HBox hBox = createTextWithStar(NSS(CTRPreferences.getTotalStarsInPack(pack).ToString() + "/" + (CTRPreferences.getLevelsInPackCount() * 3).ToString()));
            hBox.x = -20f;
            hBox.y = 20f;
            float of = 55f;
            float of2 = 10f;
            float h = 202.79999f;
            VBox vBox = new VBox().initWithOffsetAlignWidth(of, 2, SCREEN_WIDTH);
            vBox.setName("levelsBox");
            vBox.x = 0f;
            vBox.y = 110f;
            int num2 = 5;
            int num3 = 0;
            for (int i = 0; i < num2; i++)
            {
                HBox hBox2 = new HBox().initWithOffsetAlignHeight(of2, 16, h);
                for (int j = 0; j < num2; j++)
                {
                    hBox2.addChild(createButtonForLevelPack(num3++, pack));
                }
                vBox.addChild(hBox2);
            }
            Timeline timeline4 = new Timeline().initWithMaxKeyFramesOnTrack(3);
            timeline4.addKeyFrame(KeyFrame.makeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline4.addKeyFrame(KeyFrame.makeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, num));
            vBox.addTimeline(timeline4);
            hBox.anchor = hBox.parentAnchor = 12;
            hBox.setName("starText");
            hBox.x = (float)-(float)canvas.xOffsetScaled;
            Timeline timeline5 = new Timeline().initWithMaxKeyFramesOnTrack(2);
            timeline5.addKeyFrame(KeyFrame.makeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline5.addKeyFrame(KeyFrame.makeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, num));
            hBox.addTimeline(timeline5);
            menuView.addChild(hBox);
            menuView.addChild(vBox);
            Button button = createBackButtonWithDelegateID(this, 12);
            button.setName("backButton");
            Timeline timeline6 = new Timeline().initWithMaxKeyFramesOnTrack(2);
            timeline6.addKeyFrame(KeyFrame.makeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline6.addKeyFrame(KeyFrame.makeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, num));
            button.addTimeline(timeline6);
            button.x = (float)canvas.xOffsetScaled;
            menuView.addChild(button);
            addViewwithID(menuView, 6);
        }

        public override NSObject initWithParent(ViewController p)
        {
            if (base.initWithParent(p) != null)
            {
                ddMainMenu = (DelayedDispatcher)new DelayedDispatcher().init();
                ddPackSelect = (DelayedDispatcher)new DelayedDispatcher().init();
                createMainMenu();
                createOptions();
                createReset();
                createAbout();
                createMovieView();
                createPackSelect();
                createLeaderboards();
                createAchievements();
                MapPickerController c = (MapPickerController)new MapPickerController().initWithParent(this);
                addChildwithID(c, 0);
            }
            return this;
        }

        public override void dealloc()
        {
            ddMainMenu.cancelAllDispatches();
            ddMainMenu.dealloc();
            ddMainMenu = null;
            ddPackSelect.cancelAllDispatches();
            ddPackSelect.dealloc();
            ddPackSelect = null;
            base.dealloc();
        }

        public override void activate()
        {
            showNextPackStatus = false;
            base.activate();
            CTRRootController cTRRootController = (CTRRootController)Application.sharedRootController();
            pack = cTRRootController.getPack();
            if (viewToShow == 6)
            {
                currentPack = pack;
                preLevelSelect();
            }
            showView(viewToShow);
            CTRSoundMgr._stopMusic();
            CTRSoundMgr._playMusic(145);
        }

        public virtual void showNextPack()
        {
            CTRRootController cTRRootController = (CTRRootController)Application.sharedRootController();
            int num = cTRRootController.getPack();
            if (num < CTRPreferences.getPacksCount() - 1)
            {
                packContainer.delegateScrollableContainerProtocol = this;
                packContainer.moveToScrollPointmoveMultiplier(num + 1, 0.8);
                showNextPackStatus = true;
                return;
            }
            replayingIntroMovie = false;
            packContainer.placeToScrollPoint(cTRRootController.getPack() + 1);
            CTRSoundMgr._stopMusic();
            Application.sharedMovieMgr().delegateMovieMgrDelegate = this;
            Application.sharedMovieMgr().playURL(NSS("outro"), !Preferences._getBooleanForKey("MUSIC_ON") && !Preferences._getBooleanForKey("SOUND_ON"));
        }

        public override void onChildDeactivated(int n)
        {
            base.onChildDeactivated(n);
            ((CTRRootController)Application.sharedRootController()).setSurvival(false);
            deactivate();
        }

        public virtual void moviePlaybackFinished(NSString url)
        {
            if (replayingIntroMovie)
            {
                replayingIntroMovie = false;
                activateChild(0);
                return;
            }
            if (url != null)
            {
                CTRSoundMgr._playMusic(145);
            }
            if (CTRPreferences.shouldPlayLevelScroll())
            {
                packContainer.placeToScrollPoint(CTRPreferences.getPacksCount() - 1);
                packContainer.moveToScrollPointmoveMultiplier(0, 0.6);
                CTRPreferences.disablePlayLevelScroll();
            }
            else
            {
                packContainer.placeToScrollPoint(CTRPreferences.getLastPack());
            }
            showView(5);
            if (url != null && url.hasSuffix("outro"))
            {
                packContainer.moveToScrollPointmoveMultiplier(CTRPreferences.getPacksCount(), 0.8);
                showGameFinishedPopup();
            }
        }

        public virtual void preLevelSelect()
        {
            CTRResourceMgr cTRResourceMgr = Application.sharedResourceMgr();
            int[] array = null;
            switch (pack)
            {
                case 0:
                    array = PACK_GAME_COVER_01;
                    break;
                case 1:
                    array = PACK_GAME_COVER_02;
                    break;
                case 2:
                    array = PACK_GAME_COVER_03;
                    break;
                case 3:
                    array = PACK_GAME_COVER_04;
                    break;
                case 4:
                    array = PACK_GAME_COVER_05;
                    break;
                case 5:
                    array = PACK_GAME_COVER_06;
                    break;
                case 6:
                    array = PACK_GAME_COVER_07;
                    break;
                case 7:
                    array = PACK_GAME_COVER_08;
                    break;
                case 8:
                    array = PACK_GAME_COVER_09;
                    break;
                case 9:
                    array = PACK_GAME_COVER_10;
                    break;
                case 10:
                    array = PACK_GAME_COVER_11;
                    break;
            }
            cTRResourceMgr.initLoading();
            cTRResourceMgr.loadPack(array);
            cTRResourceMgr.loadImmediately();
            if (getView(6) != null)
            {
                deleteView(6);
            }
            createLevelSelect();
        }

        public virtual void timelineFinished(Timeline t)
        {
            CTRSoundMgr._stopMusic();
            CTRRootController ctrrootController = (CTRRootController)Application.sharedRootController();
            ctrrootController.setPack(pack);
            ctrrootController.setLevel(level);
            Application.sharedRootController().setViewTransition(-1);
            ((MapPickerController)getChild(0)).setAutoLoadMap(LevelsList.LEVEL_NAMES[pack, level]);
            if (pack == 0 && level == 0 && CTRPreferences.getScoreForPackLevel(0, 0) != 0)
            {
                replayingIntroMovie = true;
                showView(7);
                CTRSoundMgr._stopMusic();
                Application.sharedMovieMgr().delegateMovieMgrDelegate = this;
                Application.sharedMovieMgr().playURL(NSS("intro"), !Preferences._getBooleanForKey("MUSIC_ON") && !Preferences._getBooleanForKey("SOUND_ON"));
                return;
            }
            activateChild(0);
        }

        public virtual void recreateOptions()
        {
            deleteView(1);
            createOptions();
        }

        public virtual void onButtonPressed(int n)
        {
            if (n != -1 && n != 34)
            {
                CTRSoundMgr._playSound(9);
            }
            if (n >= 1000)
            {
                level = n - 1000;
                activeView().getChildWithName("levelsBox").playTimeline(0);
                activeView().getChildWithName("shadow").playTimeline(0);
                activeView().getChildWithName("levelsBack").playTimeline(0);
                activeView().getChildWithName("starText").playTimeline(0);
                activeView().getChildWithName("backButton").playTimeline(0);
                return;
            }
            switch (n)
            {
                case 0:
                    {
                        for (int i = 0; i < CTRPreferences.getPacksCount(); i++)
                        {
                            GameController.checkForBoxPerfect(i);
                        }
                        replayingIntroMovie = false;
                        if (CTRPreferences.getScoreForPackLevel(0, 0) == 0)
                        {
                            showView(7);
                            CTRSoundMgr._stopMusic();
                            Application.sharedMovieMgr().delegateMovieMgrDelegate = this;
                            Application.sharedMovieMgr().playURL(NSS("intro"), !Preferences._getBooleanForKey("MUSIC_ON") && !Preferences._getBooleanForKey("SOUND_ON"));
                            return;
                        }
                        moviePlaybackFinished(null);
                        return;
                    }
                case 1:
                    showView(1);
                    return;
                case 2:
                    ((CTRRootController)Application.sharedRootController()).setPack(0);
                    preLevelSelect();
                    Application.sharedRootController().setViewTransition(-1);
                    ((MapPickerController)getChild(0)).setNormalMode();
                    activateChild(0);
                    return;
                case 3:
                    {
                        CTRSoundMgr._stopMusic();
                        pack = 0;
                        Application.sharedRootController().setViewTransition(-1);
                        CTRRootController ctrrootController = (CTRRootController)Application.sharedRootController();
                        CTRResourceMgr ctrresourceMgr = Application.sharedResourceMgr();
                        ctrresourceMgr.initLoading();
                        ctrresourceMgr.loadPack(PACK_GAME_COVER_01);
                        ctrresourceMgr.loadImmediately();
                        ctrrootController.setSurvival(true);
                        ctrrootController.setPack(pack);
                        deactivate();
                        return;
                    }
                case 4:
                    CTRRootController.openFullVersionPage();
                    return;
                case 5:
                    Preferences._setBooleanforKey(!Preferences._getBooleanForKey("SOUND_ON"), "SOUND_ON", true);
                    return;
                case 6:
                    {
                        bool flag6 = Preferences._getBooleanForKey("MUSIC_ON");
                        Preferences._setBooleanforKey(!flag6, "MUSIC_ON", true);
                        if (flag6)
                        {
                            CTRSoundMgr._stopMusic();
                            return;
                        }
                        CTRSoundMgr._playMusic(145);
                        return;
                    }
                case 7:
                    aboutContainer.setScroll(vect(0f, 0f));
                    aboutAutoScroll = true;
                    showView(3);
                    return;
                case 8:
                    showView(4);
                    return;
                case 9:
                case 18:
                case 19:
                    break;
                case 10:
                    showView(1);
                    return;
                case 11:
                    {
                        bool flag7 = Preferences._getBooleanForKey("PREFS_CLICK_TO_CUT");
                        Preferences._setBooleanforKey(!flag7, "PREFS_CLICK_TO_CUT", true);
                        NSS(flag7 ? "off" : "on");
                        return;
                    }
                case 12:
                    showView(5);
                    packContainer.moveToScrollPointmoveMultiplier(pack, 0.8);
                    return;
                case 13:
                    {
                        CTRPreferences ctrpreferences = Application.sharedPreferences();
                        ctrpreferences.resetToDefaults();
                        ctrpreferences.savePreferences();
                        deleteView(5);
                        createPackSelect();
                        showView(1);
                        return;
                    }
                case 14:
                    showView(1);
                    return;
                case 15:
                    ((Popup)activeView().getChildWithName("popup")).hidePopup();
                    return;
                case 16:
                    AndroidAPI.openUrl("http://twitter.com/zeptolab");
                    return;
                case 17:
                    AndroidAPI.openUrl("http://www.facebook.com/cuttherope");
                    return;
                case 20:
                    {
                        int num2 = currentPack;
                        int num3 = scrollPacksLeft + 1;
                        scrollPacksLeft = num3;
                        int sp2 = FixScrollPoint(num2 + num3 - scrollPacksRight);
                        packContainer.moveToScrollPointmoveMultiplier(sp2, 0.8);
                        bScrolling = true;
                        return;
                    }
                case 21:
                    {
                        int num4 = currentPack;
                        int num3 = scrollPacksRight + 1;
                        scrollPacksRight = num3;
                        int sp3 = FixScrollPoint(num4 - num3 + scrollPacksLeft);
                        packContainer.moveToScrollPointmoveMultiplier(sp3, 0.8);
                        bScrolling = true;
                        break;
                    }
                case 22:
                    {
                        NSString @string = Application.sharedAppSettings().getString(8);
                        NSString[] array3 =
                        [
                    NSS("en"),
                    NSS("ru"),
                    NSS("de"),
                    NSS("fr")
                        ];
                        int num = array3.Length;
                        bool flag4 = false;
                        for (int j = 0; j < num; j++)
                        {
                            if (@string.isEqualToString(array3[j]))
                            {
                                NSString nSString = array3[(j + 1) % num];
                                Application.sharedAppSettings().setString(8, nSString);
                                Application.sharedPreferences().setStringforKey(nSString.ToString(), "PREFS_LOCALE", true);
                                flag4 = true;
                                break;
                            }
                        }
                        if (!flag4)
                        {
                            Application.sharedAppSettings().setString(8, array3[1]);
                            Application.sharedPreferences().setStringforKey(array3[1].ToString(), "PREFS_LOCALE", true);
                        }
                        CTRResourceMgr ctrresourceMgr2 = Application.sharedResourceMgr();
                        ctrresourceMgr2.freePack(PACK_LOCALIZATION_MENU);
                        ctrresourceMgr2.clearCachedResources();
                        ctrresourceMgr2.initLoading();
                        ctrresourceMgr2.loadPack(PACK_LOCALIZATION_MENU);
                        ctrresourceMgr2.loadImmediately();
                        deleteView(5);
                        createPackSelect();
                        deleteView(0);
                        createMainMenu();
                        deleteView(4);
                        createReset();
                        deleteView(3);
                        createAbout();
                        createLeaderboards();
                        ddMainMenu.callObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(selector_recreateOptions), null, 0.01);
                        ((CTRRootController)Application.sharedRootController()).recreateLoadingController();
                        return;
                    }
                case 23:
                case 24:
                case 25:
                case 26:
                case 27:
                case 28:
                case 29:
                case 30:
                case 31:
                case 32:
                case 33:
                case 34:
                    {
                        if (pack != n - 23)
                        {
                            packContainer.moveToScrollPointmoveMultiplier(n - 23, 0.8);
                            return;
                        }
                        CTRPreferences.setLastPack(pack);
                        bool flag5 = CTRPreferences.getUnlockedForPackLevel(n - 23, 0) == UNLOCKED_STATE.UNLOCKED_STATE_LOCKED && n - 23 != CTRPreferences.getPacksCount();
                        if (n != 34 && !flag5)
                        {
                            preLevelSelect();
                            showView(6);
                            return;
                        }
                        break;
                    }
                case 35:
                case 36:
                case 37:
                case 38:
                    {
                        NSString[] array4 =
                        [
                    NSS("BS"),
                    NSS("OP"),
                    NSS("LB"),
                    NSS("AC")
                        ];
                        NSString[] array5 = new NSString[4];
                        array5[0] = NSS("BS_BACK_PRESSED");
                        array5[1] = NSS("OP_BACK_PRESSED");
                        NSString nsstring = array4[n - 35];
                        NSString nsstring2 = array5[n - 35];
                        showView(0);
                        return;
                    }
                case 39:
                    Global.XnaGame.Exit();
                    return;
                case 40:
                    if (ep != null)
                    {
                        ep.hidePopup();
                        ep = null;
                        return;
                    }
                    break;
                case 41:
                    showYesNoPopup(Application.getString(655505), 39, 40);
                    return;
                default:
                    return;
            }
        }

        private int FixScrollPoint(int moveToPack)
        {
            if (moveToPack >= packContainer.getTotalScrollPoints())
            {
                moveToPack = packContainer.getTotalScrollPoints() - 1;
            }
            else if (moveToPack < 0)
            {
                moveToPack = 0;
            }
            return moveToPack;
        }

        public override void update(float delta)
        {
            base.update(delta);
            MovieMgr movieMgr = Application.sharedMovieMgr();
            if (movieMgr.isPlaying())
            {
                movieMgr.update();
                return;
            }
            if (activeViewID == 3 && aboutAutoScroll)
            {
                Vector scroll = aboutContainer.getScroll();
                Vector maxScroll = aboutContainer.getMaxScroll();
                scroll.y += 0.5f;
                scroll.y = FIT_TO_BOUNDARIES((double)scroll.y, 0.0, (double)maxScroll.y);
                aboutContainer.setScroll(scroll);
                return;
            }
            if (activeViewID == 5 && ddPackSelect != null)
            {
                ddPackSelect.update(delta);
                if (Global.XnaGame.IsKeyPressed(Keys.Left))
                {
                    onButtonPressed(21);
                    return;
                }
                if (Global.XnaGame.IsKeyPressed(Keys.Right))
                {
                    onButtonPressed(20);
                    return;
                }
                if ((Global.XnaGame.IsKeyPressed(Keys.Space) || Global.XnaGame.IsKeyPressed(Keys.Enter)) && !bScrolling)
                {
                    onButtonPressed(23 + currentPack);
                    return;
                }
            }
            else
            {
                if (activeViewID == 0 && ddMainMenu != null)
                {
                    ddMainMenu.update(delta);
                    return;
                }
                if (activeViewID == 1 && ddMainMenu != null)
                {
                    ddMainMenu.update(delta);
                }
            }
        }

        public override bool touchesBeganwithEvent(IList<TouchLocation> touches)
        {
            bool flag = base.touchesBeganwithEvent(touches);
            if (activeViewID == 3 && aboutAutoScroll)
            {
                aboutAutoScroll = false;
            }
            return flag;
        }

        public virtual void timelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
        {
        }

        public override void fullscreenToggled(bool isFullscreen)
        {
            deleteView(5);
            createPackSelect();
            BaseElement childWithName = getView(0).getChild(0).getChildWithName("container");
            if (childWithName != null)
            {
                childWithName.x = (float)-(float)canvas.xOffsetScaled;
            }
            BaseElement childWithName2 = getView(5).getChild(0).getChildWithName("text");
            if (childWithName2 != null)
            {
                childWithName2.x = -20f - (float)canvas.xOffsetScaled;
            }
            for (int i = 0; i < 10; i++)
            {
                View view3 = getView(i);
                if (view3 != null)
                {
                    BaseElement childWithName3 = view3.getChildWithName("backb");
                    if (childWithName3 != null)
                    {
                        childWithName3.x = (float)canvas.xOffsetScaled;
                    }
                }
            }
            BaseElement view4 = getView(6);
            if (view4 != null)
            {
                view4.getChildWithName("backButton").x = (float)canvas.xOffsetScaled;
                view4.getChildWithName("starText").x = (float)-(float)canvas.xOffsetScaled;
            }
        }

        public void selector_recreateOptions(NSObject param)
        {
            recreateOptions();
        }

        public override bool backButtonPressed()
        {
            int num = activeViewID;
            if (num == 0)
            {
                if (ep != null)
                {
                    onButtonPressed(40);
                }
                else
                {
                    onButtonPressed(41);
                }
            }
            switch (num)
            {
                case 1:
                    onButtonPressed(36);
                    break;
                case 3:
                case 4:
                    onButtonPressed(10);
                    break;
                case 5:
                    onButtonPressed(35);
                    break;
                case 6:
                    onButtonPressed(12);
                    break;
            }
            return true;
        }

        private const int CHILD_PICKER = 0;

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

        private const int MM_VIEWS_COUNT = 10;

        private const int BUTTON_PLAY = 0;

        private const int BUTTON_OPTIONS = 1;

        private const int BUTTON_EXTRAS = 2;

        private const int BUTTON_SURVIVAL = 3;

        private const int BUTTON_BUYGAME = 4;

        private const int BUTTON_SOUND_ONOFF = 5;

        private const int BUTTON_MUSIC_ONOFF = 6;

        private const int BUTTON_ABOUT = 7;

        private const int BUTTON_RESET = 8;

        private const int BUTTON_HELP = 9;

        private const int BUTTON_BACK_TO_OPTIONS = 10;

        private const int BUTTON_SWAP_CONTROL = 11;

        private const int BUTTON_BACK_TO_PACK_SELECT = 12;

        private const int BUTTON_RESET_YES = 13;

        private const int BUTTON_RESET_NO = 14;

        private const int BUTTON_CANT_UNLOCK_OK = 15;

        private const int BUTTON_TWITTER = 16;

        private const int BUTTON_FACEBOOK = 17;

        private const int BUTTON_LEADERBOARDS = 18;

        private const int BUTTON_ACHIEVEMENTS = 19;

        private const int BUTTON_NEXT_PACK = 20;

        private const int BUTTON_PREVIOUS_PACK = 21;

        private const int BUTTON_LOCALIZATION = 22;

        private const int BUTTON_PACK1 = 23;

        private const int BUTTON_PACK2 = 24;

        private const int BUTTON_PACK3 = 25;

        private const int BUTTON_PACK4 = 26;

        private const int BUTTON_PACK5 = 27;

        private const int BUTTON_PACK6 = 28;

        private const int BUTTON_PACK7 = 29;

        private const int BUTTON_PACK8 = 30;

        private const int BUTTON_PACK9 = 31;

        private const int BUTTON_PACK10 = 32;

        private const int BUTTON_PACK11 = 33;

        private const int BUTTON_SOON = 34;

        private const int BUTTON_PACK_SELECTION_BACK = 35;

        private const int BUTTON_OPTIONS_BACK = 36;

        private const int BUTTON_LEADERBOARDS_BACK = 37;

        private const int BUTTON_ACHIEVEMENTS_BACK = 38;

        private const int BUTTON_EXIT_YES = 39;

        private const int BUTTON_POPUP_HIDE = 40;

        private const int BUTTON_QIUT = 41;

        public const int BUTTON_LEVEL_1 = 1000;

        private const string TWITTER_LINK = "http://twitter.com/zeptolab";

        private const string FACEBOOK_LINK = "http://www.facebook.com/cuttherope";

        private const float BOX_OFFSET = -20f;

        private const float BOX_X_SHIFT = 0f;

        private const float BOX_Y_SHIFT = 0f;

        private const float BOX_TOUCH_X_SHIFT = 235f;

        private const float BOX_TOUCH_WIDTH_SHIFT = 70f;

        private const float BOX_WIDTH_INCREASE = 1000f;

        public DelayedDispatcher ddMainMenu;

        public DelayedDispatcher ddPackSelect;

        private ScrollableContainer helpContainer;

        private ScrollableContainer aboutContainer;

        private ScrollableContainer packContainer;

        private BaseElement[] boxes = new BaseElement[CTRPreferences.getPacksCount() + 1];

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

        public class TouchBaseElement : BaseElement
        {
            public override bool onTouchDownXY(float tx, float ty)
            {
                base.onTouchDownXY(tx, ty);
                CTRRectangle r = MakeRectangle(drawX + bbc.x, drawY + bbc.y, (float)width + bbc.w, (float)height + bbc.h);
                CTRRectangle rectangle = rectInRectIntersection(MakeRectangle(0.0, 0.0, (double)SCREEN_WIDTH, (double)SCREEN_HEIGHT), r);
                if (pointInRect(tx, ty, r.x, r.y, r.w, r.h) && (double)rectangle.w > (double)r.w / 2.0)
                {
                    delegateValue.onButtonPressed(bid);
                    return true;
                }
                return false;
            }

            public int bid;

            public CTRRectangle bbc;

            public ButtonDelegate delegateValue;
        }

        public class MonsterSlot : Image
        {
            public static MonsterSlot MonsterSlot_create(CTRTexture2D t)
            {
                return (MonsterSlot)new MonsterSlot().initWithTexture(t);
            }

            public static MonsterSlot MonsterSlot_createWithResID(int r)
            {
                return MonsterSlot_create(Application.getTexture(r));
            }

            public static MonsterSlot MonsterSlot_createWithResIDQuad(int r, int q)
            {
                MonsterSlot monsterSlot = MonsterSlot_create(Application.getTexture(r));
                monsterSlot.setDrawQuad(q);
                return monsterSlot;
            }

            public override void draw()
            {
                preDraw();
                if (quadToDraw == -1)
                {
                    GLDrawer.drawImage(texture, drawX, drawY);
                }
                else
                {
                    drawQuad(quadToDraw);
                }
                float num = c.getScroll().x;
                Vector preCutSize = Application.getTexture(52).preCutSize;
                if (num >= s && num < e)
                {
                    num -= preCutSize.x + -20f;
                    float num2 = num - (s + e) / 2f;
                    OpenGL.setScissorRectangle(250.0 - (double)num2, 0.0, 200.0, (double)SCREEN_HEIGHT);
                    postDraw();
                    OpenGL.setScissorRectangle(c.drawX, c.drawY, (float)c.width, (float)c.height);
                }
            }

            public ScrollableContainer c;

            public float s;

            public float e;
        }
    }
}
