﻿using System;
using System.Collections.Generic;
using System.Reflection;
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

namespace CutTheRope.game
{
	// Token: 0x02000087 RID: 135
	internal class MenuController : ViewController, ButtonDelegate, MovieMgrDelegate, ScrollableContainerProtocol, TimelineDelegate
	{
		// Token: 0x06000567 RID: 1383 RVA: 0x0002A6B4 File Offset: 0x000288B4
		public static Button createButtonWithTextIDDelegate(NSString str, int bid, ButtonDelegate d)
		{
			Image image = Image.Image_createWithResIDQuad(2, 0);
			Image image2 = Image.Image_createWithResIDQuad(2, 1);
			FontGeneric font = Application.getFont(3);
			Text text = new Text().initWithFont(font);
			text.setString(str);
			Text text2 = new Text().initWithFont(font);
			text2.setString(str);
			text.anchor = (text.parentAnchor = 18);
			text2.anchor = (text2.parentAnchor = 18);
			image.addChild(text);
			image2.addChild(text2);
			Button button = new Button().initWithUpElementDownElementandID(image, image2, bid);
			button.setTouchIncreaseLeftRightTopBottom(15.0, 15.0, 15.0, 15.0);
			button.delegateButtonDelegate = d;
			return button;
		}

		// Token: 0x06000568 RID: 1384 RVA: 0x0002A778 File Offset: 0x00028978
		public static Button createShortButtonWithTextIDDelegate(NSString str, int bid, ButtonDelegate d)
		{
			Image image = Image.Image_createWithResIDQuad(61, 1);
			Image image2 = Image.Image_createWithResIDQuad(61, 0);
			FontGeneric font = Application.getFont(3);
			Text text = new Text().initWithFont(font);
			text.setString(str);
			Text text2 = new Text().initWithFont(font);
			text2.setString(str);
			text.anchor = (text.parentAnchor = 18);
			text2.anchor = (text2.parentAnchor = 18);
			image.addChild(text);
			image2.addChild(text2);
			Button button = new Button().initWithUpElementDownElementandID(image, image2, bid);
			button.setTouchIncreaseLeftRightTopBottom(15.0, 15.0, 15.0, 15.0);
			button.delegateButtonDelegate = d;
			return button;
		}

		// Token: 0x06000569 RID: 1385 RVA: 0x0002A840 File Offset: 0x00028A40
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
			text.anchor = (text.parentAnchor = 18);
			text2.anchor = (text2.parentAnchor = 18);
			text3.anchor = (text3.parentAnchor = 18);
			text4.anchor = (text4.parentAnchor = 18);
			image.addChild(text);
			image2.addChild(text2);
			image3.addChild(text3);
			image4.addChild(text4);
			ToggleButton toggleButton = new ToggleButton().initWithUpElement1DownElement1UpElement2DownElement2andID(image, image2, image3, image4, bid);
			toggleButton.setTouchIncreaseLeftRightTopBottom(10.0, 10.0, 10.0, 10.0);
			toggleButton.delegateButtonDelegate = d;
			return toggleButton;
		}

		// Token: 0x0600056A RID: 1386 RVA: 0x0002A988 File Offset: 0x00028B88
		public static Button createBackButtonWithDelegateID(ButtonDelegate d, int bid)
		{
			Button button = MenuController.createButtonWithImageQuad1Quad2IDDelegate(54, 0, 1, bid, d);
			button.anchor = (button.parentAnchor = 33);
			return button;
		}

		// Token: 0x0600056B RID: 1387 RVA: 0x0002A9B4 File Offset: 0x00028BB4
		public static Button createButtonWithImageIDDelegate(int resID, int bid, ButtonDelegate d)
		{
			Texture2D texture = Application.getTexture(resID);
			Image up = Image.Image_create(texture);
			Image image = Image.Image_create(texture);
			image.scaleX = 1.2f;
			image.scaleY = 1.2f;
			Button button = new Button().initWithUpElementDownElementandID(up, image, bid);
			button.setTouchIncreaseLeftRightTopBottom(10.0, 10.0, 10.0, 10.0);
			button.delegateButtonDelegate = d;
			return button;
		}

		// Token: 0x0600056C RID: 1388 RVA: 0x0002AA28 File Offset: 0x00028C28
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

		// Token: 0x0600056D RID: 1389 RVA: 0x0002AA8C File Offset: 0x00028C8C
		public static Button createButtonWithImageQuad1Quad2IDDelegate(int res, int q1, int q2, int bid, ButtonDelegate d)
		{
			Image image = Image.Image_createWithResIDQuad(res, q1);
			Image image2 = Image.Image_createWithResIDQuad(res, q2);
			image.doRestoreCutTransparency();
			image2.doRestoreCutTransparency();
			Button button = new Button().initWithUpElementDownElementandID(image, image2, bid);
			button.delegateButtonDelegate = d;
			Texture2D texture = Application.getTexture(res);
			button.forceTouchRect(FrameworkTypes.MakeRectangle(texture.quadOffsets[q1].x, texture.quadOffsets[q1].y, texture.quadRects[q1].w, texture.quadRects[q1].h));
			return button;
		}

