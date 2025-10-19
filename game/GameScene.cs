﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.sfe;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using CutTheRope.windows;
using Microsoft.Xna.Framework;

namespace CutTheRope.game
{
	// Token: 0x0200007D RID: 125
	internal class GameScene : BaseElement, TimelineDelegate, ButtonDelegate
	{
		// Token: 0x060004FE RID: 1278 RVA: 0x0001CAE0 File Offset: 0x0001ACE0
		private static void drawCut(Vector fls, Vector frs, Vector start, Vector end, float startSize, float endSize, RGBAColor c, ref Vector le, ref Vector re)
		{
			Vector vector5 = CutTheRope.iframework.helpers.MathHelper.vectNormalize(CutTheRope.iframework.helpers.MathHelper.vectSub(end, start));
			Vector v3 = CutTheRope.iframework.helpers.MathHelper.vectRperp(vector5);
			Vector v4 = CutTheRope.iframework.helpers.MathHelper.vectPerp(vector5);
			Vector vector = (CutTheRope.iframework.helpers.MathHelper.vectEqual(frs, CutTheRope.iframework.helpers.MathHelper.vectUndefined) ? CutTheRope.iframework.helpers.MathHelper.vectAdd(start, CutTheRope.iframework.helpers.MathHelper.vectMult(v3, startSize)) : frs);
			Vector vector2 = (CutTheRope.iframework.helpers.MathHelper.vectEqual(fls, CutTheRope.iframework.helpers.MathHelper.vectUndefined) ? CutTheRope.iframework.helpers.MathHelper.vectAdd(start, CutTheRope.iframework.helpers.MathHelper.vectMult(v4, startSize)) : fls);
			Vector vector3 = CutTheRope.iframework.helpers.MathHelper.vectAdd(end, CutTheRope.iframework.helpers.MathHelper.vectMult(v3, endSize));
			Vector vector4 = CutTheRope.iframework.helpers.MathHelper.vectAdd(end, CutTheRope.iframework.helpers.MathHelper.vectMult(v4, endSize));
			GLDrawer.drawSolidPolygonWOBorder(new float[] { vector2.x, vector2.y, vector.x, vector.y, vector3.x, vector3.y, vector4.x, vector4.y }, 4, c);
			le = vector4;
			re = vector3;
		}

		// Token: 0x060004FF RID: 1279 RVA: 0x0001CBD0 File Offset: 0x0001ADD0
		private static float maxOf4(float v1, float v2, float v3, float v4)
		{
			if (v1 >= v2 && v1 >= v3 && v1 >= v4)
			{
				return v1;
			}
			if (v2 >= v1 && v2 >= v3 && v2 >= v4)
			{
				return v2;
			}
			if (v3 >= v2 && v3 >= v1 && v3 >= v4)
			{
				return v3;
			}
			if (v4 >= v2 && v4 >= v3 && v4 >= v1)
			{
				return v4;
			}
			return -1f;
		}

		// Token: 0x06000500 RID: 1280 RVA: 0x0001CC0F File Offset: 0x0001AE0F
		private static float minOf4(float v1, float v2, float v3, float v4)
		{
			if (v1 <= v2 && v1 <= v3 && v1 <= v4)
			{
				return v1;
			}
			if (v2 <= v1 && v2 <= v3 && v2 <= v4)
			{
				return v2;
			}
			if (v3 <= v2 && v3 <= v1 && v3 <= v4)
			{
				return v3;
			}
			if (v4 <= v2 && v4 <= v3 && v4 <= v1)
			{
				return v4;
			}
			return -1f;
		}

		// Token: 0x06000501 RID: 1281 RVA: 0x0001CC50 File Offset: 0x0001AE50
		public static ToggleButton createGravityButtonWithDelegate(ButtonDelegate d)
		{
			Image u = Image.Image_createWithResIDQuad(78, 56);
			Image d2 = Image.Image_createWithResIDQuad(78, 56);
			Image u2 = Image.Image_createWithResIDQuad(78, 57);
			Image d3 = Image.Image_createWithResIDQuad(78, 57);
			ToggleButton toggleButton = new ToggleButton().initWithUpElement1DownElement1UpElement2DownElement2andID(u, d2, u2, d3, 0);
			toggleButton.delegateButtonDelegate = d;
			return toggleButton;
		}

		// Token: 0x06000502 RID: 1282 RVA: 0x0001CC9B File Offset: 0x0001AE9B
		public virtual bool pointOutOfScreen(ConstraintedPoint p)
		{
			return p.pos.y > this.mapHeight + 400f || p.pos.y < -400f;
		}

		// Token: 0x06000503 RID: 1283 RVA: 0x0001CCCC File Offset: 0x0001AECC
		public override NSObject init()
		{
			if (base.init() != null)
			{
				CTRRootController cTRRootController = (CTRRootController)Application.sharedRootController();
				this.dd = (DelayedDispatcher)new DelayedDispatcher().init();
				this.initialCameraToStarDistance = -1f;
				this.restartState = -1;
				this.aniPool = (AnimationsPool)new AnimationsPool().init();
				this.aniPool.visible = false;
				this.addChild(this.aniPool);
				this.staticAniPool = (AnimationsPool)new AnimationsPool().init();
				this.staticAniPool.visible = false;
				this.addChild(this.staticAniPool);
				this.camera = new Camera2D().initWithSpeedandType(14f, CAMERA_TYPE.CAMERA_SPEED_DELAY);
				int textureResID = 104 + cTRRootController.getPack() * 2;
				this.back = new TileMap().initWithRowsColumns(1, 1);
				this.back.setRepeatHorizontally(TileMap.Repeat.REPEAT_NONE);
				this.back.setRepeatVertically(TileMap.Repeat.REPEAT_ALL);
				this.back.addTileQuadwithID(Application.getTexture(textureResID), 0, 0);
				this.back.fillStartAtRowColumnRowsColumnswithTile(0, 0, 1, 1, 0);
				if (base.canvas.isFullscreen)
				{
					this.back.scaleX = (float)Global.ScreenSizeManager.ScreenWidth / (float)base.canvas.backingWidth;
				}
				this.back.scaleX *= 1.25f;
				this.back.scaleY *= 1.25f;
				for (int i = 0; i < 3; i++)
				{
					this.hudStar[i] = Animation.Animation_createWithResID(79);
					this.hudStar[i].doRestoreCutTransparency();
					this.hudStar[i].addAnimationDelayLoopFirstLast(0.05, Timeline.LoopType.TIMELINE_NO_LOOP, 0, 10);
					this.hudStar[i].setPauseAtIndexforAnimation(10, 0);
					this.hudStar[i].x = (float)(this.hudStar[i].width * i + base.canvas.xOffsetScaled);
					this.hudStar[i].y = 0f;
					this.addChild(this.hudStar[i]);
				}
				for (int j = 0; j < 5; j++)
				{
					this.fingerCuts[j] = (DynamicArray)new DynamicArray().init();
					this.fingerCuts[j].retain();
				}
				this.clickToCut = Preferences._getBooleanForKey("PREFS_CLICK_TO_CUT");
			}
			return this;
		}

		// Token: 0x06000504 RID: 1284 RVA: 0x0001CF23 File Offset: 0x0001B123
		public virtual void xmlLoaderFinishedWithfromwithSuccess(XMLNode rootNode, NSString url, bool success)
		{
			((CTRRootController)Application.sharedRootController()).setMap(rootNode);
			if (this.animateRestartDim)
			{
				this.animateLevelRestart();
				return;
			}
			this.restart();
		}

		// Token: 0x06000505 RID: 1285 RVA: 0x0001CF4C File Offset: 0x0001B14C
		public virtual void reload()
		{
			this.dd.cancelAllDispatches();
			CTRRootController cTRRootController = (CTRRootController)Application.sharedRootController();
			if (cTRRootController.isPicker())
			{
				this.xmlLoaderFinishedWithfromwithSuccess(XMLNode.parseXML("mappicker://reload"), NSObject.NSS("mappicker://reload"), true);
				return;
			}
			int pack = cTRRootController.getPack();
			int level = cTRRootController.getLevel();
			this.xmlLoaderFinishedWithfromwithSuccess(XMLNode.parseXML("maps/" + LevelsList.LEVEL_NAMES[pack, level].ToString()), NSObject.NSS("maps/" + LevelsList.LEVEL_NAMES[pack, level].ToString()), true);
		}

		// Token: 0x06000506 RID: 1286 RVA: 0x0001CFE8 File Offset: 0x0001B1E8
		public virtual void loadNextMap()
		{
			this.dd.cancelAllDispatches();
			this.initialCameraToStarDistance = -1f;
			this.animateRestartDim = false;
			CTRRootController cTRRootController = (CTRRootController)Application.sharedRootController();
			if (cTRRootController.isPicker())
			{
				this.xmlLoaderFinishedWithfromwithSuccess(XMLNode.parseXML("mappicker://next"), NSObject.NSS("mappicker://next"), true);
				return;
			}
			int pack = cTRRootController.getPack();
			int level = cTRRootController.getLevel();
			if (level < CTRPreferences.getLevelsInPackCount() - 1)
			{
				cTRRootController.setLevel(++level);
				cTRRootController.setMapName(LevelsList.LEVEL_NAMES[pack, level]);
				this.xmlLoaderFinishedWithfromwithSuccess(XMLNode.parseXML("maps/" + LevelsList.LEVEL_NAMES[pack, level].ToString()), NSObject.NSS("maps/" + LevelsList.LEVEL_NAMES[pack, level].ToString()), true);
			}
		}

		// Token: 0x06000507 RID: 1287 RVA: 0x0001D0BD File Offset: 0x0001B2BD
		public virtual void restart()
		{
			this.hide();
			this.show();
		}

