using CutTheRope.desktop;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.sfe;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace CutTheRope.game
{
    internal sealed partial class GameScene
    {
        public override void Show()
        {
            CTRSoundMgr.EnableLoopedSounds(true);
            aniPool.RemoveAllChilds();
            staticAniPool.RemoveAllChilds();
            gravityButton = null;
            gravityTouchDown = -1;
            twoParts = 2;
            partsDist = 0f;
            targetSock = null;
            CTRSoundMgr.StopLoopedSounds();
            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
            XMLNode map = cTRRootController.GetMap();
            bungees = (DynamicArray)new DynamicArray().Init();
            razors = (DynamicArray)new DynamicArray().Init();
            spikes = (DynamicArray)new DynamicArray().Init();
            stars = (DynamicArray)new DynamicArray().Init();
            bubbles = (DynamicArray)new DynamicArray().Init();
            pumps = (DynamicArray)new DynamicArray().Init();
            socks = (DynamicArray)new DynamicArray().Init();
            tutorialImages = (DynamicArray)new DynamicArray().Init();
            tutorials = (DynamicArray)new DynamicArray().Init();
            bouncers = (DynamicArray)new DynamicArray().Init();
            rotatedCircles = (DynamicArray)new DynamicArray().Init();
            pollenDrawer = (PollenDrawer)new PollenDrawer().Init();
            star = (ConstraintedPoint)new ConstraintedPoint().Init();
            star.SetWeight(1f);
            starL = (ConstraintedPoint)new ConstraintedPoint().Init();
            starL.SetWeight(1f);
            starR = (ConstraintedPoint)new ConstraintedPoint().Init();
            starR.SetWeight(1f);
            candy = GameObject.GameObject_createWithResIDQuad(63, 0);
            candy.DoRestoreCutTransparency();
            candy.Retain();
            candy.anchor = 18;
            candy.bb = MakeRectangle(142f, 157f, 112f, 104f);
            candy.passTransformationsToChilds = false;
            candy.scaleX = candy.scaleY = 0.71f;
            candyMain = GameObject.GameObject_createWithResIDQuad(63, 1);
            candyMain.DoRestoreCutTransparency();
            candyMain.anchor = candyMain.parentAnchor = 18;
            _ = candy.AddChild(candyMain);
            candyMain.scaleX = candyMain.scaleY = 0.71f;
            candyTop = GameObject.GameObject_createWithResIDQuad(63, 2);
            candyTop.DoRestoreCutTransparency();
            candyTop.anchor = candyTop.parentAnchor = 18;
            _ = candy.AddChild(candyTop);
            candyTop.scaleX = candyTop.scaleY = 0.71f;
            candyBlink = Animation.Animation_createWithResID(63);
            candyBlink.AddAnimationWithIDDelayLoopFirstLast(0, 0.07f, Timeline.LoopType.TIMELINE_NO_LOOP, 8, 17);
            candyBlink.AddAnimationWithIDDelayLoopCountSequence(1, 0.3f, Timeline.LoopType.TIMELINE_NO_LOOP, 2, 18, [18]);
            Timeline timeline7 = candyBlink.GetTimeline(1);
            timeline7.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline7.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.2));
            candyBlink.visible = false;
            candyBlink.anchor = candyBlink.parentAnchor = 18;
            candyBlink.scaleX = candyBlink.scaleY = 0.71f;
            _ = candy.AddChild(candyBlink);
            candyBubbleAnimation = Animation.Animation_createWithResID(72);
            candyBubbleAnimation.x = candy.x;
            candyBubbleAnimation.y = candy.y;
            candyBubbleAnimation.parentAnchor = candyBubbleAnimation.anchor = 18;
            _ = candyBubbleAnimation.AddAnimationDelayLoopFirstLast(0.05, Timeline.LoopType.TIMELINE_REPLAY, 0, 12);
            candyBubbleAnimation.PlayTimeline(0);
            _ = candy.AddChild(candyBubbleAnimation);
            candyBubbleAnimation.visible = false;
            float num = 3f;
            float num2 = 0f;
            float num3 = 0f;
            for (int i = 0; i < 3; i++)
            {
                Timeline timeline2 = hudStar[i].GetCurrentTimeline();
                timeline2?.StopTimeline();
                hudStar[i].SetDrawQuad(0);
            }
            int num4 = 0;
            int num5 = 0;
            List<XMLNode> list = map.Childs();
            foreach (XMLNode xmlnode in list)
            {
                foreach (XMLNode item2 in xmlnode.Childs())
                {
                    if (item2.Name == "map")
                    {
                        mapWidth = item2["width"].FloatValue();
                        mapHeight = item2["height"].FloatValue();
                        num3 = (2560f - (mapWidth * num)) / 2f;
                        mapWidth *= num;
                        mapHeight *= num;
                        if (cTRRootController.GetPack() == 7)
                        {
                            earthAnims = (DynamicArray)new DynamicArray().Init();
                            if (mapWidth > SCREEN_WIDTH)
                            {
                                CreateEarthImageWithOffsetXY(back.width, 0f);
                            }
                            if (mapHeight > SCREEN_HEIGHT)
                            {
                                CreateEarthImageWithOffsetXY(0f, back.height);
                            }
                            CreateEarthImageWithOffsetXY(0f, 0f);
                        }
                    }
                    else if (item2.Name == "gameDesign")
                    {
                        num4 = item2["mapOffsetX"].IntValue();
                        num5 = item2["mapOffsetY"].IntValue();
                        special = item2["special"].IntValue();
                        ropePhysicsSpeed = item2["ropePhysicsSpeed"].FloatValue();
                        nightLevel = item2["nightLevel"].IsEqualToString("true");
                        twoParts = !item2["twoParts"].IsEqualToString("true") ? 2 : 0;
                        ropePhysicsSpeed *= 1.4f;
                    }
                    else if (item2.Name == "candyL")
                    {
                        starL.pos.x = (item2["x"].IntValue() * num) + num3 + num4;
                        starL.pos.y = (item2["y"].IntValue() * num) + num2 + num5;
                        candyL = GameObject.GameObject_createWithResIDQuad(63, 19);
                        candyL.scaleX = candyL.scaleY = 0.71f;
                        candyL.passTransformationsToChilds = false;
                        candyL.DoRestoreCutTransparency();
                        candyL.Retain();
                        candyL.anchor = 18;
                        candyL.x = starL.pos.x;
                        candyL.y = starL.pos.y;
                        candyL.bb = MakeRectangle(155.0, 176.0, 88.0, 76.0);
                    }
                    else if (item2.Name == "candyR")
                    {
                        starR.pos.x = (item2["x"].IntValue() * num) + num3 + num4;
                        starR.pos.y = (item2["y"].IntValue() * num) + num2 + num5;
                        candyR = GameObject.GameObject_createWithResIDQuad(63, 20);
                        candyR.scaleX = candyR.scaleY = 0.71f;
                        candyR.passTransformationsToChilds = false;
                        candyR.DoRestoreCutTransparency();
                        candyR.Retain();
                        candyR.anchor = 18;
                        candyR.x = starR.pos.x;
                        candyR.y = starR.pos.y;
                        candyR.bb = MakeRectangle(155.0, 176.0, 88.0, 76.0);
                    }
                    else if (item2.Name == "candy")
                    {
                        star.pos.x = (item2["x"].IntValue() * num) + num3 + num4;
                        star.pos.y = (item2["y"].IntValue() * num) + num2 + num5;
                    }
                }
            }
            foreach (XMLNode xmlnode2 in list)
            {
                foreach (XMLNode item3 in xmlnode2.Childs())
                {
                    if (item3.Name == "gravitySwitch")
                    {
                        gravityButton = CreateGravityButtonWithDelegate(this);
                        gravityButton.visible = false;
                        gravityButton.touchable = false;
                        _ = AddChild(gravityButton);
                        gravityButton.x = (item3["x"].IntValue() * num) + num3 + num4;
                        gravityButton.y = (item3["y"].IntValue() * num) + num2 + num5;
                        gravityButton.anchor = 18;
                    }
                    else if (item3.Name == "star")
                    {
                        Star star = Star.Star_createWithResID(78);
                        star.x = (item3["x"].IntValue() * num) + num3 + num4;
                        star.y = (item3["y"].IntValue() * num) + num2 + num5;
                        star.timeout = item3["timeout"].FloatValue();
                        star.CreateAnimations();
                        star.bb = MakeRectangle(70.0, 64.0, 82.0, 82.0);
                        star.ParseMover(item3);
                        star.Update(0f);
                        _ = stars.AddObject(star);
                    }
                    else if (item3.Name == "tutorialText")
                    {
                        if (!ShouldSkipTutorialElement(item3))
                        {
                            TutorialText tutorialText = (TutorialText)new TutorialText().InitWithFont(Application.GetFont(4));
                            tutorialText.color = RGBAColor.MakeRGBA(1.0, 1.0, 1.0, 0.9);
                            tutorialText.x = (item3["x"].IntValue() * num) + num3 + num4;
                            tutorialText.y = (item3["y"].IntValue() * num) + num2 + num5;
                            tutorialText.special = item3["special"].IntValue();
                            tutorialText.SetAlignment(2);
                            NSString newString = item3["text"];
                            tutorialText.SetStringandWidth(newString, item3["width"].IntValue() * num);
                            tutorialText.color = RGBAColor.transparentRGBA;
                            float num6 = tutorialText.special == 3 ? 12f : 0f;
                            Timeline timeline3 = new Timeline().InitWithMaxKeyFramesOnTrack(4);
                            timeline3.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, num6));
                            timeline3.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1.0));
                            if (cTRRootController.GetPack() == 0 && cTRRootController.GetLevel() == 0)
                            {
                                timeline3.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 10.0));
                            }
                            else
                            {
                                timeline3.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 5.0));
                            }
                            timeline3.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                            tutorialText.AddTimelinewithID(timeline3, 0);
                            if (tutorialText.special is 0 or 3)
                            {
                                tutorialText.PlayTimeline(0);
                            }
                            _ = tutorials.AddObject(tutorialText);
                        }
                    }
                    else if (item3.Name is "tutorial01" or "tutorial02" or "tutorial03" or "tutorial04" or "tutorial05" or "tutorial06" or "tutorial07" or "tutorial08" or "tutorial09" or "tutorial10" or "tutorial11")
                    {
                        if (!ShouldSkipTutorialElement(item3))
                        {
                            int q = new NSString(item3.Name[8..]).IntValue() - 1;
                            GameObjectSpecial gameObjectSpecial = GameObjectSpecial.GameObjectSpecial_createWithResIDQuad(84, q);
                            gameObjectSpecial.color = RGBAColor.transparentRGBA;
                            gameObjectSpecial.x = (item3["x"].IntValue() * num) + num3 + num4;
                            gameObjectSpecial.y = (item3["y"].IntValue() * num) + num2 + num5;
                            gameObjectSpecial.rotation = item3["angle"].IntValue();
                            gameObjectSpecial.special = item3["special"].IntValue();
                            gameObjectSpecial.ParseMover(item3);
                            float num7 = gameObjectSpecial.special is 3 or 4 ? 12f : 0f;
                            Timeline timeline4 = new Timeline().InitWithMaxKeyFramesOnTrack(4);
                            timeline4.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, num7));
                            timeline4.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1.0));
                            if (cTRRootController.GetPack() == 0 && cTRRootController.GetLevel() == 0)
                            {
                                timeline4.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 10.0));
                            }
                            else
                            {
                                timeline4.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 5.2));
                            }
                            timeline4.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                            gameObjectSpecial.AddTimelinewithID(timeline4, 0);
                            if (gameObjectSpecial.special is 0 or 3)
                            {
                                gameObjectSpecial.PlayTimeline(0);
                            }
                            if (gameObjectSpecial.special is 2 or 4)
                            {
                                Timeline timeline5 = new Timeline().InitWithMaxKeyFramesOnTrack(12);
                                for (int j = 0; j < 2; j++)
                                {
                                    timeline5.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, j == 1 ? 0f : num7));
                                    timeline5.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                                    timeline5.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1.0));
                                    timeline5.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1.1));
                                    timeline5.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                                    timeline5.AddKeyFrame(KeyFrame.MakePos(gameObjectSpecial.x, gameObjectSpecial.y, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, (double)(j == 1 ? 0f : num7)));
                                    timeline5.AddKeyFrame(KeyFrame.MakePos(gameObjectSpecial.x, gameObjectSpecial.y, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                                    timeline5.AddKeyFrame(KeyFrame.MakePos(gameObjectSpecial.x, gameObjectSpecial.y, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1.0));
                                    timeline5.AddKeyFrame(KeyFrame.MakePos(gameObjectSpecial.x + 230.0, gameObjectSpecial.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.5));
                                    timeline5.AddKeyFrame(KeyFrame.MakePos(gameObjectSpecial.x + 440.0, gameObjectSpecial.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.5));
                                    timeline5.AddKeyFrame(KeyFrame.MakePos(gameObjectSpecial.x + 440.0, gameObjectSpecial.y, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.6));
                                }
                                timeline5.SetTimelineLoopType(Timeline.LoopType.TIMELINE_NO_LOOP);
                                gameObjectSpecial.AddTimelinewithID(timeline5, 1);
                                gameObjectSpecial.PlayTimeline(1);
                                gameObjectSpecial.rotation = 10f;
                            }
                            _ = tutorialImages.AddObject(gameObjectSpecial);
                        }
                    }
                    else if (item3.Name == "bubble")
                    {
                        int q2 = RND_RANGE(1, 3);
                        Bubble bubble = Bubble.Bubble_createWithResIDQuad(75, q2);
                        bubble.DoRestoreCutTransparency();
                        bubble.bb = MakeRectangle(48.0, 48.0, 152.0, 152.0);
                        bubble.initial_x = bubble.x = (item3["x"].IntValue() * num) + num3 + num4;
                        bubble.initial_y = bubble.y = (item3["y"].IntValue() * num) + num2 + num5;
                        bubble.initial_rotation = 0f;
                        bubble.initial_rotatedCircle = null;
                        bubble.anchor = 18;
                        bubble.popped = false;
                        Image image = Image.Image_createWithResIDQuad(75, 0);
                        image.DoRestoreCutTransparency();
                        image.parentAnchor = image.anchor = 18;
                        _ = bubble.AddChild(image);
                        _ = bubbles.AddObject(bubble);
                    }
                    else if (item3.Name == "pump")
                    {
                        Pump pump = Pump.Pump_createWithResID(83);
                        pump.DoRestoreCutTransparency();
                        _ = pump.AddAnimationWithDelayLoopedCountSequence(0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 4, 1, [2, 3, 0]);
                        pump.bb = MakeRectangle(300f, 300f, 175f, 175f);
                        pump.initial_x = pump.x = (item3["x"].IntValue() * num) + num3 + num4;
                        pump.initial_y = pump.y = (item3["y"].IntValue() * num) + num2 + num5;
                        pump.initial_rotation = 0f;
                        pump.initial_rotatedCircle = null;
                        pump.rotation = item3["angle"].FloatValue() + 90f;
                        pump.UpdateRotation();
                        pump.anchor = 18;
                        _ = pumps.AddObject(pump);
                    }
                    else if (item3.Name == "sock")
                    {
                        Sock sock = Sock.Sock_createWithResID(85);
                        sock.CreateAnimations();
                        sock.scaleX = sock.scaleY = 0.7f;
                        sock.DoRestoreCutTransparency();
                        sock.x = (item3["x"].IntValue() * num) + num3 + num4;
                        sock.y = (item3["y"].IntValue() * num) + num2 + num5;
                        sock.group = item3["group"].IntValue();
                        sock.anchor = 10;
                        sock.rotationCenterY -= (sock.height / 2f) - 85f;
                        if (sock.group == 0)
                        {
                            sock.SetDrawQuad(0);
                        }
                        else
                        {
                            sock.SetDrawQuad(1);
                        }
                        sock.state = Sock.SOCK_IDLE;
                        sock.ParseMover(item3);
                        sock.rotation += 90f;
                        if (sock.mover != null)
                        {
                            sock.mover.angle_ += 90.0;
                            sock.mover.angle_initial = sock.mover.angle_;
                            if (cTRRootController.GetPack() == 3 && cTRRootController.GetLevel() == 24)
                            {
                                sock.mover.use_angle_initial = true;
                            }
                        }
                        sock.UpdateRotation();
                        _ = socks.AddObject(sock);
                    }
                    else if (item3.Name is "spike1" or "spike2" or "spike3" or "spike4" or "electro")
                    {
                        float px = (item3["x"].IntValue() * num) + num3 + num4;
                        float py = (item3["y"].IntValue() * num) + num2 + num5;
                        int w = item3["size"].IntValue();
                        double an = item3["angle"].IntValue();
                        NSString nSString2 = item3["toggled"];
                        int num8 = -1;
                        if (nSString2.Length() > 0)
                        {
                            num8 = nSString2.IsEqualToString("false") ? -1 : nSString2.IntValue();
                        }
                        Spikes spikes = (Spikes)new Spikes().InitWithPosXYWidthAndAngleToggled(px, py, w, an, num8);
                        spikes.ParseMover(item3);
                        if (num8 != 0)
                        {
                            spikes.delegateRotateAllSpikesWithID = new Spikes.rotateAllSpikesWithID(RotateAllSpikesWithID);
                        }
                        if (item3.Name == "electro")
                        {
                            spikes.electro = true;
                            spikes.initialDelay = item3["initialDelay"].FloatValue();
                            spikes.onTime = item3["onTime"].FloatValue();
                            spikes.offTime = item3["offTime"].FloatValue();
                            spikes.electroTimer = 0f;
                            spikes.TurnElectroOff();
                            spikes.electroTimer += spikes.initialDelay;
                            spikes.UpdateRotation();
                        }
                        else
                        {
                            spikes.electro = false;
                        }
                        _ = this.spikes.AddObject(spikes);
                    }
                    else if (item3.Name == "rotatedCircle")
                    {
                        float num9 = (item3["x"].IntValue() * num) + num3 + num4;
                        float num10 = (item3["y"].IntValue() * num) + num2 + num5;
                        float num11 = item3["size"].IntValue();
                        float d = item3["handleAngle"].IntValue();
                        bool hasOneHandle = item3["oneHandle"].BoolValue();
                        RotatedCircle rotatedCircle = (RotatedCircle)new RotatedCircle().Init();
                        rotatedCircle.anchor = 18;
                        rotatedCircle.x = num9;
                        rotatedCircle.y = num10;
                        rotatedCircle.rotation = d;
                        rotatedCircle.inithanlde1 = rotatedCircle.handle1 = Vect(rotatedCircle.x - (num11 * num), rotatedCircle.y);
                        rotatedCircle.inithanlde2 = rotatedCircle.handle2 = Vect(rotatedCircle.x + (num11 * num), rotatedCircle.y);
                        rotatedCircle.handle1 = VectRotateAround(rotatedCircle.handle1, (double)DEGREES_TO_RADIANS(d), rotatedCircle.x, rotatedCircle.y);
                        rotatedCircle.handle2 = VectRotateAround(rotatedCircle.handle2, (double)DEGREES_TO_RADIANS(d), rotatedCircle.x, rotatedCircle.y);
                        rotatedCircle.SetSize(num11);
                        rotatedCircle.SetHasOneHandle(hasOneHandle);
                        _ = rotatedCircles.AddObject(rotatedCircle);
                    }
                    else if (item3.Name is "bouncer1" or "bouncer2")
                    {
                        float px2 = (item3["x"].IntValue() * num) + num3 + num4;
                        float py2 = (item3["y"].IntValue() * num) + num2 + num5;
                        int w2 = item3["size"].IntValue();
                        double an2 = item3["angle"].IntValue();
                        Bouncer bouncer = (Bouncer)new Bouncer().InitWithPosXYWidthAndAngle(px2, py2, w2, an2);
                        bouncer.ParseMover(item3);
                        _ = bouncers.AddObject(bouncer);
                    }
                    else if (item3.Name == "grab")
                    {
                        float hx = (item3["x"].IntValue() * num) + num3 + num4;
                        float hy = (item3["y"].IntValue() * num) + num2 + num5;
                        float len = item3["length"].IntValue() * num;
                        float num12 = item3["radius"].FloatValue();
                        bool wheel = item3["wheel"].IsEqualToString("true");
                        float k = item3["moveLength"].FloatValue() * num;
                        bool v = item3["moveVertical"].IsEqualToString("true");
                        float o = item3["moveOffset"].FloatValue() * num;
                        bool spider = item3["spider"].IsEqualToString("true");
                        bool flag = item3["part"].IsEqualToString("L");
                        bool flag2 = item3["hidePath"].IsEqualToString("true");
                        Grab grab = (Grab)new Grab().Init();
                        grab.initial_x = grab.x = hx;
                        grab.initial_y = grab.y = hy;
                        grab.initial_rotation = 0f;
                        grab.wheel = wheel;
                        grab.SetSpider(spider);
                        grab.ParseMover(item3);
                        if (grab.mover != null)
                        {
                            grab.SetBee();
                            if (!flag2)
                            {
                                int num13 = 3;
                                bool flag3 = item3["path"].HasPrefix(NSS("R"));
                                for (int l = 0; l < grab.mover.pathLen - 1; l++)
                                {
                                    if (!flag3 || l % num13 == 0)
                                    {
                                        pollenDrawer.FillWithPolenFromPathIndexToPathIndexGrab(l, l + 1, grab);
                                    }
                                }
                                if (grab.mover.pathLen > 2)
                                {
                                    pollenDrawer.FillWithPolenFromPathIndexToPathIndexGrab(0, grab.mover.pathLen - 1, grab);
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
                            Bungee bungee = (Bungee)new Bungee().InitWithHeadAtXYTailAtTXTYandLength(null, hx, hy, constraintedPoint, constraintedPoint.pos.x, constraintedPoint.pos.y, len);
                            bungee.bungeeAnchor.pin = bungee.bungeeAnchor.pos;
                            grab.SetRope(bungee);
                        }
                        grab.SetRadius(num12);
                        grab.SetMoveLengthVerticalOffset(k, v, o);
                        _ = bungees.AddObject(grab);
                    }
                    else if (item3.Name == "target")
                    {
                        int pack = ((CTRRootController)Application.SharedRootController()).GetPack();
                        support = Image.Image_createWithResIDQuad(100, pack);
                        support.Retain();
                        support.DoRestoreCutTransparency();
                        support.anchor = 18;
                        target = CharAnimations.CharAnimations_createWithResID(80);
                        target.DoRestoreCutTransparency();
                        target.passColorToChilds = false;
                        NSString nSString3 = item3["x"];
                        target.x = support.x = (nSString3.IntValue() * num) + num3 + num4;
                        NSString nSString4 = item3["y"];
                        target.y = support.y = (nSString4.IntValue() * num) + num2 + num5;
                        target.AddImage(101);
                        target.AddImage(102);
                        target.bb = MakeRectangle(264.0, 350.0, 108.0, 2.0);
                        target.AddAnimationWithIDDelayLoopFirstLast(0, 0.05f, Timeline.LoopType.TIMELINE_REPLAY, 0, 18);
                        target.AddAnimationWithIDDelayLoopFirstLast(1, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 43, 67);
                        int num14 = 68;
                        target.AddAnimationWithIDDelayLoopCountSequence(2, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 32, num14,
                        [
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
                        ]);
                        target.AddAnimationWithIDDelayLoopFirstLast(7, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 19, 27);
                        target.AddAnimationWithIDDelayLoopFirstLast(8, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 28, 31);
                        target.AddAnimationWithIDDelayLoopFirstLast(9, 0.05f, Timeline.LoopType.TIMELINE_REPLAY, 32, 40);
                        target.AddAnimationWithIDDelayLoopFirstLast(6, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 28, 31);
                        target.AddAnimationWithIDDelayLoopFirstLast(101, 10, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 47, 76);
                        target.AddAnimationWithIDDelayLoopFirstLast(101, 3, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 0, 19);
                        target.AddAnimationWithIDDelayLoopFirstLast(101, 4, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 20, 46);
                        target.AddAnimationWithIDDelayLoopFirstLast(102, 5, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 0, 12);
                        target.SwitchToAnimationatEndOfAnimationDelay(9, 6, 0.05f);
                        target.SwitchToAnimationatEndOfAnimationDelay(101, 4, 80, 8, 0.05f);
                        target.SwitchToAnimationatEndOfAnimationDelay(80, 0, 101, 10, 0.05f);
                        target.SwitchToAnimationatEndOfAnimationDelay(80, 0, 80, 1, 0.05f);
                        target.SwitchToAnimationatEndOfAnimationDelay(80, 0, 80, 2, 0.05f);
                        target.SwitchToAnimationatEndOfAnimationDelay(80, 0, 101, 3, 0.05f);
                        target.SwitchToAnimationatEndOfAnimationDelay(80, 0, 101, 4, 0.05f);
                        target.Retain();
                        if (CTRRootController.IsShowGreeting())
                        {
                            dd.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(Selector_showGreeting), null, 1.3f);
                            CTRRootController.SetShowGreeting(false);
                        }
                        target.PlayTimeline(0);
                        target.GetTimeline(0).delegateTimelineDelegate = this;
                        target.SetPauseAtIndexforAnimation(8, 7);
                        blink = Animation.Animation_createWithResID(80);
                        blink.parentAnchor = 9;
                        blink.visible = false;
                        blink.AddAnimationWithIDDelayLoopCountSequence(0, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 4, 41, [41, 42, 42, 42]);
                        blink.SetActionTargetParamSubParamAtIndexforAnimation("ACTION_SET_VISIBLE", blink, 0, 0, 2, 0);
                        blinkTimer = 3;
                        blink.DoRestoreCutTransparency();
                        _ = target.AddChild(blink);
                        idlesTimer = RND_RANGE(5, 20);
                    }
                }
            }
            if (twoParts != 2)
            {
                candyBubbleAnimationL = Animation.Animation_createWithResID(72);
                candyBubbleAnimationL.parentAnchor = candyBubbleAnimationL.anchor = 18;
                _ = candyBubbleAnimationL.AddAnimationDelayLoopFirstLast(0.05, Timeline.LoopType.TIMELINE_REPLAY, 0, 12);
                candyBubbleAnimationL.PlayTimeline(0);
                _ = candyL.AddChild(candyBubbleAnimationL);
                candyBubbleAnimationL.visible = false;
                candyBubbleAnimationR = Animation.Animation_createWithResID(72);
                candyBubbleAnimationR.parentAnchor = candyBubbleAnimationR.anchor = 18;
                _ = candyBubbleAnimationR.AddAnimationDelayLoopFirstLast(0.05, Timeline.LoopType.TIMELINE_REPLAY, 0, 12);
                candyBubbleAnimationR.PlayTimeline(0);
                _ = candyR.AddChild(candyBubbleAnimationR);
                candyBubbleAnimationR.visible = false;
            }
            foreach (object obj in rotatedCircles)
            {
                RotatedCircle rotatedCircle2 = (RotatedCircle)obj;
                rotatedCircle2.operating = -1;
                rotatedCircle2.circlesArray = rotatedCircles;
            }
            StartCamera();
            tummyTeasers = 0;
            starsCollected = 0;
            candyBubble = null;
            candyBubbleL = null;
            candyBubbleR = null;
            mouthOpen = false;
            noCandy = twoParts != 2;
            noCandyL = false;
            noCandyR = false;
            blink.PlayTimeline(0);
            spiderTookCandy = false;
            time = 0f;
            score = 0;
            gravityNormal = true;
            MaterialPoint.globalGravity = Vect(0f, 784f);
            dimTime = 0f;
            ropesCutAtOnce = 0;
            ropeAtOnceTimer = 0f;
            dd.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(Selector_doCandyBlink), null, 1.0);
            Text text = Text.CreateWithFontandString(3, (cTRRootController.GetPack() + 1).ToString(CultureInfo.InvariantCulture) + " - " + (cTRRootController.GetLevel() + 1).ToString(CultureInfo.InvariantCulture));
            text.anchor = 33;
            Text text2 = Text.CreateWithFontandString(3, Application.GetString(655376));
            text2.anchor = 33;
            text2.parentAnchor = 9;
            text.SetName("levelLabel");
            text.x = 15f + Canvas.xOffsetScaled;
            text.y = SCREEN_HEIGHT + 15f;
            text2.y = 60f;
            text2.rotationCenterX -= text2.width / 2f;
            text2.scaleX = text2.scaleY = 0.7f;
            _ = text.AddChild(text2);
            Timeline timeline6 = new Timeline().InitWithMaxKeyFramesOnTrack(5);
            timeline6.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline6.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
            timeline6.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
            timeline6.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1.0));
            timeline6.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
            text.AddTimelinewithID(timeline6, 0);
            text.PlayTimeline(0);
            timeline6.delegateTimelineDelegate = staticAniPool;
            _ = staticAniPool.AddChild(text);
            for (int m = 0; m < 5; m++)
            {
                dragging[m] = false;
                startPos[m] = prevStartPos[m] = vectZero;
            }
            if (clickToCut)
            {
                ResetBungeeHighlight();
            }
            Global.MouseCursor.ReleaseButtons();
            CTRRootController.LogEvent("IG_SHOWN");
        }

        public void StartCamera()
        {
            if (mapWidth > SCREEN_WIDTH || mapHeight > SCREEN_HEIGHT)
            {
                ignoreTouches = true;
                fastenCamera = false;
                camera.type = CAMERATYPE.CAMERASPEEDPIXELS;
                camera.speed = 20f;
                cameraMoveMode = 0;
                ConstraintedPoint constraintedPoint = twoParts != 2 ? starL : star;
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
                double num6 = (double)(constraintedPoint.pos.x - (SCREEN_WIDTH / 2f));
                float num3 = constraintedPoint.pos.y - (SCREEN_HEIGHT / 2f);
                float num4 = FIT_TO_BOUNDARIES(num6, 0.0, (double)(mapWidth - SCREEN_WIDTH));
                float num5 = FIT_TO_BOUNDARIES((double)num3, 0.0, (double)(mapHeight - SCREEN_HEIGHT));
                camera.MoveToXYImmediate(num, num2, true);
                initialCameraToStarDistance = VectDistance(camera.pos, Vect(num4, num5));
                return;
            }
            ignoreTouches = false;
            camera.MoveToXYImmediate(0f, 0f, true);
        }

        public void DoCandyBlink()
        {
            candyBlink.PlayTimeline(0);
        }

        public void TimelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
        {
            if (rotatedCircles.GetObjectIndex(t.element) != -1 || i != 1)
            {
                return;
            }
            blinkTimer--;
            if (blinkTimer == 0)
            {
                blink.visible = true;
                blink.PlayTimeline(0);
                blinkTimer = 3;
            }
            idlesTimer--;
            if (idlesTimer == 0)
            {
                if (RND_RANGE(0, 1) == 1)
                {
                    target.PlayTimeline(1);
                }
                else
                {
                    target.PlayTimeline(2);
                }
                idlesTimer = RND_RANGE(5, 20);
            }
        }

        public void TimelineFinished(Timeline t)
        {
            if (rotatedCircles.GetObjectIndex(t.element) != -1)
            {
                ((RotatedCircle)t.element).removeOnNextUpdate = true;
            }
            foreach (object obj in tutorials)
            {
            }
        }
    }
}