		// Token: 0x0600056E RID: 1390 RVA: 0x0002AB20 File Offset: 0x00028D20
		public static BaseElement createBackgroundWithLogowithShadow(bool l, bool s)
		{
			BaseElement baseElement = (BaseElement)new BaseElement().init();
			baseElement.width = (int)FrameworkTypes.SCREEN_WIDTH;
			baseElement.height = (int)FrameworkTypes.SCREEN_HEIGHT;
			Image image = Image.Image_createWithResIDQuad(48, 0);
			image.anchor = (image.parentAnchor = 34);
			image.scaleX = (image.scaleY = 1.25f);
			image.rotationCenterY = (float)(image.height / 2);
			image.passTransformationsToChilds = false;
			baseElement.addChild(image);
			if (l)
			{
				Image image2 = Image.Image_createWithResIDQuad(48, 1);
				image2.anchor = (image2.parentAnchor = 34);
				image2.scaleX = (image2.scaleY = 1.25f);
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
				image4.anchor = (image4.parentAnchor = 18);
				image4.scaleX = (image4.scaleY = 2f);
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

		// Token: 0x0600056F RID: 1391 RVA: 0x0002ACD5 File Offset: 0x00028ED5
		public static BaseElement createBackgroundWithLogo(bool l)
		{
			return MenuController.createBackgroundWithLogowithShadow(l, true);
		}

		// Token: 0x06000570 RID: 1392 RVA: 0x0002ACE0 File Offset: 0x00028EE0
		public static Image createAudioElementForQuadwithCrosspressediconOffset(int q, bool b, bool p, Vector offset)
		{
			int num = ((p > false) ? 1 : 0);
			Image image = Image.Image_createWithResIDQuad(8, num);
			Image image2 = Image.Image_createWithResIDQuad(8, q);
			Image.setElementPositionWithRelativeQuadOffset(image2, 8, num, q);
			image2.parentAnchor = (image2.anchor = 9);
			image2.x += offset.x;
			image2.y += offset.y;
			image.addChild(image2);
			if (b)
			{
				image2.color = RGBAColor.MakeRGBA(0.5f, 0.5f, 0.5f, 0.5f);
				Image image3 = Image.Image_createWithResIDQuad(8, 4);
				image3.parentAnchor = (image3.anchor = 9);
				Image.setElementPositionWithRelativeQuadOffset(image3, 8, num, 4);
				image.addChild(image3);
			}
			return image;
		}

		// Token: 0x06000571 RID: 1393 RVA: 0x0002AD9C File Offset: 0x00028F9C
		public static ToggleButton createAudioButtonWithQuadDelegateIDiconOffset(int q, ButtonDelegate delegateValue, int bid, Vector offset)
		{
			Image u = MenuController.createAudioElementForQuadwithCrosspressediconOffset(q, false, false, offset);
			Image d = MenuController.createAudioElementForQuadwithCrosspressediconOffset(q, false, true, offset);
			Image u2 = MenuController.createAudioElementForQuadwithCrosspressediconOffset(q, true, false, offset);
			Image d2 = MenuController.createAudioElementForQuadwithCrosspressediconOffset(q, true, true, offset);
			ToggleButton toggleButton = new ToggleButton().initWithUpElement1DownElement1UpElement2DownElement2andID(u, d, u2, d2, bid);
			toggleButton.delegateButtonDelegate = delegateValue;
			return toggleButton;
		}

		// Token: 0x06000572 RID: 1394 RVA: 0x0002ADE8 File Offset: 0x00028FE8
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
			text.anchor = (text.parentAnchor = 18);
			text2.anchor = (text2.parentAnchor = 18);
			image.addChild(text);
			image2.addChild(text2);
			Image image3 = Image.Image_createWithResIDQuad(54, q);
			Image image4 = Image.Image_createWithResIDQuad(54, q);
			image4.parentAnchor = (image3.parentAnchor = 20);
			image4.anchor = (image3.anchor = 20);
			text.addChild(image3);
			text2.addChild(image4);
			text.width += (int)((float)image3.width + FrameworkTypes.RTPD(10.0));
			text2.width += (int)((float)image4.width + FrameworkTypes.RTPD(10.0));
			Button button = new Button().initWithUpElementDownElementandID(image, image2, bid);
			button.setTouchIncreaseLeftRightTopBottom(15.0, 15.0, 15.0, 15.0);
			button.delegateButtonDelegate = d;
			return button;
		}

		// Token: 0x06000573 RID: 1395 RVA: 0x0002AF9E File Offset: 0x0002919E
		public static BaseElement createElementWithResIdquad(int resId, int quad)
		{
			if (resId != -1 && quad != -1)
			{
				return Image.Image_createWithResIDQuad(resId, quad);
			}
			return (BaseElement)new BaseElement().init();
		}

		// Token: 0x06000574 RID: 1396 RVA: 0x0002AFC0 File Offset: 0x000291C0
		public static ToggleButton createToggleButtonWithResquadquad2buttonIDdelegate(int res, int quad, int quad2, int bId, ButtonDelegate delegateValue)
		{
			BaseElement baseElement = MenuController.createElementWithResIdquad(res, quad);
			BaseElement baseElement2 = MenuController.createElementWithResIdquad(res, quad);
			BaseElement baseElement3 = MenuController.createElementWithResIdquad(res, quad2);
			BaseElement baseElement4 = MenuController.createElementWithResIdquad(res, quad2);
			int width = MathHelper.MAX(baseElement.width, baseElement3.width);
			int height = MathHelper.MAX(baseElement.height, baseElement3.height);
			baseElement.width = (baseElement2.width = width);
			baseElement.height = (baseElement2.height = height);
			baseElement3.width = (baseElement4.width = width);
			baseElement3.height = (baseElement4.height = height);
			baseElement2.scaleX = (baseElement2.scaleY = (baseElement4.scaleX = (baseElement4.scaleY = 1.2f)));
			ToggleButton toggleButton = new ToggleButton().initWithUpElement1DownElement1UpElement2DownElement2andID(baseElement, baseElement2, baseElement3, baseElement4, bId);
			toggleButton.delegateButtonDelegate = delegateValue;
			return toggleButton;
		}

		// Token: 0x06000575 RID: 1397 RVA: 0x0002B0A4 File Offset: 0x000292A4
		public static BaseElement createControlButtontitleAnchortextbuttonIDdelegate(int q, int tq, NSString str, int bId, ButtonDelegate delegateValue)
		{
			Image image = Image.Image_createWithResIDQuad(8, q);
			Text text = Text.createWithFontandString(4, str);
			text.parentAnchor = 9;
			text.anchor = 18;
			text.scaleX = (text.scaleY = 0.75f);
			image.addChild(text);
			Image.setElementPositionWithRelativeQuadOffset(text, 8, q, tq);
			if (bId != -1)
			{
				ToggleButton toggleButton = MenuController.createToggleButtonWithResquadquad2buttonIDdelegate(8, -1, 8, bId, delegateValue);
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

		// Token: 0x06000576 RID: 1398 RVA: 0x0002B18C File Offset: 0x0002938C
		public static Image createBlankScoresButtonWithIconpressed(int quad, bool pressed)
		{
			Image image3 = Image.Image_createWithResIDQuad(59, (pressed > false) ? 1 : 0);
			Image image2 = Image.Image_createWithResIDQuad(59, quad);
			image3.addChild(image2);
			image2.parentAnchor = 9;
			Image.setElementPositionWithRelativeQuadOffset(image2, 59, 0, quad);
			return image3;
		}

		// Token: 0x06000577 RID: 1399 RVA: 0x0002B1C8 File Offset: 0x000293C8
		public static Button createScoresButtonWithIconbuttonIDdelegate(int quad, int bId, ButtonDelegate delegateValue)
		{
			Image up = MenuController.createBlankScoresButtonWithIconpressed(quad, false);
			Image image = MenuController.createBlankScoresButtonWithIconpressed(quad, true);
			Image.setElementPositionWithRelativeQuadOffset(image, 59, 0, 1);
			Button button = new Button().initWithUpElementDownElementandID(up, image, bId);
			button.delegateButtonDelegate = delegateValue;
			return button;
		}

		// Token: 0x06000578 RID: 1400 RVA: 0x0002B204 File Offset: 0x00029404
		public virtual void createMainMenu()
		{
			MenuView menuView = (MenuView)new MenuView().initFullscreen();
			BaseElement baseElement = MenuController.createBackgroundWithLogo(true);
			VBox vBox = new VBox().initWithOffsetAlignWidth(5.0, 2, (double)FrameworkTypes.SCREEN_WIDTH);
			vBox.anchor = (vBox.parentAnchor = 34);
			vBox.y = -85f;
			Button c = MenuController.createButtonWithTextIDDelegate(Application.getString(655360), 0, this);
			vBox.addChild(c);
			Button c2 = MenuController.createButtonWithTextIDDelegate(Application.getString(655361), 1, this);
			vBox.addChild(c2);
			Button c3 = MenuController.createButtonWithTextIDDelegate(Application.getString(655506), 41, this);
			vBox.addChild(c3);
			baseElement.addChild(vBox);
			bool flag = Application.getString(655507).length() > 0;
			if (flag)
			{
				BaseElement baseElement2 = (BaseElement)new BaseElement().init();
				baseElement2.setName("container");
				baseElement2.parentAnchor = (baseElement2.anchor = 18);
				baseElement2.width = baseElement.width;
				baseElement2.height = baseElement.height;
				baseElement2.x -= (float)base.canvas.xOffsetScaled;
				baseElement.addChild(baseElement2);
				Texture2D texture = Application.getTexture(54);
				Button button = MenuController.createButton2WithImageQuad1Quad2IDDelegate(54, 3, 3, 16, this);
				button.anchor = 9;
				button.parentAnchor = 36;
				Image.setElementPositionWithQuadOffset(button, 54, 3);
				button.x -= texture.preCutSize.x;
				button.y -= texture.preCutSize.y;
				baseElement2.addChild(button);
				Button button2 = MenuController.createButton2WithImageQuad1Quad2IDDelegate(54, 2, 2, 17, this);
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
			this.addViewwithID(menuView, 0);
		}

		// Token: 0x06000579 RID: 1401 RVA: 0x0002B4A0 File Offset: 0x000296A0
		public virtual void createOptions()
		{
			MenuView menuView = (MenuView)new MenuView().initFullscreen();
			BaseElement baseElement = MenuController.createBackgroundWithLogowithShadow(false, false);
			menuView.addChild(baseElement);
			BaseElement baseElement2 = MenuController.createControlButtontitleAnchortextbuttonIDdelegate(5, 10, Application.getString(655423), -1, null);
			BaseElement baseElement3 = MenuController.createControlButtontitleAnchortextbuttonIDdelegate(6, 9, Application.getString(655422), 11, this);
			HBox hBox = new HBox().initWithOffsetAlignHeight(FrameworkTypes.RTPD(80.0), 16, (float)MathHelper.MAX(baseElement2.height, baseElement3.height));
			hBox.parentAnchor = (hBox.anchor = 18);
			hBox.addChild(baseElement2);
			hBox.addChild(baseElement3);
			menuView.addChild(hBox);
			Image image = Image.Image_createWithResIDQuad(60, 0);
			image.anchor = (image.parentAnchor = 18);
			image.scaleX = (image.scaleY = 2f);
			Timeline timeline = new Timeline().initWithMaxKeyFramesOnTrack(3);
			timeline.addKeyFrame(KeyFrame.makeRotation(45.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
			timeline.addKeyFrame(KeyFrame.makeRotation(405.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 75.0));
			timeline.setTimelineLoopType(Timeline.LoopType.TIMELINE_REPLAY);
			image.addTimeline(timeline);
			image.playTimeline(0);
			menuView.addChild(image);
			VBox vBox = new VBox().initWithOffsetAlignWidth(5f, 2, FrameworkTypes.SCREEN_WIDTH);
			vBox.anchor = (vBox.parentAnchor = 18);
			Vector offset = MathHelper.vectSub(Image.getQuadCenter(8, 0), Image.getQuadOffset(8, 12));
			ToggleButton toggleButton = MenuController.createAudioButtonWithQuadDelegateIDiconOffset(3, this, 6, MathHelper.vectZero);
			ToggleButton toggleButton2 = MenuController.createAudioButtonWithQuadDelegateIDiconOffset(2, this, 5, offset);
			HBox hBox2 = new HBox().initWithOffsetAlignHeight(-10f, 16, (float)toggleButton.height);
			hBox2.addChild(toggleButton2);
			hBox2.addChild(toggleButton);
			vBox.addChild(hBox2);
			Button c = MenuController.createLanguageButtonWithIDDelegate(22, this);
			vBox.addChild(c);
			Button c2 = MenuController.createButtonWithTextIDDelegate(Application.getString(655364), 8, this);
			vBox.addChild(c2);
			Button c3 = MenuController.createButtonWithTextIDDelegate(Application.getString(655365), 7, this);
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
			Button button = MenuController.createBackButtonWithDelegateID(this, 36);
			button.setName("backb");
			button.x = (float)base.canvas.xOffsetScaled;
			menuView.addChild(button);
			this.addViewwithID(menuView, 1);
		}

		// Token: 0x0600057A RID: 1402 RVA: 0x0002B7A0 File Offset: 0x000299A0
		public virtual void createReset()
		{
			MenuView menuView = (MenuView)new MenuView().initFullscreen();
			BaseElement baseElement = MenuController.createBackgroundWithLogo(false);
			Text text = new Text().initWithFont(Application.getFont(3));
			text.setAlignment(2);
			text.setStringandWidth(Application.getString(655371), (double)Global.ScreenSizeManager.CurrentSize.Width * 0.95);
			text.anchor = (text.parentAnchor = 18);
			baseElement.addChild(text);
			text.y = -200f;
			Button button = MenuController.createButtonWithTextIDDelegate(Application.getString(655382), 13, this);
			button.anchor = (button.parentAnchor = 34);
			button.y = -540f;
			Button button2 = MenuController.createButtonWithTextIDDelegate(Application.getString(655383), 14, this);
			button2.anchor = (button2.parentAnchor = 34);
			button2.y = -320f;
			baseElement.addChild(button);
			baseElement.addChild(button2);
			menuView.addChild(baseElement);
			Button button3 = MenuController.createBackButtonWithDelegateID(this, 10);
			button3.setName("backb");
			button3.x = (float)base.canvas.xOffsetScaled;
			menuView.addChild(button3);
			this.addViewwithID(menuView, 4);
		}

		// Token: 0x0600057B RID: 1403 RVA: 0x0002B8E8 File Offset: 0x00029AE8
		public virtual void createMovieView()
		{
			MovieView movieView = (MovieView)new MovieView().initFullscreen();
			RectangleElement rectangleElement = (RectangleElement)new RectangleElement().init();
			rectangleElement.width = (int)FrameworkTypes.SCREEN_WIDTH;
			rectangleElement.height = (int)FrameworkTypes.SCREEN_HEIGHT;
			rectangleElement.color = RGBAColor.blackRGBA;
			movieView.addChild(rectangleElement);
			this.addViewwithID(movieView, 7);
		}

		// Token: 0x0600057C RID: 1404 RVA: 0x0002B948 File Offset: 0x00029B48
		public virtual void createAbout()
		{
			MenuView menuView = (MenuView)new MenuView().initFullscreen();
			BaseElement baseElement = MenuController.createBackgroundWithLogo(false);
			string text = Application.getString(655416).ToString();
			string[] separator = new string[] { "%@" };
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
			text2.setStringandWidth(NSObject.NSS(text), (float)((int)num));
			this.aboutContainer = new ScrollableContainer().initWithWidthHeightContainer(num, h, vBox);
			this.aboutContainer.anchor = (this.aboutContainer.parentAnchor = 18);
			vBox.addChild(text2);
			Image c2 = Image.Image_createWithResIDQuad(50, 2);
			vBox.addChild(c2);
			NSString @string = Application.getString(655417);
			Text text3 = new Text().initWithFont(Application.getFont(4));
			text3.setAlignment(2);
			text3.setStringandWidth(@string, num);
			vBox.addChild(text3);
			baseElement.addChild(this.aboutContainer);
			menuView.addChild(baseElement);
			Button button = MenuController.createBackButtonWithDelegateID(this, 10);
			button.setName("backb");
			button.x = (float)base.canvas.xOffsetScaled;
			menuView.addChild(button);
			this.addViewwithID(menuView, 3);
		}

		// Token: 0x0600057D RID: 1405 RVA: 0x0002BB68 File Offset: 0x00029D68
		public virtual HBox createTextWithStar(NSString t)
		{
			HBox hbox = new HBox().initWithOffsetAlignHeight(0.0, 16, (double)FrameworkTypes.RTD(50.0));
			Text text = new Text().initWithFont(Application.getFont(3));
			text.setString(t);
			text.scaleX = (text.scaleY = 0.7f);
			text.rotationCenterX = (float)(-(float)text.width / 2);
			text.width = (int)((float)text.width * 0.7f);
			hbox.addChild(text);
			Image c = Image.Image_createWithResIDQuad(52, 3);
			hbox.addChild(c);
			return hbox;
		}

		// Token: 0x0600057E RID: 1406 RVA: 0x0002BC02 File Offset: 0x00029E02
		public virtual float getBoxWidth()
		{
			return Image.getQuadSize(52, 4).x + Image.getQuadOffset(52, 4).x * 2f;
		}

		// Token: 0x0600057F RID: 1407 RVA: 0x0002BC28 File Offset: 0x00029E28
		public virtual float getPackOffset()
		{
			float num = FrameworkTypes.SCREEN_WIDTH + (float)(base.canvas.xOffset * 2);
			float boxWidth = this.getBoxWidth();
			if (boxWidth * 3f > num - 200f)
			{
				return boxWidth / 2f;
			}
			return 0f;
		}

		// Token: 0x06000580 RID: 1408 RVA: 0x0002BC70 File Offset: 0x00029E70
		public virtual BaseElement createPackElementforContainer(int n, ScrollableContainer c)
		{
			MenuController.TouchBaseElement touchBaseElement = (MenuController.TouchBaseElement)new MenuController.TouchBaseElement().init();
			touchBaseElement.delegateValue = this;
			BaseElement baseElement = (BaseElement)new BaseElement().init();
			baseElement.setName("boxContainer");
			baseElement.anchor = (baseElement.parentAnchor = 12);
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
				nsstring = NSObject.NSS(text3 + text4 + ((@string != null) ? @string.ToString() : null));
			}
			NSString nSString = nsstring;
			UNLOCKED_STATE unlockedForPackLevel = CTRPreferences.getUnlockedForPackLevel(n, 0);
			bool flag = unlockedForPackLevel == UNLOCKED_STATE.UNLOCKED_STATE_LOCKED && n != CTRPreferences.getPacksCount();
			touchBaseElement.bid = 23 + n;
			Image image = Image.Image_createWithResIDQuad(r, q);
			image.doRestoreCutTransparency();
			image.anchor = (image.parentAnchor = 9);
			if (flag)
			{
				baseElement.addChild(image);
				int num = CTRPreferences.packUnlockStars(n);
				Image image2 = Image.Image_createWithResIDQuad(52, 2);
				image2.doRestoreCutTransparency();
				image2.anchor = (image2.parentAnchor = 9);
				image.addChild(image2);
				HBox hBox = this.createTextWithStar(NSObject.NSS(num.ToString()));
				hBox.anchor = (hBox.parentAnchor = 18);
				hBox.y = 110f;
				image2.addChild(hBox);
				Text text = new Text().initWithFont(Application.getFont(4));
				NSString newString = NSObject.NSS(Application.getString(655390).ToString().Replace("%d", num.ToString()));
				text.setAlignment(2);
				text.anchor = 10;
				text.parentAnchor = 34;
				text.setStringandWidth(newString, 700f);
				text.y = -70f;
				text.scaleX = (text.scaleY = 0.7f);
				text.rotationCenterY = (float)(-(float)text.height / 2);
				touchBaseElement.addChild(text);
			}
			else
			{
				if (n != CTRPreferences.getPacksCount())
				{
					int q2 = 0;
					int q3 = 1;
					MenuController.MonsterSlot monsterSlot = MenuController.MonsterSlot.MonsterSlot_createWithResIDQuad(52, q2);
					monsterSlot.c = c;
					monsterSlot.doRestoreCutTransparency();
					monsterSlot.anchor = 9;
					monsterSlot.parentAnchor = 9;
					monsterSlot.y = image.y;
					baseElement.addChild(monsterSlot);
					Image image3 = Image.Image_createWithResIDQuad(52, q3);
					image3.doRestoreCutTransparency();
					image3.anchor = 17;
					monsterSlot.s = (float)(image.width * (n - 1)) + -20f * (float)n + this.packContainer.x + 50f;
					monsterSlot.e = monsterSlot.s + 1200f;
					image3.x = this.packContainer.x - 0f + (float)monsterSlot.width + -20f - this.getPackOffset();
					image3.y = this.packContainer.y + FrameworkTypes.SCREEN_HEIGHT / 2f;
					image3.parentAnchor = -1;
					monsterSlot.addChild(image3);
				}
				baseElement.addChild(image);
				if (unlockedForPackLevel == UNLOCKED_STATE.UNLOCKED_STATE_JUST_UNLOCKED)
				{
					Image image4 = Image.Image_createWithResIDQuad(52, 2);
					image4.setName("lockHideMe");
					image4.doRestoreCutTransparency();
					image4.anchor = (image4.parentAnchor = 9);
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
			text2.anchor = (text2.parentAnchor = 10);
			text2.scaleX = (text2.scaleY = 0.75f);
			if (ResDataPhoneFull.LANGUAGE == Language.LANG_DE || ResDataPhoneFull.LANGUAGE == Language.LANG_EN)
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
			baseElement.height = (touchBaseElement.height = image.height);
			baseElement.width = (touchBaseElement.width = image.width);
			return touchBaseElement;
		}

		// Token: 0x06000581 RID: 1409 RVA: 0x0002C248 File Offset: 0x0002A448
		public virtual void createPackSelect()
		{
			MenuView menuView = (MenuView)new MenuView().initFullscreen();
			BaseElement baseElement = MenuController.createBackgroundWithLogo(false);
			string text = Application.getString(655388).ToString();
			text = text.Replace("%d", "");
			HBox hBox = this.createTextWithStar(NSObject.NSS(text + CTRPreferences.getTotalStars().ToString()));
			hBox.x = -30f - (float)base.canvas.xOffsetScaled;
			hBox.y = 40f;
			hBox.setName("text");
			HBox hBox2 = new HBox().initWithOffsetAlignHeight(-20f, 16, FrameworkTypes.SCREEN_HEIGHT);
			float num = FrameworkTypes.SCREEN_WIDTH + (float)(base.canvas.xOffset * 2);
			float boxWidth = this.getBoxWidth();
			float num2 = boxWidth * 3f;
			if (num2 > num - 200f)
			{
				num2 = boxWidth * 2f;
			}
			this.packContainer = new ScrollableContainer().initWithWidthHeightContainer(num2, FrameworkTypes.SCREEN_HEIGHT, hBox2);
			this.packContainer.minAutoScrollToSpointLength = FrameworkTypes.RTD(5.0);
			this.packContainer.shouldBounceHorizontally = true;
			this.packContainer.resetScrollOnShow = false;
			this.packContainer.dontHandleTouchDownsHandledByChilds = true;
			this.packContainer.dontHandleTouchMovesHandledByChilds = true;
			this.packContainer.dontHandleTouchUpsHandledByChilds = true;
			this.packContainer.turnScrollPointsOnWithCapacity(CTRPreferences.getPacksCount() + 2);
			this.packContainer.delegateScrollableContainerProtocol = this;
			this.packContainer.x = FrameworkTypes.SCREEN_WIDTH / 2f - (float)(this.packContainer.width / 2);
			hBox.anchor = (hBox.parentAnchor = 12);
			baseElement.addChild(hBox);
			Texture2D texture = Application.getTexture(52);
			BaseElement baseElement2 = (BaseElement)new BaseElement().init();
			baseElement2.width = (int)texture.preCutSize.x;
			baseElement2.height = (int)texture.preCutSize.y;
			hBox2.addChild(baseElement2);
			float num3 = 0f + this.getPackOffset();
			for (int i = 0; i < CTRPreferences.getPacksCount() + 1; i++)
			{
				MenuController.TouchBaseElement touchBaseElement = (MenuController.TouchBaseElement)this.createPackElementforContainer(i, this.packContainer);
				this.boxes[i] = touchBaseElement;
				hBox2.addChild(touchBaseElement);
				touchBaseElement.x -= 0f;
				touchBaseElement.y -= 0f;
				this.packContainer.addScrollPointAtXY((double)num3, 0.0);
				touchBaseElement.bbc = FrameworkTypes.MakeRectangle(0f, 0f, -20f, 0f);
				num3 += (float)touchBaseElement.width + -20f;
			}
			hBox2.width += 1000;
			Image image = Image.Image_createWithResIDQuad(52, 11);
			image.anchor = 17;
			image.y += FrameworkTypes.SCREEN_HEIGHT / 2f;
			image.x = this.packContainer.x - 2f;
			baseElement.addChild(image);
			Image image2 = Image.Image_createWithResIDQuad(52, 11);
			image2.anchor = 20;
			image2.y += FrameworkTypes.SCREEN_HEIGHT / 2f;
			image2.x = this.packContainer.x + (float)this.packContainer.width + 2f;
			baseElement.addChild(image2);
			image2.scaleX = (image2.scaleY = -1f);
			baseElement.addChild(this.packContainer);
			Image image3 = Image.Image_createWithResIDQuad(52, 12);
			image3.anchor = 20;
			image3.y += FrameworkTypes.SCREEN_HEIGHT / 2f;
			image3.x = this.packContainer.x + 3f;
			baseElement.addChild(image3);
			Image image4 = Image.Image_createWithResIDQuad(52, 12);
			image4.anchor = 17;
			image4.y += FrameworkTypes.SCREEN_HEIGHT / 2f;
			image4.x = this.packContainer.x + (float)this.packContainer.width - 3f;
			image4.scaleX = (image4.scaleY = -1f);
			baseElement.addChild(image4);
			this.prevb = MenuController.createButton2WithImageQuad1Quad2IDDelegate(52, 13, 14, 21, this);
			this.prevb.parentAnchor = 17;
			this.prevb.anchor = 20;
			this.prevb.x = this.packContainer.x - 40f;
			baseElement.addChild(this.prevb);
			this.nextb = MenuController.createButton2WithImageQuad1Quad2IDDelegate(52, 13, 14, 20, this);
			this.nextb.anchor = (this.nextb.parentAnchor = 17);
			this.nextb.x = this.packContainer.x + (float)this.packContainer.width + 40f;
			this.nextb.scaleX = -1f;
			baseElement.addChild(this.nextb);
			menuView.addChild(baseElement);
			this.addViewwithID(menuView, 5);
			Button button = MenuController.createBackButtonWithDelegateID(this, 35);
			button.setName("backb");
			button.x = (float)base.canvas.xOffsetScaled;
			menuView.addChild(button);
			int lastPack = CTRPreferences.getLastPack();
			this.packContainer.placeToScrollPoint(lastPack);
			this.scrollableContainerchangedTargetScrollPoint(this.packContainer, lastPack);
		}

		// Token: 0x06000582 RID: 1410 RVA: 0x0002C7E2 File Offset: 0x0002A9E2
		public virtual void createLeaderboards()
		{
		}

		// Token: 0x06000583 RID: 1411 RVA: 0x0002C7E4 File Offset: 0x0002A9E4
		public virtual void createAchievements()
		{
		}

		// Token: 0x06000584 RID: 1412 RVA: 0x0002C7E8 File Offset: 0x0002A9E8
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
			HBox hBox = this.createTextWithStar(NSObject.NSS((CTRPreferences.packUnlockStars(cTRRootController.getPack() + 1) - totalStars).ToString()));
			hBox.anchor = 18;
			Image.setElementPositionWithQuadOffset(hBox, 49, 5);
			hBox.y -= (float)num;
			popup.addChild(hBox);
			Button button = MenuController.createButtonWithTextIDDelegate(Application.getString(655389), 15, this);
			button.anchor = 18;
			Image.setElementPositionWithQuadOffset(button, 49, 4);
			popup.addChild(button);
			popup.showPopup();
			this.activeView().addChild(popup);
		}

		// Token: 0x06000585 RID: 1413 RVA: 0x0002C9EC File Offset: 0x0002ABEC
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
			Button button = MenuController.createButtonWithTextIDDelegate(Application.getString(655389), 15, this);
			button.anchor = 18;
			Image.setElementPositionWithQuadOffset(button, 49, 4);
			image.addChild(button);
			popup.showPopup();
			this.activeView().addChild(popup);
		}

		// Token: 0x06000586 RID: 1414 RVA: 0x0002CB28 File Offset: 0x0002AD28
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
			Button button = MenuController.createButtonWithTextIDDelegate(Application.getString(655382), buttonYesId, this);
			button.anchor = 18;
			Image.setElementPositionWithQuadOffset(button, 49, 4);
			button.y -= (float)button.height;
			image.addChild(button);
			Button button2 = MenuController.createButtonWithTextIDDelegate(Application.getString(655383), buttonNoId, this);
			button2.anchor = 18;
			Image.setElementPositionWithQuadOffset(button2, 49, 4);
			image.addChild(button2);
			popup.showPopup();
			this.ep = popup;
			this.activeView().addChild(popup);
		}

		// Token: 0x06000587 RID: 1415 RVA: 0x0002CC44 File Offset: 0x0002AE44
		public virtual void scrollableContainerreachedScrollPoint(ScrollableContainer e, int i)
		{
			this.currentPack = i;
			this.pack = i;
			this.scrollPacksLeft = 0;
			this.scrollPacksRight = 0;
			this.bScrolling = false;
			if (this.prevb.isEnabled())
			{
				this.prevb.setState(Button.BUTTON_STATE.BUTTON_UP);
			}
			if (this.nextb.isEnabled())
			{
				this.nextb.setState(Button.BUTTON_STATE.BUTTON_UP);
			}
			if (i == CTRPreferences.getPacksCount())
			{
				return;
			}
			this.boxes[i].getChildWithName("boxContainer").playTimeline(0);
			UNLOCKED_STATE unlockedForPackLevel = CTRPreferences.getUnlockedForPackLevel(i, 0);
			BaseElement childWithName = this.boxes[i].getChildWithName("lockHideMe");
			if (childWithName != null && unlockedForPackLevel == UNLOCKED_STATE.UNLOCKED_STATE_JUST_UNLOCKED)
			{
				CTRPreferences.setUnlockedForPackLevel(UNLOCKED_STATE.UNLOCKED_STATE_UNLOCKED, i, 0);
				childWithName.playTimeline(0);
			}
			CTRRootController cTRRootController = (CTRRootController)Application.sharedRootController();
			if (this.showNextPackStatus && i == cTRRootController.getPack() + 1)
			{
				this.showNextPackStatus = false;
				if (unlockedForPackLevel == UNLOCKED_STATE.UNLOCKED_STATE_LOCKED)
				{
					this.showCantUnlockPopup();
				}
			}
		}

		// Token: 0x06000588 RID: 1416 RVA: 0x0002CD28 File Offset: 0x0002AF28
		public virtual void scrollableContainerchangedTargetScrollPoint(ScrollableContainer e, int i)
		{
			this.currentPack = i;
			this.pack = i;
			CTRPreferences.setLastPack(i);
		}

		// Token: 0x06000589 RID: 1417 RVA: 0x0002CD4C File Offset: 0x0002AF4C
		public virtual BaseElement createButtonForLevelPack(int l, int p)
		{
			bool flag = CTRPreferences.getUnlockedForPackLevel(p, l) == UNLOCKED_STATE.UNLOCKED_STATE_LOCKED;
			int starsForPackLevel = CTRPreferences.getStarsForPackLevel(p, l);
			MenuController.TouchBaseElement touchBaseElement = (MenuController.TouchBaseElement)new MenuController.TouchBaseElement().init();
			touchBaseElement.bbc = FrameworkTypes.MakeRectangle(5.0, 0.0, -10.0, 0.0);
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
				NSString @string = NSObject.NSS((l + 1).ToString());
				text.setString(@string);
				text.anchor = (text.parentAnchor = 18);
				text.y -= 5f;
				image.addChild(text);
				Image image2 = Image.Image_createWithResIDQuad(51, 2 + starsForPackLevel);
				image2.doRestoreCutTransparency();
				image2.anchor = (image2.parentAnchor = 9);
				image.addChild(image2);
			}
			image.anchor = (image.parentAnchor = 18);
			touchBaseElement.addChild(image);
			touchBaseElement.setSizeToChildsBounds();
			return touchBaseElement;
		}

		// Token: 0x0600058A RID: 1418 RVA: 0x0002CE98 File Offset: 0x0002B098
		public virtual void createLevelSelect()
		{
			float num = 0.3f;
			MenuView menuView = (MenuView)new MenuView().initFullscreen();
			int num4 = 126 + this.pack;
			Image image = Image.Image_createWithResIDQuad(num4, 0);
			Image image2 = Image.Image_createWithResIDQuad(num4, 0);
			Vector quadSize = Image.getQuadSize(num4, 0);
			float x = FrameworkTypes.SCREEN_WIDTH / 2f - quadSize.x;
			image.x = x;
			image2.x = FrameworkTypes.SCREEN_WIDTH / 2f;
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
			image5.anchor = (image5.parentAnchor = 18);
			image5.scaleX = (image5.scaleY = 2f);
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
			HBox hBox = this.createTextWithStar(NSObject.NSS(CTRPreferences.getTotalStarsInPack(this.pack).ToString() + "/" + (CTRPreferences.getLevelsInPackCount() * 3).ToString()));
			hBox.x = -20f;
			hBox.y = 20f;
			float of = 55f;
			float of2 = 10f;
			float h = 202.79999f;
			VBox vBox = new VBox().initWithOffsetAlignWidth(of, 2, FrameworkTypes.SCREEN_WIDTH);
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
					hBox2.addChild(this.createButtonForLevelPack(num3++, this.pack));
				}
				vBox.addChild(hBox2);
			}
			Timeline timeline4 = new Timeline().initWithMaxKeyFramesOnTrack(3);
			timeline4.addKeyFrame(KeyFrame.makeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
			timeline4.addKeyFrame(KeyFrame.makeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, num));
			vBox.addTimeline(timeline4);
			hBox.anchor = (hBox.parentAnchor = 12);
			hBox.setName("starText");
			hBox.x = (float)(-(float)base.canvas.xOffsetScaled);
			Timeline timeline5 = new Timeline().initWithMaxKeyFramesOnTrack(2);
			timeline5.addKeyFrame(KeyFrame.makeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
			timeline5.addKeyFrame(KeyFrame.makeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, num));
			hBox.addTimeline(timeline5);
			menuView.addChild(hBox);
			menuView.addChild(vBox);
			Button button = MenuController.createBackButtonWithDelegateID(this, 12);
			button.setName("backButton");
			Timeline timeline6 = new Timeline().initWithMaxKeyFramesOnTrack(2);
			timeline6.addKeyFrame(KeyFrame.makeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
			timeline6.addKeyFrame(KeyFrame.makeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, num));
			button.addTimeline(timeline6);
			button.x = (float)base.canvas.xOffsetScaled;
			menuView.addChild(button);
			this.addViewwithID(menuView, 6);
		}

		// Token: 0x0600058B RID: 1419 RVA: 0x0002D374 File Offset: 0x0002B574
		public override NSObject initWithParent(ViewController p)
		{
			if (base.initWithParent(p) != null)
			{
				this.ddMainMenu = (DelayedDispatcher)new DelayedDispatcher().init();
				this.ddPackSelect = (DelayedDispatcher)new DelayedDispatcher().init();
				this.createMainMenu();
				this.createOptions();
				this.createReset();
				this.createAbout();
				this.createMovieView();
				this.createPackSelect();
				this.createLeaderboards();
				this.createAchievements();
				MapPickerController c = (MapPickerController)new MapPickerController().initWithParent(this);
				this.addChildwithID(c, 0);
			}
			return this;
		}

		// Token: 0x0600058C RID: 1420 RVA: 0x0002D400 File Offset: 0x0002B600
		public override void dealloc()
		{
			this.ddMainMenu.cancelAllDispatches();
			this.ddMainMenu.dealloc();
			this.ddMainMenu = null;
			this.ddPackSelect.cancelAllDispatches();
			this.ddPackSelect.dealloc();
			this.ddPackSelect = null;
			base.dealloc();
		}

		// Token: 0x0600058D RID: 1421 RVA: 0x0002D450 File Offset: 0x0002B650
		public override void activate()
		{
			this.showNextPackStatus = false;
			base.activate();
			CTRRootController cTRRootController = (CTRRootController)Application.sharedRootController();
			this.pack = cTRRootController.getPack();
			if (this.viewToShow == 6)
			{
				this.currentPack = this.pack;
				this.preLevelSelect();
			}
			this.showView(this.viewToShow);
			CTRSoundMgr._stopMusic();
			CTRSoundMgr._playMusic(145);
		}

		// Token: 0x0600058E RID: 1422 RVA: 0x0002D4B8 File Offset: 0x0002B6B8
		public virtual void showNextPack()
		{
			CTRRootController cTRRootController = (CTRRootController)Application.sharedRootController();
			int num = cTRRootController.getPack();
			if (num < CTRPreferences.getPacksCount() - 1)
			{
				this.packContainer.delegateScrollableContainerProtocol = this;
				this.packContainer.moveToScrollPointmoveMultiplier(num + 1, 0.8);
				this.showNextPackStatus = true;
				return;
			}
			this.replayingIntroMovie = false;
			this.packContainer.placeToScrollPoint(cTRRootController.getPack() + 1);
			CTRSoundMgr._stopMusic();
			Application.sharedMovieMgr().delegateMovieMgrDelegate = this;
			Application.sharedMovieMgr().playURL(NSObject.NSS("outro"), !Preferences._getBooleanForKey("MUSIC_ON") && !Preferences._getBooleanForKey("SOUND_ON"));
		}

		// Token: 0x0600058F RID: 1423 RVA: 0x0002D566 File Offset: 0x0002B766
		public override void onChildDeactivated(int n)
		{
			base.onChildDeactivated(n);
			((CTRRootController)Application.sharedRootController()).setSurvival(false);
			this.deactivate();
		}

		// Token: 0x06000590 RID: 1424 RVA: 0x0002D588 File Offset: 0x0002B788
		public virtual void moviePlaybackFinished(NSString url)
		{
			if (this.replayingIntroMovie)
			{
				this.replayingIntroMovie = false;
				this.activateChild(0);
				return;
			}
			if (url != null)
			{
				CTRSoundMgr._playMusic(145);
			}
			if (CTRPreferences.shouldPlayLevelScroll())
			{
				this.packContainer.placeToScrollPoint(CTRPreferences.getPacksCount() - 1);
				this.packContainer.moveToScrollPointmoveMultiplier(0, 0.6);
				CTRPreferences.disablePlayLevelScroll();
			}
			else
			{
				this.packContainer.placeToScrollPoint(CTRPreferences.getLastPack());
			}
			this.showView(5);
			if (url != null && url.hasSuffix("outro"))
			{
				this.packContainer.moveToScrollPointmoveMultiplier(CTRPreferences.getPacksCount(), 0.8);
				this.showGameFinishedPopup();
			}
		}

		// Token: 0x06000591 RID: 1425 RVA: 0x0002D634 File Offset: 0x0002B834
		public virtual void preLevelSelect()
		{
			CTRResourceMgr cTRResourceMgr = Application.sharedResourceMgr();
			int[] array = null;
			switch (this.pack)
			{
			case 0:
				array = ResDataPhoneFull.PACK_GAME_COVER_01;
				break;
			case 1:
				array = ResDataPhoneFull.PACK_GAME_COVER_02;
				break;
			case 2:
				array = ResDataPhoneFull.PACK_GAME_COVER_03;
				break;
			case 3:
				array = ResDataPhoneFull.PACK_GAME_COVER_04;
				break;
			case 4:
				array = ResDataPhoneFull.PACK_GAME_COVER_05;
				break;
			case 5:
				array = ResDataPhoneFull.PACK_GAME_COVER_06;
				break;
			case 6:
				array = ResDataPhoneFull.PACK_GAME_COVER_07;
				break;
			case 7:
				array = ResDataPhoneFull.PACK_GAME_COVER_08;
				break;
			case 8:
				array = ResDataPhoneFull.PACK_GAME_COVER_09;
				break;
			case 9:
				array = ResDataPhoneFull.PACK_GAME_COVER_10;
				break;
			case 10:
				array = ResDataPhoneFull.PACK_GAME_COVER_11;
				break;
			}
			cTRResourceMgr.initLoading();
			cTRResourceMgr.loadPack(array);
			cTRResourceMgr.loadImmediately();
			if (this.getView(6) != null)
			{
				this.deleteView(6);
			}
			this.createLevelSelect();
		}

		// Token: 0x06000592 RID: 1426 RVA: 0x0002D704 File Offset: 0x0002B904
		public virtual void timelineFinished(Timeline t)
		{
			CTRSoundMgr._stopMusic();
			CTRRootController ctrrootController = (CTRRootController)Application.sharedRootController();
			ctrrootController.setPack(this.pack);
			ctrrootController.setLevel(this.level);
			Application.sharedRootController().setViewTransition(-1);
			((MapPickerController)this.getChild(0)).setAutoLoadMap(LevelsList.LEVEL_NAMES[this.pack, this.level]);
			if (this.pack == 0 && this.level == 0 && CTRPreferences.getScoreForPackLevel(0, 0) != 0)
			{
				this.replayingIntroMovie = true;
				this.showView(7);
				CTRSoundMgr._stopMusic();
				Application.sharedMovieMgr().delegateMovieMgrDelegate = this;
				Application.sharedMovieMgr().playURL(NSObject.NSS("intro"), !Preferences._getBooleanForKey("MUSIC_ON") && !Preferences._getBooleanForKey("SOUND_ON"));
				return;
			}
			this.activateChild(0);
		}

		// Token: 0x06000593 RID: 1427 RVA: 0x0002D7D8 File Offset: 0x0002B9D8
		public virtual void recreateOptions()
		{
			this.deleteView(1);
			this.createOptions();
		}

		// Token: 0x06000594 RID: 1428 RVA: 0x0002D7E8 File Offset: 0x0002B9E8
		public virtual void onButtonPressed(int n)
		{
			if (n != -1 && n != 34)
			{
				CTRSoundMgr._playSound(9);
			}
			if (n >= 1000)
			{
				this.level = n - 1000;
				this.activeView().getChildWithName("levelsBox").playTimeline(0);
				this.activeView().getChildWithName("shadow").playTimeline(0);
				this.activeView().getChildWithName("levelsBack").playTimeline(0);
				this.activeView().getChildWithName("starText").playTimeline(0);
				this.activeView().getChildWithName("backButton").playTimeline(0);
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
				this.replayingIntroMovie = false;
				if (CTRPreferences.getScoreForPackLevel(0, 0) == 0)
				{
					this.showView(7);
					CTRSoundMgr._stopMusic();
					Application.sharedMovieMgr().delegateMovieMgrDelegate = this;
					Application.sharedMovieMgr().playURL(NSObject.NSS("intro"), !Preferences._getBooleanForKey("MUSIC_ON") && !Preferences._getBooleanForKey("SOUND_ON"));
					return;
				}
				this.moviePlaybackFinished(null);
				return;
			}
			case 1:
				this.showView(1);
				return;
			case 2:
				((CTRRootController)Application.sharedRootController()).setPack(0);
				this.preLevelSelect();
				Application.sharedRootController().setViewTransition(-1);
				((MapPickerController)this.getChild(0)).setNormalMode();
				this.activateChild(0);
				return;
			case 3:
			{
				CTRSoundMgr._stopMusic();
				this.pack = 0;
				Application.sharedRootController().setViewTransition(-1);
				CTRRootController ctrrootController = (CTRRootController)Application.sharedRootController();
				CTRResourceMgr ctrresourceMgr = Application.sharedResourceMgr();
				ctrresourceMgr.initLoading();
				ctrresourceMgr.loadPack(ResDataPhoneFull.PACK_GAME_COVER_01);
				ctrresourceMgr.loadImmediately();
				ctrrootController.setSurvival(true);
				ctrrootController.setPack(this.pack);
				this.deactivate();
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
				this.aboutContainer.setScroll(MathHelper.vect(0f, 0f));
				this.aboutAutoScroll = true;
				this.showView(3);
				return;
			case 8:
				this.showView(4);
				return;
			case 9:
			case 18:
			case 19:
				break;
			case 10:
				this.showView(1);
				return;
			case 11:
			{
				bool flag7 = Preferences._getBooleanForKey("PREFS_CLICK_TO_CUT");
				Preferences._setBooleanforKey(!flag7, "PREFS_CLICK_TO_CUT", true);
				NSObject.NSS(flag7 ? "off" : "on");
				return;
			}
			case 12:
				this.showView(5);
				this.packContainer.moveToScrollPointmoveMultiplier(this.pack, 0.8);
				return;
			case 13:
			{
				CTRPreferences ctrpreferences = Application.sharedPreferences();
				ctrpreferences.resetToDefaults();
				ctrpreferences.savePreferences();
				this.deleteView(5);
				this.createPackSelect();
				this.showView(1);
				return;
			}
			case 14:
				this.showView(1);
				return;
			case 15:
				((Popup)this.activeView().getChildWithName("popup")).hidePopup();
				return;
			case 16:
				FrameworkTypes.AndroidAPI.openUrl("http://twitter.com/zeptolab");
				return;
			case 17:
				FrameworkTypes.AndroidAPI.openUrl("http://www.facebook.com/cuttherope");
				return;
			case 20:
			{
				int num2 = this.currentPack;
				int num3 = this.scrollPacksLeft + 1;
				this.scrollPacksLeft = num3;
				int sp2 = this.FixScrollPoint(num2 + num3 - this.scrollPacksRight);
				this.packContainer.moveToScrollPointmoveMultiplier(sp2, 0.8);
				this.bScrolling = true;
				return;
			}
			case 21:
			{
				int num4 = this.currentPack;
				int num3 = this.scrollPacksRight + 1;
				this.scrollPacksRight = num3;
				int sp3 = this.FixScrollPoint(num4 - num3 + this.scrollPacksLeft);
				this.packContainer.moveToScrollPointmoveMultiplier(sp3, 0.8);
				this.bScrolling = true;
				break;
			}
			case 22:
			{
				NSString @string = Application.sharedAppSettings().getString(8);
				NSString[] array3 = new NSString[]
				{
					NSObject.NSS("en"),
					NSObject.NSS("ru"),
					NSObject.NSS("de"),
					NSObject.NSS("fr")
				};
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
				ctrresourceMgr2.freePack(ResDataPhoneFull.PACK_LOCALIZATION_MENU);
				ctrresourceMgr2.clearCachedResources();
				ctrresourceMgr2.initLoading();
				ctrresourceMgr2.loadPack(ResDataPhoneFull.PACK_LOCALIZATION_MENU);
				ctrresourceMgr2.loadImmediately();
				this.deleteView(5);
				this.createPackSelect();
				this.deleteView(0);
				this.createMainMenu();
				this.deleteView(4);
				this.createReset();
				this.deleteView(3);
				this.createAbout();
				this.createLeaderboards();
				this.ddMainMenu.callObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(this.selector_recreateOptions), null, 0.01);
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
				if (this.pack != n - 23)
				{
					this.packContainer.moveToScrollPointmoveMultiplier(n - 23, 0.8);
					return;
				}
				CTRPreferences.setLastPack(this.pack);
				bool flag5 = CTRPreferences.getUnlockedForPackLevel(n - 23, 0) == UNLOCKED_STATE.UNLOCKED_STATE_LOCKED && n - 23 != CTRPreferences.getPacksCount();
				if (n != 34 && !flag5)
				{
					this.preLevelSelect();
					this.showView(6);
					return;
				}
				break;
			}
			case 35:
			case 36:
			case 37:
			case 38:
			{
				NSString[] array4 = new NSString[]
				{
					NSObject.NSS("BS"),
					NSObject.NSS("OP"),
					NSObject.NSS("LB"),
					NSObject.NSS("AC")
				};
				NSString[] array5 = new NSString[4];
				array5[0] = NSObject.NSS("BS_BACK_PRESSED");
				array5[1] = NSObject.NSS("OP_BACK_PRESSED");
				NSString nsstring = array4[n - 35];
				NSString nsstring2 = array5[n - 35];
				this.showView(0);
				return;
			}
			case 39:
				Global.XnaGame.Exit();
				return;
			case 40:
				if (this.ep != null)
				{
					this.ep.hidePopup();
					this.ep = null;
					return;
				}
				break;
			case 41:
				this.showYesNoPopup(Application.getString(655505), 39, 40);
				return;
			default:
				return;
			}
		}