		// Token: 0x06000508 RID: 1288 RVA: 0x0001D0CC File Offset: 0x0001B2CC
		public virtual void createEarthImageWithOffsetXY(float xs, float ys)
		{
			Image image = Image.Image_createWithResIDQuad(78, 58);
			image.anchor = 18;
			Timeline timeline = new Timeline().initWithMaxKeyFramesOnTrack(2);
			timeline.addKeyFrame(KeyFrame.makeRotation(0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
			timeline.addKeyFrame(KeyFrame.makeRotation(180.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.3));
			image.addTimelinewithID(timeline, 1);
			timeline = new Timeline().initWithMaxKeyFramesOnTrack(2);
			timeline.addKeyFrame(KeyFrame.makeRotation(180.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
			timeline.addKeyFrame(KeyFrame.makeRotation(0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.3));
			image.addTimelinewithID(timeline, 0);
			Image.setElementPositionWithQuadOffset(image, 118, 1);
			if (base.canvas.isFullscreen)
			{
				int screenWidth = Global.ScreenSizeManager.ScreenWidth;
			}
			image.scaleX = 0.8f;
			image.scaleY = 0.8f;
			image.x += xs;
			image.y += ys;
			this.earthAnims.addObject(image);
		}

		// Token: 0x06000509 RID: 1289 RVA: 0x0001D1EC File Offset: 0x0001B3EC
		public virtual bool shouldSkipTutorialElement(XMLNode c)
		{
			CTRRootController cTRRootController = (CTRRootController)Application.sharedRootController();
			if (cTRRootController.getPack() == 0 && cTRRootController.getLevel() == 1)
			{
				return true;
			}
			NSString @string = Application.sharedAppSettings().getString(8);
			NSString nSString = c["locale"];
			if (@string.isEqualToString("en") || @string.isEqualToString("ru") || @string.isEqualToString("de") || @string.isEqualToString("fr"))
			{
				if (!nSString.isEqualToString(@string))
				{
					return true;
				}
			}
			else if (!nSString.isEqualToString("en"))
			{
				return true;
			}
			return false;
		}

		// Token: 0x0600050A RID: 1290 RVA: 0x0001D27E File Offset: 0x0001B47E
		public virtual void showGreeting()
		{
			this.target.playAnimationtimeline(101, 10);
		}

		// Token: 0x0600050B RID: 1291 RVA: 0x0001D290 File Offset: 0x0001B490
		public override void show()
		{
			CTRSoundMgr.EnableLoopedSounds(true);
			this.aniPool.removeAllChilds();
			this.staticAniPool.removeAllChilds();
			this.gravityButton = null;
			this.gravityTouchDown = -1;
			this.twoParts = 2;
			this.partsDist = 0f;
			this.targetSock = null;
			CTRSoundMgr._stopLoopedSounds();
			CTRRootController cTRRootController = (CTRRootController)Application.sharedRootController();
			XMLNode map = cTRRootController.getMap();
			this.bungees = (DynamicArray)new DynamicArray().init();
			this.razors = (DynamicArray)new DynamicArray().init();
			this.spikes = (DynamicArray)new DynamicArray().init();
			this.stars = (DynamicArray)new DynamicArray().init();
			this.bubbles = (DynamicArray)new DynamicArray().init();
			this.pumps = (DynamicArray)new DynamicArray().init();
			this.socks = (DynamicArray)new DynamicArray().init();
			this.tutorialImages = (DynamicArray)new DynamicArray().init();
			this.tutorials = (DynamicArray)new DynamicArray().init();
			this.bouncers = (DynamicArray)new DynamicArray().init();
			this.rotatedCircles = (DynamicArray)new DynamicArray().init();
			this.pollenDrawer = (PollenDrawer)new PollenDrawer().init();
			this.star = (ConstraintedPoint)new ConstraintedPoint().init();
			this.star.setWeight(1f);
			this.starL = (ConstraintedPoint)new ConstraintedPoint().init();
			this.starL.setWeight(1f);
			this.starR = (ConstraintedPoint)new ConstraintedPoint().init();
			this.starR.setWeight(1f);
			this.candy = GameObject.GameObject_createWithResIDQuad(63, 0);
			this.candy.doRestoreCutTransparency();
			this.candy.retain();
			this.candy.anchor = 18;
			this.candy.bb = FrameworkTypes.MakeRectangle(142f, 157f, 112f, 104f);
			this.candy.passTransformationsToChilds = false;
			this.candy.scaleX = (this.candy.scaleY = 0.71f);
			this.candyMain = GameObject.GameObject_createWithResIDQuad(63, 1);
			this.candyMain.doRestoreCutTransparency();
			this.candyMain.anchor = (this.candyMain.parentAnchor = 18);
			this.candy.addChild(this.candyMain);
			this.candyMain.scaleX = (this.candyMain.scaleY = 0.71f);
			this.candyTop = GameObject.GameObject_createWithResIDQuad(63, 2);
			this.candyTop.doRestoreCutTransparency();
			this.candyTop.anchor = (this.candyTop.parentAnchor = 18);
			this.candy.addChild(this.candyTop);
			this.candyTop.scaleX = (this.candyTop.scaleY = 0.71f);
			this.candyBlink = Animation.Animation_createWithResID(63);
			this.candyBlink.addAnimationWithIDDelayLoopFirstLast(0, 0.07f, Timeline.LoopType.TIMELINE_NO_LOOP, 8, 17);
			this.candyBlink.addAnimationWithIDDelayLoopCountSequence(1, 0.3f, Timeline.LoopType.TIMELINE_NO_LOOP, 2, 18, new List<int> { 18 });
			Timeline timeline7 = this.candyBlink.getTimeline(1);
			timeline7.addKeyFrame(KeyFrame.makeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
			timeline7.addKeyFrame(KeyFrame.makeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.2));
			this.candyBlink.visible = false;
			this.candyBlink.anchor = (this.candyBlink.parentAnchor = 18);
			this.candyBlink.scaleX = (this.candyBlink.scaleY = 0.71f);
			this.candy.addChild(this.candyBlink);
			this.candyBubbleAnimation = Animation.Animation_createWithResID(72);
			this.candyBubbleAnimation.x = this.candy.x;
			this.candyBubbleAnimation.y = this.candy.y;
			this.candyBubbleAnimation.parentAnchor = (this.candyBubbleAnimation.anchor = 18);
			this.candyBubbleAnimation.addAnimationDelayLoopFirstLast(0.05, Timeline.LoopType.TIMELINE_REPLAY, 0, 12);
			this.candyBubbleAnimation.playTimeline(0);
			this.candy.addChild(this.candyBubbleAnimation);
			this.candyBubbleAnimation.visible = false;
			float num = 3f;
			float num2 = 0f;
			float num3 = 0f;
			for (int i = 0; i < 3; i++)
			{
				Timeline timeline2 = this.hudStar[i].getCurrentTimeline();
				if (timeline2 != null)
				{
					timeline2.stopTimeline();
				}
				this.hudStar[i].setDrawQuad(0);
			}
			int num4 = 0;
			int num5 = 0;
			List<XMLNode> list = map.childs();
			foreach (XMLNode xmlnode in list)
			{
				foreach (XMLNode item2 in xmlnode.childs())
				{
					if (item2.Name == "map")
					{
						this.mapWidth = item2["width"].floatValue();
						this.mapHeight = item2["height"].floatValue();
						num3 = (2560f - this.mapWidth * num) / 2f;
						this.mapWidth *= num;
						this.mapHeight *= num;
						if (cTRRootController.getPack() == 7)
						{
							this.earthAnims = (DynamicArray)new DynamicArray().init();
							if (this.mapWidth > FrameworkTypes.SCREEN_WIDTH)
							{
								this.createEarthImageWithOffsetXY((float)this.back.width, 0f);
							}
							if (this.mapHeight > FrameworkTypes.SCREEN_HEIGHT)
							{
								this.createEarthImageWithOffsetXY(0f, (float)this.back.height);
							}
							this.createEarthImageWithOffsetXY(0f, 0f);
						}
					}
					else if (item2.Name == "gameDesign")
					{
						num4 = item2["mapOffsetX"].intValue();
						num5 = item2["mapOffsetY"].intValue();
						this.special = item2["special"].intValue();
						this.ropePhysicsSpeed = item2["ropePhysicsSpeed"].floatValue();
						this.nightLevel = item2["nightLevel"].isEqualToString("true");
						this.twoParts = ((!item2["twoParts"].isEqualToString("true")) ? 2 : 0);
						this.ropePhysicsSpeed *= 1.4f;
					}
					else if (item2.Name == "candyL")
					{
						this.starL.pos.x = (float)item2["x"].intValue() * num + num3 + (float)num4;
						this.starL.pos.y = (float)item2["y"].intValue() * num + num2 + (float)num5;
						this.candyL = GameObject.GameObject_createWithResIDQuad(63, 19);
						this.candyL.scaleX = (this.candyL.scaleY = 0.71f);
						this.candyL.passTransformationsToChilds = false;
						this.candyL.doRestoreCutTransparency();
						this.candyL.retain();
						this.candyL.anchor = 18;
						this.candyL.x = this.starL.pos.x;
						this.candyL.y = this.starL.pos.y;
						this.candyL.bb = FrameworkTypes.MakeRectangle(155.0, 176.0, 88.0, 76.0);
					}
					else if (item2.Name == "candyR")
					{
						this.starR.pos.x = (float)item2["x"].intValue() * num + num3 + (float)num4;
						this.starR.pos.y = (float)item2["y"].intValue() * num + num2 + (float)num5;
						this.candyR = GameObject.GameObject_createWithResIDQuad(63, 20);
						this.candyR.scaleX = (this.candyR.scaleY = 0.71f);
						this.candyR.passTransformationsToChilds = false;
						this.candyR.doRestoreCutTransparency();
						this.candyR.retain();
						this.candyR.anchor = 18;
						this.candyR.x = this.starR.pos.x;
						this.candyR.y = this.starR.pos.y;
						this.candyR.bb = FrameworkTypes.MakeRectangle(155.0, 176.0, 88.0, 76.0);
					}
					else if (item2.Name == "candy")
					{
						this.star.pos.x = (float)item2["x"].intValue() * num + num3 + (float)num4;
						this.star.pos.y = (float)item2["y"].intValue() * num + num2 + (float)num5;
					}
				}
			}
			foreach (XMLNode xmlnode2 in list)
			{
				foreach (XMLNode item3 in xmlnode2.childs())
				{
					if (item3.Name == "gravitySwitch")
					{
						this.gravityButton = GameScene.createGravityButtonWithDelegate(this);
						this.gravityButton.visible = false;
						this.gravityButton.touchable = false;
						this.addChild(this.gravityButton);
						this.gravityButton.x = (float)item3["x"].intValue() * num + num3 + (float)num4;
						this.gravityButton.y = (float)item3["y"].intValue() * num + num2 + (float)num5;
						this.gravityButton.anchor = 18;
					}
					else if (item3.Name == "star")
					{
						Star star = Star.Star_createWithResID(78);
						star.x = (float)item3["x"].intValue() * num + num3 + (float)num4;
						star.y = (float)item3["y"].intValue() * num + num2 + (float)num5;
						star.timeout = item3["timeout"].floatValue();
						star.createAnimations();
						star.bb = FrameworkTypes.MakeRectangle(70.0, 64.0, 82.0, 82.0);
						star.parseMover(item3);
						star.update(0f);
						this.stars.addObject(star);
					}
					else if (item3.Name == "tutorialText")
					{
						if (!this.shouldSkipTutorialElement(item3))
						{
							GameScene.TutorialText tutorialText = (GameScene.TutorialText)new GameScene.TutorialText().initWithFont(Application.getFont(4));
							tutorialText.color = RGBAColor.MakeRGBA(1.0, 1.0, 1.0, 0.9);
							tutorialText.x = (float)item3["x"].intValue() * num + num3 + (float)num4;
							tutorialText.y = (float)item3["y"].intValue() * num + num2 + (float)num5;
							tutorialText.special = item3["special"].intValue();
							tutorialText.setAlignment(2);
							NSString newString = item3["text"];
							tutorialText.setStringandWidth(newString, (float)item3["width"].intValue() * num);
							tutorialText.color = RGBAColor.transparentRGBA;
							float num6 = ((tutorialText.special == 3) ? 12f : 0f);
							Timeline timeline3 = new Timeline().initWithMaxKeyFramesOnTrack(4);
							timeline3.addKeyFrame(KeyFrame.makeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, num6));
							timeline3.addKeyFrame(KeyFrame.makeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1.0));
							if (cTRRootController.getPack() == 0 && cTRRootController.getLevel() == 0)
							{
								timeline3.addKeyFrame(KeyFrame.makeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 10.0));
							}
							else
							{
								timeline3.addKeyFrame(KeyFrame.makeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 5.0));
							}
							timeline3.addKeyFrame(KeyFrame.makeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
							tutorialText.addTimelinewithID(timeline3, 0);
							if (tutorialText.special == 0 || tutorialText.special == 3)
							{
								tutorialText.playTimeline(0);
							}
							this.tutorials.addObject(tutorialText);
						}
					}
					else if (item3.Name == "tutorial01" || item3.Name == "tutorial02" || item3.Name == "tutorial03" || item3.Name == "tutorial04" || item3.Name == "tutorial05" || item3.Name == "tutorial06" || item3.Name == "tutorial07" || item3.Name == "tutorial08" || item3.Name == "tutorial09" || item3.Name == "tutorial10" || item3.Name == "tutorial11")
					{
						if (!this.shouldSkipTutorialElement(item3))
						{
							int q = new NSString(item3.Name.Substring(8)).intValue() - 1;
							GameScene.GameObjectSpecial gameObjectSpecial = GameScene.GameObjectSpecial.GameObjectSpecial_createWithResIDQuad(84, q);
							gameObjectSpecial.color = RGBAColor.transparentRGBA;
							gameObjectSpecial.x = (float)item3["x"].intValue() * num + num3 + (float)num4;
							gameObjectSpecial.y = (float)item3["y"].intValue() * num + num2 + (float)num5;
							gameObjectSpecial.rotation = (float)item3["angle"].intValue();
							gameObjectSpecial.special = item3["special"].intValue();
							gameObjectSpecial.parseMover(item3);
							float num7 = ((gameObjectSpecial.special == 3 || gameObjectSpecial.special == 4) ? 12f : 0f);
							Timeline timeline4 = new Timeline().initWithMaxKeyFramesOnTrack(4);
							timeline4.addKeyFrame(KeyFrame.makeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, num7));
							timeline4.addKeyFrame(KeyFrame.makeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1.0));
							if (cTRRootController.getPack() == 0 && cTRRootController.getLevel() == 0)
							{
								timeline4.addKeyFrame(KeyFrame.makeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 10.0));
							}
							else
							{
								timeline4.addKeyFrame(KeyFrame.makeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 5.2));
							}
							timeline4.addKeyFrame(KeyFrame.makeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
							gameObjectSpecial.addTimelinewithID(timeline4, 0);
							if (gameObjectSpecial.special == 0 || gameObjectSpecial.special == 3)
							{
								gameObjectSpecial.playTimeline(0);
							}
							if (gameObjectSpecial.special == 2 || gameObjectSpecial.special == 4)
							{
								Timeline timeline5 = new Timeline().initWithMaxKeyFramesOnTrack(12);
								for (int j = 0; j < 2; j++)
								{
									timeline5.addKeyFrame(KeyFrame.makeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, (j == 1) ? 0f : num7));
									timeline5.addKeyFrame(KeyFrame.makeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
									timeline5.addKeyFrame(KeyFrame.makeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1.0));
									timeline5.addKeyFrame(KeyFrame.makeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1.1));
									timeline5.addKeyFrame(KeyFrame.makeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
									timeline5.addKeyFrame(KeyFrame.makePos((double)gameObjectSpecial.x, (double)gameObjectSpecial.y, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, (double)((j == 1) ? 0f : num7)));
									timeline5.addKeyFrame(KeyFrame.makePos((double)gameObjectSpecial.x, (double)gameObjectSpecial.y, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
									timeline5.addKeyFrame(KeyFrame.makePos((double)gameObjectSpecial.x, (double)gameObjectSpecial.y, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1.0));
									timeline5.addKeyFrame(KeyFrame.makePos((double)gameObjectSpecial.x + 230.0, (double)gameObjectSpecial.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.5));
									timeline5.addKeyFrame(KeyFrame.makePos((double)gameObjectSpecial.x + 440.0, (double)gameObjectSpecial.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.5));
									timeline5.addKeyFrame(KeyFrame.makePos((double)gameObjectSpecial.x + 440.0, (double)gameObjectSpecial.y, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.6));
								}
								timeline5.setTimelineLoopType(Timeline.LoopType.TIMELINE_NO_LOOP);
								gameObjectSpecial.addTimelinewithID(timeline5, 1);
								gameObjectSpecial.playTimeline(1);
								gameObjectSpecial.rotation = 10f;
							}
							this.tutorialImages.addObject(gameObjectSpecial);
						}
					}
					else if (item3.Name == "bubble")
					{
						int q2 = CutTheRope.iframework.helpers.MathHelper.RND_RANGE(1, 3);
						Bubble bubble = Bubble.Bubble_createWithResIDQuad(75, q2);
						bubble.doRestoreCutTransparency();
						bubble.bb = FrameworkTypes.MakeRectangle(48.0, 48.0, 152.0, 152.0);
						bubble.initial_x = (bubble.x = (float)item3["x"].intValue() * num + num3 + (float)num4);
						bubble.initial_y = (bubble.y = (float)item3["y"].intValue() * num + num2 + (float)num5);
						bubble.initial_rotation = 0f;
						bubble.initial_rotatedCircle = null;
						bubble.anchor = 18;
						bubble.popped = false;
						Image image = Image.Image_createWithResIDQuad(75, 0);
						image.doRestoreCutTransparency();
						image.parentAnchor = (image.anchor = 18);
						bubble.addChild(image);
						this.bubbles.addObject(bubble);
					}
					else if (item3.Name == "pump")
					{
						Pump pump = Pump.Pump_createWithResID(83);
						pump.doRestoreCutTransparency();
						pump.addAnimationWithDelayLoopedCountSequence(0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 4, 1, new List<int> { 2, 3, 0 });
						pump.bb = FrameworkTypes.MakeRectangle(300f, 300f, 175f, 175f);
						pump.initial_x = (pump.x = (float)item3["x"].intValue() * num + num3 + (float)num4);
						pump.initial_y = (pump.y = (float)item3["y"].intValue() * num + num2 + (float)num5);
						pump.initial_rotation = 0f;
						pump.initial_rotatedCircle = null;
						pump.rotation = item3["angle"].floatValue() + 90f;
						pump.updateRotation();
						pump.anchor = 18;
						this.pumps.addObject(pump);
					}
					else if (item3.Name == "sock")
					{
						Sock sock = Sock.Sock_createWithResID(85);
						sock.createAnimations();
						sock.scaleX = (sock.scaleY = 0.7f);
						sock.doRestoreCutTransparency();
						sock.x = (float)item3["x"].intValue() * num + num3 + (float)num4;
						sock.y = (float)item3["y"].intValue() * num + num2 + (float)num5;
						sock.group = item3["group"].intValue();
						sock.anchor = 10;
						sock.rotationCenterY -= (float)sock.height / 2f - 85f;
						if (sock.group == 0)
						{
							sock.setDrawQuad(0);
						}
						else
						{
							sock.setDrawQuad(1);
						}
						sock.state = Sock.SOCK_IDLE;
						sock.parseMover(item3);
						sock.rotation += 90f;
						if (sock.mover != null)
						{
							sock.mover.angle_ += 90.0;
							sock.mover.angle_initial = sock.mover.angle_;
							if (cTRRootController.getPack() == 3 && cTRRootController.getLevel() == 24)
							{
								sock.mover.use_angle_initial = true;
							}
						}
						sock.updateRotation();
						this.socks.addObject(sock);
					}
					else if (item3.Name == "spike1" || item3.Name == "spike2" || item3.Name == "spike3" || item3.Name == "spike4" || item3.Name == "electro")
					{
						float px = (float)item3["x"].intValue() * num + num3 + (float)num4;
						float py = (float)item3["y"].intValue() * num + num2 + (float)num5;
						int w = item3["size"].intValue();
						double an = (double)item3["angle"].intValue();
						NSString nSString2 = item3["toggled"];
						int num8 = -1;
						if (nSString2.length() > 0)
						{
							num8 = (nSString2.isEqualToString("false") ? (-1) : nSString2.intValue());
						}
						Spikes spikes = (Spikes)new Spikes().initWithPosXYWidthAndAngleToggled(px, py, w, an, num8);
						spikes.parseMover(item3);
						if (num8 != 0)
						{
							spikes.delegateRotateAllSpikesWithID = new Spikes.rotateAllSpikesWithID(this.rotateAllSpikesWithID);
						}
						if (item3.Name == "electro")
						{
							spikes.electro = true;
							spikes.initialDelay = item3["initialDelay"].floatValue();
							spikes.onTime = item3["onTime"].floatValue();
							spikes.offTime = item3["offTime"].floatValue();
							spikes.electroTimer = 0f;
							spikes.turnElectroOff();
							spikes.electroTimer += spikes.initialDelay;
							spikes.updateRotation();
						}
						else
						{
							spikes.electro = false;
						}
						this.spikes.addObject(spikes);
					}
					else if (item3.Name == "rotatedCircle")
					{
						float num9 = (float)item3["x"].intValue() * num + num3 + (float)num4;
						float num10 = (float)item3["y"].intValue() * num + num2 + (float)num5;
						float num11 = (float)item3["size"].intValue();
						float d = (float)item3["handleAngle"].intValue();
						bool hasOneHandle = item3["oneHandle"].boolValue();
						RotatedCircle rotatedCircle = (RotatedCircle)new RotatedCircle().init();
						rotatedCircle.anchor = 18;
						rotatedCircle.x = num9;
						rotatedCircle.y = num10;
						rotatedCircle.rotation = d;
						rotatedCircle.inithanlde1 = (rotatedCircle.handle1 = CutTheRope.iframework.helpers.MathHelper.vect(rotatedCircle.x - num11 * num, rotatedCircle.y));
						rotatedCircle.inithanlde2 = (rotatedCircle.handle2 = CutTheRope.iframework.helpers.MathHelper.vect(rotatedCircle.x + num11 * num, rotatedCircle.y));
						rotatedCircle.handle1 = CutTheRope.iframework.helpers.MathHelper.vectRotateAround(rotatedCircle.handle1, (double)CutTheRope.iframework.helpers.MathHelper.DEGREES_TO_RADIANS(d), rotatedCircle.x, rotatedCircle.y);
						rotatedCircle.handle2 = CutTheRope.iframework.helpers.MathHelper.vectRotateAround(rotatedCircle.handle2, (double)CutTheRope.iframework.helpers.MathHelper.DEGREES_TO_RADIANS(d), rotatedCircle.x, rotatedCircle.y);
						rotatedCircle.setSize(num11);
						rotatedCircle.setHasOneHandle(hasOneHandle);
						this.rotatedCircles.addObject(rotatedCircle);
					}
					else if (item3.Name == "bouncer1" || item3.Name == "bouncer2")
					{
						float px2 = (float)item3["x"].intValue() * num + num3 + (float)num4;
						float py2 = (float)item3["y"].intValue() * num + num2 + (float)num5;
						int w2 = item3["size"].intValue();
						double an2 = (double)item3["angle"].intValue();
						Bouncer bouncer = (Bouncer)new Bouncer().initWithPosXYWidthAndAngle(px2, py2, w2, an2);
						bouncer.parseMover(item3);
						this.bouncers.addObject(bouncer);
					}
					else if (item3.Name == "grab")
					{
						float hx = (float)item3["x"].intValue() * num + num3 + (float)num4;
						float hy = (float)item3["y"].intValue() * num + num2 + (float)num5;
						float len = (float)item3["length"].intValue() * num;
						float num12 = item3["radius"].floatValue();
						bool wheel = item3["wheel"].isEqualToString("true");
						float k = item3["moveLength"].floatValue() * num;
						bool v = item3["moveVertical"].isEqualToString("true");
						float o = item3["moveOffset"].floatValue() * num;
						bool spider = item3["spider"].isEqualToString("true");
						bool flag = item3["part"].isEqualToString("L");
						bool flag2 = item3["hidePath"].isEqualToString("true");
						Grab grab = (Grab)new Grab().init();
						grab.initial_x = (grab.x = hx);
						grab.initial_y = (grab.y = hy);
						grab.initial_rotation = 0f;
						grab.wheel = wheel;
						grab.setSpider(spider);
						grab.parseMover(item3);
						if (grab.mover != null)
						{
							grab.setBee();
							if (!flag2)
							{
								int num13 = 3;
								bool flag3 = item3["path"].hasPrefix(NSObject.NSS("R"));
								for (int l = 0; l < grab.mover.pathLen - 1; l++)
								{
									if (!flag3 || l % num13 == 0)
									{
										this.pollenDrawer.fillWithPolenFromPathIndexToPathIndexGrab(l, l + 1, grab);
									}
								}
								if (grab.mover.pathLen > 2)
								{
									this.pollenDrawer.fillWithPolenFromPathIndexToPathIndexGrab(0, grab.mover.pathLen - 1, grab);
								}
							}
						}
						if (num12 != -1f)
						{
							num12 *= num;
						}
						if (num12 == -1f)
						{
							ConstraintedPoint constraintedPoint = this.star;
							if (this.twoParts != 2)
							{
								constraintedPoint = (flag ? this.starL : this.starR);
							}
							Bungee bungee = (Bungee)new Bungee().initWithHeadAtXYTailAtTXTYandLength(null, hx, hy, constraintedPoint, constraintedPoint.pos.x, constraintedPoint.pos.y, len);
							bungee.bungeeAnchor.pin = bungee.bungeeAnchor.pos;
							grab.setRope(bungee);
						}
						grab.setRadius(num12);
						grab.setMoveLengthVerticalOffset(k, v, o);
						this.bungees.addObject(grab);
					}
					else if (item3.Name == "target")
					{
						int pack = ((CTRRootController)Application.sharedRootController()).getPack();
						this.support = Image.Image_createWithResIDQuad(100, pack);
						this.support.retain();
						this.support.doRestoreCutTransparency();
						this.support.anchor = 18;
						this.target = CharAnimations.CharAnimations_createWithResID(80);
						this.target.doRestoreCutTransparency();
						this.target.passColorToChilds = false;
						NSString nSString3 = item3["x"];
						this.target.x = (this.support.x = (float)nSString3.intValue() * num + num3 + (float)num4);
						NSString nSString4 = item3["y"];
						this.target.y = (this.support.y = (float)nSString4.intValue() * num + num2 + (float)num5);
						this.target.addImage(101);
						this.target.addImage(102);
						this.target.bb = FrameworkTypes.MakeRectangle(264.0, 350.0, 108.0, 2.0);
						this.target.addAnimationWithIDDelayLoopFirstLast(0, 0.05f, Timeline.LoopType.TIMELINE_REPLAY, 0, 18);
						this.target.addAnimationWithIDDelayLoopFirstLast(1, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 43, 67);
						int num14 = 68;
						this.target.addAnimationWithIDDelayLoopCountSequence(2, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 32, num14, new List<int>
						{
							num14 + 1,
							num14 + 2,
							num14 + 3,
							num14 + 4,
							num14 + 5,
							num14 + 6,
							num14 + 7,
							num14 + 8,
							num14 + 9,
							num14 + 10,
							num14 + 11,
							num14 + 12,
							num14 + 13,
							num14 + 14,
							num14 + 15,
							num14,
							num14 + 1,
							num14 + 2,
							num14 + 3,
							num14 + 4,
							num14 + 5,
							num14 + 6,
							num14 + 7,
							num14 + 8,
							num14 + 9,
							num14 + 10,
							num14 + 11,
							num14 + 12,
							num14 + 13,
							num14 + 14,
							num14 + 15
						});
						this.target.addAnimationWithIDDelayLoopFirstLast(7, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 19, 27);
						this.target.addAnimationWithIDDelayLoopFirstLast(8, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 28, 31);
						this.target.addAnimationWithIDDelayLoopFirstLast(9, 0.05f, Timeline.LoopType.TIMELINE_REPLAY, 32, 40);
						this.target.addAnimationWithIDDelayLoopFirstLast(6, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 28, 31);
						this.target.addAnimationWithIDDelayLoopFirstLast(101, 10, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 47, 76);
						this.target.addAnimationWithIDDelayLoopFirstLast(101, 3, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 0, 19);
						this.target.addAnimationWithIDDelayLoopFirstLast(101, 4, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 20, 46);
						this.target.addAnimationWithIDDelayLoopFirstLast(102, 5, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 0, 12);
						this.target.switchToAnimationatEndOfAnimationDelay(9, 6, 0.05f);
						this.target.switchToAnimationatEndOfAnimationDelay(101, 4, 80, 8, 0.05f);
						this.target.switchToAnimationatEndOfAnimationDelay(80, 0, 101, 10, 0.05f);
						this.target.switchToAnimationatEndOfAnimationDelay(80, 0, 80, 1, 0.05f);
						this.target.switchToAnimationatEndOfAnimationDelay(80, 0, 80, 2, 0.05f);
						this.target.switchToAnimationatEndOfAnimationDelay(80, 0, 101, 3, 0.05f);
						this.target.switchToAnimationatEndOfAnimationDelay(80, 0, 101, 4, 0.05f);
						this.target.retain();
						if (CTRRootController.isShowGreeting())
						{
							this.dd.callObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(this.selector_showGreeting), null, 1.3f);
							CTRRootController.setShowGreeting(false);
						}
						this.target.playTimeline(0);
						this.target.getTimeline(0).delegateTimelineDelegate = this;
						this.target.setPauseAtIndexforAnimation(8, 7);
						this.blink = Animation.Animation_createWithResID(80);
						this.blink.parentAnchor = 9;
						this.blink.visible = false;
						this.blink.addAnimationWithIDDelayLoopCountSequence(0, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 4, 41, new List<int> { 41, 42, 42, 42 });
						this.blink.setActionTargetParamSubParamAtIndexforAnimation("ACTION_SET_VISIBLE", this.blink, 0, 0, 2, 0);
						this.blinkTimer = 3;
						this.blink.doRestoreCutTransparency();
						this.target.addChild(this.blink);
						this.idlesTimer = CutTheRope.iframework.helpers.MathHelper.RND_RANGE(5, 20);
					}
				}
			}
			if (this.twoParts != 2)
			{
				this.candyBubbleAnimationL = Animation.Animation_createWithResID(72);
				this.candyBubbleAnimationL.parentAnchor = (this.candyBubbleAnimationL.anchor = 18);
				this.candyBubbleAnimationL.addAnimationDelayLoopFirstLast(0.05, Timeline.LoopType.TIMELINE_REPLAY, 0, 12);
				this.candyBubbleAnimationL.playTimeline(0);
				this.candyL.addChild(this.candyBubbleAnimationL);
				this.candyBubbleAnimationL.visible = false;
				this.candyBubbleAnimationR = Animation.Animation_createWithResID(72);
				this.candyBubbleAnimationR.parentAnchor = (this.candyBubbleAnimationR.anchor = 18);
				this.candyBubbleAnimationR.addAnimationDelayLoopFirstLast(0.05, Timeline.LoopType.TIMELINE_REPLAY, 0, 12);
				this.candyBubbleAnimationR.playTimeline(0);
				this.candyR.addChild(this.candyBubbleAnimationR);
				this.candyBubbleAnimationR.visible = false;
			}
			foreach (object obj in this.rotatedCircles)
			{
				RotatedCircle rotatedCircle2 = (RotatedCircle)obj;
				rotatedCircle2.operating = -1;
				rotatedCircle2.circlesArray = this.rotatedCircles;
			}
			this.startCamera();
			this.tummyTeasers = 0;
			this.starsCollected = 0;
			this.candyBubble = null;
			this.candyBubbleL = null;
			this.candyBubbleR = null;
			this.mouthOpen = false;
			this.noCandy = this.twoParts != 2;
			this.noCandyL = false;
			this.noCandyR = false;
			this.blink.playTimeline(0);
			this.spiderTookCandy = false;
			this.time = 0f;
			this.score = 0;
			this.gravityNormal = true;
			MaterialPoint.globalGravity = CutTheRope.iframework.helpers.MathHelper.vect(0f, 784f);
			this.dimTime = 0f;
			this.ropesCutAtOnce = 0;
			this.ropeAtOnceTimer = 0f;
			this.dd.callObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(this.selector_doCandyBlink), null, 1.0);
			Text text = Text.createWithFontandString(3, (cTRRootController.getPack() + 1).ToString() + " - " + (cTRRootController.getLevel() + 1).ToString());
			text.anchor = 33;
			Text text2 = Text.createWithFontandString(3, Application.getString(655376));
			text2.anchor = 33;
			text2.parentAnchor = 9;
			text.setName("levelLabel");
			text.x = 15f + (float)base.canvas.xOffsetScaled;
			text.y = FrameworkTypes.SCREEN_HEIGHT + 15f;
			text2.y = 60f;
			text2.rotationCenterX -= (float)text2.width / 2f;
			text2.scaleX = (text2.scaleY = 0.7f);
			text.addChild(text2);
			Timeline timeline6 = new Timeline().initWithMaxKeyFramesOnTrack(5);
			timeline6.addKeyFrame(KeyFrame.makeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
			timeline6.addKeyFrame(KeyFrame.makeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
			timeline6.addKeyFrame(KeyFrame.makeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
			timeline6.addKeyFrame(KeyFrame.makeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1.0));
			timeline6.addKeyFrame(KeyFrame.makeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
			text.addTimelinewithID(timeline6, 0);
			text.playTimeline(0);
			timeline6.delegateTimelineDelegate = this.staticAniPool;
			this.staticAniPool.addChild(text);
			for (int m = 0; m < 5; m++)
			{
				this.dragging[m] = false;
				this.startPos[m] = (this.prevStartPos[m] = CutTheRope.iframework.helpers.MathHelper.vectZero);
			}
			if (this.clickToCut)
			{
				this.resetBungeeHighlight();
			}
			Global.MouseCursor.ReleaseButtons();
			CTRRootController.logEvent("IG_SHOWN");
		}

		// Token: 0x0600050C RID: 1292 RVA: 0x0001F948 File Offset: 0x0001DB48
		public virtual void startCamera()
		{
			if (this.mapWidth > FrameworkTypes.SCREEN_WIDTH || this.mapHeight > FrameworkTypes.SCREEN_HEIGHT)
			{
				this.ignoreTouches = true;
				this.fastenCamera = false;
				this.camera.type = CAMERA_TYPE.CAMERA_SPEED_PIXELS;
				this.camera.speed = 20f;
				this.cameraMoveMode = 0;
				ConstraintedPoint constraintedPoint = ((this.twoParts != 2) ? this.starL : this.star);
				float num;
				float num2;
				if (this.mapWidth > FrameworkTypes.SCREEN_WIDTH)
				{
					if ((double)constraintedPoint.pos.x > (double)this.mapWidth / 2.0)
					{
						num = 0f;
						num2 = 0f;
					}
					else
					{
						num = this.mapWidth - FrameworkTypes.SCREEN_WIDTH;
						num2 = 0f;
					}
				}
				else if ((double)constraintedPoint.pos.y > (double)this.mapHeight / 2.0)
				{
					num = 0f;
					num2 = 0f;
				}
				else
				{
					num = 0f;
					num2 = this.mapHeight - FrameworkTypes.SCREEN_HEIGHT;
				}
				double num6 = (double)(constraintedPoint.pos.x - FrameworkTypes.SCREEN_WIDTH / 2f);
				float num3 = constraintedPoint.pos.y - FrameworkTypes.SCREEN_HEIGHT / 2f;
				float num4 = CutTheRope.iframework.helpers.MathHelper.FIT_TO_BOUNDARIES(num6, 0.0, (double)(this.mapWidth - FrameworkTypes.SCREEN_WIDTH));
				float num5 = CutTheRope.iframework.helpers.MathHelper.FIT_TO_BOUNDARIES((double)num3, 0.0, (double)(this.mapHeight - FrameworkTypes.SCREEN_HEIGHT));
				this.camera.moveToXYImmediate(num, num2, true);
				this.initialCameraToStarDistance = CutTheRope.iframework.helpers.MathHelper.vectDistance(this.camera.pos, CutTheRope.iframework.helpers.MathHelper.vect(num4, num5));
				return;
			}
			this.ignoreTouches = false;
			this.camera.moveToXYImmediate(0f, 0f, true);
		}

		// Token: 0x0600050D RID: 1293 RVA: 0x0001FB01 File Offset: 0x0001DD01
		public virtual void doCandyBlink()
		{
			this.candyBlink.playTimeline(0);
		}

		// Token: 0x0600050E RID: 1294 RVA: 0x0001FB10 File Offset: 0x0001DD10
		public virtual void timelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
		{
			if (this.rotatedCircles.getObjectIndex(t.element) != -1 || i != 1)
			{
				return;
			}
			this.blinkTimer--;
			if (this.blinkTimer == 0)
			{
				this.blink.visible = true;
				this.blink.playTimeline(0);
				this.blinkTimer = 3;
			}
			this.idlesTimer--;
			if (this.idlesTimer == 0)
			{
				if (CutTheRope.iframework.helpers.MathHelper.RND_RANGE(0, 1) == 1)
				{
					this.target.playTimeline(1);
				}
				else
				{
					this.target.playTimeline(2);
				}
				this.idlesTimer = CutTheRope.iframework.helpers.MathHelper.RND_RANGE(5, 20);
			}
		}

		// Token: 0x0600050F RID: 1295 RVA: 0x0001FBB4 File Offset: 0x0001DDB4
		public virtual void timelineFinished(Timeline t)
		{
			if (this.rotatedCircles.getObjectIndex(t.element) != -1)
			{
				((RotatedCircle)t.element).removeOnNextUpdate = true;
			}
			foreach (object obj in this.tutorials)
			{
				BaseElement baseElement = (BaseElement)obj;
			}
		}

		// Token: 0x06000510 RID: 1296 RVA: 0x0001FC2C File Offset: 0x0001DE2C
		public override void hide()
		{
			if (this.gravityButton != null)
			{
				this.removeChild(this.gravityButton);
			}
			this.pollenDrawer.release();
			if (this.earthAnims != null)
			{
				this.earthAnims.release();
			}
			this.candy.release();
			this.star.release();
			if (this.candyL != null)
			{
				this.candyL.release();
			}
			if (this.candyR != null)
			{
				this.candyR.release();
			}
			this.starL.release();
			this.starR.release();
			this.razors.release();
			this.spikes.release();
			this.bungees.release();
			this.stars.release();
			this.bubbles.release();
			this.pumps.release();
			this.socks.release();
			this.bouncers.release();
			this.rotatedCircles.release();
			this.target.release();
			this.support.release();
			this.tutorialImages.release();
			this.tutorials.release();
			this.candyL = null;
			this.candyR = null;
			this.starL = null;
			this.starR = null;
		}

		// Token: 0x06000511 RID: 1297 RVA: 0x0001FD68 File Offset: 0x0001DF68
		public override void update(float delta)
		{
			delta = 0.016f;
			base.update(delta);
			this.dd.update(delta);
			this.pollenDrawer.update(delta);
			for (int i = 0; i < 5; i++)
			{
				for (int j = 0; j < this.fingerCuts[i].count(); j++)
				{
					GameScene.FingerCut fingerCut = (GameScene.FingerCut)this.fingerCuts[i].objectAtIndex(j);
					if (Mover.moveVariableToTarget(ref fingerCut.c.a, 0.0, 10.0, (double)delta))
					{
						this.fingerCuts[i].removeObject(fingerCut);
						j--;
					}
				}
			}
			if (this.earthAnims != null)
			{
				foreach (object obj in this.earthAnims)
				{
					((Image)obj).update(delta);
				}
			}
			Mover.moveVariableToTarget(ref this.ropeAtOnceTimer, 0.0, 1.0, (double)delta);
			ConstraintedPoint constraintedPoint4 = ((this.twoParts != 2) ? this.starL : this.star);
			float num = constraintedPoint4.pos.x - FrameworkTypes.SCREEN_WIDTH / 2f;
			double num19 = (double)(constraintedPoint4.pos.y - FrameworkTypes.SCREEN_HEIGHT / 2f);
			float num2 = CutTheRope.iframework.helpers.MathHelper.FIT_TO_BOUNDARIES((double)num, 0.0, (double)(this.mapWidth - FrameworkTypes.SCREEN_WIDTH));
			float num3 = CutTheRope.iframework.helpers.MathHelper.FIT_TO_BOUNDARIES(num19, 0.0, (double)(this.mapHeight - FrameworkTypes.SCREEN_HEIGHT));
			this.camera.moveToXYImmediate(num2, num3, false);
			if (!this.freezeCamera || this.camera.type != CAMERA_TYPE.CAMERA_SPEED_DELAY)
			{
				this.camera.update(delta);
			}
			if (this.camera.type == CAMERA_TYPE.CAMERA_SPEED_PIXELS)
			{
				float num4 = 100f;
				float num5 = 800f;
				float num6 = 400f;
				float a = 1000f;
				float a2 = 300f;
				float num7 = CutTheRope.iframework.helpers.MathHelper.vectDistance(this.camera.pos, CutTheRope.iframework.helpers.MathHelper.vect(num2, num3));
				if (num7 < num4)
				{
					this.ignoreTouches = false;
				}
				if (this.fastenCamera)
				{
					if (this.camera.speed < 5500f)
					{
						this.camera.speed *= 1.5f;
					}
				}
				else if ((double)num7 > (double)this.initialCameraToStarDistance / 2.0)
				{
					this.camera.speed += delta * num5;
					this.camera.speed = CutTheRope.iframework.helpers.MathHelper.MIN(a, this.camera.speed);
				}
				else
				{
					this.camera.speed -= delta * num6;
					this.camera.speed = CutTheRope.iframework.helpers.MathHelper.MAX(a2, this.camera.speed);
				}
				if ((double)Math.Abs(this.camera.pos.x - num2) < 1.0 && (double)Math.Abs(this.camera.pos.y - num3) < 1.0)
				{
					this.camera.type = CAMERA_TYPE.CAMERA_SPEED_DELAY;
					this.camera.speed = 14f;
				}
			}
			else
			{
				this.time += delta;
			}
			if (this.bungees.count() > 0)
			{
				bool flag = false;
				bool flag2 = false;
				bool flag3 = false;
				int num8 = this.bungees.count();
				int k = 0;
				while (k < num8)
				{
					Grab grab = (Grab)this.bungees.objectAtIndex(k);
					grab.update(delta);
					Bungee rope = grab.rope;
					if (grab.mover != null)
					{
						if (grab.rope != null)
						{
							grab.rope.bungeeAnchor.pos = CutTheRope.iframework.helpers.MathHelper.vect(grab.x, grab.y);
							grab.rope.bungeeAnchor.pin = grab.rope.bungeeAnchor.pos;
						}
						if (grab.radius != -1f)
						{
							grab.reCalcCircle();
						}
					}
					if (rope == null)
					{
						goto IL_0478;
					}
					if (rope.cut == -1 || (double)rope.cutTime != 0.0)
					{
						if (rope != null)
						{
							rope.update(delta * this.ropePhysicsSpeed);
						}
						if (!grab.hasSpider)
						{
							goto IL_0478;
						}
						if (this.camera.type != CAMERA_TYPE.CAMERA_SPEED_PIXELS || !this.ignoreTouches)
						{
							grab.updateSpider(delta);
						}
						if (grab.spiderPos == -1f)
						{
							this.spiderWon(grab);
							break;
						}
						goto IL_0478;
					}
					IL_08D4:
					k++;
					continue;
					IL_0478:
					if (grab.radius != -1f && grab.rope == null)
					{
						if (this.twoParts != 2)
						{
							if (!this.noCandyL && CutTheRope.iframework.helpers.MathHelper.vectDistance(CutTheRope.iframework.helpers.MathHelper.vect(grab.x, grab.y), this.starL.pos) <= grab.radius + 42f)
							{
								Bungee bungee = (Bungee)new Bungee().initWithHeadAtXYTailAtTXTYandLength(null, grab.x, grab.y, this.starL, this.starL.pos.x, this.starL.pos.y, grab.radius + 42f);
								bungee.bungeeAnchor.pin = bungee.bungeeAnchor.pos;
								grab.hideRadius = true;
								grab.setRope(bungee);
								CTRSoundMgr._playSound(24);
								if (grab.mover != null)
								{
									CTRSoundMgr._playSound(44);
								}
							}
							if (!this.noCandyR && grab.rope == null && CutTheRope.iframework.helpers.MathHelper.vectDistance(CutTheRope.iframework.helpers.MathHelper.vect(grab.x, grab.y), this.starR.pos) <= grab.radius + 42f)
							{
								Bungee bungee2 = (Bungee)new Bungee().initWithHeadAtXYTailAtTXTYandLength(null, grab.x, grab.y, this.starR, this.starR.pos.x, this.starR.pos.y, grab.radius + 42f);
								bungee2.bungeeAnchor.pin = bungee2.bungeeAnchor.pos;
								grab.hideRadius = true;
								grab.setRope(bungee2);
								CTRSoundMgr._playSound(24);
								if (grab.mover != null)
								{
									CTRSoundMgr._playSound(44);
								}
							}
						}
						else if (CutTheRope.iframework.helpers.MathHelper.vectDistance(CutTheRope.iframework.helpers.MathHelper.vect(grab.x, grab.y), this.star.pos) <= grab.radius + 42f)
						{
							Bungee bungee3 = (Bungee)new Bungee().initWithHeadAtXYTailAtTXTYandLength(null, grab.x, grab.y, this.star, this.star.pos.x, this.star.pos.y, grab.radius + 42f);
							bungee3.bungeeAnchor.pin = bungee3.bungeeAnchor.pos;
							grab.hideRadius = true;
							grab.setRope(bungee3);
							CTRSoundMgr._playSound(24);
							if (grab.mover != null)
							{
								CTRSoundMgr._playSound(44);
							}
						}
					}
					if (rope == null)
					{
						goto IL_08D4;
					}
					MaterialPoint bungeeAnchor = rope.bungeeAnchor;
					ConstraintedPoint constraintedPoint2 = rope.parts[rope.parts.Count - 1];
					Vector v = CutTheRope.iframework.helpers.MathHelper.vectSub(bungeeAnchor.pos, constraintedPoint2.pos);
					bool flag4 = false;
					if (this.twoParts != 2)
					{
						if (constraintedPoint2 == this.starL && !this.noCandyL && !flag2)
						{
							flag4 = true;
						}
						if (constraintedPoint2 == this.starR && !this.noCandyR && !flag3)
						{
							flag4 = true;
						}
					}
					else if (!this.noCandy && !flag)
					{
						flag4 = true;
					}
					if (rope.relaxed != 0 && rope.cut == -1 && flag4)
					{
						float num9 = CutTheRope.iframework.helpers.MathHelper.RADIANS_TO_DEGREES(CutTheRope.iframework.helpers.MathHelper.vectAngleNormalized(v));
						if (this.twoParts != 2)
						{
							GameObject gameObject = ((constraintedPoint2 == this.starL) ? this.candyL : this.candyR);
							if (!rope.chosenOne)
							{
								rope.initialCandleAngle = gameObject.rotation - num9;
							}
							if (constraintedPoint2 == this.starL)
							{
								this.lastCandyRotateDeltaL = num9 + rope.initialCandleAngle - gameObject.rotation;
								flag2 = true;
							}
							else
							{
								this.lastCandyRotateDeltaR = num9 + rope.initialCandleAngle - gameObject.rotation;
								flag3 = true;
							}
							gameObject.rotation = num9 + rope.initialCandleAngle;
						}
						else
						{
							if (!rope.chosenOne)
							{
								rope.initialCandleAngle = this.candyMain.rotation - num9;
							}
							this.lastCandyRotateDelta = num9 + rope.initialCandleAngle - this.candyMain.rotation;
							this.candyMain.rotation = num9 + rope.initialCandleAngle;
							flag = true;
						}
						rope.chosenOne = true;
						goto IL_08D4;
					}
					rope.chosenOne = false;
					goto IL_08D4;
				}
				if (this.twoParts != 2)
				{
					if (!flag2 && !this.noCandyL)
					{
						this.candyL.rotation += CutTheRope.iframework.helpers.MathHelper.MIN(5.0, (double)this.lastCandyRotateDeltaL);
						this.lastCandyRotateDeltaL *= 0.98f;
					}
					if (!flag3 && !this.noCandyR)
					{
						this.candyR.rotation += CutTheRope.iframework.helpers.MathHelper.MIN(5.0, (double)this.lastCandyRotateDeltaR);
						this.lastCandyRotateDeltaR *= 0.98f;
					}
				}
				else if (!flag && !this.noCandy)
				{
					this.candyMain.rotation += CutTheRope.iframework.helpers.MathHelper.MIN(5.0, (double)this.lastCandyRotateDelta);
					this.lastCandyRotateDelta *= 0.98f;
				}
			}
			if (!this.noCandy)
			{
				this.star.update(delta * this.ropePhysicsSpeed);
				this.candy.x = this.star.pos.x;
				this.candy.y = this.star.pos.y;
				this.candy.update(delta);
				BaseElement.calculateTopLeft(this.candy);
			}
			if (this.twoParts != 2)
			{
				this.candyL.update(delta);
				this.starL.update(delta * this.ropePhysicsSpeed);
				this.candyR.update(delta);
				this.starR.update(delta * this.ropePhysicsSpeed);
				if (this.twoParts == 1)
				{
					for (int l = 0; l < 30; l++)
					{
						ConstraintedPoint.satisfyConstraints(this.starL);
						ConstraintedPoint.satisfyConstraints(this.starR);
					}
				}
				if ((double)this.partsDist > 0.0)
				{
					if (Mover.moveVariableToTarget(ref this.partsDist, 0.0, 200.0, (double)delta))
					{
						CTRSoundMgr._playSound(40);
						this.twoParts = 2;
						this.noCandy = false;
						this.noCandyL = true;
						this.noCandyR = true;
						int num20 = Preferences._getIntForKey("PREFS_CANDIES_UNITED") + 1;
						Preferences._setIntforKey(num20, "PREFS_CANDIES_UNITED", false);
						if (num20 == 100)
						{
							CTRRootController.postAchievementName("1432722351", FrameworkTypes.ACHIEVEMENT_STRING("\"Romantic Soul\""));
						}
						if (this.candyBubbleL != null || this.candyBubbleR != null)
						{
							this.candyBubble = ((this.candyBubbleL != null) ? this.candyBubbleL : this.candyBubbleR);
							this.candyBubbleAnimation.visible = true;
						}
						this.lastCandyRotateDelta = 0f;
						this.lastCandyRotateDeltaL = 0f;
						this.lastCandyRotateDeltaR = 0f;
						this.star.pos.x = this.starL.pos.x;
						this.star.pos.y = this.starL.pos.y;
						this.candy.x = this.star.pos.x;
						this.candy.y = this.star.pos.y;
						BaseElement.calculateTopLeft(this.candy);
						Vector vector = CutTheRope.iframework.helpers.MathHelper.vectSub(this.starL.pos, this.starL.prevPos);
						Vector vector2 = CutTheRope.iframework.helpers.MathHelper.vectSub(this.starR.pos, this.starR.prevPos);
						Vector v2 = CutTheRope.iframework.helpers.MathHelper.vect((vector.x + vector2.x) / 2f, (vector.y + vector2.y) / 2f);
						this.star.prevPos = CutTheRope.iframework.helpers.MathHelper.vectSub(this.star.pos, v2);
						int num10 = this.bungees.count();
						for (int m = 0; m < num10; m++)
						{
							Bungee rope2 = ((Grab)this.bungees.objectAtIndex(m)).rope;
							if (rope2 != null && rope2.cut != rope2.parts.Count - 3 && (rope2.tail == this.starL || rope2.tail == this.starR))
							{
								ConstraintedPoint constraintedPoint3 = rope2.parts[rope2.parts.Count - 2];
								int num11 = (int)rope2.tail.restLengthFor(constraintedPoint3);
								this.star.addConstraintwithRestLengthofType(constraintedPoint3, (float)num11, Constraint.CONSTRAINT.CONSTRAINT_DISTANCE);
								rope2.tail = this.star;
								rope2.parts[rope2.parts.Count - 1] = this.star;
								rope2.initialCandleAngle = 0f;
								rope2.chosenOne = false;
							}
						}
						Animation animation = Animation.Animation_createWithResID(63);
						animation.doRestoreCutTransparency();
						animation.x = this.candy.x;
						animation.y = this.candy.y;
						animation.anchor = 18;
						int n = animation.addAnimationDelayLoopFirstLast(0.05, Timeline.LoopType.TIMELINE_NO_LOOP, 21, 25);
						animation.getTimeline(n).delegateTimelineDelegate = this.aniPool;
						animation.playTimeline(0);
						this.aniPool.addChild(animation);
					}
					else
					{
						this.starL.changeRestLengthToFor(this.partsDist, this.starR);
						this.starR.changeRestLengthToFor(this.partsDist, this.starL);
					}
				}
				if (!this.noCandyL && !this.noCandyR && GameObject.objectsIntersect(this.candyL, this.candyR) && this.twoParts == 0)
				{
					this.twoParts = 1;
					this.partsDist = CutTheRope.iframework.helpers.MathHelper.vectDistance(this.starL.pos, this.starR.pos);
					this.starL.addConstraintwithRestLengthofType(this.starR, this.partsDist, Constraint.CONSTRAINT.CONSTRAINT_NOT_MORE_THAN);
					this.starR.addConstraintwithRestLengthofType(this.starL, this.partsDist, Constraint.CONSTRAINT.CONSTRAINT_NOT_MORE_THAN);
				}
			}
			this.target.update(delta);
			if (this.camera.type != CAMERA_TYPE.CAMERA_SPEED_PIXELS || !this.ignoreTouches)
			{
				foreach (object obj2 in this.stars)
				{
					Star star = (Star)obj2;
					star.update(delta);
					if ((double)star.timeout > 0.0 && (double)star.time == 0.0)
					{
						star.getTimeline(1).delegateTimelineDelegate = this.aniPool;
						this.aniPool.addChild(star);
						this.stars.removeObject(star);
						star.timedAnim.playTimeline(1);
						star.playTimeline(1);
						break;
					}
					if ((this.twoParts == 2) ? (GameObject.objectsIntersect(this.candy, star) && !this.noCandy) : ((GameObject.objectsIntersect(this.candyL, star) && !this.noCandyL) || (GameObject.objectsIntersect(this.candyR, star) && !this.noCandyR)))
					{
						this.candyBlink.playTimeline(1);
						this.starsCollected++;
						this.hudStar[this.starsCollected - 1].playTimeline(0);
						Animation animation2 = Animation.Animation_createWithResID(71);
						animation2.doRestoreCutTransparency();
						animation2.x = star.x;
						animation2.y = star.y;
						animation2.anchor = 18;
						int n2 = animation2.addAnimationDelayLoopFirstLast(0.05, Timeline.LoopType.TIMELINE_NO_LOOP, 0, 12);
						animation2.getTimeline(n2).delegateTimelineDelegate = this.aniPool;
						animation2.playTimeline(0);
						this.aniPool.addChild(animation2);
						this.stars.removeObject(star);
						CTRSoundMgr._playSound(25 + this.starsCollected - 1);
						if (this.target.getCurrentTimelineIndex() == 0)
						{
							this.target.playAnimationtimeline(101, 3);
							break;
						}
						break;
					}
				}
			}
			foreach (object obj3 in this.bubbles)
			{
				Bubble bubble3 = (Bubble)obj3;
				bubble3.update(delta);
				float num12 = 85f;
				if (this.twoParts != 2)
				{
					if (!this.noCandyL && !bubble3.popped && CutTheRope.iframework.helpers.MathHelper.pointInRect(this.candyL.x, this.candyL.y, bubble3.x - num12, bubble3.y - num12, num12 * 2f, num12 * 2f))
					{
						if (this.candyBubbleL != null)
						{
							this.popBubbleAtXY(bubble3.x, bubble3.y);
						}
						this.candyBubbleL = bubble3;
						this.candyBubbleAnimationL.visible = true;
						CTRSoundMgr._playSound(13);
						bubble3.popped = true;
						bubble3.removeChildWithID(0);
						break;
					}
					if (!this.noCandyR && !bubble3.popped && CutTheRope.iframework.helpers.MathHelper.pointInRect(this.candyR.x, this.candyR.y, bubble3.x - num12, bubble3.y - num12, num12 * 2f, num12 * 2f))
					{
						if (this.candyBubbleR != null)
						{
							this.popBubbleAtXY(bubble3.x, bubble3.y);
						}
						this.candyBubbleR = bubble3;
						this.candyBubbleAnimationR.visible = true;
						CTRSoundMgr._playSound(13);
						bubble3.popped = true;
						bubble3.removeChildWithID(0);
						break;
					}
				}
				else if (!this.noCandy && !bubble3.popped && CutTheRope.iframework.helpers.MathHelper.pointInRect(this.candy.x, this.candy.y, bubble3.x - num12, bubble3.y - num12, num12 * 2f, num12 * 2f))
				{
					if (this.candyBubble != null)
					{
						this.popBubbleAtXY(bubble3.x, bubble3.y);
					}
					this.candyBubble = bubble3;
					this.candyBubbleAnimation.visible = true;
					CTRSoundMgr._playSound(13);
					bubble3.popped = true;
					bubble3.removeChildWithID(0);
					break;
				}
				if (!bubble3.withoutShadow)
				{
					foreach (object obj4 in this.rotatedCircles)
					{
						RotatedCircle rotatedCircle5 = (RotatedCircle)obj4;
						if (CutTheRope.iframework.helpers.MathHelper.vectDistance(CutTheRope.iframework.helpers.MathHelper.vect(bubble3.x, bubble3.y), CutTheRope.iframework.helpers.MathHelper.vect(rotatedCircle5.x, rotatedCircle5.y)) < rotatedCircle5.sizeInPixels)
						{
							bubble3.withoutShadow = true;
						}
					}
				}
			}
			foreach (object obj5 in this.tutorials)
			{
				((Text)obj5).update(delta);
			}
			foreach (object obj6 in this.tutorialImages)
			{
				((GameObject)obj6).update(delta);
			}
			foreach (object obj7 in this.pumps)
			{
				Pump pump = (Pump)obj7;
				pump.update(delta);
				if (Mover.moveVariableToTarget(ref pump.pumpTouchTimer, 0.0, 1.0, (double)delta))
				{
					this.operatePump(pump);
				}
			}
			RotatedCircle rotatedCircle6 = null;
			foreach (object obj8 in this.rotatedCircles)
			{
				RotatedCircle rotatedCircle7 = (RotatedCircle)obj8;
				foreach (object obj9 in this.bungees)
				{
					Grab bungee4 = (Grab)obj9;
					if (CutTheRope.iframework.helpers.MathHelper.vectDistance(CutTheRope.iframework.helpers.MathHelper.vect(bungee4.x, bungee4.y), CutTheRope.iframework.helpers.MathHelper.vect(rotatedCircle7.x, rotatedCircle7.y)) <= rotatedCircle7.sizeInPixels + FrameworkTypes.RTPD(5.0) * 3f)
					{
						if (rotatedCircle7.containedObjects.getObjectIndex(bungee4) == -1)
						{
							rotatedCircle7.containedObjects.addObject(bungee4);
						}
					}
					else if (rotatedCircle7.containedObjects.getObjectIndex(bungee4) != -1)
					{
						rotatedCircle7.containedObjects.removeObject(bungee4);
					}
				}
				foreach (object obj10 in this.bubbles)
				{
					Bubble bubble4 = (Bubble)obj10;
					if (CutTheRope.iframework.helpers.MathHelper.vectDistance(CutTheRope.iframework.helpers.MathHelper.vect(bubble4.x, bubble4.y), CutTheRope.iframework.helpers.MathHelper.vect(rotatedCircle7.x, rotatedCircle7.y)) <= rotatedCircle7.sizeInPixels + FrameworkTypes.RTPD(10.0) * 3f)
					{
						if (rotatedCircle7.containedObjects.getObjectIndex(bubble4) == -1)
						{
							rotatedCircle7.containedObjects.addObject(bubble4);
						}
					}
					else if (rotatedCircle7.containedObjects.getObjectIndex(bubble4) != -1)
					{
						rotatedCircle7.containedObjects.removeObject(bubble4);
					}
				}
				if (rotatedCircle7.removeOnNextUpdate)
				{
					rotatedCircle6 = rotatedCircle7;
				}
				rotatedCircle7.update(delta);
			}
			if (rotatedCircle6 != null)
			{
				this.rotatedCircles.removeObject(rotatedCircle6);
			}
			float num13 = FrameworkTypes.RTPD(20.0);
			foreach (object obj11 in this.socks)
			{
				Sock sock3 = (Sock)obj11;
				sock3.update(delta);
				if (Mover.moveVariableToTarget(ref sock3.idleTimeout, 0.0, 1.0, (double)delta))
				{
					sock3.state = Sock.SOCK_IDLE;
				}
				float num14 = sock3.rotation;
				sock3.rotation = 0f;
				sock3.updateRotation();
				ref Vector ptr = CutTheRope.iframework.helpers.MathHelper.vectRotate(this.star.posDelta, (double)CutTheRope.iframework.helpers.MathHelper.DEGREES_TO_RADIANS(0f - num14));
				sock3.rotation = num14;
				sock3.updateRotation();
				if ((double)ptr.y >= 0.0 && (CutTheRope.iframework.helpers.MathHelper.lineInRect(sock3.t1.x, sock3.t1.y, sock3.t2.x, sock3.t2.y, this.star.pos.x - num13, this.star.pos.y - num13, num13 * 2f, num13 * 2f) || CutTheRope.iframework.helpers.MathHelper.lineInRect(sock3.b1.x, sock3.b1.y, sock3.b2.x, sock3.b2.y, this.star.pos.x - num13, this.star.pos.y - num13, num13 * 2f, num13 * 2f)))
				{
					if (sock3.state != Sock.SOCK_IDLE)
					{
						continue;
					}
					using (IEnumerator enumerator2 = this.socks.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							object obj12 = enumerator2.Current;
							Sock sock4 = (Sock)obj12;
							if (sock4 != sock3 && sock4.group == sock3.group)
							{
								sock3.state = Sock.SOCK_RECEIVING;
								sock4.state = Sock.SOCK_THROWING;
								this.releaseAllRopes(false);
								this.savedSockSpeed = 0.9f * CutTheRope.iframework.helpers.MathHelper.vectLength(this.star.v);
								this.savedSockSpeed *= 1.4f;
								this.targetSock = sock4;
								sock3.light.playTimeline(0);
								sock3.light.visible = true;
								CTRSoundMgr._playSound(45);
								this.dd.callObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(this.selector_teleport), null, 0.1);
								break;
							}
						}
						break;
					}
				}
				if (sock3.state != Sock.SOCK_IDLE && sock3.idleTimeout == 0f)
				{
					sock3.idleTimeout = 0.8f;
				}
			}
			foreach (object obj13 in this.razors)
			{
				Razor razor = (Razor)obj13;
				razor.update(delta);
				this.cutWithRazorOrLine1Line2Immediate(razor, CutTheRope.iframework.helpers.MathHelper.vectZero, CutTheRope.iframework.helpers.MathHelper.vectZero, false);
			}
			foreach (object obj14 in this.spikes)
			{
				Spikes spike = (Spikes)obj14;
				spike.update(delta);
				float num15 = 15f;
				if (!spike.electro || (spike.electro && spike.electroOn))
				{
					bool flag5 = false;
					bool flag6;
					if (this.twoParts != 2)
					{
						flag6 = (CutTheRope.iframework.helpers.MathHelper.lineInRect(spike.t1.x, spike.t1.y, spike.t2.x, spike.t2.y, this.starL.pos.x - num15, this.starL.pos.y - num15, num15 * 2f, num15 * 2f) || CutTheRope.iframework.helpers.MathHelper.lineInRect(spike.b1.x, spike.b1.y, spike.b2.x, spike.b2.y, this.starL.pos.x - num15, this.starL.pos.y - num15, num15 * 2f, num15 * 2f)) && !this.noCandyL;
						if (flag6)
						{
							flag5 = true;
						}
						else
						{
							flag6 = (CutTheRope.iframework.helpers.MathHelper.lineInRect(spike.t1.x, spike.t1.y, spike.t2.x, spike.t2.y, this.starR.pos.x - num15, this.starR.pos.y - num15, num15 * 2f, num15 * 2f) || CutTheRope.iframework.helpers.MathHelper.lineInRect(spike.b1.x, spike.b1.y, spike.b2.x, spike.b2.y, this.starR.pos.x - num15, this.starR.pos.y - num15, num15 * 2f, num15 * 2f)) && !this.noCandyR;
						}
					}
					else
					{
						flag6 = (CutTheRope.iframework.helpers.MathHelper.lineInRect(spike.t1.x, spike.t1.y, spike.t2.x, spike.t2.y, this.star.pos.x - num15, this.star.pos.y - num15, num15 * 2f, num15 * 2f) || CutTheRope.iframework.helpers.MathHelper.lineInRect(spike.b1.x, spike.b1.y, spike.b2.x, spike.b2.y, this.star.pos.x - num15, this.star.pos.y - num15, num15 * 2f, num15 * 2f)) && !this.noCandy;
					}
					if (flag6)
					{
						if (this.twoParts != 2)
						{
							if (flag5)
							{
								if (this.candyBubbleL != null)
								{
									this.popCandyBubble(true);
								}
							}
							else if (this.candyBubbleR != null)
							{
								this.popCandyBubble(false);
							}
						}
						else if (this.candyBubble != null)
						{
							this.popCandyBubble(false);
						}
						Image image2 = Image.Image_createWithResID(63);
						image2.doRestoreCutTransparency();
						CandyBreak candyBreak = (CandyBreak)new CandyBreak().initWithTotalParticlesandImageGrid(5, image2);
						if (this.gravityButton != null && !this.gravityNormal)
						{
							candyBreak.gravity.y = -500f;
							candyBreak.angle = 90f;
						}
						candyBreak.particlesDelegate = new Particles.ParticlesFinished(this.aniPool.particlesFinished);
						if (this.twoParts != 2)
						{
							if (flag5)
							{
								candyBreak.x = this.candyL.x;
								candyBreak.y = this.candyL.y;
								this.noCandyL = true;
							}
							else
							{
								candyBreak.x = this.candyR.x;
								candyBreak.y = this.candyR.y;
								this.noCandyR = true;
							}
						}
						else
						{
							candyBreak.x = this.candy.x;
							candyBreak.y = this.candy.y;
							this.noCandy = true;
						}
						candyBreak.startSystem(5);
						this.aniPool.addChild(candyBreak);
						CTRSoundMgr._playSound(14);
						this.releaseAllRopes(flag5);
						if (this.restartState != 0 && (this.twoParts == 2 || !this.noCandyL || !this.noCandyR))
						{
							this.dd.callObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(this.selector_gameLost), null, 0.3);
						}
						return;
					}
				}
			}
			foreach (object obj15 in this.bouncers)
			{
				Bouncer bouncer = (Bouncer)obj15;
				bouncer.update(delta);
				float num16 = 40f;
				bool flag7 = false;
				bool flag8;
				if (this.twoParts != 2)
				{
					flag8 = (CutTheRope.iframework.helpers.MathHelper.lineInRect(bouncer.t1.x, bouncer.t1.y, bouncer.t2.x, bouncer.t2.y, this.starL.pos.x - num16, this.starL.pos.y - num16, num16 * 2f, num16 * 2f) || CutTheRope.iframework.helpers.MathHelper.lineInRect(bouncer.b1.x, bouncer.b1.y, bouncer.b2.x, bouncer.b2.y, this.starL.pos.x - num16, this.starL.pos.y - num16, num16 * 2f, num16 * 2f)) && !this.noCandyL;
					if (flag8)
					{
						flag7 = true;
					}
					else
					{
						flag8 = (CutTheRope.iframework.helpers.MathHelper.lineInRect(bouncer.t1.x, bouncer.t1.y, bouncer.t2.x, bouncer.t2.y, this.starR.pos.x - num16, this.starR.pos.y - num16, num16 * 2f, num16 * 2f) || CutTheRope.iframework.helpers.MathHelper.lineInRect(bouncer.b1.x, bouncer.b1.y, bouncer.b2.x, bouncer.b2.y, this.starR.pos.x - num16, this.starR.pos.y - num16, num16 * 2f, num16 * 2f)) && !this.noCandyR;
					}
				}
				else
				{
					flag8 = (CutTheRope.iframework.helpers.MathHelper.lineInRect(bouncer.t1.x, bouncer.t1.y, bouncer.t2.x, bouncer.t2.y, this.star.pos.x - num16, this.star.pos.y - num16, num16 * 2f, num16 * 2f) || CutTheRope.iframework.helpers.MathHelper.lineInRect(bouncer.b1.x, bouncer.b1.y, bouncer.b2.x, bouncer.b2.y, this.star.pos.x - num16, this.star.pos.y - num16, num16 * 2f, num16 * 2f)) && !this.noCandy;
				}
				if (flag8)
				{
					if (this.twoParts != 2)
					{
						if (flag7)
						{
							this.handleBouncePtDelta(bouncer, this.starL, delta);
						}
						else
						{
							this.handleBouncePtDelta(bouncer, this.starR, delta);
						}
					}
					else
					{
						this.handleBouncePtDelta(bouncer, this.star, delta);
					}
				}
				else
				{
					bouncer.skip = false;
				}
			}
			float num17 = -40f;
			float num18 = 14f;
			if (this.twoParts == 0)
			{
				if (this.candyBubbleL != null)
				{
					if (this.gravityButton != null && !this.gravityNormal)
					{
						this.starL.applyImpulseDelta(CutTheRope.iframework.helpers.MathHelper.vect((0f - this.starL.v.x) / num18, (0f - this.starL.v.y) / num18 - num17), delta);
					}
					else
					{
						this.starL.applyImpulseDelta(CutTheRope.iframework.helpers.MathHelper.vect((0f - this.starL.v.x) / num18, (0f - this.starL.v.y) / num18 + num17), delta);
					}
				}
				if (this.candyBubbleR != null)
				{
					if (this.gravityButton != null && !this.gravityNormal)
					{
						this.starR.applyImpulseDelta(CutTheRope.iframework.helpers.MathHelper.vect((0f - this.starR.v.x) / num18, (0f - this.starR.v.y) / num18 - num17), delta);
					}
					else
					{
						this.starR.applyImpulseDelta(CutTheRope.iframework.helpers.MathHelper.vect((0f - this.starR.v.x) / num18, (0f - this.starR.v.y) / num18 + num17), delta);
					}
				}
			}
			if (this.twoParts == 1)
			{
				if (this.candyBubbleR != null || this.candyBubbleL != null)
				{
					if (this.gravityButton != null && !this.gravityNormal)
					{
						this.starL.applyImpulseDelta(CutTheRope.iframework.helpers.MathHelper.vect((0f - this.starL.v.x) / num18, (0f - this.starL.v.y) / num18 - num17), delta);
						this.starR.applyImpulseDelta(CutTheRope.iframework.helpers.MathHelper.vect((0f - this.starR.v.x) / num18, (0f - this.starR.v.y) / num18 - num17), delta);
					}
					else
					{
						this.starL.applyImpulseDelta(CutTheRope.iframework.helpers.MathHelper.vect((0f - this.starL.v.x) / num18, (0f - this.starL.v.y) / num18 + num17), delta);
						this.starR.applyImpulseDelta(CutTheRope.iframework.helpers.MathHelper.vect((0f - this.starR.v.x) / num18, (0f - this.starR.v.y) / num18 + num17), delta);
					}
				}
			}
			else if (this.candyBubble != null)
			{
				if (this.gravityButton != null && !this.gravityNormal)
				{
					this.star.applyImpulseDelta(CutTheRope.iframework.helpers.MathHelper.vect((0f - this.star.v.x) / num18, (0f - this.star.v.y) / num18 - num17), delta);
				}
				else
				{
					this.star.applyImpulseDelta(CutTheRope.iframework.helpers.MathHelper.vect((0f - this.star.v.x) / num18, (0f - this.star.v.y) / num18 + num17), delta);
				}
			}
			if (!this.noCandy)
			{
				if (!this.mouthOpen)
				{
					if (CutTheRope.iframework.helpers.MathHelper.vectDistance(this.star.pos, CutTheRope.iframework.helpers.MathHelper.vect(this.target.x, this.target.y)) < 200f)
					{
						this.mouthOpen = true;
						this.target.playTimeline(7);
						CTRSoundMgr._playSound(17);
						this.mouthCloseTimer = 1f;
					}
				}
				else if ((double)this.mouthCloseTimer > 0.0)
				{
					Mover.moveVariableToTarget(ref this.mouthCloseTimer, 0.0, 1.0, (double)delta);
					if ((double)this.mouthCloseTimer <= 0.0)
					{
						if (CutTheRope.iframework.helpers.MathHelper.vectDistance(this.star.pos, CutTheRope.iframework.helpers.MathHelper.vect(this.target.x, this.target.y)) > 200f)
						{
							this.mouthOpen = false;
							this.target.playTimeline(8);
							CTRSoundMgr._playSound(16);
							this.tummyTeasers++;
							if (this.tummyTeasers >= 10)
							{
								CTRRootController.postAchievementName("1058281905", FrameworkTypes.ACHIEVEMENT_STRING("\"Tummy Teaser\""));
							}
						}
						else
						{
							this.mouthCloseTimer = 1f;
						}
					}
				}
				if (this.restartState != 0 && GameObject.objectsIntersect(this.candy, this.target))
				{
					this.gameWon();
					return;
				}
			}
			bool flag9 = this.twoParts == 2 && this.pointOutOfScreen(this.star) && !this.noCandy;
			bool flag10 = this.twoParts != 2 && this.pointOutOfScreen(this.starL) && !this.noCandyL;
			bool flag11 = this.twoParts != 2 && this.pointOutOfScreen(this.starR) && !this.noCandyR;
			if (flag10 || flag11 || flag9)
			{
				if (flag9)
				{
					this.noCandy = true;
				}
				if (flag10)
				{
					this.noCandyL = true;
				}
				if (flag11)
				{
					this.noCandyR = true;
				}
				if (this.restartState != 0)
				{
					int num21 = Preferences._getIntForKey("PREFS_CANDIES_LOST") + 1;
					Preferences._setIntforKey(num21, "PREFS_CANDIES_LOST", false);
					if (num21 == 50)
					{
						CTRRootController.postAchievementName("681497443", FrameworkTypes.ACHIEVEMENT_STRING("\"Weight Loser\""));
					}
					if (num21 == 200)
					{
						CTRRootController.postAchievementName("1058341297", FrameworkTypes.ACHIEVEMENT_STRING("\"Calorie Minimizer\""));
					}
					if (this.twoParts == 2 || !this.noCandyL || !this.noCandyR)
					{
						this.gameLost();
					}
					return;
				}
			}
			if (this.special != 0 && this.special == 1 && !this.noCandy && this.candyBubble != null && this.candy.y < 400f && this.candy.x > 1200f)
			{
				this.special = 0;
				foreach (object obj16 in this.tutorials)
				{
					GameScene.TutorialText tutorial2 = (GameScene.TutorialText)obj16;
					if (tutorial2.special == 1)
					{
						tutorial2.playTimeline(0);
					}
				}
				foreach (object obj17 in this.tutorialImages)
				{
					GameScene.GameObjectSpecial tutorialImage2 = (GameScene.GameObjectSpecial)obj17;
					if (tutorialImage2.special == 1)
					{
						tutorialImage2.playTimeline(0);
					}
				}
			}
			if (this.clickToCut && !this.ignoreTouches)
			{
				this.resetBungeeHighlight();
				bool flag12 = false;
				Vector p = CutTheRope.iframework.helpers.MathHelper.vectAdd(this.slastTouch, this.camera.pos);
				if (this.gravityButton != null && ((Button)this.gravityButton.getChild((this.gravityButton.on() > false) ? 1 : 0)).isInTouchZoneXYforTouchDown(p.x, p.y, true))
				{
					flag12 = true;
				}
				if (this.candyBubble != null || (this.twoParts != 2 && (this.candyBubbleL != null || this.candyBubbleR != null)))
				{
					foreach (object obj18 in this.bubbles)
					{
						Bubble bubble5 = (Bubble)obj18;
						if (this.candyBubble != null && CutTheRope.iframework.helpers.MathHelper.pointInRect(p.x, p.y, this.star.pos.x - 60f, this.star.pos.y - 60f, 120f, 120f))
						{
							flag12 = true;
							break;
						}
						if (this.candyBubbleL != null && CutTheRope.iframework.helpers.MathHelper.pointInRect(p.x, p.y, this.starL.pos.x - 60f, this.starL.pos.y - 60f, 120f, 120f))
						{
							flag12 = true;
							break;
						}
						if (this.candyBubbleR != null && CutTheRope.iframework.helpers.MathHelper.pointInRect(p.x, p.y, this.starR.pos.x - 60f, this.starR.pos.y - 60f, 120f, 120f))
						{
							flag12 = true;
							break;
						}
					}
				}
				foreach (object obj19 in this.spikes)
				{
					Spikes spike2 = (Spikes)obj19;
					if (spike2.rotateButton != null && spike2.rotateButton.isInTouchZoneXYforTouchDown(p.x, p.y, true))
					{
						flag12 = true;
					}
				}
				foreach (object obj20 in this.pumps)
				{
					Pump pump2 = (Pump)obj20;
					if (GameObject.pointInObject(p, pump2))
					{
						flag12 = true;
						break;
					}
				}
				foreach (object obj21 in this.rotatedCircles)
				{
					RotatedCircle rotatedCircle8 = (RotatedCircle)obj21;
					if (rotatedCircle8.isLeftControllerActive() || rotatedCircle8.isRightControllerActive())
					{
						flag12 = true;
						break;
					}
					if (CutTheRope.iframework.helpers.MathHelper.vectDistance(CutTheRope.iframework.helpers.MathHelper.vect(p.x, p.y), CutTheRope.iframework.helpers.MathHelper.vect(rotatedCircle8.handle1.x, rotatedCircle8.handle1.y)) <= 90f || CutTheRope.iframework.helpers.MathHelper.vectDistance(CutTheRope.iframework.helpers.MathHelper.vect(p.x, p.y), CutTheRope.iframework.helpers.MathHelper.vect(rotatedCircle8.handle2.x, rotatedCircle8.handle2.y)) <= 90f)
					{
						flag12 = true;
						break;
					}
				}
				foreach (object obj22 in this.bungees)
				{
					Grab bungee5 = (Grab)obj22;
					if (bungee5.wheel && CutTheRope.iframework.helpers.MathHelper.pointInRect(p.x, p.y, bungee5.x - 110f, bungee5.y - 110f, 220f, 220f))
					{
						flag12 = true;
						break;
					}
					if ((double)bungee5.moveLength > 0.0 && (CutTheRope.iframework.helpers.MathHelper.pointInRect(p.x, p.y, bungee5.x - 65f, bungee5.y - 65f, 130f, 130f) || bungee5.moverDragging != -1))
					{
						flag12 = true;
						break;
					}
				}
				if (!flag12)
				{
					Vector s = default(Vector);
					Grab grab2 = null;
					Bungee nearestBungeeSegmentByBeziersPointsatXYgrab = this.getNearestBungeeSegmentByBeziersPointsatXYgrab(ref s, this.slastTouch.x + this.camera.pos.x, this.slastTouch.y + this.camera.pos.y, ref grab2);
					if (nearestBungeeSegmentByBeziersPointsatXYgrab != null)
					{
						nearestBungeeSegmentByBeziersPointsatXYgrab.highlighted = true;
					}
				}
			}
			if (Mover.moveVariableToTarget(ref this.dimTime, 0.0, 1.0, (double)delta))
			{
				if (this.restartState == 0)
				{
					this.restartState = 1;
					this.hide();
					this.show();
					this.dimTime = 0.15f;
					return;
				}
				this.restartState = -1;
			}
		}

		// Token: 0x06000512 RID: 1298 RVA: 0x00022DD0 File Offset: 0x00020FD0
		public virtual void teleport()
		{
			if (this.targetSock != null)
			{
				this.targetSock.light.playTimeline(0);
				this.targetSock.light.visible = true;
				Vector v = CutTheRope.iframework.helpers.MathHelper.vect(0f, -16f);
				v = CutTheRope.iframework.helpers.MathHelper.vectRotate(v, (double)CutTheRope.iframework.helpers.MathHelper.DEGREES_TO_RADIANS(this.targetSock.rotation));
				this.star.pos.x = this.targetSock.x;
				this.star.pos.y = this.targetSock.y;
				this.star.pos = CutTheRope.iframework.helpers.MathHelper.vectAdd(this.star.pos, v);
				this.star.prevPos.x = this.star.pos.x;
				this.star.prevPos.y = this.star.pos.y;
				this.star.v = CutTheRope.iframework.helpers.MathHelper.vectMult(CutTheRope.iframework.helpers.MathHelper.vectRotate(CutTheRope.iframework.helpers.MathHelper.vect(0f, -1f), (double)CutTheRope.iframework.helpers.MathHelper.DEGREES_TO_RADIANS(this.targetSock.rotation)), this.savedSockSpeed);
				this.star.posDelta = CutTheRope.iframework.helpers.MathHelper.vectDiv(this.star.v, 60f);
				this.star.prevPos = CutTheRope.iframework.helpers.MathHelper.vectSub(this.star.pos, this.star.posDelta);
				this.targetSock = null;
			}
		}

		// Token: 0x06000513 RID: 1299 RVA: 0x00022F4C File Offset: 0x0002114C
		public virtual void animateLevelRestart()
		{
			this.restartState = 0;
			this.dimTime = 0.15f;
		}

		// Token: 0x06000514 RID: 1300 RVA: 0x00022F60 File Offset: 0x00021160
		public virtual void releaseAllRopes(bool left)
		{
			int num = this.bungees.count();
			for (int i = 0; i < num; i++)
			{
				Grab grab = (Grab)this.bungees.objectAtIndex(i);
				Bungee rope = grab.rope;
				if (rope != null && (rope.tail == this.star || (rope.tail == this.starL && left) || (rope.tail == this.starR && !left)))
				{
					if (rope.cut == -1)
					{
						rope.setCut(rope.parts.Count - 2);
					}
					else
					{
						rope.hideTailParts = true;
					}
					if (grab.hasSpider && grab.spiderActive)
					{
						this.spiderBusted(grab);
					}
				}
			}
		}

		// Token: 0x06000515 RID: 1301 RVA: 0x00023014 File Offset: 0x00021214
		public virtual void calculateScore()
		{
			this.timeBonus = (int)CutTheRope.iframework.helpers.MathHelper.MAX(0f, 30f - this.time) * 100;
			this.timeBonus /= 10;
			this.timeBonus *= 10;
			this.starBonus = 1000 * this.starsCollected;
			this.score = (int)CutTheRope.iframework.helpers.MathHelper.ceil((double)(this.timeBonus + this.starBonus));
		}

		// Token: 0x06000516 RID: 1302 RVA: 0x0002308C File Offset: 0x0002128C
		public virtual void gameWon()
		{
			this.dd.cancelAllDispatches();
			this.target.playTimeline(6);
			CTRSoundMgr._playSound(15);
			if (this.candyBubble != null)
			{
				this.popCandyBubble(false);
			}
			this.noCandy = true;
			this.candy.passTransformationsToChilds = true;
			this.candyMain.scaleX = (this.candyMain.scaleY = 1f);
			this.candyTop.scaleX = (this.candyTop.scaleY = 1f);
			Timeline timeline = new Timeline().initWithMaxKeyFramesOnTrack(2);
			timeline.addKeyFrame(KeyFrame.makePos((double)this.candy.x, (double)this.candy.y, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
			timeline.addKeyFrame(KeyFrame.makePos((double)this.target.x, (double)this.target.y + 10.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.1));
			timeline.addKeyFrame(KeyFrame.makeScale(0.71, 0.71, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
			timeline.addKeyFrame(KeyFrame.makeScale(0.0, 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.1));
			timeline.addKeyFrame(KeyFrame.makeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
			timeline.addKeyFrame(KeyFrame.makeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.1));
			this.candy.addTimelinewithID(timeline, 0);
			this.candy.playTimeline(0);
			timeline.delegateTimelineDelegate = this.aniPool;
			this.aniPool.addChild(this.candy);
			this.dd.callObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(this.selector_gameWon), null, 2.0);
			this.calculateScore();
			this.releaseAllRopes(false);
		}

		// Token: 0x06000517 RID: 1303 RVA: 0x0002326C File Offset: 0x0002146C
		public virtual void gameLost()
		{
			this.dd.cancelAllDispatches();
			this.target.playAnimationtimeline(102, 5);
			CTRSoundMgr._playSound(18);
			this.dd.callObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(this.selector_animateLevelRestart), null, 1.0);
			this.gameSceneDelegate.gameLost();
		}

		// Token: 0x06000518 RID: 1304 RVA: 0x000232C8 File Offset: 0x000214C8
		public override void draw()
		{
			OpenGL.glClear(0);
			base.preDraw();
			this.camera.applyCameraTransformation();
			OpenGL.glEnable(0);
			OpenGL.glDisable(1);
			Vector pos = CutTheRope.iframework.helpers.MathHelper.vectDiv(this.camera.pos, 1.25f);
			this.back.updateWithCameraPos(pos);
			float num = (float)base.canvas.xOffsetScaled;
			float num2 = 0f;
			OpenGL.glPushMatrix();
			OpenGL.glTranslatef((double)num, (double)num2, 0.0);
			OpenGL.glScalef((double)this.back.scaleX, (double)this.back.scaleY, 1.0);
			OpenGL.glTranslatef((double)(0f - num), (double)(0f - num2), 0.0);
			OpenGL.glTranslatef((double)base.canvas.xOffsetScaled, 0.0, 0.0);
			this.back.draw();
			if (this.mapHeight > FrameworkTypes.SCREEN_HEIGHT)
			{
				float num3 = FrameworkTypes.RTD(2.0);
				int pack = ((CTRRootController)Application.sharedRootController()).getPack();
				Texture2D texture = Application.getTexture(105 + pack * 2);
				int num4 = 0;
				float num5 = texture.quadOffsets[num4].y;
				CutTheRope.iframework.Rectangle r = texture.quadRects[num4];
				r.y += num3;
				r.h -= num3 * 2f;
				GLDrawer.drawImagePart(texture, r, 0.0, (double)(num5 + num3));
			}
			OpenGL.glEnable(1);
			OpenGL.glBlendFunc(BlendingFactor.GL_ONE, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
			if (this.earthAnims != null)
			{
				foreach (object obj in this.earthAnims)
				{
					((Image)obj).draw();
				}
			}
			OpenGL.glTranslatef((double)(-(double)base.canvas.xOffsetScaled), 0.0, 0.0);
			OpenGL.glPopMatrix();
			OpenGL.glEnable(1);
			OpenGL.glBlendFunc(BlendingFactor.GL_ONE, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
			this.pollenDrawer.draw();
			if (this.gravityButton != null)
			{
				this.gravityButton.draw();
			}
			OpenGL.glColor4f(Color.White);
			OpenGL.glEnable(0);
			OpenGL.glBlendFunc(BlendingFactor.GL_ONE, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
			this.support.draw();
			this.target.draw();
			foreach (object obj2 in this.tutorials)
			{
				((Text)obj2).draw();
			}
			foreach (object obj3 in this.tutorialImages)
			{
				((GameObject)obj3).draw();
			}
			foreach (object obj4 in this.razors)
			{
				((Razor)obj4).draw();
			}
			foreach (object obj5 in this.rotatedCircles)
			{
				((RotatedCircle)obj5).draw();
			}
			foreach (object obj6 in this.bubbles)
			{
				((GameObject)obj6).draw();
			}
			foreach (object obj7 in this.pumps)
			{
				((GameObject)obj7).draw();
			}
			foreach (object obj8 in this.spikes)
			{
				((Spikes)obj8).draw();
			}
			foreach (object obj9 in this.bouncers)
			{
				((Bouncer)obj9).draw();
			}
			foreach (object obj10 in this.socks)
			{
				Sock sock = (Sock)obj10;
				sock.y -= 85f;
				sock.draw();
				sock.y += 85f;
			}
			OpenGL.glBlendFunc(BlendingFactor.GL_SRC_ALPHA, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
			foreach (object obj11 in this.bungees)
			{
				((Grab)obj11).drawBack();
			}
			foreach (object obj12 in this.bungees)
			{
				((Grab)obj12).draw();
			}
			OpenGL.glBlendFunc(BlendingFactor.GL_ONE, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
			foreach (object obj13 in this.stars)
			{
				((GameObject)obj13).draw();
			}
			if (!this.noCandy && this.targetSock == null)
			{
				this.candy.x = this.star.pos.x;
				this.candy.y = this.star.pos.y;
				this.candy.draw();
				if (this.candyBlink.getCurrentTimeline() != null)
				{
					OpenGL.glBlendFunc(BlendingFactor.GL_SRC_ALPHA, BlendingFactor.GL_ONE);
					this.candyBlink.draw();
					OpenGL.glBlendFunc(BlendingFactor.GL_ONE, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
				}
			}
			if (this.twoParts != 2)
			{
				if (!this.noCandyL)
				{
					this.candyL.x = this.starL.pos.x;
					this.candyL.y = this.starL.pos.y;
					this.candyL.draw();
				}
				if (!this.noCandyR)
				{
					this.candyR.x = this.starR.pos.x;
					this.candyR.y = this.starR.pos.y;
					this.candyR.draw();
				}
			}
			foreach (object obj14 in this.bungees)
			{
				Grab bungee3 = (Grab)obj14;
				if (bungee3.hasSpider)
				{
					bungee3.drawSpider();
				}
			}
			this.aniPool.draw();
			bool flag = this.nightLevel;
			OpenGL.glBlendFunc(BlendingFactor.GL_SRC_ALPHA, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
			OpenGL.glDisable(0);
			OpenGL.glColor4f(Color.White);
			this.drawCuts();
			OpenGL.glEnable(0);
			OpenGL.glBlendFunc(BlendingFactor.GL_ONE, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
			this.camera.cancelCameraTransformation();
			this.staticAniPool.draw();
			if (this.nightLevel)
			{
				OpenGL.glDisable(4);
			}
			base.postDraw();
		}

		// Token: 0x06000519 RID: 1305 RVA: 0x00023AF4 File Offset: 0x00021CF4
		public virtual void drawCuts()
		{
			for (int i = 0; i < 5; i++)
			{
				int num = this.fingerCuts[i].count();
				if (num > 0)
				{
					float num2 = FrameworkTypes.RTD(6.0);
					float num3 = 1f;
					int num4 = 0;
					int j = 0;
					Vector[] array = new Vector[num + 1];
					int num5 = 0;
					while (j < num)
					{
						GameScene.FingerCut fingerCut = (GameScene.FingerCut)this.fingerCuts[i].objectAtIndex(j);
						if (j == 0)
						{
							array[num5++] = fingerCut.start;
						}
						array[num5++] = fingerCut.end;
						j++;
					}
					List<Vector> list = new List<Vector>();
					Vector vector = default(Vector);
					bool flag = true;
					for (int k = 0; k < array.Count<Vector>(); k++)
					{
						if (k == 0)
						{
							list.Add(array[k]);
						}
						else if (array[k].x != vector.x || array[k].y != vector.y)
						{
							list.Add(array[k]);
							flag = false;
						}
						vector = array[k];
					}
					if (!flag)
					{
						array = list.ToArray();
						num = array.Count<Vector>() - 1;
						int num6 = num * 2;
						float[] array2 = new float[num6 * 2];
						float num7 = 1f / (float)num6;
						float num8 = 0f;
						int num9 = 0;
						for (;;)
						{
							if ((double)num8 > 1.0)
							{
								num8 = 1f;
							}
							Vector vector2 = GLDrawer.calcPathBezier(array, num + 1, num8);
							if (num9 > array2.Count<float>() - 2)
							{
								break;
							}
							array2[num9++] = vector2.x;
							array2[num9++] = vector2.y;
							if ((double)num8 == 1.0)
							{
								break;
							}
							num8 += num7;
						}
						float num10 = num2 / (float)num6;
						float[] array3 = new float[num6 * 4];
						for (int l = 0; l < num6 - 1; l++)
						{
							float s = num3;
							float s2 = ((l == num6 - 2) ? 1f : (num3 + num10));
							Vector vector3 = CutTheRope.iframework.helpers.MathHelper.vect(array2[l * 2], array2[l * 2 + 1]);
							Vector vector8 = CutTheRope.iframework.helpers.MathHelper.vect(array2[(l + 1) * 2], array2[(l + 1) * 2 + 1]);
							Vector vector9 = CutTheRope.iframework.helpers.MathHelper.vectNormalize(CutTheRope.iframework.helpers.MathHelper.vectSub(vector8, vector3));
							Vector v4 = CutTheRope.iframework.helpers.MathHelper.vectRperp(vector9);
							Vector v5 = CutTheRope.iframework.helpers.MathHelper.vectPerp(vector9);
							if (num4 == 0)
							{
								Vector vector4 = CutTheRope.iframework.helpers.MathHelper.vectAdd(vector3, CutTheRope.iframework.helpers.MathHelper.vectMult(v4, s));
								Vector vector5 = CutTheRope.iframework.helpers.MathHelper.vectAdd(vector3, CutTheRope.iframework.helpers.MathHelper.vectMult(v5, s));
								array3[num4++] = vector5.x;
								array3[num4++] = vector5.y;
								array3[num4++] = vector4.x;
								array3[num4++] = vector4.y;
							}
							Vector vector6 = CutTheRope.iframework.helpers.MathHelper.vectAdd(vector8, CutTheRope.iframework.helpers.MathHelper.vectMult(v4, s2));
							Vector vector7 = CutTheRope.iframework.helpers.MathHelper.vectAdd(vector8, CutTheRope.iframework.helpers.MathHelper.vectMult(v5, s2));
							array3[num4++] = vector7.x;
							array3[num4++] = vector7.y;
							array3[num4++] = vector6.x;
							array3[num4++] = vector6.y;
							num3 += num10;
						}
						OpenGL.glColor4f(Color.White);
						OpenGL.glVertexPointer(2, 5, 0, array3);
						OpenGL.glDrawArrays(8, 0, num4 / 2);
					}
				}
			}
		}

		// Token: 0x0600051A RID: 1306 RVA: 0x00023E5C File Offset: 0x0002205C
		public virtual void handlePumpFlowPtSkin(Pump p, ConstraintedPoint s, GameObject c)
		{
			float num = 624f;
			if (GameObject.rectInObject(p.x - num, p.y - num, p.x + num, p.y + num, c))
			{
				Vector v = CutTheRope.iframework.helpers.MathHelper.vect(c.x, c.y);
				Vector vector = default(Vector);
				vector.x = p.x - p.bb.w / 2f;
				Vector vector2 = default(Vector);
				vector2.x = p.x + p.bb.w / 2f;
				vector.y = (vector2.y = p.y);
				if (p.angle != 0.0)
				{
					v = CutTheRope.iframework.helpers.MathHelper.vectRotateAround(v, 0.0 - p.angle, p.x, p.y);
				}
				if (v.y < vector.y && CutTheRope.iframework.helpers.MathHelper.rectInRect((float)((double)v.x - (double)c.bb.w / 2.0), (float)((double)v.y - (double)c.bb.h / 2.0), (float)((double)v.x + (double)c.bb.w / 2.0), (float)((double)v.y + (double)c.bb.h / 2.0), vector.x, vector.y - num, vector2.x, vector2.y))
				{
					float num2 = num * 2f * (num - (vector.y - v.y)) / num;
					Vector v2 = CutTheRope.iframework.helpers.MathHelper.vect(0f, 0f - num2);
					v2 = CutTheRope.iframework.helpers.MathHelper.vectRotate(v2, p.angle);
					s.applyImpulseDelta(v2, 0.016f);
				}
			}
		}

		// Token: 0x0600051B RID: 1307 RVA: 0x00024040 File Offset: 0x00022240
		public virtual void handleBouncePtDelta(Bouncer b, ConstraintedPoint s, float delta)
		{
			if (!b.skip)
			{
				b.skip = true;
				Vector vector = CutTheRope.iframework.helpers.MathHelper.vectSub(s.prevPos, s.pos);
				int num = ((CutTheRope.iframework.helpers.MathHelper.vectRotateAround(s.prevPos, (double)(0f - b.angle), b.x, b.y).y >= b.y) ? 1 : (-1));
				float s2 = CutTheRope.iframework.helpers.MathHelper.MAX((double)(CutTheRope.iframework.helpers.MathHelper.vectLength(vector) * 40f), 840.0) * (float)num;
				Vector impulse = CutTheRope.iframework.helpers.MathHelper.vectMult(CutTheRope.iframework.helpers.MathHelper.vectPerp(CutTheRope.iframework.helpers.MathHelper.vectForAngle(b.angle)), s2);
				s.pos = CutTheRope.iframework.helpers.MathHelper.vectRotateAround(s.pos, (double)(0f - b.angle), b.x, b.y);
				s.prevPos = CutTheRope.iframework.helpers.MathHelper.vectRotateAround(s.prevPos, (double)(0f - b.angle), b.x, b.y);
				s.prevPos.y = s.pos.y;
				s.pos = CutTheRope.iframework.helpers.MathHelper.vectRotateAround(s.pos, (double)b.angle, b.x, b.y);
				s.prevPos = CutTheRope.iframework.helpers.MathHelper.vectRotateAround(s.prevPos, (double)b.angle, b.x, b.y);
				s.applyImpulseDelta(impulse, delta);
				b.playTimeline(0);
				CTRSoundMgr._playSound(41);
			}
		}

		// Token: 0x0600051C RID: 1308 RVA: 0x000241A4 File Offset: 0x000223A4
		public virtual void operatePump(Pump p)
		{
			p.playTimeline(0);
			CTRSoundMgr._playSound(CutTheRope.iframework.helpers.MathHelper.RND_RANGE(29, 32));
			Image grid = Image.Image_createWithResID(83);
			PumpDirt pumpDirt = new PumpDirt().initWithTotalParticlesAngleandImageGrid(5, CutTheRope.iframework.helpers.MathHelper.RADIANS_TO_DEGREES((float)p.angle) - 90f, grid);
			pumpDirt.particlesDelegate = new Particles.ParticlesFinished(this.aniPool.particlesFinished);
			Vector v = CutTheRope.iframework.helpers.MathHelper.vect(p.x + 80f, p.y);
			v = CutTheRope.iframework.helpers.MathHelper.vectRotateAround(v, p.angle - 1.5707963267948966, p.x, p.y);
			pumpDirt.x = v.x;
			pumpDirt.y = v.y;
			pumpDirt.startSystem(5);
			this.aniPool.addChild(pumpDirt);
			if (!this.noCandy)
			{
				this.handlePumpFlowPtSkin(p, this.star, this.candy);
			}
			if (this.twoParts != 2)
			{
				if (!this.noCandyL)
				{
					this.handlePumpFlowPtSkin(p, this.starL, this.candyL);
				}
				if (!this.noCandyR)
				{
					this.handlePumpFlowPtSkin(p, this.starR, this.candyR);
				}
			}
		}

		// Token: 0x0600051D RID: 1309 RVA: 0x000242C8 File Offset: 0x000224C8
		public virtual int cutWithRazorOrLine1Line2Immediate(Razor r, Vector v1, Vector v2, bool im)
		{
			int num = 0;
			for (int i = 0; i < this.bungees.count(); i++)
			{
				Grab grab = (Grab)this.bungees.objectAtIndex(i);
				Bungee rope = grab.rope;
				if (rope != null && rope.cut == -1)
				{
					for (int j = 0; j < rope.parts.Count - 1; j++)
					{
						ConstraintedPoint constraintedPoint = rope.parts[j];
						ConstraintedPoint constraintedPoint2 = rope.parts[j + 1];
						bool flag = false;
						if (r == null)
						{
							flag = (!grab.wheel || !CutTheRope.iframework.helpers.MathHelper.lineInRect(v1.x, v1.y, v2.x, v2.y, grab.x - 110f, grab.y - 110f, 220f, 220f)) && base.lineInLine(v1.x, v1.y, v2.x, v2.y, constraintedPoint.pos.x, constraintedPoint.pos.y, constraintedPoint2.pos.x, constraintedPoint2.pos.y);
						}
						else if (constraintedPoint.prevPos.x != 2.1474836E+09f)
						{
							float num2 = GameScene.minOf4(constraintedPoint.pos.x, constraintedPoint.prevPos.x, constraintedPoint2.pos.x, constraintedPoint2.prevPos.x);
							float y1t = GameScene.minOf4(constraintedPoint.pos.y, constraintedPoint.prevPos.y, constraintedPoint2.pos.y, constraintedPoint2.prevPos.y);
							float x1r = GameScene.maxOf4(constraintedPoint.pos.x, constraintedPoint.prevPos.x, constraintedPoint2.pos.x, constraintedPoint2.prevPos.x);
							float y1b = GameScene.maxOf4(constraintedPoint.pos.y, constraintedPoint.prevPos.y, constraintedPoint2.pos.y, constraintedPoint2.prevPos.y);
							flag = CutTheRope.iframework.helpers.MathHelper.rectInRect(num2, y1t, x1r, y1b, r.drawX, r.drawY, r.drawX + (float)r.width, r.drawY + (float)r.height);
						}
						if (flag)
						{
							num++;
							if (grab.hasSpider && grab.spiderActive)
							{
								this.spiderBusted(grab);
							}
							CTRSoundMgr._playSound(20 + rope.relaxed);
							rope.setCut(j);
							if (im)
							{
								rope.cutTime = 0f;
								rope.removePart(j);
							}
							return num;
						}
					}
				}
			}
			return num;
		}

		// Token: 0x0600051E RID: 1310 RVA: 0x00024580 File Offset: 0x00022780
		public virtual void spiderBusted(Grab g)
		{
			int num = Preferences._getIntForKey("PREFS_SPIDERS_BUSTED") + 1;
			Preferences._setIntforKey(num, "PREFS_SPIDERS_BUSTED", false);
			if (num == 40)
			{
				CTRRootController.postAchievementName("681486608", FrameworkTypes.ACHIEVEMENT_STRING("\"Spider Busted\""));
			}
			if (num == 200)
			{
				CTRRootController.postAchievementName("1058341284", FrameworkTypes.ACHIEVEMENT_STRING("\"Spider Tammer\""));
			}
			CTRSoundMgr._playSound(34);
			g.hasSpider = false;
			Image image = Image.Image_createWithResIDQuad(64, 11);
			image.doRestoreCutTransparency();
			Timeline timeline = new Timeline().initWithMaxKeyFramesOnTrack(3);
			if (this.gravityButton != null && !this.gravityNormal)
			{
				timeline.addKeyFrame(KeyFrame.makePos((double)g.spider.x, (double)g.spider.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.0));
				timeline.addKeyFrame(KeyFrame.makePos((double)g.spider.x, (double)g.spider.y + 50.0, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.3));
				timeline.addKeyFrame(KeyFrame.makePos((double)g.spider.x, (double)(g.spider.y - FrameworkTypes.SCREEN_HEIGHT), KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 1.0));
			}
			else
			{
				timeline.addKeyFrame(KeyFrame.makePos((double)g.spider.x, (double)g.spider.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.0));
				timeline.addKeyFrame(KeyFrame.makePos((double)g.spider.x, (double)g.spider.y - 50.0, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.3));
				timeline.addKeyFrame(KeyFrame.makePos((double)g.spider.x, (double)(g.spider.y + FrameworkTypes.SCREEN_HEIGHT), KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 1.0));
			}
			timeline.addKeyFrame(KeyFrame.makeRotation(0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
			timeline.addKeyFrame(KeyFrame.makeRotation((double)CutTheRope.iframework.helpers.MathHelper.RND_RANGE(-120, 120), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1.0));
			image.addTimelinewithID(timeline, 0);
			image.playTimeline(0);
			image.x = g.spider.x;
			image.y = g.spider.y;
			image.anchor = 18;
			timeline.delegateTimelineDelegate = this.aniPool;
			this.aniPool.addChild(image);
		}

		// Token: 0x0600051F RID: 1311 RVA: 0x000247DC File Offset: 0x000229DC
		public virtual void spiderWon(Grab sg)
		{
			CTRSoundMgr._playSound(35);
			int num = this.bungees.count();
			for (int i = 0; i < num; i++)
			{
				Grab grab = (Grab)this.bungees.objectAtIndex(i);
				Bungee rope = grab.rope;
				if (rope != null && rope.tail == this.star)
				{
					if (rope.cut == -1)
					{
						rope.setCut(rope.parts.Count - 2);
						rope.forceWhite = false;
					}
					if (grab.hasSpider && grab.spiderActive && sg != grab)
					{
						this.spiderBusted(grab);
					}
				}
			}
			sg.hasSpider = false;
			this.spiderTookCandy = true;
			this.noCandy = true;
			Image image = Image.Image_createWithResIDQuad(64, 12);
			image.doRestoreCutTransparency();
			this.candy.anchor = (this.candy.parentAnchor = 18);
			this.candy.x = 0f;
			this.candy.y = -5f;
			image.addChild(this.candy);
			Timeline timeline = new Timeline().initWithMaxKeyFramesOnTrack(3);
			if (this.gravityButton != null && !this.gravityNormal)
			{
				timeline.addKeyFrame(KeyFrame.makePos((double)sg.spider.x, (double)sg.spider.y - 10.0, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.0));
				timeline.addKeyFrame(KeyFrame.makePos((double)sg.spider.x, (double)sg.spider.y + 70.0, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.3));
				timeline.addKeyFrame(KeyFrame.makePos((double)sg.spider.x, (double)(sg.spider.y - FrameworkTypes.SCREEN_HEIGHT), KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 1.0));
			}
			else
			{
				timeline.addKeyFrame(KeyFrame.makePos((double)sg.spider.x, (double)sg.spider.y - 10.0, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.0));
				timeline.addKeyFrame(KeyFrame.makePos((double)sg.spider.x, (double)sg.spider.y - 70.0, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.3));
				timeline.addKeyFrame(KeyFrame.makePos((double)sg.spider.x, (double)(sg.spider.y + FrameworkTypes.SCREEN_HEIGHT), KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 1.0));
			}
			image.addTimelinewithID(timeline, 0);
			image.playTimeline(0);
			image.x = sg.spider.x;
			image.y = sg.spider.y - 10f;
			image.anchor = 18;
			timeline.delegateTimelineDelegate = this.aniPool;
			this.aniPool.addChild(image);
			if (this.restartState != 0)
			{
				this.dd.callObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(this.selector_gameLost), null, 2.0);
			}
		}

		// Token: 0x06000520 RID: 1312 RVA: 0x00024AD8 File Offset: 0x00022CD8
		public virtual void popCandyBubble(bool left)
		{
			if (this.twoParts == 2)
			{
				this.candyBubble = null;
				this.candyBubbleAnimation.visible = false;
				this.popBubbleAtXY(this.candy.x, this.candy.y);
				return;
			}
			if (left)
			{
				this.candyBubbleL = null;
				this.candyBubbleAnimationL.visible = false;
				this.popBubbleAtXY(this.candyL.x, this.candyL.y);
				return;
			}
			this.candyBubbleR = null;
			this.candyBubbleAnimationR.visible = false;
			this.popBubbleAtXY(this.candyR.x, this.candyR.y);
		}

		// Token: 0x06000521 RID: 1313 RVA: 0x00024B80 File Offset: 0x00022D80
		public virtual void popBubbleAtXY(float bx, float by)
		{
			CTRSoundMgr._playSound(12);
			Animation animation = Animation.Animation_createWithResID(73);
			animation.doRestoreCutTransparency();
			animation.x = bx;
			animation.y = by;
			animation.anchor = 18;
			int i = animation.addAnimationDelayLoopFirstLast(0.05, Timeline.LoopType.TIMELINE_NO_LOOP, 0, 11);
			animation.getTimeline(i).delegateTimelineDelegate = this.aniPool;
			animation.playTimeline(0);
			this.aniPool.addChild(animation);
		}

		// Token: 0x06000522 RID: 1314 RVA: 0x00024BF4 File Offset: 0x00022DF4
		public virtual bool handleBubbleTouchXY(ConstraintedPoint s, float tx, float ty)
		{
			if (CutTheRope.iframework.helpers.MathHelper.pointInRect(tx + this.camera.pos.x, ty + this.camera.pos.y, s.pos.x - 60f, s.pos.y - 60f, 120f, 120f))
			{
				this.popCandyBubble(s == this.starL);
				int num = Preferences._getIntForKey("PREFS_BUBBLES_POPPED") + 1;
				Preferences._setIntforKey(num, "PREFS_BUBBLES_POPPED", false);
				if (num == 50)
				{
					CTRRootController.postAchievementName("681513183", FrameworkTypes.ACHIEVEMENT_STRING("\"Bubble Popper\""));
				}
				if (num == 300)
				{
					CTRRootController.postAchievementName("1058345234", FrameworkTypes.ACHIEVEMENT_STRING("\"Bubble Master\""));
				}
				return true;
			}
			return false;
		}

		// Token: 0x06000523 RID: 1315 RVA: 0x00024CB8 File Offset: 0x00022EB8
		public virtual void resetBungeeHighlight()
		{
			for (int i = 0; i < this.bungees.count(); i++)
			{
				Bungee rope = ((Grab)this.bungees.objectAtIndex(i)).rope;
				if (rope != null && rope.cut == -1)
				{
					rope.highlighted = false;
				}
			}
		}

		// Token: 0x06000524 RID: 1316 RVA: 0x00024D08 File Offset: 0x00022F08
		public virtual Bungee getNearestBungeeSegmentByBeziersPointsatXYgrab(ref Vector s, float tx, float ty, ref Grab grab)
		{
			float num = 60f;
			Bungee result = null;
			float num2 = num;
			Vector v = CutTheRope.iframework.helpers.MathHelper.vect(tx, ty);
			for (int i = 0; i < this.bungees.count(); i++)
			{
				Grab grab2 = (Grab)this.bungees.objectAtIndex(i);
				Bungee rope = grab2.rope;
				if (rope != null)
				{
					for (int j = 0; j < rope.drawPtsCount; j += 2)
					{
						Vector vector = CutTheRope.iframework.helpers.MathHelper.vect(rope.drawPts[j], rope.drawPts[j + 1]);
						float num3 = CutTheRope.iframework.helpers.MathHelper.vectDistance(vector, v);
						if (num3 < num && num3 < num2)
						{
							num2 = num3;
							result = rope;
							s = vector;
							grab = grab2;
						}
					}
				}
			}
			return result;
		}

		// Token: 0x06000525 RID: 1317 RVA: 0x00024DC4 File Offset: 0x00022FC4
		public virtual Bungee getNearestBungeeSegmentByConstraintsforGrab(ref Vector s, Grab g)
		{
			float num4 = 2.1474836E+09f;
			Bungee result = null;
			float num2 = num4;
			Vector v = s;
			Bungee rope = g.rope;
			if (rope == null || rope.cut != -1)
			{
				return null;
			}
			for (int i = 0; i < rope.parts.Count - 1; i++)
			{
				ConstraintedPoint constraintedPoint = rope.parts[i];
				float num3 = CutTheRope.iframework.helpers.MathHelper.vectDistance(constraintedPoint.pos, v);
				if (num3 < num2 && (!g.wheel || !CutTheRope.iframework.helpers.MathHelper.pointInRect(constraintedPoint.pos.x, constraintedPoint.pos.y, g.x - 110f, g.y - 110f, 220f, 220f)))
				{
					num2 = num3;
					result = rope;
					s = constraintedPoint.pos;
				}
			}
			return result;
		}

		// Token: 0x06000526 RID: 1318 RVA: 0x00024E98 File Offset: 0x00023098
		public virtual bool touchDownXYIndex(float tx, float ty, int ti)
		{
			if (this.ignoreTouches)
			{
				if (this.camera.type == CAMERA_TYPE.CAMERA_SPEED_PIXELS)
				{
					this.fastenCamera = true;
				}
				return true;
			}
			if (ti >= 5)
			{
				return true;
			}
			if (this.gravityButton != null && ((Button)this.gravityButton.getChild((this.gravityButton.on() > false) ? 1 : 0)).isInTouchZoneXYforTouchDown(tx + this.camera.pos.x, ty + this.camera.pos.y, true))
			{
				this.gravityTouchDown = ti;
			}
			Vector vector = CutTheRope.iframework.helpers.MathHelper.vect(tx, ty);
			if (this.candyBubble != null && this.handleBubbleTouchXY(this.star, tx, ty))
			{
				return true;
			}
			if (this.twoParts != 2)
			{
				if (this.candyBubbleL != null && this.handleBubbleTouchXY(this.starL, tx, ty))
				{
					return true;
				}
				if (this.candyBubbleR != null && this.handleBubbleTouchXY(this.starR, tx, ty))
				{
					return true;
				}
			}
			if (!this.dragging[ti])
			{
				this.dragging[ti] = true;
				this.prevStartPos[ti] = (this.startPos[ti] = vector);
			}
			foreach (object obj in this.spikes)
			{
				Spikes spike = (Spikes)obj;
				if (spike.rotateButton != null && spike.touchIndex == -1 && spike.rotateButton.onTouchDownXY(tx + this.camera.pos.x, ty + this.camera.pos.y))
				{
					spike.touchIndex = ti;
					return true;
				}
			}
			int num = this.pumps.count();
			for (int i = 0; i < num; i++)
			{
				Pump pump = (Pump)this.pumps.objectAtIndex(i);
				if (GameObject.pointInObject(CutTheRope.iframework.helpers.MathHelper.vect(tx + this.camera.pos.x, ty + this.camera.pos.y), pump))
				{
					pump.pumpTouchTimer = 0.05f;
					pump.pumpTouch = ti;
					return true;
				}
			}
			RotatedCircle rotatedCircle = null;
			bool flag = false;
			bool flag2 = false;
			foreach (object obj2 in this.rotatedCircles)
			{
				RotatedCircle rotatedCircle2 = (RotatedCircle)obj2;
				float num2 = CutTheRope.iframework.helpers.MathHelper.vectDistance(CutTheRope.iframework.helpers.MathHelper.vect(tx + this.camera.pos.x, ty + this.camera.pos.y), rotatedCircle2.handle1);
				float num3 = CutTheRope.iframework.helpers.MathHelper.vectDistance(CutTheRope.iframework.helpers.MathHelper.vect(tx + this.camera.pos.x, ty + this.camera.pos.y), rotatedCircle2.handle2);
				if ((num2 < 90f && !rotatedCircle2.hasOneHandle()) || num3 < 90f)
				{
					foreach (object obj3 in this.rotatedCircles)
					{
						RotatedCircle rotatedCircle3 = (RotatedCircle)obj3;
						if (this.rotatedCircles.getObjectIndex(rotatedCircle3) > this.rotatedCircles.getObjectIndex(rotatedCircle2))
						{
							float num4 = CutTheRope.iframework.helpers.MathHelper.vectDistance(CutTheRope.iframework.helpers.MathHelper.vect(rotatedCircle3.x, rotatedCircle3.y), CutTheRope.iframework.helpers.MathHelper.vect(rotatedCircle2.x, rotatedCircle2.y));
							if (num4 + rotatedCircle3.sizeInPixels <= rotatedCircle2.sizeInPixels)
							{
								flag = true;
							}
							if (num4 <= rotatedCircle2.sizeInPixels + rotatedCircle3.sizeInPixels)
							{
								flag2 = true;
							}
						}
					}
					rotatedCircle2.lastTouch = CutTheRope.iframework.helpers.MathHelper.vect(tx + this.camera.pos.x, ty + this.camera.pos.y);
					rotatedCircle2.operating = ti;
					if (num2 < 90f)
					{
						rotatedCircle2.setIsLeftControllerActive(true);
					}
					if (num3 < 90f)
					{
						rotatedCircle2.setIsRightControllerActive(true);
					}
					rotatedCircle = rotatedCircle2;
					break;
				}
			}
			if (this.rotatedCircles.getObjectIndex(rotatedCircle) != this.rotatedCircles.count() - 1 && flag2 && !flag)
			{
				Timeline timeline = new Timeline().initWithMaxKeyFramesOnTrack(2);
				timeline.addKeyFrame(KeyFrame.makeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
				timeline.addKeyFrame(KeyFrame.makeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.2));
				Timeline timeline2 = new Timeline().initWithMaxKeyFramesOnTrack(1);
				timeline2.addKeyFrame(KeyFrame.makeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.2));
				timeline2.delegateTimelineDelegate = this;
				RotatedCircle rotatedCircle4 = (RotatedCircle)rotatedCircle.copy();
				rotatedCircle4.addTimeline(timeline2);
				rotatedCircle4.playTimeline(0);
				rotatedCircle.addTimeline(timeline);
				rotatedCircle.playTimeline(0);
				rotatedCircle.retain();
				this.rotatedCircles.setObjectAt(rotatedCircle4, this.rotatedCircles.getObjectIndex(rotatedCircle));
				this.rotatedCircles.addObject(rotatedCircle);
				rotatedCircle.release();
			}
			foreach (object obj4 in this.bungees)
			{
				Grab bungee = (Grab)obj4;
				if (bungee.wheel && CutTheRope.iframework.helpers.MathHelper.pointInRect(tx + this.camera.pos.x, ty + this.camera.pos.y, bungee.x - 110f, bungee.y - 110f, 220f, 220f))
				{
					bungee.handleWheelTouch(CutTheRope.iframework.helpers.MathHelper.vect(tx + this.camera.pos.x, ty + this.camera.pos.y));
					bungee.wheelOperating = ti;
				}
				if ((double)bungee.moveLength > 0.0 && CutTheRope.iframework.helpers.MathHelper.pointInRect(tx + this.camera.pos.x, ty + this.camera.pos.y, bungee.x - 65f, bungee.y - 65f, 130f, 130f))
				{
					bungee.moverDragging = ti;
					return true;
				}
			}
			if (this.clickToCut && !this.ignoreTouches)
			{
				Vector s = default(Vector);
				Grab grab2 = null;
				Bungee nearestBungeeSegmentByBeziersPointsatXYgrab = this.getNearestBungeeSegmentByBeziersPointsatXYgrab(ref s, tx + this.camera.pos.x, ty + this.camera.pos.y, ref grab2);
				if (nearestBungeeSegmentByBeziersPointsatXYgrab != null && nearestBungeeSegmentByBeziersPointsatXYgrab.highlighted && this.getNearestBungeeSegmentByConstraintsforGrab(ref s, grab2) != null)
				{
					this.cutWithRazorOrLine1Line2Immediate(null, s, s, false);
				}
			}
			return true;
		}

		// Token: 0x06000527 RID: 1319 RVA: 0x000255C4 File Offset: 0x000237C4
		public virtual bool touchUpXYIndex(float tx, float ty, int ti)
		{
			if (this.ignoreTouches)
			{
				return true;
			}
			this.dragging[ti] = false;
			if (ti >= 5)
			{
				return true;
			}
			if (this.gravityButton != null && this.gravityTouchDown == ti)
			{
				if (((Button)this.gravityButton.getChild((this.gravityButton.on() > false) ? 1 : 0)).isInTouchZoneXYforTouchDown(tx + this.camera.pos.x, ty + this.camera.pos.y, true))
				{
					this.gravityButton.toggle();
					this.onButtonPressed(0);
				}
				this.gravityTouchDown = -1;
			}
			foreach (object obj in this.spikes)
			{
				Spikes spike = (Spikes)obj;
				if (spike.rotateButton != null && spike.touchIndex == ti)
				{
					spike.touchIndex = -1;
					if (spike.rotateButton.onTouchUpXY(tx + this.camera.pos.x, ty + this.camera.pos.y))
					{
						return true;
					}
				}
			}
			foreach (object obj2 in this.rotatedCircles)
			{
				RotatedCircle rotatedCircle = (RotatedCircle)obj2;
				if (rotatedCircle.operating == ti)
				{
					rotatedCircle.operating = -1;
					rotatedCircle.soundPlaying = -1;
					rotatedCircle.setIsLeftControllerActive(false);
					rotatedCircle.setIsRightControllerActive(false);
				}
			}
			foreach (object obj3 in this.bungees)
			{
				Grab bungee = (Grab)obj3;
				if (bungee.wheel && bungee.wheelOperating == ti)
				{
					bungee.wheelOperating = -1;
				}
				if ((double)bungee.moveLength > 0.0 && bungee.moverDragging == ti)
				{
					bungee.moverDragging = -1;
				}
			}
			return true;
		}

		// Token: 0x06000528 RID: 1320 RVA: 0x000257E4 File Offset: 0x000239E4
		public virtual bool touchMoveXYIndex(float tx, float ty, int ti)
		{
			if (this.ignoreTouches)
			{
				return true;
			}
			Vector vector = CutTheRope.iframework.helpers.MathHelper.vect(tx, ty);
			if (ti >= 5)
			{
				return true;
			}
			foreach (object obj in this.pumps)
			{
				Pump pump3 = (Pump)obj;
				if (pump3.pumpTouch == ti && (double)pump3.pumpTouchTimer != 0.0 && (double)CutTheRope.iframework.helpers.MathHelper.vectDistance(this.startPos[ti], vector) > 10.0)
				{
					pump3.pumpTouchTimer = 0f;
				}
			}
			if (this.rotatedCircles != null)
			{
				for (int i = 0; i < this.rotatedCircles.count(); i++)
				{
					RotatedCircle rotatedCircle = (RotatedCircle)this.rotatedCircles[i];
					if (rotatedCircle != null && rotatedCircle.operating == ti)
					{
						Vector v = CutTheRope.iframework.helpers.MathHelper.vect(rotatedCircle.x, rotatedCircle.y);
						Vector vector2 = CutTheRope.iframework.helpers.MathHelper.vect(tx + this.camera.pos.x, ty + this.camera.pos.y);
						Vector v2 = CutTheRope.iframework.helpers.MathHelper.vectSub(rotatedCircle.lastTouch, v);
						float num = CutTheRope.iframework.helpers.MathHelper.vectAngleNormalized(CutTheRope.iframework.helpers.MathHelper.vectSub(vector2, v)) - CutTheRope.iframework.helpers.MathHelper.vectAngleNormalized(v2);
						float initial_rotation = CutTheRope.iframework.helpers.MathHelper.DEGREES_TO_RADIANS(rotatedCircle.rotation);
						rotatedCircle.rotation += CutTheRope.iframework.helpers.MathHelper.RADIANS_TO_DEGREES(num);
						float a = CutTheRope.iframework.helpers.MathHelper.DEGREES_TO_RADIANS(rotatedCircle.rotation);
						a = GameScene.FBOUND_PI(a);
						rotatedCircle.handle1 = CutTheRope.iframework.helpers.MathHelper.vectRotateAround(rotatedCircle.inithanlde1, (double)a, rotatedCircle.x, rotatedCircle.y);
						rotatedCircle.handle2 = CutTheRope.iframework.helpers.MathHelper.vectRotateAround(rotatedCircle.inithanlde2, (double)a, rotatedCircle.x, rotatedCircle.y);
						int num2 = ((num > 0f) ? 46 : 47);
						if ((double)Math.Abs(num) < 0.07)
						{
							num2 = -1;
						}
						if (rotatedCircle.soundPlaying != num2 && num2 != -1)
						{
							CTRSoundMgr._playSound(num2);
							rotatedCircle.soundPlaying = num2;
						}
						for (int j = 0; j < this.bungees.count(); j++)
						{
							Grab grab = (Grab)this.bungees[j];
							if (CutTheRope.iframework.helpers.MathHelper.vectDistance(CutTheRope.iframework.helpers.MathHelper.vect(grab.x, grab.y), CutTheRope.iframework.helpers.MathHelper.vect(rotatedCircle.x, rotatedCircle.y)) <= rotatedCircle.sizeInPixels + 5f)
							{
								if (grab.initial_rotatedCircle != rotatedCircle)
								{
									grab.initial_x = grab.x;
									grab.initial_y = grab.y;
									grab.initial_rotatedCircle = rotatedCircle;
									grab.initial_rotation = initial_rotation;
								}
								float a2 = CutTheRope.iframework.helpers.MathHelper.DEGREES_TO_RADIANS(rotatedCircle.rotation) - grab.initial_rotation;
								a2 = GameScene.FBOUND_PI(a2);
								Vector vector3 = CutTheRope.iframework.helpers.MathHelper.vectRotateAround(CutTheRope.iframework.helpers.MathHelper.vect(grab.initial_x, grab.initial_y), (double)a2, rotatedCircle.x, rotatedCircle.y);
								grab.x = vector3.x;
								grab.y = vector3.y;
								if (grab.rope != null)
								{
									grab.rope.bungeeAnchor.pos = CutTheRope.iframework.helpers.MathHelper.vect(grab.x, grab.y);
									grab.rope.bungeeAnchor.pin = grab.rope.bungeeAnchor.pos;
								}
								if (grab.radius != -1f)
								{
									grab.reCalcCircle();
								}
							}
						}
						for (int k = 0; k < this.pumps.count(); k++)
						{
							Pump pump4 = (Pump)this.pumps[k];
							if (CutTheRope.iframework.helpers.MathHelper.vectDistance(CutTheRope.iframework.helpers.MathHelper.vect(pump4.x, pump4.y), CutTheRope.iframework.helpers.MathHelper.vect(rotatedCircle.x, rotatedCircle.y)) <= rotatedCircle.sizeInPixels + 5f)
							{
								if (pump4.initial_rotatedCircle != rotatedCircle)
								{
									pump4.initial_x = pump4.x;
									pump4.initial_y = pump4.y;
									pump4.initial_rotatedCircle = rotatedCircle;
									pump4.initial_rotation = initial_rotation;
								}
								float a3 = CutTheRope.iframework.helpers.MathHelper.DEGREES_TO_RADIANS(rotatedCircle.rotation) - pump4.initial_rotation;
								a3 = GameScene.FBOUND_PI(a3);
								Vector vector4 = CutTheRope.iframework.helpers.MathHelper.vectRotateAround(CutTheRope.iframework.helpers.MathHelper.vect(pump4.initial_x, pump4.initial_y), (double)a3, rotatedCircle.x, rotatedCircle.y);
								pump4.x = vector4.x;
								pump4.y = vector4.y;
								pump4.rotation += CutTheRope.iframework.helpers.MathHelper.RADIANS_TO_DEGREES(num);
								pump4.updateRotation();
							}
						}
						for (int l = 0; l < this.bubbles.count(); l++)
						{
							Bubble bubble = (Bubble)this.bubbles[l];
							if (CutTheRope.iframework.helpers.MathHelper.vectDistance(CutTheRope.iframework.helpers.MathHelper.vect(bubble.x, bubble.y), CutTheRope.iframework.helpers.MathHelper.vect(rotatedCircle.x, rotatedCircle.y)) <= rotatedCircle.sizeInPixels + 10f && bubble != this.candyBubble && bubble != this.candyBubbleR && bubble != this.candyBubbleL)
							{
								if (bubble.initial_rotatedCircle != rotatedCircle)
								{
									bubble.initial_x = bubble.x;
									bubble.initial_y = bubble.y;
									bubble.initial_rotatedCircle = rotatedCircle;
									bubble.initial_rotation = initial_rotation;
								}
								float a4 = CutTheRope.iframework.helpers.MathHelper.DEGREES_TO_RADIANS(rotatedCircle.rotation) - bubble.initial_rotation;
								a4 = GameScene.FBOUND_PI(a4);
								Vector vector5 = CutTheRope.iframework.helpers.MathHelper.vectRotateAround(CutTheRope.iframework.helpers.MathHelper.vect(bubble.initial_x, bubble.initial_y), (double)a4, rotatedCircle.x, rotatedCircle.y);
								bubble.x = vector5.x;
								bubble.y = vector5.y;
							}
						}
						if (CutTheRope.iframework.helpers.MathHelper.pointInRect(this.target.x, this.target.y, rotatedCircle.x - rotatedCircle.size, rotatedCircle.y - rotatedCircle.size, 2f * rotatedCircle.size, 2f * rotatedCircle.size))
						{
							Vector vector6 = CutTheRope.iframework.helpers.MathHelper.vectRotateAround(CutTheRope.iframework.helpers.MathHelper.vect(this.target.x, this.target.y), (double)num, rotatedCircle.x, rotatedCircle.y);
							this.target.x = vector6.x;
							this.target.y = vector6.y;
						}
						rotatedCircle.lastTouch = vector2;
						return true;
					}
				}
			}
			int num3 = this.bungees.count();
			for (int m = 0; m < num3; m++)
			{
				Grab grab2 = (Grab)this.bungees.objectAtIndex(m);
				if (grab2 != null)
				{
					if (grab2.wheel && grab2.wheelOperating == ti)
					{
						grab2.handleWheelRotate(CutTheRope.iframework.helpers.MathHelper.vect(tx + this.camera.pos.x, ty + this.camera.pos.y));
						return true;
					}
					if ((double)grab2.moveLength > 0.0 && grab2.moverDragging == ti)
					{
						if (grab2.moveVertical)
						{
							grab2.y = CutTheRope.iframework.helpers.MathHelper.FIT_TO_BOUNDARIES(ty + this.camera.pos.y, grab2.minMoveValue, grab2.maxMoveValue);
						}
						else
						{
							grab2.x = CutTheRope.iframework.helpers.MathHelper.FIT_TO_BOUNDARIES(tx + this.camera.pos.x, grab2.minMoveValue, grab2.maxMoveValue);
						}
						if (grab2.rope != null)
						{
							grab2.rope.bungeeAnchor.pos = CutTheRope.iframework.helpers.MathHelper.vect(grab2.x, grab2.y);
							grab2.rope.bungeeAnchor.pin = grab2.rope.bungeeAnchor.pos;
						}
						if (grab2.radius != -1f)
						{
							grab2.reCalcCircle();
						}
						return true;
					}
				}
			}
			if (this.dragging[ti])
			{
				Vector start = CutTheRope.iframework.helpers.MathHelper.vectAdd(this.startPos[ti], this.camera.pos);
				Vector end = CutTheRope.iframework.helpers.MathHelper.vectAdd(CutTheRope.iframework.helpers.MathHelper.vect(tx, ty), this.camera.pos);
				GameScene.FingerCut fingerCut = (GameScene.FingerCut)new GameScene.FingerCut().init();
				fingerCut.start = start;
				fingerCut.end = end;
				fingerCut.startSize = 5f;
				fingerCut.endSize = 5f;
				fingerCut.c = RGBAColor.whiteRGBA;
				this.fingerCuts[ti].addObject(fingerCut);
				int num4 = 0;
				foreach (object obj2 in this.fingerCuts[ti])
				{
					GameScene.FingerCut item = (GameScene.FingerCut)obj2;
					num4 += this.cutWithRazorOrLine1Line2Immediate(null, item.start, item.end, false);
				}
				if (num4 > 0)
				{
					this.freezeCamera = false;
					if (this.ropesCutAtOnce > 0 && (double)this.ropeAtOnceTimer > 0.0)
					{
						this.ropesCutAtOnce += num4;
					}
					else
					{
						this.ropesCutAtOnce = num4;
					}
					this.ropeAtOnceTimer = 0.1f;
					int num5 = Preferences._getIntForKey("PREFS_ROPES_CUT") + 1;
					Preferences._setIntforKey(num5, "PREFS_ROPES_CUT", false);
					if (num5 == 100)
					{
						CTRRootController.postAchievementName("681461850", FrameworkTypes.ACHIEVEMENT_STRING("\"Rope Cutter\""));
					}
					if (this.ropesCutAtOnce >= 3 && this.ropesCutAtOnce < 5)
					{
						CTRRootController.postAchievementName("681464917", FrameworkTypes.ACHIEVEMENT_STRING("\"Quick Finger\""));
					}
					if (this.ropesCutAtOnce >= 5)
					{
						CTRRootController.postAchievementName("681508316", FrameworkTypes.ACHIEVEMENT_STRING("\"Master Finger\""));
					}
					if (num5 == 800)
					{
						CTRRootController.postAchievementName("681457931", FrameworkTypes.ACHIEVEMENT_STRING("\"Rope Cutter Maniac\""));
					}
					if (num5 == 2000)
					{
						CTRRootController.postAchievementName("1058248892", FrameworkTypes.ACHIEVEMENT_STRING("\"Ultimate Rope Cutter\""));
					}
				}
				this.prevStartPos[ti] = this.startPos[ti];
				this.startPos[ti] = vector;
			}
			return true;
		}

		// Token: 0x06000529 RID: 1321 RVA: 0x00026234 File Offset: 0x00024434
		public virtual bool touchDraggedXYIndex(float tx, float ty, int index)
		{
			if (index > 5)
			{
				return false;
			}
			this.slastTouch = CutTheRope.iframework.helpers.MathHelper.vect(tx, ty);
			return true;
		}

		// Token: 0x0600052A RID: 1322 RVA: 0x0002624C File Offset: 0x0002444C
		public virtual void onButtonPressed(int n)
		{
			if ((double)MaterialPoint.globalGravity.y == 784.0)
			{
				MaterialPoint.globalGravity.y = -784f;
				this.gravityNormal = false;
				CTRSoundMgr._playSound(39);
			}
			else
			{
				MaterialPoint.globalGravity.y = 784f;
				this.gravityNormal = true;
				CTRSoundMgr._playSound(38);
			}
			if (this.earthAnims == null)
			{
				return;
			}
			foreach (object obj in this.earthAnims)
			{
				Image earthAnim = (Image)obj;
				if (this.gravityNormal)
				{
					earthAnim.playTimeline(0);
				}
				else
				{
					earthAnim.playTimeline(1);
				}
			}
		}

		// Token: 0x0600052B RID: 1323 RVA: 0x00026314 File Offset: 0x00024514
		public virtual void rotateAllSpikesWithID(int sid)
		{
			foreach (object obj in this.spikes)
			{
				Spikes spike = (Spikes)obj;
				if (spike.getToggled() == sid)
				{
					spike.rotateSpikes();
				}
			}
		}

		// Token: 0x0600052C RID: 1324 RVA: 0x00026378 File Offset: 0x00024578
		public override void dealloc()
		{
			for (int i = 0; i < 5; i++)
			{
				this.fingerCuts[i].release();
			}
			this.dd.release();
			this.camera.release();
			this.back.release();
			base.dealloc();
		}

		// Token: 0x0600052D RID: 1325 RVA: 0x000263C8 File Offset: 0x000245C8
		public virtual void fullscreenToggled(bool isFullscreen)
		{
			BaseElement childWithName = this.staticAniPool.getChildWithName("levelLabel");
			if (childWithName != null)
			{
				childWithName.x = 15f + (float)base.canvas.xOffsetScaled;
			}
			for (int i = 0; i < 3; i++)
			{
				this.hudStar[i].x = (float)(this.hudStar[i].width * i + base.canvas.xOffsetScaled);
			}
			if (isFullscreen)
			{
				float num = (float)Global.ScreenSizeManager.ScreenWidth;
				this.back.scaleX = num / (float)base.canvas.backingWidth * 1.25f;
				return;
			}
			this.back.scaleX = 1.25f;
		}

		// Token: 0x0600052E RID: 1326 RVA: 0x00026475 File Offset: 0x00024675
		private void selector_gameLost(NSObject param)
		{
			this.gameLost();
		}

		// Token: 0x0600052F RID: 1327 RVA: 0x0002647D File Offset: 0x0002467D
		private void selector_gameWon(NSObject param)
		{
			CTRSoundMgr.EnableLoopedSounds(false);
			if (this.gameSceneDelegate != null)
			{
				this.gameSceneDelegate.gameWon();
			}
		}

		// Token: 0x06000530 RID: 1328 RVA: 0x00026498 File Offset: 0x00024698
		private void selector_animateLevelRestart(NSObject param)
		{
			this.animateLevelRestart();
		}

		// Token: 0x06000531 RID: 1329 RVA: 0x000264A0 File Offset: 0x000246A0
		private void selector_showGreeting(NSObject param)
		{
			this.showGreeting();
		}

		// Token: 0x06000532 RID: 1330 RVA: 0x000264A8 File Offset: 0x000246A8
		private void selector_doCandyBlink(NSObject param)
		{
			this.doCandyBlink();
		}

		// Token: 0x06000533 RID: 1331 RVA: 0x000264B0 File Offset: 0x000246B0
		private void selector_teleport(NSObject param)
		{
			this.teleport();
		}

		// Token: 0x06000534 RID: 1332 RVA: 0x000264B8 File Offset: 0x000246B8
		public static float FBOUND_PI(float a)
		{
			return (float)(((double)a > 3.141592653589793) ? ((double)a - 6.283185307179586) : (((double)a < -3.141592653589793) ? ((double)a + 6.283185307179586) : ((double)a)));
		}

		// Token: 0x04000399 RID: 921
		public const int MAX_TOUCHES = 5;

		// Token: 0x0400039A RID: 922
		public const float DIM_TIMEOUT = 0.15f;

		// Token: 0x0400039B RID: 923
		public const int RESTART_STATE_FADE_IN = 0;

		// Token: 0x0400039C RID: 924
		public const int RESTART_STATE_FADE_OUT = 1;

		// Token: 0x0400039D RID: 925
		public const int S_MOVE_DOWN = 0;

		// Token: 0x0400039E RID: 926
		public const int S_WAIT = 1;

		// Token: 0x0400039F RID: 927
		public const int S_MOVE_UP = 2;

		// Token: 0x040003A0 RID: 928
		public const int CAMERA_MOVE_TO_CANDY_PART = 0;

		// Token: 0x040003A1 RID: 929
		public const int CAMERA_MOVE_TO_CANDY = 1;

		// Token: 0x040003A2 RID: 930
		public const int BUTTON_GRAVITY = 0;

		// Token: 0x040003A3 RID: 931
		public const int PARTS_SEPARATE = 0;

		// Token: 0x040003A4 RID: 932
		public const int PARTS_DIST = 1;

		// Token: 0x040003A5 RID: 933
		public const int PARTS_NONE = 2;

		// Token: 0x040003A6 RID: 934
		public const float SCOMBO_TIMEOUT = 0.2f;

		// Token: 0x040003A7 RID: 935
		public const int SCUT_SCORE = 10;

		// Token: 0x040003A8 RID: 936
		public const int MAX_LOST_CANDIES = 3;

		// Token: 0x040003A9 RID: 937
		public const float ROPE_CUT_AT_ONCE_TIMEOUT = 0.1f;

		// Token: 0x040003AA RID: 938
		public const int STAR_RADIUS = 42;

		// Token: 0x040003AB RID: 939
		public const float MOUTH_OPEN_RADIUS = 200f;

		// Token: 0x040003AC RID: 940
		public const int BLINK_SKIP = 3;

		// Token: 0x040003AD RID: 941
		public const float MOUTH_OPEN_TIME = 1f;

		// Token: 0x040003AE RID: 942
		public const float PUMP_TIMEOUT = 0.05f;

		// Token: 0x040003AF RID: 943
		public const int CAMERA_SPEED = 14;

		// Token: 0x040003B0 RID: 944
		public const float SOCK_SPEED_K = 0.9f;

		// Token: 0x040003B1 RID: 945
		public const int SOCK_COLLISION_Y_OFFSET = 85;

		// Token: 0x040003B2 RID: 946
		public const int BUBBLE_RADIUS = 60;

		// Token: 0x040003B3 RID: 947
		public const int WHEEL_RADIUS = 110;

		// Token: 0x040003B4 RID: 948
		public const int GRAB_MOVE_RADIUS = 65;

		// Token: 0x040003B5 RID: 949
		public const int RC_CONTROLLER_RADIUS = 90;

		// Token: 0x040003B6 RID: 950
		public const int CANDY_BLINK_INITIAL = 0;

		// Token: 0x040003B7 RID: 951
		public const int CANDY_BLINK_STAR = 1;

		// Token: 0x040003B8 RID: 952
		public const int TUTORIAL_SHOW_ANIM = 0;

		// Token: 0x040003B9 RID: 953
		public const int TUTORIAL_HIDE_ANIM = 1;

		// Token: 0x040003BA RID: 954
		public const int EARTH_NORMAL_ANIM = 0;

		// Token: 0x040003BB RID: 955
		public const int EARTH_UPSIDEDOWN_ANIM = 1;

		// Token: 0x040003BC RID: 956
		private const int CHAR_ANIMATION_IDLE = 0;

		// Token: 0x040003BD RID: 957
		private const int CHAR_ANIMATION_IDLE2 = 1;

		// Token: 0x040003BE RID: 958
		private const int CHAR_ANIMATION_IDLE3 = 2;

		// Token: 0x040003BF RID: 959
		private const int CHAR_ANIMATION_EXCITED = 3;

		// Token: 0x040003C0 RID: 960
		private const int CHAR_ANIMATION_PUZZLED = 4;

		// Token: 0x040003C1 RID: 961
		private const int CHAR_ANIMATION_FAIL = 5;

		// Token: 0x040003C2 RID: 962
		private const int CHAR_ANIMATION_WIN = 6;

		// Token: 0x040003C3 RID: 963
		private const int CHAR_ANIMATION_MOUTH_OPEN = 7;

		// Token: 0x040003C4 RID: 964
		private const int CHAR_ANIMATION_MOUTH_CLOSE = 8;

		// Token: 0x040003C5 RID: 965
		private const int CHAR_ANIMATION_CHEW = 9;

		// Token: 0x040003C6 RID: 966
		private const int CHAR_ANIMATION_GREETING = 10;

		// Token: 0x040003C7 RID: 967
		private DelayedDispatcher dd;

		// Token: 0x040003C8 RID: 968
		public GameSceneDelegate gameSceneDelegate;

		// Token: 0x040003C9 RID: 969
		private AnimationsPool aniPool;

		// Token: 0x040003CA RID: 970
		private AnimationsPool staticAniPool;

		// Token: 0x040003CB RID: 971
		private PollenDrawer pollenDrawer;

		// Token: 0x040003CC RID: 972
		private TileMap back;

		// Token: 0x040003CD RID: 973
		private CharAnimations target;

		// Token: 0x040003CE RID: 974
		private Image support;

		// Token: 0x040003CF RID: 975
		private GameObject candy;

		// Token: 0x040003D0 RID: 976
		private Image candyMain;

		// Token: 0x040003D1 RID: 977
		private Image candyTop;

		// Token: 0x040003D2 RID: 978
		private Animation candyBlink;

		// Token: 0x040003D3 RID: 979
		private Animation candyBubbleAnimation;

		// Token: 0x040003D4 RID: 980
		private Animation candyBubbleAnimationL;

		// Token: 0x040003D5 RID: 981
		private Animation candyBubbleAnimationR;

		// Token: 0x040003D6 RID: 982
		private ConstraintedPoint star;

		// Token: 0x040003D7 RID: 983
		private DynamicArray bungees;

		// Token: 0x040003D8 RID: 984
		private DynamicArray razors;

		// Token: 0x040003D9 RID: 985
		private DynamicArray spikes;

		// Token: 0x040003DA RID: 986
		private DynamicArray stars;

		// Token: 0x040003DB RID: 987
		private DynamicArray bubbles;

		// Token: 0x040003DC RID: 988
		private DynamicArray pumps;

		// Token: 0x040003DD RID: 989
		private DynamicArray socks;

		// Token: 0x040003DE RID: 990
		private DynamicArray bouncers;

		// Token: 0x040003DF RID: 991
		private DynamicArray rotatedCircles;

		// Token: 0x040003E0 RID: 992
		private DynamicArray tutorialImages;

		// Token: 0x040003E1 RID: 993
		private DynamicArray tutorials;

		// Token: 0x040003E2 RID: 994
		private GameObject candyL;

		// Token: 0x040003E3 RID: 995
		private GameObject candyR;

		// Token: 0x040003E4 RID: 996
		private ConstraintedPoint starL;

		// Token: 0x040003E5 RID: 997
		private ConstraintedPoint starR;

		// Token: 0x040003E6 RID: 998
		private Animation blink;

		// Token: 0x040003E7 RID: 999
		private bool[] dragging = new bool[5];

		// Token: 0x040003E8 RID: 1000
		private Vector[] startPos = new Vector[5];

		// Token: 0x040003E9 RID: 1001
		private Vector[] prevStartPos = new Vector[5];

		// Token: 0x040003EA RID: 1002
		private float ropePhysicsSpeed;

		// Token: 0x040003EB RID: 1003
		private GameObject candyBubble;

		// Token: 0x040003EC RID: 1004
		private GameObject candyBubbleL;

		// Token: 0x040003ED RID: 1005
		private GameObject candyBubbleR;

		// Token: 0x040003EE RID: 1006
		private Animation[] hudStar = new Animation[3];

		// Token: 0x040003EF RID: 1007
		private Camera2D camera;

		// Token: 0x040003F0 RID: 1008
		private float mapWidth;

		// Token: 0x040003F1 RID: 1009
		private float mapHeight;

		// Token: 0x040003F2 RID: 1010
		private bool mouthOpen;

		// Token: 0x040003F3 RID: 1011
		private bool noCandy;

		// Token: 0x040003F4 RID: 1012
		private int blinkTimer;

		// Token: 0x040003F5 RID: 1013
		private int idlesTimer;

		// Token: 0x040003F6 RID: 1014
		private float mouthCloseTimer;

		// Token: 0x040003F7 RID: 1015
		private float lastCandyRotateDelta;

		// Token: 0x040003F8 RID: 1016
		private float lastCandyRotateDeltaL;

		// Token: 0x040003F9 RID: 1017
		private float lastCandyRotateDeltaR;

		// Token: 0x040003FA RID: 1018
		private bool spiderTookCandy;

		// Token: 0x040003FB RID: 1019
		private int special;

		// Token: 0x040003FC RID: 1020
		private bool fastenCamera;

		// Token: 0x040003FD RID: 1021
		private float savedSockSpeed;

		// Token: 0x040003FE RID: 1022
		private Sock targetSock;

		// Token: 0x040003FF RID: 1023
		private int ropesCutAtOnce;

		// Token: 0x04000400 RID: 1024
		private float ropeAtOnceTimer;

		// Token: 0x04000401 RID: 1025
		private bool clickToCut;

		// Token: 0x04000402 RID: 1026
		public int starsCollected;

		// Token: 0x04000403 RID: 1027
		public int starBonus;

		// Token: 0x04000404 RID: 1028
		public int timeBonus;

		// Token: 0x04000405 RID: 1029
		public int score;

		// Token: 0x04000406 RID: 1030
		public float time;

		// Token: 0x04000407 RID: 1031
		public float initialCameraToStarDistance;

		// Token: 0x04000408 RID: 1032
		public float dimTime;

		// Token: 0x04000409 RID: 1033
		public int restartState;

		// Token: 0x0400040A RID: 1034
		public bool animateRestartDim;

		// Token: 0x0400040B RID: 1035
		public bool freezeCamera;

		// Token: 0x0400040C RID: 1036
		public int cameraMoveMode;

		// Token: 0x0400040D RID: 1037
		public bool ignoreTouches;

		// Token: 0x0400040E RID: 1038
		public bool nightLevel;

		// Token: 0x0400040F RID: 1039
		public bool gravityNormal;

		// Token: 0x04000410 RID: 1040
		public ToggleButton gravityButton;

		// Token: 0x04000411 RID: 1041
		public int gravityTouchDown;

		// Token: 0x04000412 RID: 1042
		public int twoParts;

		// Token: 0x04000413 RID: 1043
		public bool noCandyL;

		// Token: 0x04000414 RID: 1044
		public bool noCandyR;

		// Token: 0x04000415 RID: 1045
		public float partsDist;

		// Token: 0x04000416 RID: 1046
		public DynamicArray earthAnims;

		// Token: 0x04000417 RID: 1047
		public int tummyTeasers;

		// Token: 0x04000418 RID: 1048
		public Vector slastTouch;

		// Token: 0x04000419 RID: 1049
		public DynamicArray[] fingerCuts = new DynamicArray[5];

		// Token: 0x020000C4 RID: 196
		private class FingerCut : NSObject
		{
			// Token: 0x040008EC RID: 2284
			public Vector start;

			// Token: 0x040008ED RID: 2285
			public Vector end;

			// Token: 0x040008EE RID: 2286
			public float startSize;

			// Token: 0x040008EF RID: 2287
			public float endSize;

			// Token: 0x040008F0 RID: 2288
			public RGBAColor c;
		}

		// Token: 0x020000C5 RID: 197
		private class SCandy : ConstraintedPoint
		{
			// Token: 0x040008F1 RID: 2289
			public bool good;

			// Token: 0x040008F2 RID: 2290
			public float speed;

			// Token: 0x040008F3 RID: 2291
			public float angle;

			// Token: 0x040008F4 RID: 2292
			public float lastAngleChange;
		}

		// Token: 0x020000C6 RID: 198
		private class TutorialText : Text
		{
			// Token: 0x040008F5 RID: 2293
			public int special;
		}

		// Token: 0x020000C7 RID: 199
		private class GameObjectSpecial : CTRGameObject
		{
			// Token: 0x0600067E RID: 1662 RVA: 0x00033C5D File Offset: 0x00031E5D
			private static GameScene.GameObjectSpecial GameObjectSpecial_create(Texture2D t)
			{
				GameScene.GameObjectSpecial gameObjectSpecial = new GameScene.GameObjectSpecial();
				gameObjectSpecial.initWithTexture(t);
				return gameObjectSpecial;
			}

			// Token: 0x0600067F RID: 1663 RVA: 0x00033C6C File Offset: 0x00031E6C
			public static GameScene.GameObjectSpecial GameObjectSpecial_createWithResIDQuad(int r, int q)
			{
				GameScene.GameObjectSpecial gameObjectSpecial = GameScene.GameObjectSpecial.GameObjectSpecial_create(Application.getTexture(r));
				gameObjectSpecial.setDrawQuad(q);
				return gameObjectSpecial;
			}

			// Token: 0x040008F6 RID: 2294
			public int special;
		}
	}
}
