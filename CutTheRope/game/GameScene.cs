using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.sfe;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using CutTheRope.windows;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CutTheRope.game
{
    internal class GameScene : BaseElement, TimelineDelegate, ButtonDelegate
    {
        private static void drawCut(Vector fls, Vector frs, Vector start, Vector end, float startSize, float endSize, RGBAColor c, ref Vector le, ref Vector re)
        {
            Vector vector5 = vectNormalize(vectSub(end, start));
            Vector v3 = vectRperp(vector5);
            Vector v4 = vectPerp(vector5);
            Vector vector = vectEqual(frs, vectUndefined) ? vectAdd(start, vectMult(v3, startSize)) : frs;
            Vector vector2 = vectEqual(fls, vectUndefined) ? vectAdd(start, vectMult(v4, startSize)) : fls;
            Vector vector3 = vectAdd(end, vectMult(v3, endSize));
            Vector vector4 = vectAdd(end, vectMult(v4, endSize));
            GLDrawer.drawSolidPolygonWOBorder([vector2.x, vector2.y, vector.x, vector.y, vector3.x, vector3.y, vector4.x, vector4.y], 4, c);
            le = vector4;
            re = vector3;
        }

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

        public virtual bool pointOutOfScreen(ConstraintedPoint p)
        {
            return p.pos.y > mapHeight + 400f || p.pos.y < -400f;
        }

        public override NSObject init()
        {
            if (base.init() != null)
            {
                CTRRootController cTRRootController = (CTRRootController)Application.sharedRootController();
                dd = (DelayedDispatcher)new DelayedDispatcher().init();
                initialCameraToStarDistance = -1f;
                restartState = -1;
                aniPool = (AnimationsPool)new AnimationsPool().init();
                aniPool.visible = false;
                addChild(aniPool);
                staticAniPool = (AnimationsPool)new AnimationsPool().init();
                staticAniPool.visible = false;
                addChild(staticAniPool);
                camera = new Camera2D().initWithSpeedandType(14f, CAMERA_TYPE.CAMERA_SPEED_DELAY);
                int textureResID = 104 + cTRRootController.getPack() * 2;
                back = new TileMap().initWithRowsColumns(1, 1);
                back.setRepeatHorizontally(TileMap.Repeat.REPEAT_NONE);
                back.setRepeatVertically(TileMap.Repeat.REPEAT_ALL);
                back.addTileQuadwithID(Application.getTexture(textureResID), 0, 0);
                back.fillStartAtRowColumnRowsColumnswithTile(0, 0, 1, 1, 0);
                if (canvas.isFullscreen)
                {
                    back.scaleX = Global.ScreenSizeManager.ScreenWidth / (float)canvas.backingWidth;
                }
                back.scaleX *= 1.25f;
                back.scaleY *= 1.25f;
                for (int i = 0; i < 3; i++)
                {
                    hudStar[i] = Animation.Animation_createWithResID(79);
                    hudStar[i].doRestoreCutTransparency();
                    hudStar[i].addAnimationDelayLoopFirstLast(0.05, Timeline.LoopType.TIMELINE_NO_LOOP, 0, 10);
                    hudStar[i].setPauseAtIndexforAnimation(10, 0);
                    hudStar[i].x = hudStar[i].width * i + canvas.xOffsetScaled;
                    hudStar[i].y = 0f;
                    addChild(hudStar[i]);
                }
                for (int j = 0; j < 5; j++)
                {
                    fingerCuts[j] = (DynamicArray)new DynamicArray().init();
                    fingerCuts[j].retain();
                }
                clickToCut = Preferences._getBooleanForKey("PREFS_CLICK_TO_CUT");
            }
            return this;
        }

        public virtual void xmlLoaderFinishedWithfromwithSuccess(XMLNode rootNode, NSString url, bool success)
        {
            ((CTRRootController)Application.sharedRootController()).setMap(rootNode);
            if (animateRestartDim)
            {
                animateLevelRestart();
                return;
            }
            restart();
        }

        public virtual void reload()
        {
            dd.cancelAllDispatches();
            CTRRootController cTRRootController = (CTRRootController)Application.sharedRootController();
            if (cTRRootController.isPicker())
            {
                xmlLoaderFinishedWithfromwithSuccess(XMLNode.parseXML("mappicker://reload"), NSS("mappicker://reload"), true);
                return;
            }
            int pack = cTRRootController.getPack();
            int level = cTRRootController.getLevel();
            xmlLoaderFinishedWithfromwithSuccess(XMLNode.parseXML("maps/" + LevelsList.LEVEL_NAMES[pack, level].ToString()), NSS("maps/" + LevelsList.LEVEL_NAMES[pack, level].ToString()), true);
        }

        public virtual void loadNextMap()
        {
            dd.cancelAllDispatches();
            initialCameraToStarDistance = -1f;
            animateRestartDim = false;
            CTRRootController cTRRootController = (CTRRootController)Application.sharedRootController();
            if (cTRRootController.isPicker())
            {
                xmlLoaderFinishedWithfromwithSuccess(XMLNode.parseXML("mappicker://next"), NSS("mappicker://next"), true);
                return;
            }
            int pack = cTRRootController.getPack();
            int level = cTRRootController.getLevel();
            if (level < CTRPreferences.getLevelsInPackCount() - 1)
            {
                cTRRootController.setLevel(++level);
                cTRRootController.setMapName(LevelsList.LEVEL_NAMES[pack, level]);
                xmlLoaderFinishedWithfromwithSuccess(XMLNode.parseXML("maps/" + LevelsList.LEVEL_NAMES[pack, level].ToString()), NSS("maps/" + LevelsList.LEVEL_NAMES[pack, level].ToString()), true);
            }
        }

        public virtual void restart()
        {
            hide();
            show();
        }

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
            if (canvas.isFullscreen)
            {
                int screenWidth = Global.ScreenSizeManager.ScreenWidth;
            }
            image.scaleX = 0.8f;
            image.scaleY = 0.8f;
            image.x += xs;
            image.y += ys;
            earthAnims.addObject(image);
        }

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

        public virtual void showGreeting()
        {
            target.playAnimationtimeline(101, 10);
        }

        public override void show()
        {
            CTRSoundMgr.EnableLoopedSounds(true);
            aniPool.removeAllChilds();
            staticAniPool.removeAllChilds();
            gravityButton = null;
            gravityTouchDown = -1;
            twoParts = 2;
            partsDist = 0f;
            targetSock = null;
            CTRSoundMgr._stopLoopedSounds();
            CTRRootController cTRRootController = (CTRRootController)Application.sharedRootController();
            XMLNode map = cTRRootController.getMap();
            bungees = (DynamicArray)new DynamicArray().init();
            razors = (DynamicArray)new DynamicArray().init();
            spikes = (DynamicArray)new DynamicArray().init();
            stars = (DynamicArray)new DynamicArray().init();
            bubbles = (DynamicArray)new DynamicArray().init();
            pumps = (DynamicArray)new DynamicArray().init();
            socks = (DynamicArray)new DynamicArray().init();
            tutorialImages = (DynamicArray)new DynamicArray().init();
            tutorials = (DynamicArray)new DynamicArray().init();
            bouncers = (DynamicArray)new DynamicArray().init();
            rotatedCircles = (DynamicArray)new DynamicArray().init();
            pollenDrawer = (PollenDrawer)new PollenDrawer().init();
            star = (ConstraintedPoint)new ConstraintedPoint().init();
            star.setWeight(1f);
            starL = (ConstraintedPoint)new ConstraintedPoint().init();
            starL.setWeight(1f);
            starR = (ConstraintedPoint)new ConstraintedPoint().init();
            starR.setWeight(1f);
            candy = GameObject.GameObject_createWithResIDQuad(63, 0);
            candy.doRestoreCutTransparency();
            candy.retain();
            candy.anchor = 18;
            candy.bb = MakeRectangle(142f, 157f, 112f, 104f);
            candy.passTransformationsToChilds = false;
            candy.scaleX = candy.scaleY = 0.71f;
            candyMain = GameObject.GameObject_createWithResIDQuad(63, 1);
            candyMain.doRestoreCutTransparency();
            candyMain.anchor = candyMain.parentAnchor = 18;
            candy.addChild(candyMain);
            candyMain.scaleX = candyMain.scaleY = 0.71f;
            candyTop = GameObject.GameObject_createWithResIDQuad(63, 2);
            candyTop.doRestoreCutTransparency();
            candyTop.anchor = candyTop.parentAnchor = 18;
            candy.addChild(candyTop);
            candyTop.scaleX = candyTop.scaleY = 0.71f;
            candyBlink = Animation.Animation_createWithResID(63);
            candyBlink.addAnimationWithIDDelayLoopFirstLast(0, 0.07f, Timeline.LoopType.TIMELINE_NO_LOOP, 8, 17);
            candyBlink.addAnimationWithIDDelayLoopCountSequence(1, 0.3f, Timeline.LoopType.TIMELINE_NO_LOOP, 2, 18, new List<int> { 18 });
            Timeline timeline7 = candyBlink.getTimeline(1);
            timeline7.addKeyFrame(KeyFrame.makeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline7.addKeyFrame(KeyFrame.makeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.2));
            candyBlink.visible = false;
            candyBlink.anchor = candyBlink.parentAnchor = 18;
            candyBlink.scaleX = candyBlink.scaleY = 0.71f;
            candy.addChild(candyBlink);
            candyBubbleAnimation = Animation.Animation_createWithResID(72);
            candyBubbleAnimation.x = candy.x;
            candyBubbleAnimation.y = candy.y;
            candyBubbleAnimation.parentAnchor = candyBubbleAnimation.anchor = 18;
            candyBubbleAnimation.addAnimationDelayLoopFirstLast(0.05, Timeline.LoopType.TIMELINE_REPLAY, 0, 12);
            candyBubbleAnimation.playTimeline(0);
            candy.addChild(candyBubbleAnimation);
            candyBubbleAnimation.visible = false;
            float num = 3f;
            float num2 = 0f;
            float num3 = 0f;
            for (int i = 0; i < 3; i++)
            {
                Timeline timeline2 = hudStar[i].getCurrentTimeline();
                timeline2?.stopTimeline();
                hudStar[i].setDrawQuad(0);
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
                        mapWidth = item2["width"].floatValue();
                        mapHeight = item2["height"].floatValue();
                        num3 = (2560f - mapWidth * num) / 2f;
                        mapWidth *= num;
                        mapHeight *= num;
                        if (cTRRootController.getPack() == 7)
                        {
                            earthAnims = (DynamicArray)new DynamicArray().init();
                            if (mapWidth > SCREEN_WIDTH)
                            {
                                createEarthImageWithOffsetXY(back.width, 0f);
                            }
                            if (mapHeight > SCREEN_HEIGHT)
                            {
                                createEarthImageWithOffsetXY(0f, back.height);
                            }
                            createEarthImageWithOffsetXY(0f, 0f);
                        }
                    }
                    else if (item2.Name == "gameDesign")
                    {
                        num4 = item2["mapOffsetX"].intValue();
                        num5 = item2["mapOffsetY"].intValue();
                        special = item2["special"].intValue();
                        ropePhysicsSpeed = item2["ropePhysicsSpeed"].floatValue();
                        nightLevel = item2["nightLevel"].isEqualToString("true");
                        twoParts = (!item2["twoParts"].isEqualToString("true")) ? 2 : 0;
                        ropePhysicsSpeed *= 1.4f;
                    }
                    else if (item2.Name == "candyL")
                    {
                        starL.pos.x = item2["x"].intValue() * num + num3 + num4;
                        starL.pos.y = item2["y"].intValue() * num + num2 + num5;
                        candyL = GameObject.GameObject_createWithResIDQuad(63, 19);
                        candyL.scaleX = candyL.scaleY = 0.71f;
                        candyL.passTransformationsToChilds = false;
                        candyL.doRestoreCutTransparency();
                        candyL.retain();
                        candyL.anchor = 18;
                        candyL.x = starL.pos.x;
                        candyL.y = starL.pos.y;
                        candyL.bb = MakeRectangle(155.0, 176.0, 88.0, 76.0);
                    }
                    else if (item2.Name == "candyR")
                    {
                        starR.pos.x = item2["x"].intValue() * num + num3 + num4;
                        starR.pos.y = item2["y"].intValue() * num + num2 + num5;
                        candyR = GameObject.GameObject_createWithResIDQuad(63, 20);
                        candyR.scaleX = candyR.scaleY = 0.71f;
                        candyR.passTransformationsToChilds = false;
                        candyR.doRestoreCutTransparency();
                        candyR.retain();
                        candyR.anchor = 18;
                        candyR.x = starR.pos.x;
                        candyR.y = starR.pos.y;
                        candyR.bb = MakeRectangle(155.0, 176.0, 88.0, 76.0);
                    }
                    else if (item2.Name == "candy")
                    {
                        star.pos.x = item2["x"].intValue() * num + num3 + num4;
                        star.pos.y = item2["y"].intValue() * num + num2 + num5;
                    }
                }
            }
            foreach (XMLNode xmlnode2 in list)
            {
                foreach (XMLNode item3 in xmlnode2.childs())
                {
                    if (item3.Name == "gravitySwitch")
                    {
                        gravityButton = createGravityButtonWithDelegate(this);
                        gravityButton.visible = false;
                        gravityButton.touchable = false;
                        addChild(gravityButton);
                        gravityButton.x = item3["x"].intValue() * num + num3 + num4;
                        gravityButton.y = item3["y"].intValue() * num + num2 + num5;
                        gravityButton.anchor = 18;
                    }
                    else if (item3.Name == "star")
                    {
                        Star star = Star.Star_createWithResID(78);
                        star.x = item3["x"].intValue() * num + num3 + num4;
                        star.y = item3["y"].intValue() * num + num2 + num5;
                        star.timeout = item3["timeout"].floatValue();
                        star.createAnimations();
                        star.bb = MakeRectangle(70.0, 64.0, 82.0, 82.0);
                        star.parseMover(item3);
                        star.update(0f);
                        stars.addObject(star);
                    }
                    else if (item3.Name == "tutorialText")
                    {
                        if (!shouldSkipTutorialElement(item3))
                        {
                            TutorialText tutorialText = (TutorialText)new TutorialText().initWithFont(Application.getFont(4));
                            tutorialText.color = RGBAColor.MakeRGBA(1.0, 1.0, 1.0, 0.9);
                            tutorialText.x = item3["x"].intValue() * num + num3 + num4;
                            tutorialText.y = item3["y"].intValue() * num + num2 + num5;
                            tutorialText.special = item3["special"].intValue();
                            tutorialText.setAlignment(2);
                            NSString newString = item3["text"];
                            tutorialText.setStringandWidth(newString, item3["width"].intValue() * num);
                            tutorialText.color = RGBAColor.transparentRGBA;
                            float num6 = (tutorialText.special == 3) ? 12f : 0f;
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
                            tutorials.addObject(tutorialText);
                        }
                    }
                    else if (item3.Name == "tutorial01" || item3.Name == "tutorial02" || item3.Name == "tutorial03" || item3.Name == "tutorial04" || item3.Name == "tutorial05" || item3.Name == "tutorial06" || item3.Name == "tutorial07" || item3.Name == "tutorial08" || item3.Name == "tutorial09" || item3.Name == "tutorial10" || item3.Name == "tutorial11")
                    {
                        if (!shouldSkipTutorialElement(item3))
                        {
                            int q = new NSString(item3.Name.Substring(8)).intValue() - 1;
                            GameObjectSpecial gameObjectSpecial = GameObjectSpecial.GameObjectSpecial_createWithResIDQuad(84, q);
                            gameObjectSpecial.color = RGBAColor.transparentRGBA;
                            gameObjectSpecial.x = item3["x"].intValue() * num + num3 + num4;
                            gameObjectSpecial.y = item3["y"].intValue() * num + num2 + num5;
                            gameObjectSpecial.rotation = item3["angle"].intValue();
                            gameObjectSpecial.special = item3["special"].intValue();
                            gameObjectSpecial.parseMover(item3);
                            float num7 = (gameObjectSpecial.special == 3 || gameObjectSpecial.special == 4) ? 12f : 0f;
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
                                    timeline5.addKeyFrame(KeyFrame.makePos(gameObjectSpecial.x, gameObjectSpecial.y, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, (double)((j == 1) ? 0f : num7)));
                                    timeline5.addKeyFrame(KeyFrame.makePos(gameObjectSpecial.x, gameObjectSpecial.y, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                                    timeline5.addKeyFrame(KeyFrame.makePos(gameObjectSpecial.x, gameObjectSpecial.y, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1.0));
                                    timeline5.addKeyFrame(KeyFrame.makePos(gameObjectSpecial.x + 230.0, gameObjectSpecial.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.5));
                                    timeline5.addKeyFrame(KeyFrame.makePos(gameObjectSpecial.x + 440.0, gameObjectSpecial.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.5));
                                    timeline5.addKeyFrame(KeyFrame.makePos(gameObjectSpecial.x + 440.0, gameObjectSpecial.y, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.6));
                                }
                                timeline5.setTimelineLoopType(Timeline.LoopType.TIMELINE_NO_LOOP);
                                gameObjectSpecial.addTimelinewithID(timeline5, 1);
                                gameObjectSpecial.playTimeline(1);
                                gameObjectSpecial.rotation = 10f;
                            }
                            tutorialImages.addObject(gameObjectSpecial);
                        }
                    }
                    else if (item3.Name == "bubble")
                    {
                        int q2 = RND_RANGE(1, 3);
                        Bubble bubble = Bubble.Bubble_createWithResIDQuad(75, q2);
                        bubble.doRestoreCutTransparency();
                        bubble.bb = MakeRectangle(48.0, 48.0, 152.0, 152.0);
                        bubble.initial_x = bubble.x = item3["x"].intValue() * num + num3 + num4;
                        bubble.initial_y = bubble.y = item3["y"].intValue() * num + num2 + num5;
                        bubble.initial_rotation = 0f;
                        bubble.initial_rotatedCircle = null;
                        bubble.anchor = 18;
                        bubble.popped = false;
                        Image image = Image.Image_createWithResIDQuad(75, 0);
                        image.doRestoreCutTransparency();
                        image.parentAnchor = image.anchor = 18;
                        bubble.addChild(image);
                        bubbles.addObject(bubble);
                    }
                    else if (item3.Name == "pump")
                    {
                        Pump pump = Pump.Pump_createWithResID(83);
                        pump.doRestoreCutTransparency();
                        pump.addAnimationWithDelayLoopedCountSequence(0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 4, 1, new List<int> { 2, 3, 0 });
                        pump.bb = MakeRectangle(300f, 300f, 175f, 175f);
                        pump.initial_x = pump.x = item3["x"].intValue() * num + num3 + num4;
                        pump.initial_y = pump.y = item3["y"].intValue() * num + num2 + num5;
                        pump.initial_rotation = 0f;
                        pump.initial_rotatedCircle = null;
                        pump.rotation = item3["angle"].floatValue() + 90f;
                        pump.updateRotation();
                        pump.anchor = 18;
                        pumps.addObject(pump);
                    }
                    else if (item3.Name == "sock")
                    {
                        Sock sock = Sock.Sock_createWithResID(85);
                        sock.createAnimations();
                        sock.scaleX = sock.scaleY = 0.7f;
                        sock.doRestoreCutTransparency();
                        sock.x = item3["x"].intValue() * num + num3 + num4;
                        sock.y = item3["y"].intValue() * num + num2 + num5;
                        sock.group = item3["group"].intValue();
                        sock.anchor = 10;
                        sock.rotationCenterY -= sock.height / 2f - 85f;
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
                        socks.addObject(sock);
                    }
                    else if (item3.Name == "spike1" || item3.Name == "spike2" || item3.Name == "spike3" || item3.Name == "spike4" || item3.Name == "electro")
                    {
                        float px = item3["x"].intValue() * num + num3 + num4;
                        float py = item3["y"].intValue() * num + num2 + num5;
                        int w = item3["size"].intValue();
                        double an = item3["angle"].intValue();
                        NSString nSString2 = item3["toggled"];
                        int num8 = -1;
                        if (nSString2.length() > 0)
                        {
                            num8 = nSString2.isEqualToString("false") ? (-1) : nSString2.intValue();
                        }
                        Spikes spikes = (Spikes)new Spikes().initWithPosXYWidthAndAngleToggled(px, py, w, an, num8);
                        spikes.parseMover(item3);
                        if (num8 != 0)
                        {
                            spikes.delegateRotateAllSpikesWithID = new Spikes.rotateAllSpikesWithID(rotateAllSpikesWithID);
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
                        float num9 = item3["x"].intValue() * num + num3 + num4;
                        float num10 = item3["y"].intValue() * num + num2 + num5;
                        float num11 = item3["size"].intValue();
                        float d = item3["handleAngle"].intValue();
                        bool hasOneHandle = item3["oneHandle"].boolValue();
                        RotatedCircle rotatedCircle = (RotatedCircle)new RotatedCircle().init();
                        rotatedCircle.anchor = 18;
                        rotatedCircle.x = num9;
                        rotatedCircle.y = num10;
                        rotatedCircle.rotation = d;
                        rotatedCircle.inithanlde1 = rotatedCircle.handle1 = vect(rotatedCircle.x - num11 * num, rotatedCircle.y);
                        rotatedCircle.inithanlde2 = rotatedCircle.handle2 = vect(rotatedCircle.x + num11 * num, rotatedCircle.y);
                        rotatedCircle.handle1 = vectRotateAround(rotatedCircle.handle1, (double)DEGREES_TO_RADIANS(d), rotatedCircle.x, rotatedCircle.y);
                        rotatedCircle.handle2 = vectRotateAround(rotatedCircle.handle2, (double)DEGREES_TO_RADIANS(d), rotatedCircle.x, rotatedCircle.y);
                        rotatedCircle.setSize(num11);
                        rotatedCircle.setHasOneHandle(hasOneHandle);
                        rotatedCircles.addObject(rotatedCircle);
                    }
                    else if (item3.Name == "bouncer1" || item3.Name == "bouncer2")
                    {
                        float px2 = item3["x"].intValue() * num + num3 + num4;
                        float py2 = item3["y"].intValue() * num + num2 + num5;
                        int w2 = item3["size"].intValue();
                        double an2 = item3["angle"].intValue();
                        Bouncer bouncer = (Bouncer)new Bouncer().initWithPosXYWidthAndAngle(px2, py2, w2, an2);
                        bouncer.parseMover(item3);
                        bouncers.addObject(bouncer);
                    }
                    else if (item3.Name == "grab")
                    {
                        float hx = item3["x"].intValue() * num + num3 + num4;
                        float hy = item3["y"].intValue() * num + num2 + num5;
                        float len = item3["length"].intValue() * num;
                        float num12 = item3["radius"].floatValue();
                        bool wheel = item3["wheel"].isEqualToString("true");
                        float k = item3["moveLength"].floatValue() * num;
                        bool v = item3["moveVertical"].isEqualToString("true");
                        float o = item3["moveOffset"].floatValue() * num;
                        bool spider = item3["spider"].isEqualToString("true");
                        bool flag = item3["part"].isEqualToString("L");
                        bool flag2 = item3["hidePath"].isEqualToString("true");
                        Grab grab = (Grab)new Grab().init();
                        grab.initial_x = grab.x = hx;
                        grab.initial_y = grab.y = hy;
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
                                bool flag3 = item3["path"].hasPrefix(NSS("R"));
                                for (int l = 0; l < grab.mover.pathLen - 1; l++)
                                {
                                    if (!flag3 || l % num13 == 0)
                                    {
                                        pollenDrawer.fillWithPolenFromPathIndexToPathIndexGrab(l, l + 1, grab);
                                    }
                                }
                                if (grab.mover.pathLen > 2)
                                {
                                    pollenDrawer.fillWithPolenFromPathIndexToPathIndexGrab(0, grab.mover.pathLen - 1, grab);
                                }
                            }
                        }
                        if (num12 != -1f)
                        {
                            num12 *= num;
                        }
                        if (num12 == -1f)
                        {
                            ConstraintedPoint constraintedPoint = star;
                            if (twoParts != 2)
                            {
                                constraintedPoint = flag ? starL : starR;
                            }
                            Bungee bungee = (Bungee)new Bungee().initWithHeadAtXYTailAtTXTYandLength(null, hx, hy, constraintedPoint, constraintedPoint.pos.x, constraintedPoint.pos.y, len);
                            bungee.bungeeAnchor.pin = bungee.bungeeAnchor.pos;
                            grab.setRope(bungee);
                        }
                        grab.setRadius(num12);
                        grab.setMoveLengthVerticalOffset(k, v, o);
                        bungees.addObject(grab);
                    }
                    else if (item3.Name == "target")
                    {
                        int pack = ((CTRRootController)Application.sharedRootController()).getPack();
                        support = Image.Image_createWithResIDQuad(100, pack);
                        support.retain();
                        support.doRestoreCutTransparency();
                        support.anchor = 18;
                        target = CharAnimations.CharAnimations_createWithResID(80);
                        target.doRestoreCutTransparency();
                        target.passColorToChilds = false;
                        NSString nSString3 = item3["x"];
                        target.x = support.x = nSString3.intValue() * num + num3 + num4;
                        NSString nSString4 = item3["y"];
                        target.y = support.y = nSString4.intValue() * num + num2 + num5;
                        target.addImage(101);
                        target.addImage(102);
                        target.bb = MakeRectangle(264.0, 350.0, 108.0, 2.0);
                        target.addAnimationWithIDDelayLoopFirstLast(0, 0.05f, Timeline.LoopType.TIMELINE_REPLAY, 0, 18);
                        target.addAnimationWithIDDelayLoopFirstLast(1, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 43, 67);
                        int num14 = 68;
                        target.addAnimationWithIDDelayLoopCountSequence(2, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 32, num14, new List<int>
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
                        target.addAnimationWithIDDelayLoopFirstLast(7, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 19, 27);
                        target.addAnimationWithIDDelayLoopFirstLast(8, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 28, 31);
                        target.addAnimationWithIDDelayLoopFirstLast(9, 0.05f, Timeline.LoopType.TIMELINE_REPLAY, 32, 40);
                        target.addAnimationWithIDDelayLoopFirstLast(6, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 28, 31);
                        target.addAnimationWithIDDelayLoopFirstLast(101, 10, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 47, 76);
                        target.addAnimationWithIDDelayLoopFirstLast(101, 3, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 0, 19);
                        target.addAnimationWithIDDelayLoopFirstLast(101, 4, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 20, 46);
                        target.addAnimationWithIDDelayLoopFirstLast(102, 5, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 0, 12);
                        target.switchToAnimationatEndOfAnimationDelay(9, 6, 0.05f);
                        target.switchToAnimationatEndOfAnimationDelay(101, 4, 80, 8, 0.05f);
                        target.switchToAnimationatEndOfAnimationDelay(80, 0, 101, 10, 0.05f);
                        target.switchToAnimationatEndOfAnimationDelay(80, 0, 80, 1, 0.05f);
                        target.switchToAnimationatEndOfAnimationDelay(80, 0, 80, 2, 0.05f);
                        target.switchToAnimationatEndOfAnimationDelay(80, 0, 101, 3, 0.05f);
                        target.switchToAnimationatEndOfAnimationDelay(80, 0, 101, 4, 0.05f);
                        target.retain();
                        if (CTRRootController.isShowGreeting())
                        {
                            dd.callObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(selector_showGreeting), null, 1.3f);
                            CTRRootController.setShowGreeting(false);
                        }
                        target.playTimeline(0);
                        target.getTimeline(0).delegateTimelineDelegate = this;
                        target.setPauseAtIndexforAnimation(8, 7);
                        blink = Animation.Animation_createWithResID(80);
                        blink.parentAnchor = 9;
                        blink.visible = false;
                        blink.addAnimationWithIDDelayLoopCountSequence(0, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 4, 41, new List<int> { 41, 42, 42, 42 });
                        blink.setActionTargetParamSubParamAtIndexforAnimation("ACTION_SET_VISIBLE", blink, 0, 0, 2, 0);
                        blinkTimer = 3;
                        blink.doRestoreCutTransparency();
                        target.addChild(blink);
                        idlesTimer = RND_RANGE(5, 20);
                    }
                }
            }
            if (twoParts != 2)
            {
                candyBubbleAnimationL = Animation.Animation_createWithResID(72);
                candyBubbleAnimationL.parentAnchor = candyBubbleAnimationL.anchor = 18;
                candyBubbleAnimationL.addAnimationDelayLoopFirstLast(0.05, Timeline.LoopType.TIMELINE_REPLAY, 0, 12);
                candyBubbleAnimationL.playTimeline(0);
                candyL.addChild(candyBubbleAnimationL);
                candyBubbleAnimationL.visible = false;
                candyBubbleAnimationR = Animation.Animation_createWithResID(72);
                candyBubbleAnimationR.parentAnchor = candyBubbleAnimationR.anchor = 18;
                candyBubbleAnimationR.addAnimationDelayLoopFirstLast(0.05, Timeline.LoopType.TIMELINE_REPLAY, 0, 12);
                candyBubbleAnimationR.playTimeline(0);
                candyR.addChild(candyBubbleAnimationR);
                candyBubbleAnimationR.visible = false;
            }
            foreach (object obj in rotatedCircles)
            {
                RotatedCircle rotatedCircle2 = (RotatedCircle)obj;
                rotatedCircle2.operating = -1;
                rotatedCircle2.circlesArray = rotatedCircles;
            }
            startCamera();
            tummyTeasers = 0;
            starsCollected = 0;
            candyBubble = null;
            candyBubbleL = null;
            candyBubbleR = null;
            mouthOpen = false;
            noCandy = twoParts != 2;
            noCandyL = false;
            noCandyR = false;
            blink.playTimeline(0);
            spiderTookCandy = false;
            time = 0f;
            score = 0;
            gravityNormal = true;
            MaterialPoint.globalGravity = vect(0f, 784f);
            dimTime = 0f;
            ropesCutAtOnce = 0;
            ropeAtOnceTimer = 0f;
            dd.callObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(selector_doCandyBlink), null, 1.0);
            Text text = Text.createWithFontandString(3, (cTRRootController.getPack() + 1).ToString() + " - " + (cTRRootController.getLevel() + 1).ToString());
            text.anchor = 33;
            Text text2 = Text.createWithFontandString(3, Application.getString(655376));
            text2.anchor = 33;
            text2.parentAnchor = 9;
            text.setName("levelLabel");
            text.x = 15f + canvas.xOffsetScaled;
            text.y = SCREEN_HEIGHT + 15f;
            text2.y = 60f;
            text2.rotationCenterX -= text2.width / 2f;
            text2.scaleX = text2.scaleY = 0.7f;
            text.addChild(text2);
            Timeline timeline6 = new Timeline().initWithMaxKeyFramesOnTrack(5);
            timeline6.addKeyFrame(KeyFrame.makeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline6.addKeyFrame(KeyFrame.makeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
            timeline6.addKeyFrame(KeyFrame.makeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
            timeline6.addKeyFrame(KeyFrame.makeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1.0));
            timeline6.addKeyFrame(KeyFrame.makeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
            text.addTimelinewithID(timeline6, 0);
            text.playTimeline(0);
            timeline6.delegateTimelineDelegate = staticAniPool;
            staticAniPool.addChild(text);
            for (int m = 0; m < 5; m++)
            {
                dragging[m] = false;
                startPos[m] = prevStartPos[m] = vectZero;
            }
            if (clickToCut)
            {
                resetBungeeHighlight();
            }
            Global.MouseCursor.ReleaseButtons();
            CTRRootController.logEvent("IG_SHOWN");
        }

        public virtual void startCamera()
        {
            if (mapWidth > SCREEN_WIDTH || mapHeight > SCREEN_HEIGHT)
            {
                ignoreTouches = true;
                fastenCamera = false;
                camera.type = CAMERA_TYPE.CAMERA_SPEED_PIXELS;
                camera.speed = 20f;
                cameraMoveMode = 0;
                ConstraintedPoint constraintedPoint = (twoParts != 2) ? starL : star;
                float num;
                float num2;
                if (mapWidth > SCREEN_WIDTH)
                {
                    if (constraintedPoint.pos.x > mapWidth / 2.0)
                    {
                        num = 0f;
                        num2 = 0f;
                    }
                    else
                    {
                        num = mapWidth - SCREEN_WIDTH;
                        num2 = 0f;
                    }
                }
                else if (constraintedPoint.pos.y > mapHeight / 2.0)
                {
                    num = 0f;
                    num2 = 0f;
                }
                else
                {
                    num = 0f;
                    num2 = mapHeight - SCREEN_HEIGHT;
                }
                double num6 = (double)(constraintedPoint.pos.x - SCREEN_WIDTH / 2f);
                float num3 = constraintedPoint.pos.y - SCREEN_HEIGHT / 2f;
                float num4 = FIT_TO_BOUNDARIES(num6, 0.0, (double)(mapWidth - SCREEN_WIDTH));
                float num5 = FIT_TO_BOUNDARIES((double)num3, 0.0, (double)(mapHeight - SCREEN_HEIGHT));
                camera.moveToXYImmediate(num, num2, true);
                initialCameraToStarDistance = vectDistance(camera.pos, vect(num4, num5));
                return;
            }
            ignoreTouches = false;
            camera.moveToXYImmediate(0f, 0f, true);
        }

        public virtual void doCandyBlink()
        {
            candyBlink.playTimeline(0);
        }

        public virtual void timelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
        {
            if (rotatedCircles.getObjectIndex(t.element) != -1 || i != 1)
            {
                return;
            }
            blinkTimer--;
            if (blinkTimer == 0)
            {
                blink.visible = true;
                blink.playTimeline(0);
                blinkTimer = 3;
            }
            idlesTimer--;
            if (idlesTimer == 0)
            {
                if (RND_RANGE(0, 1) == 1)
                {
                    target.playTimeline(1);
                }
                else
                {
                    target.playTimeline(2);
                }
                idlesTimer = RND_RANGE(5, 20);
            }
        }

        public virtual void timelineFinished(Timeline t)
        {
            if (rotatedCircles.getObjectIndex(t.element) != -1)
            {
                ((RotatedCircle)t.element).removeOnNextUpdate = true;
            }
            foreach (object obj in tutorials)
            {
                BaseElement baseElement = (BaseElement)obj;
            }
        }

        public override void hide()
        {
            if (gravityButton != null)
            {
                removeChild(gravityButton);
            }
            pollenDrawer.release();
            earthAnims?.release();
            candy.release();
            star.release();
            candyL?.release();
            candyR?.release();
            starL.release();
            starR.release();
            razors.release();
            spikes.release();
            bungees.release();
            stars.release();
            bubbles.release();
            pumps.release();
            socks.release();
            bouncers.release();
            rotatedCircles.release();
            target.release();
            support.release();
            tutorialImages.release();
            tutorials.release();
            candyL = null;
            candyR = null;
            starL = null;
            starR = null;
        }

        public override void update(float delta)
        {
            delta = 0.016f;
            base.update(delta);
            dd.update(delta);
            pollenDrawer.update(delta);
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < fingerCuts[i].count(); j++)
                {
                    FingerCut fingerCut = (FingerCut)fingerCuts[i].objectAtIndex(j);
                    if (Mover.moveVariableToTarget(ref fingerCut.c.a, 0.0, 10.0, (double)delta))
                    {
                        fingerCuts[i].removeObject(fingerCut);
                        j--;
                    }
                }
            }
            if (earthAnims != null)
            {
                foreach (object obj in earthAnims)
                {
                    ((Image)obj).update(delta);
                }
            }
            Mover.moveVariableToTarget(ref ropeAtOnceTimer, 0.0, 1.0, (double)delta);
            ConstraintedPoint constraintedPoint4 = (twoParts != 2) ? starL : star;
            float num = constraintedPoint4.pos.x - SCREEN_WIDTH / 2f;
            double num19 = (double)(constraintedPoint4.pos.y - SCREEN_HEIGHT / 2f);
            float num2 = FIT_TO_BOUNDARIES((double)num, 0.0, (double)(mapWidth - SCREEN_WIDTH));
            float num3 = FIT_TO_BOUNDARIES(num19, 0.0, (double)(mapHeight - SCREEN_HEIGHT));
            camera.moveToXYImmediate(num2, num3, false);
            if (!freezeCamera || camera.type != CAMERA_TYPE.CAMERA_SPEED_DELAY)
            {
                camera.update(delta);
            }
            if (camera.type == CAMERA_TYPE.CAMERA_SPEED_PIXELS)
            {
                float num4 = 100f;
                float num5 = 800f;
                float num6 = 400f;
                float a = 1000f;
                float a2 = 300f;
                float num7 = vectDistance(camera.pos, vect(num2, num3));
                if (num7 < num4)
                {
                    ignoreTouches = false;
                }
                if (fastenCamera)
                {
                    if (camera.speed < 5500f)
                    {
                        camera.speed *= 1.5f;
                    }
                }
                else if ((double)num7 > initialCameraToStarDistance / 2.0)
                {
                    camera.speed += delta * num5;
                    camera.speed = MIN(a, camera.speed);
                }
                else
                {
                    camera.speed -= delta * num6;
                    camera.speed = MAX(a2, camera.speed);
                }
                if ((double)Math.Abs(camera.pos.x - num2) < 1.0 && (double)Math.Abs(camera.pos.y - num3) < 1.0)
                {
                    camera.type = CAMERA_TYPE.CAMERA_SPEED_DELAY;
                    camera.speed = 14f;
                }
            }
            else
            {
                time += delta;
            }
            if (bungees.count() > 0)
            {
                bool flag = false;
                bool flag2 = false;
                bool flag3 = false;
                int num8 = bungees.count();
                int k = 0;
                while (k < num8)
                {
                    Grab grab = (Grab)bungees.objectAtIndex(k);
                    grab.update(delta);
                    Bungee rope = grab.rope;
                    if (grab.mover != null)
                    {
                        if (grab.rope != null)
                        {
                            grab.rope.bungeeAnchor.pos = vect(grab.x, grab.y);
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
                    if (rope.cut == -1 || rope.cutTime != 0.0)
                    {
                        rope?.update(delta * ropePhysicsSpeed);
                        if (!grab.hasSpider)
                        {
                            goto IL_0478;
                        }
                        if (camera.type != CAMERA_TYPE.CAMERA_SPEED_PIXELS || !ignoreTouches)
                        {
                            grab.updateSpider(delta);
                        }
                        if (grab.spiderPos == -1f)
                        {
                            spiderWon(grab);
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
                        if (twoParts != 2)
                        {
                            if (!noCandyL && vectDistance(vect(grab.x, grab.y), starL.pos) <= grab.radius + 42f)
                            {
                                Bungee bungee = (Bungee)new Bungee().initWithHeadAtXYTailAtTXTYandLength(null, grab.x, grab.y, starL, starL.pos.x, starL.pos.y, grab.radius + 42f);
                                bungee.bungeeAnchor.pin = bungee.bungeeAnchor.pos;
                                grab.hideRadius = true;
                                grab.setRope(bungee);
                                CTRSoundMgr._playSound(24);
                                if (grab.mover != null)
                                {
                                    CTRSoundMgr._playSound(44);
                                }
                            }
                            if (!noCandyR && grab.rope == null && vectDistance(vect(grab.x, grab.y), starR.pos) <= grab.radius + 42f)
                            {
                                Bungee bungee2 = (Bungee)new Bungee().initWithHeadAtXYTailAtTXTYandLength(null, grab.x, grab.y, starR, starR.pos.x, starR.pos.y, grab.radius + 42f);
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
                        else if (vectDistance(vect(grab.x, grab.y), star.pos) <= grab.radius + 42f)
                        {
                            Bungee bungee3 = (Bungee)new Bungee().initWithHeadAtXYTailAtTXTYandLength(null, grab.x, grab.y, star, star.pos.x, star.pos.y, grab.radius + 42f);
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
                    Vector v = vectSub(bungeeAnchor.pos, constraintedPoint2.pos);
                    bool flag4 = false;
                    if (twoParts != 2)
                    {
                        if (constraintedPoint2 == starL && !noCandyL && !flag2)
                        {
                            flag4 = true;
                        }
                        if (constraintedPoint2 == starR && !noCandyR && !flag3)
                        {
                            flag4 = true;
                        }
                    }
                    else if (!noCandy && !flag)
                    {
                        flag4 = true;
                    }
                    if (rope.relaxed != 0 && rope.cut == -1 && flag4)
                    {
                        float num9 = RADIANS_TO_DEGREES(vectAngleNormalized(v));
                        if (twoParts != 2)
                        {
                            GameObject gameObject = (constraintedPoint2 == starL) ? candyL : candyR;
                            if (!rope.chosenOne)
                            {
                                rope.initialCandleAngle = gameObject.rotation - num9;
                            }
                            if (constraintedPoint2 == starL)
                            {
                                lastCandyRotateDeltaL = num9 + rope.initialCandleAngle - gameObject.rotation;
                                flag2 = true;
                            }
                            else
                            {
                                lastCandyRotateDeltaR = num9 + rope.initialCandleAngle - gameObject.rotation;
                                flag3 = true;
                            }
                            gameObject.rotation = num9 + rope.initialCandleAngle;
                        }
                        else
                        {
                            if (!rope.chosenOne)
                            {
                                rope.initialCandleAngle = candyMain.rotation - num9;
                            }
                            lastCandyRotateDelta = num9 + rope.initialCandleAngle - candyMain.rotation;
                            candyMain.rotation = num9 + rope.initialCandleAngle;
                            flag = true;
                        }
                        rope.chosenOne = true;
                        goto IL_08D4;
                    }
                    rope.chosenOne = false;
                    goto IL_08D4;
                }
                if (twoParts != 2)
                {
                    if (!flag2 && !noCandyL)
                    {
                        candyL.rotation += MIN(5.0, lastCandyRotateDeltaL);
                        lastCandyRotateDeltaL *= 0.98f;
                    }
                    if (!flag3 && !noCandyR)
                    {
                        candyR.rotation += MIN(5.0, lastCandyRotateDeltaR);
                        lastCandyRotateDeltaR *= 0.98f;
                    }
                }
                else if (!flag && !noCandy)
                {
                    candyMain.rotation += MIN(5.0, lastCandyRotateDelta);
                    lastCandyRotateDelta *= 0.98f;
                }
            }
            if (!noCandy)
            {
                star.update(delta * ropePhysicsSpeed);
                candy.x = star.pos.x;
                candy.y = star.pos.y;
                candy.update(delta);
                calculateTopLeft(candy);
            }
            if (twoParts != 2)
            {
                candyL.update(delta);
                starL.update(delta * ropePhysicsSpeed);
                candyR.update(delta);
                starR.update(delta * ropePhysicsSpeed);
                if (twoParts == 1)
                {
                    for (int l = 0; l < 30; l++)
                    {
                        ConstraintedPoint.satisfyConstraints(starL);
                        ConstraintedPoint.satisfyConstraints(starR);
                    }
                }
                if (partsDist > 0.0)
                {
                    if (Mover.moveVariableToTarget(ref partsDist, 0.0, 200.0, (double)delta))
                    {
                        CTRSoundMgr._playSound(40);
                        twoParts = 2;
                        noCandy = false;
                        noCandyL = true;
                        noCandyR = true;
                        int num20 = Preferences._getIntForKey("PREFS_CANDIES_UNITED") + 1;
                        Preferences._setIntforKey(num20, "PREFS_CANDIES_UNITED", false);
                        if (num20 == 100)
                        {
                            CTRRootController.postAchievementName("1432722351", ACHIEVEMENT_STRING("\"Romantic Soul\""));
                        }
                        if (candyBubbleL != null || candyBubbleR != null)
                        {
                            candyBubble = (candyBubbleL != null) ? candyBubbleL : candyBubbleR;
                            candyBubbleAnimation.visible = true;
                        }
                        lastCandyRotateDelta = 0f;
                        lastCandyRotateDeltaL = 0f;
                        lastCandyRotateDeltaR = 0f;
                        star.pos.x = starL.pos.x;
                        star.pos.y = starL.pos.y;
                        candy.x = star.pos.x;
                        candy.y = star.pos.y;
                        calculateTopLeft(candy);
                        Vector vector = vectSub(starL.pos, starL.prevPos);
                        Vector vector2 = vectSub(starR.pos, starR.prevPos);
                        Vector v2 = vect((vector.x + vector2.x) / 2f, (vector.y + vector2.y) / 2f);
                        star.prevPos = vectSub(star.pos, v2);
                        int num10 = bungees.count();
                        for (int m = 0; m < num10; m++)
                        {
                            Bungee rope2 = ((Grab)bungees.objectAtIndex(m)).rope;
                            if (rope2 != null && rope2.cut != rope2.parts.Count - 3 && (rope2.tail == starL || rope2.tail == starR))
                            {
                                ConstraintedPoint constraintedPoint3 = rope2.parts[rope2.parts.Count - 2];
                                int num11 = (int)rope2.tail.restLengthFor(constraintedPoint3);
                                star.addConstraintwithRestLengthofType(constraintedPoint3, num11, Constraint.CONSTRAINT.CONSTRAINT_DISTANCE);
                                rope2.tail = star;
                                rope2.parts[rope2.parts.Count - 1] = star;
                                rope2.initialCandleAngle = 0f;
                                rope2.chosenOne = false;
                            }
                        }
                        Animation animation = Animation.Animation_createWithResID(63);
                        animation.doRestoreCutTransparency();
                        animation.x = candy.x;
                        animation.y = candy.y;
                        animation.anchor = 18;
                        int n = animation.addAnimationDelayLoopFirstLast(0.05, Timeline.LoopType.TIMELINE_NO_LOOP, 21, 25);
                        animation.getTimeline(n).delegateTimelineDelegate = aniPool;
                        animation.playTimeline(0);
                        aniPool.addChild(animation);
                    }
                    else
                    {
                        starL.changeRestLengthToFor(partsDist, starR);
                        starR.changeRestLengthToFor(partsDist, starL);
                    }
                }
                if (!noCandyL && !noCandyR && GameObject.objectsIntersect(candyL, candyR) && twoParts == 0)
                {
                    twoParts = 1;
                    partsDist = vectDistance(starL.pos, starR.pos);
                    starL.addConstraintwithRestLengthofType(starR, partsDist, Constraint.CONSTRAINT.CONSTRAINT_NOT_MORE_THAN);
                    starR.addConstraintwithRestLengthofType(starL, partsDist, Constraint.CONSTRAINT.CONSTRAINT_NOT_MORE_THAN);
                }
            }
            target.update(delta);
            if (camera.type != CAMERA_TYPE.CAMERA_SPEED_PIXELS || !ignoreTouches)
            {
                foreach (object obj2 in stars)
                {
                    Star star = (Star)obj2;
                    star.update(delta);
                    if (star.timeout > 0.0 && star.time == 0.0)
                    {
                        star.getTimeline(1).delegateTimelineDelegate = aniPool;
                        aniPool.addChild(star);
                        stars.removeObject(star);
                        star.timedAnim.playTimeline(1);
                        star.playTimeline(1);
                        break;
                    }
                    if ((twoParts == 2) ? (GameObject.objectsIntersect(candy, star) && !noCandy) : ((GameObject.objectsIntersect(candyL, star) && !noCandyL) || (GameObject.objectsIntersect(candyR, star) && !noCandyR)))
                    {
                        candyBlink.playTimeline(1);
                        starsCollected++;
                        hudStar[starsCollected - 1].playTimeline(0);
                        Animation animation2 = Animation.Animation_createWithResID(71);
                        animation2.doRestoreCutTransparency();
                        animation2.x = star.x;
                        animation2.y = star.y;
                        animation2.anchor = 18;
                        int n2 = animation2.addAnimationDelayLoopFirstLast(0.05, Timeline.LoopType.TIMELINE_NO_LOOP, 0, 12);
                        animation2.getTimeline(n2).delegateTimelineDelegate = aniPool;
                        animation2.playTimeline(0);
                        aniPool.addChild(animation2);
                        stars.removeObject(star);
                        CTRSoundMgr._playSound(25 + starsCollected - 1);
                        if (target.getCurrentTimelineIndex() == 0)
                        {
                            target.playAnimationtimeline(101, 3);
                            break;
                        }
                        break;
                    }
                }
            }
            foreach (object obj3 in bubbles)
            {
                Bubble bubble3 = (Bubble)obj3;
                bubble3.update(delta);
                float num12 = 85f;
                if (twoParts != 2)
                {
                    if (!noCandyL && !bubble3.popped && pointInRect(candyL.x, candyL.y, bubble3.x - num12, bubble3.y - num12, num12 * 2f, num12 * 2f))
                    {
                        if (candyBubbleL != null)
                        {
                            popBubbleAtXY(bubble3.x, bubble3.y);
                        }
                        candyBubbleL = bubble3;
                        candyBubbleAnimationL.visible = true;
                        CTRSoundMgr._playSound(13);
                        bubble3.popped = true;
                        bubble3.removeChildWithID(0);
                        break;
                    }
                    if (!noCandyR && !bubble3.popped && pointInRect(candyR.x, candyR.y, bubble3.x - num12, bubble3.y - num12, num12 * 2f, num12 * 2f))
                    {
                        if (candyBubbleR != null)
                        {
                            popBubbleAtXY(bubble3.x, bubble3.y);
                        }
                        candyBubbleR = bubble3;
                        candyBubbleAnimationR.visible = true;
                        CTRSoundMgr._playSound(13);
                        bubble3.popped = true;
                        bubble3.removeChildWithID(0);
                        break;
                    }
                }
                else if (!noCandy && !bubble3.popped && pointInRect(candy.x, candy.y, bubble3.x - num12, bubble3.y - num12, num12 * 2f, num12 * 2f))
                {
                    if (candyBubble != null)
                    {
                        popBubbleAtXY(bubble3.x, bubble3.y);
                    }
                    candyBubble = bubble3;
                    candyBubbleAnimation.visible = true;
                    CTRSoundMgr._playSound(13);
                    bubble3.popped = true;
                    bubble3.removeChildWithID(0);
                    break;
                }
                if (!bubble3.withoutShadow)
                {
                    foreach (object obj4 in rotatedCircles)
                    {
                        RotatedCircle rotatedCircle5 = (RotatedCircle)obj4;
                        if (vectDistance(vect(bubble3.x, bubble3.y), vect(rotatedCircle5.x, rotatedCircle5.y)) < rotatedCircle5.sizeInPixels)
                        {
                            bubble3.withoutShadow = true;
                        }
                    }
                }
            }
            foreach (object obj5 in tutorials)
            {
                ((Text)obj5).update(delta);
            }
            foreach (object obj6 in tutorialImages)
            {
                ((GameObject)obj6).update(delta);
            }
            foreach (object obj7 in pumps)
            {
                Pump pump = (Pump)obj7;
                pump.update(delta);
                if (Mover.moveVariableToTarget(ref pump.pumpTouchTimer, 0.0, 1.0, (double)delta))
                {
                    operatePump(pump);
                }
            }
            RotatedCircle rotatedCircle6 = null;
            foreach (object obj8 in rotatedCircles)
            {
                RotatedCircle rotatedCircle7 = (RotatedCircle)obj8;
                foreach (object obj9 in bungees)
                {
                    Grab bungee4 = (Grab)obj9;
                    if (vectDistance(vect(bungee4.x, bungee4.y), vect(rotatedCircle7.x, rotatedCircle7.y)) <= rotatedCircle7.sizeInPixels + RTPD(5.0) * 3f)
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
                foreach (object obj10 in bubbles)
                {
                    Bubble bubble4 = (Bubble)obj10;
                    if (vectDistance(vect(bubble4.x, bubble4.y), vect(rotatedCircle7.x, rotatedCircle7.y)) <= rotatedCircle7.sizeInPixels + RTPD(10.0) * 3f)
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
                rotatedCircles.removeObject(rotatedCircle6);
            }
            float num13 = RTPD(20.0);
            foreach (object obj11 in socks)
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
                ref Vector ptr = vectRotate(star.posDelta, (double)DEGREES_TO_RADIANS(0f - num14));
                sock3.rotation = num14;
                sock3.updateRotation();
                if (ptr.y >= 0.0 && (lineInRect(sock3.t1.x, sock3.t1.y, sock3.t2.x, sock3.t2.y, star.pos.x - num13, star.pos.y - num13, num13 * 2f, num13 * 2f) || lineInRect(sock3.b1.x, sock3.b1.y, sock3.b2.x, sock3.b2.y, star.pos.x - num13, star.pos.y - num13, num13 * 2f, num13 * 2f)))
                {
                    if (sock3.state != Sock.SOCK_IDLE)
                    {
                        continue;
                    }
                    using (IEnumerator enumerator2 = socks.GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            object obj12 = enumerator2.Current;
                            Sock sock4 = (Sock)obj12;
                            if (sock4 != sock3 && sock4.group == sock3.group)
                            {
                                sock3.state = Sock.SOCK_RECEIVING;
                                sock4.state = Sock.SOCK_THROWING;
                                releaseAllRopes(false);
                                savedSockSpeed = 0.9f * vectLength(star.v);
                                savedSockSpeed *= 1.4f;
                                targetSock = sock4;
                                sock3.light.playTimeline(0);
                                sock3.light.visible = true;
                                CTRSoundMgr._playSound(45);
                                dd.callObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(selector_teleport), null, 0.1);
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
            foreach (object obj13 in razors)
            {
                Razor razor = (Razor)obj13;
                razor.update(delta);
                cutWithRazorOrLine1Line2Immediate(razor, vectZero, vectZero, false);
            }
            foreach (object obj14 in spikes)
            {
                Spikes spike = (Spikes)obj14;
                spike.update(delta);
                float num15 = 15f;
                if (!spike.electro || (spike.electro && spike.electroOn))
                {
                    bool flag5 = false;
                    bool flag6;
                    if (twoParts != 2)
                    {
                        flag6 = (lineInRect(spike.t1.x, spike.t1.y, spike.t2.x, spike.t2.y, starL.pos.x - num15, starL.pos.y - num15, num15 * 2f, num15 * 2f) || lineInRect(spike.b1.x, spike.b1.y, spike.b2.x, spike.b2.y, starL.pos.x - num15, starL.pos.y - num15, num15 * 2f, num15 * 2f)) && !noCandyL;
                        if (flag6)
                        {
                            flag5 = true;
                        }
                        else
                        {
                            flag6 = (lineInRect(spike.t1.x, spike.t1.y, spike.t2.x, spike.t2.y, starR.pos.x - num15, starR.pos.y - num15, num15 * 2f, num15 * 2f) || lineInRect(spike.b1.x, spike.b1.y, spike.b2.x, spike.b2.y, starR.pos.x - num15, starR.pos.y - num15, num15 * 2f, num15 * 2f)) && !noCandyR;
                        }
                    }
                    else
                    {
                        flag6 = (lineInRect(spike.t1.x, spike.t1.y, spike.t2.x, spike.t2.y, star.pos.x - num15, star.pos.y - num15, num15 * 2f, num15 * 2f) || lineInRect(spike.b1.x, spike.b1.y, spike.b2.x, spike.b2.y, star.pos.x - num15, star.pos.y - num15, num15 * 2f, num15 * 2f)) && !noCandy;
                    }
                    if (flag6)
                    {
                        if (twoParts != 2)
                        {
                            if (flag5)
                            {
                                if (candyBubbleL != null)
                                {
                                    popCandyBubble(true);
                                }
                            }
                            else if (candyBubbleR != null)
                            {
                                popCandyBubble(false);
                            }
                        }
                        else if (candyBubble != null)
                        {
                            popCandyBubble(false);
                        }
                        Image image2 = Image.Image_createWithResID(63);
                        image2.doRestoreCutTransparency();
                        CandyBreak candyBreak = (CandyBreak)new CandyBreak().initWithTotalParticlesandImageGrid(5, image2);
                        if (gravityButton != null && !gravityNormal)
                        {
                            candyBreak.gravity.y = -500f;
                            candyBreak.angle = 90f;
                        }
                        candyBreak.particlesDelegate = new Particles.ParticlesFinished(aniPool.particlesFinished);
                        if (twoParts != 2)
                        {
                            if (flag5)
                            {
                                candyBreak.x = candyL.x;
                                candyBreak.y = candyL.y;
                                noCandyL = true;
                            }
                            else
                            {
                                candyBreak.x = candyR.x;
                                candyBreak.y = candyR.y;
                                noCandyR = true;
                            }
                        }
                        else
                        {
                            candyBreak.x = candy.x;
                            candyBreak.y = candy.y;
                            noCandy = true;
                        }
                        candyBreak.startSystem(5);
                        aniPool.addChild(candyBreak);
                        CTRSoundMgr._playSound(14);
                        releaseAllRopes(flag5);
                        if (restartState != 0 && (twoParts == 2 || !noCandyL || !noCandyR))
                        {
                            dd.callObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(selector_gameLost), null, 0.3);
                        }
                        return;
                    }
                }
            }
            foreach (object obj15 in bouncers)
            {
                Bouncer bouncer = (Bouncer)obj15;
                bouncer.update(delta);
                float num16 = 40f;
                bool flag7 = false;
                bool flag8;
                if (twoParts != 2)
                {
                    flag8 = (lineInRect(bouncer.t1.x, bouncer.t1.y, bouncer.t2.x, bouncer.t2.y, starL.pos.x - num16, starL.pos.y - num16, num16 * 2f, num16 * 2f) || lineInRect(bouncer.b1.x, bouncer.b1.y, bouncer.b2.x, bouncer.b2.y, starL.pos.x - num16, starL.pos.y - num16, num16 * 2f, num16 * 2f)) && !noCandyL;
                    if (flag8)
                    {
                        flag7 = true;
                    }
                    else
                    {
                        flag8 = (lineInRect(bouncer.t1.x, bouncer.t1.y, bouncer.t2.x, bouncer.t2.y, starR.pos.x - num16, starR.pos.y - num16, num16 * 2f, num16 * 2f) || lineInRect(bouncer.b1.x, bouncer.b1.y, bouncer.b2.x, bouncer.b2.y, starR.pos.x - num16, starR.pos.y - num16, num16 * 2f, num16 * 2f)) && !noCandyR;
                    }
                }
                else
                {
                    flag8 = (lineInRect(bouncer.t1.x, bouncer.t1.y, bouncer.t2.x, bouncer.t2.y, star.pos.x - num16, star.pos.y - num16, num16 * 2f, num16 * 2f) || lineInRect(bouncer.b1.x, bouncer.b1.y, bouncer.b2.x, bouncer.b2.y, star.pos.x - num16, star.pos.y - num16, num16 * 2f, num16 * 2f)) && !noCandy;
                }
                if (flag8)
                {
                    if (twoParts != 2)
                    {
                        if (flag7)
                        {
                            handleBouncePtDelta(bouncer, starL, delta);
                        }
                        else
                        {
                            handleBouncePtDelta(bouncer, starR, delta);
                        }
                    }
                    else
                    {
                        handleBouncePtDelta(bouncer, star, delta);
                    }
                }
                else
                {
                    bouncer.skip = false;
                }
            }
            float num17 = -40f;
            float num18 = 14f;
            if (twoParts == 0)
            {
                if (candyBubbleL != null)
                {
                    if (gravityButton != null && !gravityNormal)
                    {
                        starL.applyImpulseDelta(vect((0f - starL.v.x) / num18, (0f - starL.v.y) / num18 - num17), delta);
                    }
                    else
                    {
                        starL.applyImpulseDelta(vect((0f - starL.v.x) / num18, (0f - starL.v.y) / num18 + num17), delta);
                    }
                }
                if (candyBubbleR != null)
                {
                    if (gravityButton != null && !gravityNormal)
                    {
                        starR.applyImpulseDelta(vect((0f - starR.v.x) / num18, (0f - starR.v.y) / num18 - num17), delta);
                    }
                    else
                    {
                        starR.applyImpulseDelta(vect((0f - starR.v.x) / num18, (0f - starR.v.y) / num18 + num17), delta);
                    }
                }
            }
            if (twoParts == 1)
            {
                if (candyBubbleR != null || candyBubbleL != null)
                {
                    if (gravityButton != null && !gravityNormal)
                    {
                        starL.applyImpulseDelta(vect((0f - starL.v.x) / num18, (0f - starL.v.y) / num18 - num17), delta);
                        starR.applyImpulseDelta(vect((0f - starR.v.x) / num18, (0f - starR.v.y) / num18 - num17), delta);
                    }
                    else
                    {
                        starL.applyImpulseDelta(vect((0f - starL.v.x) / num18, (0f - starL.v.y) / num18 + num17), delta);
                        starR.applyImpulseDelta(vect((0f - starR.v.x) / num18, (0f - starR.v.y) / num18 + num17), delta);
                    }
                }
            }
            else if (candyBubble != null)
            {
                if (gravityButton != null && !gravityNormal)
                {
                    star.applyImpulseDelta(vect((0f - star.v.x) / num18, (0f - star.v.y) / num18 - num17), delta);
                }
                else
                {
                    star.applyImpulseDelta(vect((0f - star.v.x) / num18, (0f - star.v.y) / num18 + num17), delta);
                }
            }
            if (!noCandy)
            {
                if (!mouthOpen)
                {
                    if (vectDistance(star.pos, vect(target.x, target.y)) < 200f)
                    {
                        mouthOpen = true;
                        target.playTimeline(7);
                        CTRSoundMgr._playSound(17);
                        mouthCloseTimer = 1f;
                    }
                }
                else if (mouthCloseTimer > 0.0)
                {
                    Mover.moveVariableToTarget(ref mouthCloseTimer, 0.0, 1.0, (double)delta);
                    if (mouthCloseTimer <= 0.0)
                    {
                        if (vectDistance(star.pos, vect(target.x, target.y)) > 200f)
                        {
                            mouthOpen = false;
                            target.playTimeline(8);
                            CTRSoundMgr._playSound(16);
                            tummyTeasers++;
                            if (tummyTeasers >= 10)
                            {
                                CTRRootController.postAchievementName("1058281905", ACHIEVEMENT_STRING("\"Tummy Teaser\""));
                            }
                        }
                        else
                        {
                            mouthCloseTimer = 1f;
                        }
                    }
                }
                if (restartState != 0 && GameObject.objectsIntersect(candy, target))
                {
                    gameWon();
                    return;
                }
            }
            bool flag9 = twoParts == 2 && pointOutOfScreen(star) && !noCandy;
            bool flag10 = twoParts != 2 && pointOutOfScreen(starL) && !noCandyL;
            bool flag11 = twoParts != 2 && pointOutOfScreen(starR) && !noCandyR;
            if (flag10 || flag11 || flag9)
            {
                if (flag9)
                {
                    noCandy = true;
                }
                if (flag10)
                {
                    noCandyL = true;
                }
                if (flag11)
                {
                    noCandyR = true;
                }
                if (restartState != 0)
                {
                    int num21 = Preferences._getIntForKey("PREFS_CANDIES_LOST") + 1;
                    Preferences._setIntforKey(num21, "PREFS_CANDIES_LOST", false);
                    if (num21 == 50)
                    {
                        CTRRootController.postAchievementName("681497443", ACHIEVEMENT_STRING("\"Weight Loser\""));
                    }
                    if (num21 == 200)
                    {
                        CTRRootController.postAchievementName("1058341297", ACHIEVEMENT_STRING("\"Calorie Minimizer\""));
                    }
                    if (twoParts == 2 || !noCandyL || !noCandyR)
                    {
                        gameLost();
                    }
                    return;
                }
            }
            if (special != 0 && special == 1 && !noCandy && candyBubble != null && candy.y < 400f && candy.x > 1200f)
            {
                special = 0;
                foreach (object obj16 in tutorials)
                {
                    TutorialText tutorial2 = (TutorialText)obj16;
                    if (tutorial2.special == 1)
                    {
                        tutorial2.playTimeline(0);
                    }
                }
                foreach (object obj17 in tutorialImages)
                {
                    GameObjectSpecial tutorialImage2 = (GameObjectSpecial)obj17;
                    if (tutorialImage2.special == 1)
                    {
                        tutorialImage2.playTimeline(0);
                    }
                }
            }
            if (clickToCut && !ignoreTouches)
            {
                resetBungeeHighlight();
                bool flag12 = false;
                Vector p = vectAdd(slastTouch, camera.pos);
                if (gravityButton != null && ((Button)gravityButton.getChild(gravityButton.on() ? 1 : 0)).isInTouchZoneXYforTouchDown(p.x, p.y, true))
                {
                    flag12 = true;
                }
                if (candyBubble != null || (twoParts != 2 && (candyBubbleL != null || candyBubbleR != null)))
                {
                    foreach (object obj18 in bubbles)
                    {
                        Bubble bubble5 = (Bubble)obj18;
                        if (candyBubble != null && pointInRect(p.x, p.y, star.pos.x - 60f, star.pos.y - 60f, 120f, 120f))
                        {
                            flag12 = true;
                            break;
                        }
                        if (candyBubbleL != null && pointInRect(p.x, p.y, starL.pos.x - 60f, starL.pos.y - 60f, 120f, 120f))
                        {
                            flag12 = true;
                            break;
                        }
                        if (candyBubbleR != null && pointInRect(p.x, p.y, starR.pos.x - 60f, starR.pos.y - 60f, 120f, 120f))
                        {
                            flag12 = true;
                            break;
                        }
                    }
                }
                foreach (object obj19 in spikes)
                {
                    Spikes spike2 = (Spikes)obj19;
                    if (spike2.rotateButton != null && spike2.rotateButton.isInTouchZoneXYforTouchDown(p.x, p.y, true))
                    {
                        flag12 = true;
                    }
                }
                foreach (object obj20 in pumps)
                {
                    Pump pump2 = (Pump)obj20;
                    if (GameObject.pointInObject(p, pump2))
                    {
                        flag12 = true;
                        break;
                    }
                }
                foreach (object obj21 in rotatedCircles)
                {
                    RotatedCircle rotatedCircle8 = (RotatedCircle)obj21;
                    if (rotatedCircle8.isLeftControllerActive() || rotatedCircle8.isRightControllerActive())
                    {
                        flag12 = true;
                        break;
                    }
                    if (vectDistance(vect(p.x, p.y), vect(rotatedCircle8.handle1.x, rotatedCircle8.handle1.y)) <= 90f || vectDistance(vect(p.x, p.y), vect(rotatedCircle8.handle2.x, rotatedCircle8.handle2.y)) <= 90f)
                    {
                        flag12 = true;
                        break;
                    }
                }
                foreach (object obj22 in bungees)
                {
                    Grab bungee5 = (Grab)obj22;
                    if (bungee5.wheel && pointInRect(p.x, p.y, bungee5.x - 110f, bungee5.y - 110f, 220f, 220f))
                    {
                        flag12 = true;
                        break;
                    }
                    if (bungee5.moveLength > 0.0 && (pointInRect(p.x, p.y, bungee5.x - 65f, bungee5.y - 65f, 130f, 130f) || bungee5.moverDragging != -1))
                    {
                        flag12 = true;
                        break;
                    }
                }
                if (!flag12)
                {
                    Vector s = default(Vector);
                    Grab grab2 = null;
                    Bungee nearestBungeeSegmentByBeziersPointsatXYgrab = getNearestBungeeSegmentByBeziersPointsatXYgrab(ref s, slastTouch.x + camera.pos.x, slastTouch.y + camera.pos.y, ref grab2);
                    if (nearestBungeeSegmentByBeziersPointsatXYgrab != null)
                    {
                        nearestBungeeSegmentByBeziersPointsatXYgrab.highlighted = true;
                    }
                }
            }
            if (Mover.moveVariableToTarget(ref dimTime, 0.0, 1.0, (double)delta))
            {
                if (restartState == 0)
                {
                    restartState = 1;
                    hide();
                    show();
                    dimTime = 0.15f;
                    return;
                }
                restartState = -1;
            }
        }

        public virtual void teleport()
        {
            if (targetSock != null)
            {
                targetSock.light.playTimeline(0);
                targetSock.light.visible = true;
                Vector v = vect(0f, -16f);
                v = vectRotate(v, (double)DEGREES_TO_RADIANS(targetSock.rotation));
                star.pos.x = targetSock.x;
                star.pos.y = targetSock.y;
                star.pos = vectAdd(star.pos, v);
                star.prevPos.x = star.pos.x;
                star.prevPos.y = star.pos.y;
                star.v = vectMult(vectRotate(vect(0f, -1f), (double)DEGREES_TO_RADIANS(targetSock.rotation)), savedSockSpeed);
                star.posDelta = vectDiv(star.v, 60f);
                star.prevPos = vectSub(star.pos, star.posDelta);
                targetSock = null;
            }
        }

        public virtual void animateLevelRestart()
        {
            restartState = 0;
            dimTime = 0.15f;
        }

        public virtual void releaseAllRopes(bool left)
        {
            int num = bungees.count();
            for (int i = 0; i < num; i++)
            {
                Grab grab = (Grab)bungees.objectAtIndex(i);
                Bungee rope = grab.rope;
                if (rope != null && (rope.tail == star || (rope.tail == starL && left) || (rope.tail == starR && !left)))
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
                        spiderBusted(grab);
                    }
                }
            }
        }

        public virtual void calculateScore()
        {
            timeBonus = (int)MAX(0f, 30f - time) * 100;
            timeBonus /= 10;
            timeBonus *= 10;
            starBonus = 1000 * starsCollected;
            score = (int)ceil(timeBonus + starBonus);
        }

        public virtual void gameWon()
        {
            dd.cancelAllDispatches();
            target.playTimeline(6);
            CTRSoundMgr._playSound(15);
            if (candyBubble != null)
            {
                popCandyBubble(false);
            }
            noCandy = true;
            candy.passTransformationsToChilds = true;
            candyMain.scaleX = candyMain.scaleY = 1f;
            candyTop.scaleX = candyTop.scaleY = 1f;
            Timeline timeline = new Timeline().initWithMaxKeyFramesOnTrack(2);
            timeline.addKeyFrame(KeyFrame.makePos(candy.x, candy.y, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.addKeyFrame(KeyFrame.makePos(target.x, target.y + 10.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.1));
            timeline.addKeyFrame(KeyFrame.makeScale(0.71, 0.71, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.addKeyFrame(KeyFrame.makeScale(0.0, 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.1));
            timeline.addKeyFrame(KeyFrame.makeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.addKeyFrame(KeyFrame.makeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.1));
            candy.addTimelinewithID(timeline, 0);
            candy.playTimeline(0);
            timeline.delegateTimelineDelegate = aniPool;
            aniPool.addChild(candy);
            dd.callObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(selector_gameWon), null, 2.0);
            calculateScore();
            releaseAllRopes(false);
        }

        public virtual void gameLost()
        {
            dd.cancelAllDispatches();
            target.playAnimationtimeline(102, 5);
            CTRSoundMgr._playSound(18);
            dd.callObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(selector_animateLevelRestart), null, 1.0);
            gameSceneDelegate.gameLost();
        }

        public override void draw()
        {
            OpenGL.glClear(0);
            base.preDraw();
            camera.applyCameraTransformation();
            OpenGL.glEnable(0);
            OpenGL.glDisable(1);
            Vector pos = vectDiv(camera.pos, 1.25f);
            back.updateWithCameraPos(pos);
            float num = canvas.xOffsetScaled;
            float num2 = 0f;
            OpenGL.glPushMatrix();
            OpenGL.glTranslatef((double)num, (double)num2, 0.0);
            OpenGL.glScalef(back.scaleX, back.scaleY, 1.0);
            OpenGL.glTranslatef((double)(0f - num), (double)(0f - num2), 0.0);
            OpenGL.glTranslatef(canvas.xOffsetScaled, 0.0, 0.0);
            back.draw();
            if (mapHeight > SCREEN_HEIGHT)
            {
                float num3 = RTD(2.0);
                int pack = ((CTRRootController)Application.sharedRootController()).getPack();
                CTRTexture2D texture = Application.getTexture(105 + pack * 2);
                int num4 = 0;
                float num5 = texture.quadOffsets[num4].y;
                CTRRectangle r = texture.quadRects[num4];
                r.y += num3;
                r.h -= num3 * 2f;
                GLDrawer.drawImagePart(texture, r, 0.0, (double)(num5 + num3));
            }
            OpenGL.glEnable(1);
            OpenGL.glBlendFunc(BlendingFactor.GL_ONE, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
            if (earthAnims != null)
            {
                foreach (object obj in earthAnims)
                {
                    ((Image)obj).draw();
                }
            }
            OpenGL.glTranslatef((double)-(double)canvas.xOffsetScaled, 0.0, 0.0);
            OpenGL.glPopMatrix();
            OpenGL.glEnable(1);
            OpenGL.glBlendFunc(BlendingFactor.GL_ONE, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
            pollenDrawer.draw();
            gravityButton?.draw();
            OpenGL.glColor4f(Color.White);
            OpenGL.glEnable(0);
            OpenGL.glBlendFunc(BlendingFactor.GL_ONE, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
            support.draw();
            target.draw();
            foreach (object obj2 in tutorials)
            {
                ((Text)obj2).draw();
            }
            foreach (object obj3 in tutorialImages)
            {
                ((GameObject)obj3).draw();
            }
            foreach (object obj4 in razors)
            {
                ((Razor)obj4).draw();
            }
            foreach (object obj5 in rotatedCircles)
            {
                ((RotatedCircle)obj5).draw();
            }
            foreach (object obj6 in bubbles)
            {
                ((GameObject)obj6).draw();
            }
            foreach (object obj7 in pumps)
            {
                ((GameObject)obj7).draw();
            }
            foreach (object obj8 in spikes)
            {
                ((Spikes)obj8).draw();
            }
            foreach (object obj9 in bouncers)
            {
                ((Bouncer)obj9).draw();
            }
            foreach (object obj10 in socks)
            {
                Sock sock = (Sock)obj10;
                sock.y -= 85f;
                sock.draw();
                sock.y += 85f;
            }
            OpenGL.glBlendFunc(BlendingFactor.GL_SRC_ALPHA, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
            foreach (object obj11 in bungees)
            {
                ((Grab)obj11).drawBack();
            }
            foreach (object obj12 in bungees)
            {
                ((Grab)obj12).draw();
            }
            OpenGL.glBlendFunc(BlendingFactor.GL_ONE, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
            foreach (object obj13 in stars)
            {
                ((GameObject)obj13).draw();
            }
            if (!noCandy && targetSock == null)
            {
                candy.x = star.pos.x;
                candy.y = star.pos.y;
                candy.draw();
                if (candyBlink.getCurrentTimeline() != null)
                {
                    OpenGL.glBlendFunc(BlendingFactor.GL_SRC_ALPHA, BlendingFactor.GL_ONE);
                    candyBlink.draw();
                    OpenGL.glBlendFunc(BlendingFactor.GL_ONE, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
                }
            }
            if (twoParts != 2)
            {
                if (!noCandyL)
                {
                    candyL.x = starL.pos.x;
                    candyL.y = starL.pos.y;
                    candyL.draw();
                }
                if (!noCandyR)
                {
                    candyR.x = starR.pos.x;
                    candyR.y = starR.pos.y;
                    candyR.draw();
                }
            }
            foreach (object obj14 in bungees)
            {
                Grab bungee3 = (Grab)obj14;
                if (bungee3.hasSpider)
                {
                    bungee3.drawSpider();
                }
            }
            aniPool.draw();
            bool flag = nightLevel;
            OpenGL.glBlendFunc(BlendingFactor.GL_SRC_ALPHA, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
            OpenGL.glDisable(0);
            OpenGL.glColor4f(Color.White);
            drawCuts();
            OpenGL.glEnable(0);
            OpenGL.glBlendFunc(BlendingFactor.GL_ONE, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
            camera.cancelCameraTransformation();
            staticAniPool.draw();
            if (nightLevel)
            {
                OpenGL.glDisable(4);
            }
            base.postDraw();
        }

        public virtual void drawCuts()
        {
            for (int i = 0; i < 5; i++)
            {
                int num = fingerCuts[i].count();
                if (num > 0)
                {
                    float num2 = RTD(6.0);
                    float num3 = 1f;
                    int num4 = 0;
                    int j = 0;
                    Vector[] array = new Vector[num + 1];
                    int num5 = 0;
                    while (j < num)
                    {
                        FingerCut fingerCut = (FingerCut)fingerCuts[i].objectAtIndex(j);
                        if (j == 0)
                        {
                            array[num5++] = fingerCut.start;
                        }
                        array[num5++] = fingerCut.end;
                        j++;
                    }
                    List<Vector> list = new();
                    Vector vector = default(Vector);
                    bool flag = true;
                    for (int k = 0; k < array.Count(); k++)
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
                        num = array.Count() - 1;
                        int num6 = num * 2;
                        float[] array2 = new float[num6 * 2];
                        float num7 = 1f / num6;
                        float num8 = 0f;
                        int num9 = 0;
                        for (; ; )
                        {
                            if ((double)num8 > 1.0)
                            {
                                num8 = 1f;
                            }
                            Vector vector2 = GLDrawer.calcPathBezier(array, num + 1, num8);
                            if (num9 > array2.Count() - 2)
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
                        float num10 = num2 / num6;
                        float[] array3 = new float[num6 * 4];
                        for (int l = 0; l < num6 - 1; l++)
                        {
                            float s = num3;
                            float s2 = (l == num6 - 2) ? 1f : (num3 + num10);
                            Vector vector3 = vect(array2[l * 2], array2[l * 2 + 1]);
                            Vector vector8 = vect(array2[(l + 1) * 2], array2[(l + 1) * 2 + 1]);
                            Vector vector9 = vectNormalize(vectSub(vector8, vector3));
                            Vector v4 = vectRperp(vector9);
                            Vector v5 = vectPerp(vector9);
                            if (num4 == 0)
                            {
                                Vector vector4 = vectAdd(vector3, vectMult(v4, s));
                                Vector vector5 = vectAdd(vector3, vectMult(v5, s));
                                array3[num4++] = vector5.x;
                                array3[num4++] = vector5.y;
                                array3[num4++] = vector4.x;
                                array3[num4++] = vector4.y;
                            }
                            Vector vector6 = vectAdd(vector8, vectMult(v4, s2));
                            Vector vector7 = vectAdd(vector8, vectMult(v5, s2));
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

        public virtual void handlePumpFlowPtSkin(Pump p, ConstraintedPoint s, GameObject c)
        {
            float num = 624f;
            if (GameObject.rectInObject(p.x - num, p.y - num, p.x + num, p.y + num, c))
            {
                Vector v = vect(c.x, c.y);
                Vector vector = default(Vector);
                vector.x = p.x - p.bb.w / 2f;
                Vector vector2 = default(Vector);
                vector2.x = p.x + p.bb.w / 2f;
                vector.y = vector2.y = p.y;
                if (p.angle != 0.0)
                {
                    v = vectRotateAround(v, 0.0 - p.angle, p.x, p.y);
                }
                if (v.y < vector.y && rectInRect((float)(v.x - c.bb.w / 2.0), (float)(v.y - c.bb.h / 2.0), (float)(v.x + c.bb.w / 2.0), (float)(v.y + c.bb.h / 2.0), vector.x, vector.y - num, vector2.x, vector2.y))
                {
                    float num2 = num * 2f * (num - (vector.y - v.y)) / num;
                    Vector v2 = vect(0f, 0f - num2);
                    v2 = vectRotate(v2, p.angle);
                    s.applyImpulseDelta(v2, 0.016f);
                }
            }
        }

        public virtual void handleBouncePtDelta(Bouncer b, ConstraintedPoint s, float delta)
        {
            if (!b.skip)
            {
                b.skip = true;
                Vector vector = vectSub(s.prevPos, s.pos);
                int num = (vectRotateAround(s.prevPos, (double)(0f - b.angle), b.x, b.y).y >= b.y) ? 1 : (-1);
                float s2 = MAX((double)(vectLength(vector) * 40f), 840.0) * num;
                Vector impulse = vectMult(vectPerp(vectForAngle(b.angle)), s2);
                s.pos = vectRotateAround(s.pos, (double)(0f - b.angle), b.x, b.y);
                s.prevPos = vectRotateAround(s.prevPos, (double)(0f - b.angle), b.x, b.y);
                s.prevPos.y = s.pos.y;
                s.pos = vectRotateAround(s.pos, b.angle, b.x, b.y);
                s.prevPos = vectRotateAround(s.prevPos, b.angle, b.x, b.y);
                s.applyImpulseDelta(impulse, delta);
                b.playTimeline(0);
                CTRSoundMgr._playSound(41);
            }
        }

        public virtual void operatePump(Pump p)
        {
            p.playTimeline(0);
            CTRSoundMgr._playSound(RND_RANGE(29, 32));
            Image grid = Image.Image_createWithResID(83);
            PumpDirt pumpDirt = new PumpDirt().initWithTotalParticlesAngleandImageGrid(5, RADIANS_TO_DEGREES((float)p.angle) - 90f, grid);
            pumpDirt.particlesDelegate = new Particles.ParticlesFinished(aniPool.particlesFinished);
            Vector v = vect(p.x + 80f, p.y);
            v = vectRotateAround(v, p.angle - 1.5707963267948966, p.x, p.y);
            pumpDirt.x = v.x;
            pumpDirt.y = v.y;
            pumpDirt.startSystem(5);
            aniPool.addChild(pumpDirt);
            if (!noCandy)
            {
                handlePumpFlowPtSkin(p, star, candy);
            }
            if (twoParts != 2)
            {
                if (!noCandyL)
                {
                    handlePumpFlowPtSkin(p, starL, candyL);
                }
                if (!noCandyR)
                {
                    handlePumpFlowPtSkin(p, starR, candyR);
                }
            }
        }

        public virtual int cutWithRazorOrLine1Line2Immediate(Razor r, Vector v1, Vector v2, bool im)
        {
            int num = 0;
            for (int i = 0; i < bungees.count(); i++)
            {
                Grab grab = (Grab)bungees.objectAtIndex(i);
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
                            flag = (!grab.wheel || !lineInRect(v1.x, v1.y, v2.x, v2.y, grab.x - 110f, grab.y - 110f, 220f, 220f)) && lineInLine(v1.x, v1.y, v2.x, v2.y, constraintedPoint.pos.x, constraintedPoint.pos.y, constraintedPoint2.pos.x, constraintedPoint2.pos.y);
                        }
                        else if (constraintedPoint.prevPos.x != 2.1474836E+09f)
                        {
                            float num2 = minOf4(constraintedPoint.pos.x, constraintedPoint.prevPos.x, constraintedPoint2.pos.x, constraintedPoint2.prevPos.x);
                            float y1t = minOf4(constraintedPoint.pos.y, constraintedPoint.prevPos.y, constraintedPoint2.pos.y, constraintedPoint2.prevPos.y);
                            float x1r = maxOf4(constraintedPoint.pos.x, constraintedPoint.prevPos.x, constraintedPoint2.pos.x, constraintedPoint2.prevPos.x);
                            float y1b = maxOf4(constraintedPoint.pos.y, constraintedPoint.prevPos.y, constraintedPoint2.pos.y, constraintedPoint2.prevPos.y);
                            flag = rectInRect(num2, y1t, x1r, y1b, r.drawX, r.drawY, r.drawX + r.width, r.drawY + r.height);
                        }
                        if (flag)
                        {
                            num++;
                            if (grab.hasSpider && grab.spiderActive)
                            {
                                spiderBusted(grab);
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

        public virtual void spiderBusted(Grab g)
        {
            int num = Preferences._getIntForKey("PREFS_SPIDERS_BUSTED") + 1;
            Preferences._setIntforKey(num, "PREFS_SPIDERS_BUSTED", false);
            if (num == 40)
            {
                CTRRootController.postAchievementName("681486608", ACHIEVEMENT_STRING("\"Spider Busted\""));
            }
            if (num == 200)
            {
                CTRRootController.postAchievementName("1058341284", ACHIEVEMENT_STRING("\"Spider Tammer\""));
            }
            CTRSoundMgr._playSound(34);
            g.hasSpider = false;
            Image image = Image.Image_createWithResIDQuad(64, 11);
            image.doRestoreCutTransparency();
            Timeline timeline = new Timeline().initWithMaxKeyFramesOnTrack(3);
            if (gravityButton != null && !gravityNormal)
            {
                timeline.addKeyFrame(KeyFrame.makePos(g.spider.x, g.spider.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.0));
                timeline.addKeyFrame(KeyFrame.makePos(g.spider.x, g.spider.y + 50.0, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.3));
                timeline.addKeyFrame(KeyFrame.makePos(g.spider.x, (double)(g.spider.y - SCREEN_HEIGHT), KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 1.0));
            }
            else
            {
                timeline.addKeyFrame(KeyFrame.makePos(g.spider.x, g.spider.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.0));
                timeline.addKeyFrame(KeyFrame.makePos(g.spider.x, g.spider.y - 50.0, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.3));
                timeline.addKeyFrame(KeyFrame.makePos(g.spider.x, (double)(g.spider.y + SCREEN_HEIGHT), KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 1.0));
            }
            timeline.addKeyFrame(KeyFrame.makeRotation(0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.addKeyFrame(KeyFrame.makeRotation(RND_RANGE(-120, 120), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1.0));
            image.addTimelinewithID(timeline, 0);
            image.playTimeline(0);
            image.x = g.spider.x;
            image.y = g.spider.y;
            image.anchor = 18;
            timeline.delegateTimelineDelegate = aniPool;
            aniPool.addChild(image);
        }

        public virtual void spiderWon(Grab sg)
        {
            CTRSoundMgr._playSound(35);
            int num = bungees.count();
            for (int i = 0; i < num; i++)
            {
                Grab grab = (Grab)bungees.objectAtIndex(i);
                Bungee rope = grab.rope;
                if (rope != null && rope.tail == star)
                {
                    if (rope.cut == -1)
                    {
                        rope.setCut(rope.parts.Count - 2);
                        rope.forceWhite = false;
                    }
                    if (grab.hasSpider && grab.spiderActive && sg != grab)
                    {
                        spiderBusted(grab);
                    }
                }
            }
            sg.hasSpider = false;
            spiderTookCandy = true;
            noCandy = true;
            Image image = Image.Image_createWithResIDQuad(64, 12);
            image.doRestoreCutTransparency();
            candy.anchor = candy.parentAnchor = 18;
            candy.x = 0f;
            candy.y = -5f;
            image.addChild(candy);
            Timeline timeline = new Timeline().initWithMaxKeyFramesOnTrack(3);
            if (gravityButton != null && !gravityNormal)
            {
                timeline.addKeyFrame(KeyFrame.makePos(sg.spider.x, sg.spider.y - 10.0, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.0));
                timeline.addKeyFrame(KeyFrame.makePos(sg.spider.x, sg.spider.y + 70.0, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.3));
                timeline.addKeyFrame(KeyFrame.makePos(sg.spider.x, (double)(sg.spider.y - SCREEN_HEIGHT), KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 1.0));
            }
            else
            {
                timeline.addKeyFrame(KeyFrame.makePos(sg.spider.x, sg.spider.y - 10.0, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.0));
                timeline.addKeyFrame(KeyFrame.makePos(sg.spider.x, sg.spider.y - 70.0, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.3));
                timeline.addKeyFrame(KeyFrame.makePos(sg.spider.x, (double)(sg.spider.y + SCREEN_HEIGHT), KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 1.0));
            }
            image.addTimelinewithID(timeline, 0);
            image.playTimeline(0);
            image.x = sg.spider.x;
            image.y = sg.spider.y - 10f;
            image.anchor = 18;
            timeline.delegateTimelineDelegate = aniPool;
            aniPool.addChild(image);
            if (restartState != 0)
            {
                dd.callObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(selector_gameLost), null, 2.0);
            }
        }

        public virtual void popCandyBubble(bool left)
        {
            if (twoParts == 2)
            {
                candyBubble = null;
                candyBubbleAnimation.visible = false;
                popBubbleAtXY(candy.x, candy.y);
                return;
            }
            if (left)
            {
                candyBubbleL = null;
                candyBubbleAnimationL.visible = false;
                popBubbleAtXY(candyL.x, candyL.y);
                return;
            }
            candyBubbleR = null;
            candyBubbleAnimationR.visible = false;
            popBubbleAtXY(candyR.x, candyR.y);
        }

        public virtual void popBubbleAtXY(float bx, float by)
        {
            CTRSoundMgr._playSound(12);
            Animation animation = Animation.Animation_createWithResID(73);
            animation.doRestoreCutTransparency();
            animation.x = bx;
            animation.y = by;
            animation.anchor = 18;
            int i = animation.addAnimationDelayLoopFirstLast(0.05, Timeline.LoopType.TIMELINE_NO_LOOP, 0, 11);
            animation.getTimeline(i).delegateTimelineDelegate = aniPool;
            animation.playTimeline(0);
            aniPool.addChild(animation);
        }

        public virtual bool handleBubbleTouchXY(ConstraintedPoint s, float tx, float ty)
        {
            if (pointInRect(tx + camera.pos.x, ty + camera.pos.y, s.pos.x - 60f, s.pos.y - 60f, 120f, 120f))
            {
                popCandyBubble(s == starL);
                int num = Preferences._getIntForKey("PREFS_BUBBLES_POPPED") + 1;
                Preferences._setIntforKey(num, "PREFS_BUBBLES_POPPED", false);
                if (num == 50)
                {
                    CTRRootController.postAchievementName("681513183", ACHIEVEMENT_STRING("\"Bubble Popper\""));
                }
                if (num == 300)
                {
                    CTRRootController.postAchievementName("1058345234", ACHIEVEMENT_STRING("\"Bubble Master\""));
                }
                return true;
            }
            return false;
        }

        public virtual void resetBungeeHighlight()
        {
            for (int i = 0; i < bungees.count(); i++)
            {
                Bungee rope = ((Grab)bungees.objectAtIndex(i)).rope;
                if (rope != null && rope.cut == -1)
                {
                    rope.highlighted = false;
                }
            }
        }

        public virtual Bungee getNearestBungeeSegmentByBeziersPointsatXYgrab(ref Vector s, float tx, float ty, ref Grab grab)
        {
            float num = 60f;
            Bungee result = null;
            float num2 = num;
            Vector v = vect(tx, ty);
            for (int i = 0; i < bungees.count(); i++)
            {
                Grab grab2 = (Grab)bungees.objectAtIndex(i);
                Bungee rope = grab2.rope;
                if (rope != null)
                {
                    for (int j = 0; j < rope.drawPtsCount; j += 2)
                    {
                        Vector vector = vect(rope.drawPts[j], rope.drawPts[j + 1]);
                        float num3 = vectDistance(vector, v);
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
                float num3 = vectDistance(constraintedPoint.pos, v);
                if (num3 < num2 && (!g.wheel || !pointInRect(constraintedPoint.pos.x, constraintedPoint.pos.y, g.x - 110f, g.y - 110f, 220f, 220f)))
                {
                    num2 = num3;
                    result = rope;
                    s = constraintedPoint.pos;
                }
            }
            return result;
        }

        public virtual bool touchDownXYIndex(float tx, float ty, int ti)
        {
            if (ignoreTouches)
            {
                if (camera.type == CAMERA_TYPE.CAMERA_SPEED_PIXELS)
                {
                    fastenCamera = true;
                }
                return true;
            }
            if (ti >= 5)
            {
                return true;
            }
            if (gravityButton != null && ((Button)gravityButton.getChild(gravityButton.on() ? 1 : 0)).isInTouchZoneXYforTouchDown(tx + camera.pos.x, ty + camera.pos.y, true))
            {
                gravityTouchDown = ti;
            }
            Vector vector = vect(tx, ty);
            if (candyBubble != null && handleBubbleTouchXY(star, tx, ty))
            {
                return true;
            }
            if (twoParts != 2)
            {
                if (candyBubbleL != null && handleBubbleTouchXY(starL, tx, ty))
                {
                    return true;
                }
                if (candyBubbleR != null && handleBubbleTouchXY(starR, tx, ty))
                {
                    return true;
                }
            }
            if (!dragging[ti])
            {
                dragging[ti] = true;
                prevStartPos[ti] = startPos[ti] = vector;
            }
            foreach (object obj in spikes)
            {
                Spikes spike = (Spikes)obj;
                if (spike.rotateButton != null && spike.touchIndex == -1 && spike.rotateButton.onTouchDownXY(tx + camera.pos.x, ty + camera.pos.y))
                {
                    spike.touchIndex = ti;
                    return true;
                }
            }
            int num = pumps.count();
            for (int i = 0; i < num; i++)
            {
                Pump pump = (Pump)pumps.objectAtIndex(i);
                if (GameObject.pointInObject(vect(tx + camera.pos.x, ty + camera.pos.y), pump))
                {
                    pump.pumpTouchTimer = 0.05f;
                    pump.pumpTouch = ti;
                    return true;
                }
            }
            RotatedCircle rotatedCircle = null;
            bool flag = false;
            bool flag2 = false;
            foreach (object obj2 in rotatedCircles)
            {
                RotatedCircle rotatedCircle2 = (RotatedCircle)obj2;
                float num2 = vectDistance(vect(tx + camera.pos.x, ty + camera.pos.y), rotatedCircle2.handle1);
                float num3 = vectDistance(vect(tx + camera.pos.x, ty + camera.pos.y), rotatedCircle2.handle2);
                if ((num2 < 90f && !rotatedCircle2.hasOneHandle()) || num3 < 90f)
                {
                    foreach (object obj3 in rotatedCircles)
                    {
                        RotatedCircle rotatedCircle3 = (RotatedCircle)obj3;
                        if (rotatedCircles.getObjectIndex(rotatedCircle3) > rotatedCircles.getObjectIndex(rotatedCircle2))
                        {
                            float num4 = vectDistance(vect(rotatedCircle3.x, rotatedCircle3.y), vect(rotatedCircle2.x, rotatedCircle2.y));
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
                    rotatedCircle2.lastTouch = vect(tx + camera.pos.x, ty + camera.pos.y);
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
            if (rotatedCircles.getObjectIndex(rotatedCircle) != rotatedCircles.count() - 1 && flag2 && !flag)
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
                rotatedCircles.setObjectAt(rotatedCircle4, rotatedCircles.getObjectIndex(rotatedCircle));
                rotatedCircles.addObject(rotatedCircle);
                rotatedCircle.release();
            }
            foreach (object obj4 in bungees)
            {
                Grab bungee = (Grab)obj4;
                if (bungee.wheel && pointInRect(tx + camera.pos.x, ty + camera.pos.y, bungee.x - 110f, bungee.y - 110f, 220f, 220f))
                {
                    bungee.handleWheelTouch(vect(tx + camera.pos.x, ty + camera.pos.y));
                    bungee.wheelOperating = ti;
                }
                if (bungee.moveLength > 0.0 && pointInRect(tx + camera.pos.x, ty + camera.pos.y, bungee.x - 65f, bungee.y - 65f, 130f, 130f))
                {
                    bungee.moverDragging = ti;
                    return true;
                }
            }
            if (clickToCut && !ignoreTouches)
            {
                Vector s = default(Vector);
                Grab grab2 = null;
                Bungee nearestBungeeSegmentByBeziersPointsatXYgrab = getNearestBungeeSegmentByBeziersPointsatXYgrab(ref s, tx + camera.pos.x, ty + camera.pos.y, ref grab2);
                if (nearestBungeeSegmentByBeziersPointsatXYgrab != null && nearestBungeeSegmentByBeziersPointsatXYgrab.highlighted && getNearestBungeeSegmentByConstraintsforGrab(ref s, grab2) != null)
                {
                    cutWithRazorOrLine1Line2Immediate(null, s, s, false);
                }
            }
            return true;
        }

        public virtual bool touchUpXYIndex(float tx, float ty, int ti)
        {
            if (ignoreTouches)
            {
                return true;
            }
            dragging[ti] = false;
            if (ti >= 5)
            {
                return true;
            }
            if (gravityButton != null && gravityTouchDown == ti)
            {
                if (((Button)gravityButton.getChild(gravityButton.on() ? 1 : 0)).isInTouchZoneXYforTouchDown(tx + camera.pos.x, ty + camera.pos.y, true))
                {
                    gravityButton.toggle();
                    onButtonPressed(0);
                }
                gravityTouchDown = -1;
            }
            foreach (object obj in spikes)
            {
                Spikes spike = (Spikes)obj;
                if (spike.rotateButton != null && spike.touchIndex == ti)
                {
                    spike.touchIndex = -1;
                    if (spike.rotateButton.onTouchUpXY(tx + camera.pos.x, ty + camera.pos.y))
                    {
                        return true;
                    }
                }
            }
            foreach (object obj2 in rotatedCircles)
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
            foreach (object obj3 in bungees)
            {
                Grab bungee = (Grab)obj3;
                if (bungee.wheel && bungee.wheelOperating == ti)
                {
                    bungee.wheelOperating = -1;
                }
                if (bungee.moveLength > 0.0 && bungee.moverDragging == ti)
                {
                    bungee.moverDragging = -1;
                }
            }
            return true;
        }

        public virtual bool touchMoveXYIndex(float tx, float ty, int ti)
        {
            if (ignoreTouches)
            {
                return true;
            }
            Vector vector = vect(tx, ty);
            if (ti >= 5)
            {
                return true;
            }
            foreach (object obj in pumps)
            {
                Pump pump3 = (Pump)obj;
                if (pump3.pumpTouch == ti && pump3.pumpTouchTimer != 0.0 && (double)vectDistance(startPos[ti], vector) > 10.0)
                {
                    pump3.pumpTouchTimer = 0f;
                }
            }
            if (rotatedCircles != null)
            {
                for (int i = 0; i < rotatedCircles.count(); i++)
                {
                    RotatedCircle rotatedCircle = (RotatedCircle)rotatedCircles[i];
                    if (rotatedCircle != null && rotatedCircle.operating == ti)
                    {
                        Vector v = vect(rotatedCircle.x, rotatedCircle.y);
                        Vector vector2 = vect(tx + camera.pos.x, ty + camera.pos.y);
                        Vector v2 = vectSub(rotatedCircle.lastTouch, v);
                        float num = vectAngleNormalized(vectSub(vector2, v)) - vectAngleNormalized(v2);
                        float initial_rotation = DEGREES_TO_RADIANS(rotatedCircle.rotation);
                        rotatedCircle.rotation += RADIANS_TO_DEGREES(num);
                        float a = DEGREES_TO_RADIANS(rotatedCircle.rotation);
                        a = FBOUND_PI(a);
                        rotatedCircle.handle1 = vectRotateAround(rotatedCircle.inithanlde1, (double)a, rotatedCircle.x, rotatedCircle.y);
                        rotatedCircle.handle2 = vectRotateAround(rotatedCircle.inithanlde2, (double)a, rotatedCircle.x, rotatedCircle.y);
                        int num2 = (num > 0f) ? 46 : 47;
                        if ((double)Math.Abs(num) < 0.07)
                        {
                            num2 = -1;
                        }
                        if (rotatedCircle.soundPlaying != num2 && num2 != -1)
                        {
                            CTRSoundMgr._playSound(num2);
                            rotatedCircle.soundPlaying = num2;
                        }
                        for (int j = 0; j < bungees.count(); j++)
                        {
                            Grab grab = (Grab)bungees[j];
                            if (vectDistance(vect(grab.x, grab.y), vect(rotatedCircle.x, rotatedCircle.y)) <= rotatedCircle.sizeInPixels + 5f)
                            {
                                if (grab.initial_rotatedCircle != rotatedCircle)
                                {
                                    grab.initial_x = grab.x;
                                    grab.initial_y = grab.y;
                                    grab.initial_rotatedCircle = rotatedCircle;
                                    grab.initial_rotation = initial_rotation;
                                }
                                float a2 = DEGREES_TO_RADIANS(rotatedCircle.rotation) - grab.initial_rotation;
                                a2 = FBOUND_PI(a2);
                                Vector vector3 = vectRotateAround(vect(grab.initial_x, grab.initial_y), (double)a2, rotatedCircle.x, rotatedCircle.y);
                                grab.x = vector3.x;
                                grab.y = vector3.y;
                                if (grab.rope != null)
                                {
                                    grab.rope.bungeeAnchor.pos = vect(grab.x, grab.y);
                                    grab.rope.bungeeAnchor.pin = grab.rope.bungeeAnchor.pos;
                                }
                                if (grab.radius != -1f)
                                {
                                    grab.reCalcCircle();
                                }
                            }
                        }
                        for (int k = 0; k < pumps.count(); k++)
                        {
                            Pump pump4 = (Pump)pumps[k];
                            if (vectDistance(vect(pump4.x, pump4.y), vect(rotatedCircle.x, rotatedCircle.y)) <= rotatedCircle.sizeInPixels + 5f)
                            {
                                if (pump4.initial_rotatedCircle != rotatedCircle)
                                {
                                    pump4.initial_x = pump4.x;
                                    pump4.initial_y = pump4.y;
                                    pump4.initial_rotatedCircle = rotatedCircle;
                                    pump4.initial_rotation = initial_rotation;
                                }
                                float a3 = DEGREES_TO_RADIANS(rotatedCircle.rotation) - pump4.initial_rotation;
                                a3 = FBOUND_PI(a3);
                                Vector vector4 = vectRotateAround(vect(pump4.initial_x, pump4.initial_y), (double)a3, rotatedCircle.x, rotatedCircle.y);
                                pump4.x = vector4.x;
                                pump4.y = vector4.y;
                                pump4.rotation += RADIANS_TO_DEGREES(num);
                                pump4.updateRotation();
                            }
                        }
                        for (int l = 0; l < bubbles.count(); l++)
                        {
                            Bubble bubble = (Bubble)bubbles[l];
                            if (vectDistance(vect(bubble.x, bubble.y), vect(rotatedCircle.x, rotatedCircle.y)) <= rotatedCircle.sizeInPixels + 10f && bubble != candyBubble && bubble != candyBubbleR && bubble != candyBubbleL)
                            {
                                if (bubble.initial_rotatedCircle != rotatedCircle)
                                {
                                    bubble.initial_x = bubble.x;
                                    bubble.initial_y = bubble.y;
                                    bubble.initial_rotatedCircle = rotatedCircle;
                                    bubble.initial_rotation = initial_rotation;
                                }
                                float a4 = DEGREES_TO_RADIANS(rotatedCircle.rotation) - bubble.initial_rotation;
                                a4 = FBOUND_PI(a4);
                                Vector vector5 = vectRotateAround(vect(bubble.initial_x, bubble.initial_y), (double)a4, rotatedCircle.x, rotatedCircle.y);
                                bubble.x = vector5.x;
                                bubble.y = vector5.y;
                            }
                        }
                        if (pointInRect(target.x, target.y, rotatedCircle.x - rotatedCircle.size, rotatedCircle.y - rotatedCircle.size, 2f * rotatedCircle.size, 2f * rotatedCircle.size))
                        {
                            Vector vector6 = vectRotateAround(vect(target.x, target.y), (double)num, rotatedCircle.x, rotatedCircle.y);
                            target.x = vector6.x;
                            target.y = vector6.y;
                        }
                        rotatedCircle.lastTouch = vector2;
                        return true;
                    }
                }
            }
            int num3 = bungees.count();
            for (int m = 0; m < num3; m++)
            {
                Grab grab2 = (Grab)bungees.objectAtIndex(m);
                if (grab2 != null)
                {
                    if (grab2.wheel && grab2.wheelOperating == ti)
                    {
                        grab2.handleWheelRotate(vect(tx + camera.pos.x, ty + camera.pos.y));
                        return true;
                    }
                    if (grab2.moveLength > 0.0 && grab2.moverDragging == ti)
                    {
                        if (grab2.moveVertical)
                        {
                            grab2.y = FIT_TO_BOUNDARIES(ty + camera.pos.y, grab2.minMoveValue, grab2.maxMoveValue);
                        }
                        else
                        {
                            grab2.x = FIT_TO_BOUNDARIES(tx + camera.pos.x, grab2.minMoveValue, grab2.maxMoveValue);
                        }
                        if (grab2.rope != null)
                        {
                            grab2.rope.bungeeAnchor.pos = vect(grab2.x, grab2.y);
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
            if (dragging[ti])
            {
                Vector start = vectAdd(startPos[ti], camera.pos);
                Vector end = vectAdd(vect(tx, ty), camera.pos);
                FingerCut fingerCut = (FingerCut)new FingerCut().init();
                fingerCut.start = start;
                fingerCut.end = end;
                fingerCut.startSize = 5f;
                fingerCut.endSize = 5f;
                fingerCut.c = RGBAColor.whiteRGBA;
                fingerCuts[ti].addObject(fingerCut);
                int num4 = 0;
                foreach (object obj2 in fingerCuts[ti])
                {
                    FingerCut item = (FingerCut)obj2;
                    num4 += cutWithRazorOrLine1Line2Immediate(null, item.start, item.end, false);
                }
                if (num4 > 0)
                {
                    freezeCamera = false;
                    if (ropesCutAtOnce > 0 && ropeAtOnceTimer > 0.0)
                    {
                        ropesCutAtOnce += num4;
                    }
                    else
                    {
                        ropesCutAtOnce = num4;
                    }
                    ropeAtOnceTimer = 0.1f;
                    int num5 = Preferences._getIntForKey("PREFS_ROPES_CUT") + 1;
                    Preferences._setIntforKey(num5, "PREFS_ROPES_CUT", false);
                    if (num5 == 100)
                    {
                        CTRRootController.postAchievementName("681461850", ACHIEVEMENT_STRING("\"Rope Cutter\""));
                    }
                    if (ropesCutAtOnce >= 3 && ropesCutAtOnce < 5)
                    {
                        CTRRootController.postAchievementName("681464917", ACHIEVEMENT_STRING("\"Quick Finger\""));
                    }
                    if (ropesCutAtOnce >= 5)
                    {
                        CTRRootController.postAchievementName("681508316", ACHIEVEMENT_STRING("\"Master Finger\""));
                    }
                    if (num5 == 800)
                    {
                        CTRRootController.postAchievementName("681457931", ACHIEVEMENT_STRING("\"Rope Cutter Maniac\""));
                    }
                    if (num5 == 2000)
                    {
                        CTRRootController.postAchievementName("1058248892", ACHIEVEMENT_STRING("\"Ultimate Rope Cutter\""));
                    }
                }
                prevStartPos[ti] = startPos[ti];
                startPos[ti] = vector;
            }
            return true;
        }

        public virtual bool touchDraggedXYIndex(float tx, float ty, int index)
        {
            if (index > 5)
            {
                return false;
            }
            slastTouch = vect(tx, ty);
            return true;
        }

        public virtual void onButtonPressed(int n)
        {
            if (MaterialPoint.globalGravity.y == 784.0)
            {
                MaterialPoint.globalGravity.y = -784f;
                gravityNormal = false;
                CTRSoundMgr._playSound(39);
            }
            else
            {
                MaterialPoint.globalGravity.y = 784f;
                gravityNormal = true;
                CTRSoundMgr._playSound(38);
            }
            if (earthAnims == null)
            {
                return;
            }
            foreach (object obj in earthAnims)
            {
                Image earthAnim = (Image)obj;
                if (gravityNormal)
                {
                    earthAnim.playTimeline(0);
                }
                else
                {
                    earthAnim.playTimeline(1);
                }
            }
        }

        public virtual void rotateAllSpikesWithID(int sid)
        {
            foreach (object obj in spikes)
            {
                Spikes spike = (Spikes)obj;
                if (spike.getToggled() == sid)
                {
                    spike.rotateSpikes();
                }
            }
        }

        public override void dealloc()
        {
            for (int i = 0; i < 5; i++)
            {
                fingerCuts[i].release();
            }
            dd.release();
            camera.release();
            back.release();
            base.dealloc();
        }

        public virtual void fullscreenToggled(bool isFullscreen)
        {
            BaseElement childWithName = staticAniPool.getChildWithName("levelLabel");
            if (childWithName != null)
            {
                childWithName.x = 15f + canvas.xOffsetScaled;
            }
            for (int i = 0; i < 3; i++)
            {
                hudStar[i].x = hudStar[i].width * i + canvas.xOffsetScaled;
            }
            if (isFullscreen)
            {
                float num = Global.ScreenSizeManager.ScreenWidth;
                back.scaleX = num / canvas.backingWidth * 1.25f;
                return;
            }
            back.scaleX = 1.25f;
        }

        private void selector_gameLost(NSObject param)
        {
            gameLost();
        }

        private void selector_gameWon(NSObject param)
        {
            CTRSoundMgr.EnableLoopedSounds(false);
            gameSceneDelegate?.gameWon();
        }

        private void selector_animateLevelRestart(NSObject param)
        {
            animateLevelRestart();
        }

        private void selector_showGreeting(NSObject param)
        {
            showGreeting();
        }

        private void selector_doCandyBlink(NSObject param)
        {
            doCandyBlink();
        }

        private void selector_teleport(NSObject param)
        {
            teleport();
        }

        public static float FBOUND_PI(float a)
        {
            return (float)(((double)a > 3.141592653589793) ? ((double)a - 6.283185307179586) : (((double)a < -3.141592653589793) ? ((double)a + 6.283185307179586) : ((double)a)));
        }

        public const int MAX_TOUCHES = 5;

        public const float DIM_TIMEOUT = 0.15f;

        public const int RESTART_STATE_FADE_IN = 0;

        public const int RESTART_STATE_FADE_OUT = 1;

        public const int S_MOVE_DOWN = 0;

        public const int S_WAIT = 1;

        public const int S_MOVE_UP = 2;

        public const int CAMERA_MOVE_TO_CANDY_PART = 0;

        public const int CAMERA_MOVE_TO_CANDY = 1;

        public const int BUTTON_GRAVITY = 0;

        public const int PARTS_SEPARATE = 0;

        public const int PARTS_DIST = 1;

        public const int PARTS_NONE = 2;

        public const float SCOMBO_TIMEOUT = 0.2f;

        public const int SCUT_SCORE = 10;

        public const int MAX_LOST_CANDIES = 3;

        public const float ROPE_CUT_AT_ONCE_TIMEOUT = 0.1f;

        public const int STAR_RADIUS = 42;

        public const float MOUTH_OPEN_RADIUS = 200f;

        public const int BLINK_SKIP = 3;

        public const float MOUTH_OPEN_TIME = 1f;

        public const float PUMP_TIMEOUT = 0.05f;

        public const int CAMERA_SPEED = 14;

        public const float SOCK_SPEED_K = 0.9f;

        public const int SOCK_COLLISION_Y_OFFSET = 85;

        public const int BUBBLE_RADIUS = 60;

        public const int WHEEL_RADIUS = 110;

        public const int GRAB_MOVE_RADIUS = 65;

        public const int RC_CONTROLLER_RADIUS = 90;

        public const int CANDY_BLINK_INITIAL = 0;

        public const int CANDY_BLINK_STAR = 1;

        public const int TUTORIAL_SHOW_ANIM = 0;

        public const int TUTORIAL_HIDE_ANIM = 1;

        public const int EARTH_NORMAL_ANIM = 0;

        public const int EARTH_UPSIDEDOWN_ANIM = 1;

        private const int CHAR_ANIMATION_IDLE = 0;

        private const int CHAR_ANIMATION_IDLE2 = 1;

        private const int CHAR_ANIMATION_IDLE3 = 2;

        private const int CHAR_ANIMATION_EXCITED = 3;

        private const int CHAR_ANIMATION_PUZZLED = 4;

        private const int CHAR_ANIMATION_FAIL = 5;

        private const int CHAR_ANIMATION_WIN = 6;

        private const int CHAR_ANIMATION_MOUTH_OPEN = 7;

        private const int CHAR_ANIMATION_MOUTH_CLOSE = 8;

        private const int CHAR_ANIMATION_CHEW = 9;

        private const int CHAR_ANIMATION_GREETING = 10;

        private DelayedDispatcher dd;

        public GameSceneDelegate gameSceneDelegate;

        private AnimationsPool aniPool;

        private AnimationsPool staticAniPool;

        private PollenDrawer pollenDrawer;

        private TileMap back;

        private CharAnimations target;

        private Image support;

        private GameObject candy;

        private Image candyMain;

        private Image candyTop;

        private Animation candyBlink;

        private Animation candyBubbleAnimation;

        private Animation candyBubbleAnimationL;

        private Animation candyBubbleAnimationR;

        private ConstraintedPoint star;

        private DynamicArray bungees;

        private DynamicArray razors;

        private DynamicArray spikes;

        private DynamicArray stars;

        private DynamicArray bubbles;

        private DynamicArray pumps;

        private DynamicArray socks;

        private DynamicArray bouncers;

        private DynamicArray rotatedCircles;

        private DynamicArray tutorialImages;

        private DynamicArray tutorials;

        private GameObject candyL;

        private GameObject candyR;

        private ConstraintedPoint starL;

        private ConstraintedPoint starR;

        private Animation blink;

        private bool[] dragging = new bool[5];

        private Vector[] startPos = new Vector[5];

        private Vector[] prevStartPos = new Vector[5];

        private float ropePhysicsSpeed;

        private GameObject candyBubble;

        private GameObject candyBubbleL;

        private GameObject candyBubbleR;

        private Animation[] hudStar = new Animation[3];

        private Camera2D camera;

        private float mapWidth;

        private float mapHeight;

        private bool mouthOpen;

        private bool noCandy;

        private int blinkTimer;

        private int idlesTimer;

        private float mouthCloseTimer;

        private float lastCandyRotateDelta;

        private float lastCandyRotateDeltaL;

        private float lastCandyRotateDeltaR;

        private bool spiderTookCandy;

        private int special;

        private bool fastenCamera;

        private float savedSockSpeed;

        private Sock targetSock;

        private int ropesCutAtOnce;

        private float ropeAtOnceTimer;

        private bool clickToCut;

        public int starsCollected;

        public int starBonus;

        public int timeBonus;

        public int score;

        public float time;

        public float initialCameraToStarDistance;

        public float dimTime;

        public int restartState;

        public bool animateRestartDim;

        public bool freezeCamera;

        public int cameraMoveMode;

        public bool ignoreTouches;

        public bool nightLevel;

        public bool gravityNormal;

        public ToggleButton gravityButton;

        public int gravityTouchDown;

        public int twoParts;

        public bool noCandyL;

        public bool noCandyR;

        public float partsDist;

        public DynamicArray earthAnims;

        public int tummyTeasers;

        public Vector slastTouch;

        public DynamicArray[] fingerCuts = new DynamicArray[5];

        private class FingerCut : NSObject
        {
            public Vector start;

            public Vector end;

            public float startSize;

            public float endSize;

            public RGBAColor c;
        }

        private class SCandy : ConstraintedPoint
        {
            public bool good;

            public float speed;

            public float angle;

            public float lastAngleChange;
        }

        private class TutorialText : Text
        {
            public int special;
        }

        private class GameObjectSpecial : CTRGameObject
        {
            private static GameObjectSpecial GameObjectSpecial_create(CTRTexture2D t)
            {
                GameObjectSpecial gameObjectSpecial = new();
                gameObjectSpecial.initWithTexture(t);
                return gameObjectSpecial;
            }

            public static GameObjectSpecial GameObjectSpecial_createWithResIDQuad(int r, int q)
            {
                GameObjectSpecial gameObjectSpecial = GameObjectSpecial_create(Application.getTexture(r));
                gameObjectSpecial.setDrawQuad(q);
                return gameObjectSpecial;
            }

            public int special;
        }
    }
}