		// Token: 0x06000595 RID: 1429 RVA: 0x0002DE84 File Offset: 0x0002C084
		private int FixScrollPoint(int moveToPack)
		{
			if (moveToPack >= this.packContainer.getTotalScrollPoints())
			{
				moveToPack = this.packContainer.getTotalScrollPoints() - 1;
			}
			else if (moveToPack < 0)
			{
				moveToPack = 0;
			}
			return moveToPack;
		}

		// Token: 0x06000596 RID: 1430 RVA: 0x0002DEB0 File Offset: 0x0002C0B0
		public override void update(float delta)
		{
			base.update(delta);
			MovieMgr movieMgr = Application.sharedMovieMgr();
			if (movieMgr.isPlaying())
			{
				movieMgr.update();
				return;
			}
			if (this.activeViewID == 3 && this.aboutAutoScroll)
			{
				Vector scroll = this.aboutContainer.getScroll();
				Vector maxScroll = this.aboutContainer.getMaxScroll();
				scroll.y += 0.5f;
				scroll.y = MathHelper.FIT_TO_BOUNDARIES((double)scroll.y, 0.0, (double)maxScroll.y);
				this.aboutContainer.setScroll(scroll);
				return;
			}
			if (this.activeViewID == 5 && this.ddPackSelect != null)
			{
				this.ddPackSelect.update(delta);
				if (Global.XnaGame.IsKeyPressed(Keys.Left))
				{
					this.onButtonPressed(21);
					return;
				}
				if (Global.XnaGame.IsKeyPressed(Keys.Right))
				{
					this.onButtonPressed(20);
					return;
				}
				if ((Global.XnaGame.IsKeyPressed(Keys.Space) || Global.XnaGame.IsKeyPressed(Keys.Enter)) && !this.bScrolling)
				{
					this.onButtonPressed(23 + this.currentPack);
					return;
				}
			}
			else
			{
				if (this.activeViewID == 0 && this.ddMainMenu != null)
				{
					this.ddMainMenu.update(delta);
					return;
				}
				if (this.activeViewID == 1 && this.ddMainMenu != null)
				{
					this.ddMainMenu.update(delta);
				}
			}
		}

		// Token: 0x06000597 RID: 1431 RVA: 0x0002DFFB File Offset: 0x0002C1FB
		public override bool touchesBeganwithEvent(IList<TouchLocation> touches)
		{
			bool flag = base.touchesBeganwithEvent(touches);
			if (this.activeViewID == 3 && this.aboutAutoScroll)
			{
				this.aboutAutoScroll = false;
			}
			return flag;
		}

		// Token: 0x06000598 RID: 1432 RVA: 0x0002E01C File Offset: 0x0002C21C
		public virtual void timelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
		{
		}

		// Token: 0x06000599 RID: 1433 RVA: 0x0002E020 File Offset: 0x0002C220
		public override void fullscreenToggled(bool isFullscreen)
		{
			this.deleteView(5);
			this.createPackSelect();
			BaseElement childWithName = this.getView(0).getChild(0).getChildWithName("container");
			if (childWithName != null)
			{
				childWithName.x = (float)(-(float)base.canvas.xOffsetScaled);
			}
			BaseElement childWithName2 = this.getView(5).getChild(0).getChildWithName("text");
			if (childWithName2 != null)
			{
				childWithName2.x = -20f - (float)base.canvas.xOffsetScaled;
			}
			for (int i = 0; i < 10; i++)
			{
				View view3 = this.getView(i);
				if (view3 != null)
				{
					BaseElement childWithName3 = view3.getChildWithName("backb");
					if (childWithName3 != null)
					{
						childWithName3.x = (float)base.canvas.xOffsetScaled;
					}
				}
			}
			BaseElement view4 = this.getView(6);
			if (view4 != null)
			{
				view4.getChildWithName("backButton").x = (float)base.canvas.xOffsetScaled;
				view4.getChildWithName("starText").x = (float)(-(float)base.canvas.xOffsetScaled);
			}
		}

		// Token: 0x0600059A RID: 1434 RVA: 0x0002E11E File Offset: 0x0002C31E
		public void selector_recreateOptions(NSObject param)
		{
			this.recreateOptions();
		}

		// Token: 0x0600059B RID: 1435 RVA: 0x0002E128 File Offset: 0x0002C328
		public override bool backButtonPressed()
		{
			int num = this.activeViewID;
			if (num == 0)
			{
				if (this.ep != null)
				{
					this.onButtonPressed(40);
				}
				else
				{
					this.onButtonPressed(41);
				}
			}
			switch (num)
			{
			case 1:
				this.onButtonPressed(36);
				break;
			case 3:
			case 4:
				this.onButtonPressed(10);
				break;
			case 5:
				this.onButtonPressed(35);
				break;
			case 6:
				this.onButtonPressed(12);
				break;
			}
			return true;
		}

		// Token: 0x04000454 RID: 1108
		private const int CHILD_PICKER = 0;

		// Token: 0x04000455 RID: 1109
		public const int VIEW_MAIN_MENU = 0;

		// Token: 0x04000456 RID: 1110
		public const int VIEW_OPTIONS = 1;

		// Token: 0x04000457 RID: 1111
		public const int VIEW_HELP = 2;

		// Token: 0x04000458 RID: 1112
		public const int VIEW_ABOUT = 3;

		// Token: 0x04000459 RID: 1113
		public const int VIEW_RESET = 4;

		// Token: 0x0400045A RID: 1114
		public const int VIEW_PACK_SELECT = 5;

		// Token: 0x0400045B RID: 1115
		public const int VIEW_LEVEL_SELECT = 6;

		// Token: 0x0400045C RID: 1116
		public const int VIEW_MOVIE = 7;

		// Token: 0x0400045D RID: 1117
		public const int VIEW_LEADERBOARDS = 8;

		// Token: 0x0400045E RID: 1118
		public const int VIEW_ACHIEVEMENTS = 9;

		// Token: 0x0400045F RID: 1119
		private const int MM_VIEWS_COUNT = 10;

		// Token: 0x04000460 RID: 1120
		private const int BUTTON_PLAY = 0;

		// Token: 0x04000461 RID: 1121
		private const int BUTTON_OPTIONS = 1;

		// Token: 0x04000462 RID: 1122
		private const int BUTTON_EXTRAS = 2;

		// Token: 0x04000463 RID: 1123
		private const int BUTTON_SURVIVAL = 3;

		// Token: 0x04000464 RID: 1124
		private const int BUTTON_BUYGAME = 4;

		// Token: 0x04000465 RID: 1125
		private const int BUTTON_SOUND_ONOFF = 5;

		// Token: 0x04000466 RID: 1126
		private const int BUTTON_MUSIC_ONOFF = 6;

		// Token: 0x04000467 RID: 1127
		private const int BUTTON_ABOUT = 7;

		// Token: 0x04000468 RID: 1128
		private const int BUTTON_RESET = 8;

		// Token: 0x04000469 RID: 1129
		private const int BUTTON_HELP = 9;

		// Token: 0x0400046A RID: 1130
		private const int BUTTON_BACK_TO_OPTIONS = 10;

		// Token: 0x0400046B RID: 1131
		private const int BUTTON_SWAP_CONTROL = 11;

		// Token: 0x0400046C RID: 1132
		private const int BUTTON_BACK_TO_PACK_SELECT = 12;

		// Token: 0x0400046D RID: 1133
		private const int BUTTON_RESET_YES = 13;

		// Token: 0x0400046E RID: 1134
		private const int BUTTON_RESET_NO = 14;

		// Token: 0x0400046F RID: 1135
		private const int BUTTON_CANT_UNLOCK_OK = 15;

		// Token: 0x04000470 RID: 1136
		private const int BUTTON_TWITTER = 16;

		// Token: 0x04000471 RID: 1137
		private const int BUTTON_FACEBOOK = 17;

		// Token: 0x04000472 RID: 1138
		private const int BUTTON_LEADERBOARDS = 18;

		// Token: 0x04000473 RID: 1139
		private const int BUTTON_ACHIEVEMENTS = 19;

		// Token: 0x04000474 RID: 1140
		private const int BUTTON_NEXT_PACK = 20;

		// Token: 0x04000475 RID: 1141
		private const int BUTTON_PREVIOUS_PACK = 21;

		// Token: 0x04000476 RID: 1142
		private const int BUTTON_LOCALIZATION = 22;

		// Token: 0x04000477 RID: 1143
		private const int BUTTON_PACK1 = 23;

		// Token: 0x04000478 RID: 1144
		private const int BUTTON_PACK2 = 24;

		// Token: 0x04000479 RID: 1145
		private const int BUTTON_PACK3 = 25;

		// Token: 0x0400047A RID: 1146
		private const int BUTTON_PACK4 = 26;

		// Token: 0x0400047B RID: 1147
		private const int BUTTON_PACK5 = 27;

		// Token: 0x0400047C RID: 1148
		private const int BUTTON_PACK6 = 28;

		// Token: 0x0400047D RID: 1149
		private const int BUTTON_PACK7 = 29;

		// Token: 0x0400047E RID: 1150
		private const int BUTTON_PACK8 = 30;

		// Token: 0x0400047F RID: 1151
		private const int BUTTON_PACK9 = 31;

		// Token: 0x04000480 RID: 1152
		private const int BUTTON_PACK10 = 32;

		// Token: 0x04000481 RID: 1153
		private const int BUTTON_PACK11 = 33;

		// Token: 0x04000482 RID: 1154
		private const int BUTTON_SOON = 34;

		// Token: 0x04000483 RID: 1155
		private const int BUTTON_PACK_SELECTION_BACK = 35;

		// Token: 0x04000484 RID: 1156
		private const int BUTTON_OPTIONS_BACK = 36;

		// Token: 0x04000485 RID: 1157
		private const int BUTTON_LEADERBOARDS_BACK = 37;

		// Token: 0x04000486 RID: 1158
		private const int BUTTON_ACHIEVEMENTS_BACK = 38;

		// Token: 0x04000487 RID: 1159
		private const int BUTTON_EXIT_YES = 39;

		// Token: 0x04000488 RID: 1160
		private const int BUTTON_POPUP_HIDE = 40;

		// Token: 0x04000489 RID: 1161
		private const int BUTTON_QIUT = 41;

		// Token: 0x0400048A RID: 1162
		public const int BUTTON_LEVEL_1 = 1000;

		// Token: 0x0400048B RID: 1163
		private const string TWITTER_LINK = "http://twitter.com/zeptolab";

		// Token: 0x0400048C RID: 1164
		private const string FACEBOOK_LINK = "http://www.facebook.com/cuttherope";

		// Token: 0x0400048D RID: 1165
		private const float BOX_OFFSET = -20f;

		// Token: 0x0400048E RID: 1166
		private const float BOX_X_SHIFT = 0f;

		// Token: 0x0400048F RID: 1167
		private const float BOX_Y_SHIFT = 0f;

		// Token: 0x04000490 RID: 1168
		private const float BOX_TOUCH_X_SHIFT = 235f;

		// Token: 0x04000491 RID: 1169
		private const float BOX_TOUCH_WIDTH_SHIFT = 70f;

		// Token: 0x04000492 RID: 1170
		private const float BOX_WIDTH_INCREASE = 1000f;

		// Token: 0x04000493 RID: 1171
		public DelayedDispatcher ddMainMenu;

		// Token: 0x04000494 RID: 1172
		public DelayedDispatcher ddPackSelect;

		// Token: 0x04000495 RID: 1173
		private ScrollableContainer helpContainer;

		// Token: 0x04000496 RID: 1174
		private ScrollableContainer aboutContainer;

		// Token: 0x04000497 RID: 1175
		private ScrollableContainer packContainer;

		// Token: 0x04000498 RID: 1176
		private BaseElement[] boxes = new BaseElement[CTRPreferences.getPacksCount() + 1];

		// Token: 0x04000499 RID: 1177
		private bool showNextPackStatus;

		// Token: 0x0400049A RID: 1178
		private bool aboutAutoScroll;

		// Token: 0x0400049B RID: 1179
		private bool replayingIntroMovie;

		// Token: 0x0400049C RID: 1180
		private int currentPack;

		// Token: 0x0400049D RID: 1181
		private int scrollPacksLeft;

		// Token: 0x0400049E RID: 1182
		private int scrollPacksRight;

		// Token: 0x0400049F RID: 1183
		private bool bScrolling;

		// Token: 0x040004A0 RID: 1184
		private Button nextb;

		// Token: 0x040004A1 RID: 1185
		private Button prevb;

		// Token: 0x040004A2 RID: 1186
		private int pack;

		// Token: 0x040004A3 RID: 1187
		private int level;

		// Token: 0x040004A4 RID: 1188
		public int viewToShow;

		// Token: 0x040004A5 RID: 1189
		private Popup ep;

		// Token: 0x020000CA RID: 202
		public class TouchBaseElement : BaseElement
		{
			// Token: 0x06000681 RID: 1665 RVA: 0x00033C88 File Offset: 0x00031E88
			public override bool onTouchDownXY(float tx, float ty)
			{
				base.onTouchDownXY(tx, ty);
				Rectangle r = FrameworkTypes.MakeRectangle(this.drawX + this.bbc.x, this.drawY + this.bbc.y, (float)this.width + this.bbc.w, (float)this.height + this.bbc.h);
				Rectangle rectangle = MathHelper.rectInRectIntersection(FrameworkTypes.MakeRectangle(0.0, 0.0, (double)FrameworkTypes.SCREEN_WIDTH, (double)FrameworkTypes.SCREEN_HEIGHT), r);
				if (MathHelper.pointInRect(tx, ty, r.x, r.y, r.w, r.h) && (double)rectangle.w > (double)r.w / 2.0)
				{
					this.delegateValue.onButtonPressed(this.bid);
					return true;
				}
				return false;
			}

			// Token: 0x040008FE RID: 2302
			public int bid;

			// Token: 0x040008FF RID: 2303
			public Rectangle bbc;

			// Token: 0x04000900 RID: 2304
			public ButtonDelegate delegateValue;
		}

		// Token: 0x020000CB RID: 203
		public class MonsterSlot : Image
		{
			// Token: 0x06000683 RID: 1667 RVA: 0x00033D6F File Offset: 0x00031F6F
			public static MenuController.MonsterSlot MonsterSlot_create(Texture2D t)
			{
				return (MenuController.MonsterSlot)new MenuController.MonsterSlot().initWithTexture(t);
			}

			// Token: 0x06000684 RID: 1668 RVA: 0x00033D81 File Offset: 0x00031F81
			public static MenuController.MonsterSlot MonsterSlot_createWithResID(int r)
			{
				return MenuController.MonsterSlot.MonsterSlot_create(Application.getTexture(r));
			}

			// Token: 0x06000685 RID: 1669 RVA: 0x00033D8E File Offset: 0x00031F8E
			public static MenuController.MonsterSlot MonsterSlot_createWithResIDQuad(int r, int q)
			{
				MenuController.MonsterSlot monsterSlot = MenuController.MonsterSlot.MonsterSlot_create(Application.getTexture(r));
				monsterSlot.setDrawQuad(q);
				return monsterSlot;
			}

			// Token: 0x06000686 RID: 1670 RVA: 0x00033DA4 File Offset: 0x00031FA4
			public override void draw()
			{
				this.preDraw();
				if (this.quadToDraw == -1)
				{
					GLDrawer.drawImage(this.texture, this.drawX, this.drawY);
				}
				else
				{
					this.drawQuad(this.quadToDraw);
				}
				float num = this.c.getScroll().x;
				Vector preCutSize = Application.getTexture(52).preCutSize;
				if (num >= this.s && num < this.e)
				{
					num -= preCutSize.x + -20f;
					float num2 = num - (this.s + this.e) / 2f;
					OpenGL.setScissorRectangle(250.0 - (double)num2, 0.0, 200.0, (double)FrameworkTypes.SCREEN_HEIGHT);
					this.postDraw();
					OpenGL.setScissorRectangle(this.c.drawX, this.c.drawY, (float)this.c.width, (float)this.c.height);
				}
			}

			// Token: 0x04000901 RID: 2305
			public ScrollableContainer c;

			// Token: 0x04000902 RID: 2306
			public float s;

			// Token: 0x04000903 RID: 2307
			public float e;
		}
	}
}
