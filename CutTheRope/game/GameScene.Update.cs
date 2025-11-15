using System;

using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.sfe;
using CutTheRope.iframework.visual;

namespace CutTheRope.game
{
    internal sealed partial class GameScene : BaseElement, ITimelineDelegate, IButtonDelegation
    {
        public override void Update(float delta)
        {
            delta = 0.016f;
            base.Update(delta);
            dd.Update(delta);
            pollenDrawer.Update(delta);
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < fingerCuts[i].Count; j++)
                {
                    FingerCut fingerCut = fingerCuts[i].ObjectAtIndex(j);
                    float alpha = fingerCut.c.a;
                    if (Mover.MoveVariableToTarget(ref alpha, 0.0f, 10.0f, (float)delta))
                    {
                        fingerCuts[i].RemoveObject(fingerCut);
                        j--;
                    }
                    else
                    {
                        fingerCut.c.a = alpha;
                    }
                }
            }
            if (earthAnims != null)
            {
                foreach (object obj in earthAnims)
                {
                    ((Image)obj).Update(delta);
                }
            }
            _ = Mover.MoveVariableToTarget(ref ropeAtOnceTimer, 0.0, 1.0, (double)delta);
            ConstraintedPoint constraintedPoint4 = twoParts != 2 ? starL : star;
            float num = constraintedPoint4.pos.x - (SCREEN_WIDTH / 2f);
            double num19 = (double)(constraintedPoint4.pos.y - (SCREEN_HEIGHT / 2f));
            float num2 = FIT_TO_BOUNDARIES((double)num, 0.0, (double)(mapWidth - SCREEN_WIDTH));
            float num3 = FIT_TO_BOUNDARIES(num19, 0.0, (double)(mapHeight - SCREEN_HEIGHT));
            camera.MoveToXYImmediate(num2, num3, false);
            if (!freezeCamera || camera.type != CAMERATYPE.CAMERASPEEDDELAY)
            {
                camera.Update(delta);
            }
            if (camera.type == CAMERATYPE.CAMERASPEEDPIXELS)
            {
                float num4 = 100f;
                float num5 = 800f;
                float num6 = 400f;
                float a = 1000f;
                float a2 = 300f;
                float num7 = VectDistance(camera.pos, Vect(num2, num3));
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
                    camera.type = CAMERATYPE.CAMERASPEEDDELAY;
                    camera.speed = 14f;
                }
            }
            else
            {
                time += delta;
            }
            if (bungees.Count > 0)
            {
                bool flag = false;
                bool flag2 = false;
                bool flag3 = false;
                int num8 = bungees.Count;
                int k = 0;
                while (k < num8)
                {
                    Grab grab = bungees.ObjectAtIndex(k);
                    grab.Update(delta);
                    Bungee rope = grab.rope;
                    if (grab.mover != null)
                    {
                        if (grab.rope != null)
                        {
                            grab.rope.bungeeAnchor.pos = Vect(grab.x, grab.y);
                            grab.rope.bungeeAnchor.pin = grab.rope.bungeeAnchor.pos;
                        }
                        if (grab.radius != -1f)
                        {
                            grab.ReCalcCircle();
                        }
                    }
                    if (rope == null)
                    {
                        goto IL_0478;
                    }
                    if (rope.cut == -1 || rope.cutTime != 0.0)
                    {
                        rope?.Update(delta * ropePhysicsSpeed);
                        if (!grab.hasSpider)
                        {
                            goto IL_0478;
                        }
                        if (camera.type != CAMERATYPE.CAMERASPEEDPIXELS || !ignoreTouches)
                        {
                            grab.UpdateSpider(delta);
                        }
                        if (grab.spiderPos == -1f)
                        {
                            SpiderWon(grab);
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
                            if (!noCandyL && VectDistance(Vect(grab.x, grab.y), starL.pos) <= grab.radius + 42f)
                            {
                                Bungee bungee = new Bungee().InitWithHeadAtXYTailAtTXTYandLength(null, grab.x, grab.y, starL, starL.pos.x, starL.pos.y, grab.radius + 42f);
                                bungee.bungeeAnchor.pin = bungee.bungeeAnchor.pos;
                                grab.hideRadius = true;
                                grab.SetRope(bungee);
                                CTRSoundMgr.PlaySound(24);
                                if (grab.mover != null)
                                {
                                    CTRSoundMgr.PlaySound(44);
                                }
                            }
                            if (!noCandyR && grab.rope == null && VectDistance(Vect(grab.x, grab.y), starR.pos) <= grab.radius + 42f)
                            {
                                Bungee bungee2 = new Bungee().InitWithHeadAtXYTailAtTXTYandLength(null, grab.x, grab.y, starR, starR.pos.x, starR.pos.y, grab.radius + 42f);
                                bungee2.bungeeAnchor.pin = bungee2.bungeeAnchor.pos;
                                grab.hideRadius = true;
                                grab.SetRope(bungee2);
                                CTRSoundMgr.PlaySound(24);
                                if (grab.mover != null)
                                {
                                    CTRSoundMgr.PlaySound(44);
                                }
                            }
                        }
                        else if (VectDistance(Vect(grab.x, grab.y), star.pos) <= grab.radius + 42f)
                        {
                            Bungee bungee3 = new Bungee().InitWithHeadAtXYTailAtTXTYandLength(null, grab.x, grab.y, star, star.pos.x, star.pos.y, grab.radius + 42f);
                            bungee3.bungeeAnchor.pin = bungee3.bungeeAnchor.pos;
                            grab.hideRadius = true;
                            grab.SetRope(bungee3);
                            CTRSoundMgr.PlaySound(24);
                            if (grab.mover != null)
                            {
                                CTRSoundMgr.PlaySound(44);
                            }
                        }
                    }
                    if (rope == null)
                    {
                        goto IL_08D4;
                    }
                    MaterialPoint bungeeAnchor = rope.bungeeAnchor;
                    ConstraintedPoint constraintedPoint2 = rope.parts[^1];
                    Vector v = VectSub(bungeeAnchor.pos, constraintedPoint2.pos);
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
                        float num9 = RADIANS_TO_DEGREES(VectAngleNormalized(v));
                        if (twoParts != 2)
                        {
                            GameObject gameObject = constraintedPoint2 == starL ? candyL : candyR;
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
                star.Update(delta * ropePhysicsSpeed);
                candy.x = star.pos.x;
                candy.y = star.pos.y;
                candy.Update(delta);
                CalculateTopLeft(candy);
            }
            if (twoParts != 2)
            {
                candyL.Update(delta);
                starL.Update(delta * ropePhysicsSpeed);
                candyR.Update(delta);
                starR.Update(delta * ropePhysicsSpeed);
                if (twoParts == 1)
                {
                    for (int l = 0; l < 30; l++)
                    {
                        ConstraintedPoint.SatisfyConstraints(starL);
                        ConstraintedPoint.SatisfyConstraints(starR);
                    }
                }
                if (partsDist > 0.0)
                {
                    if (Mover.MoveVariableToTarget(ref partsDist, 0.0, 200.0, (double)delta))
                    {
                        CTRSoundMgr.PlaySound(40);
                        twoParts = 2;
                        noCandy = false;
                        noCandyL = true;
                        noCandyR = true;
                        int num20 = Preferences.GetIntForKey("PREFS_CANDIES_UNITED") + 1;
                        Preferences.SetIntForKey(num20, "PREFS_CANDIES_UNITED", false);
                        if (num20 == 100)
                        {
                            CTRRootController.PostAchievementName("1432722351", ACHIEVEMENT_STRING("\"Romantic Soul\""));
                        }
                        if (candyBubbleL != null || candyBubbleR != null)
                        {
                            candyBubble = candyBubbleL ?? candyBubbleR;
                            candyBubbleAnimation.visible = true;
                        }
                        lastCandyRotateDelta = 0f;
                        lastCandyRotateDeltaL = 0f;
                        lastCandyRotateDeltaR = 0f;
                        star.pos.x = starL.pos.x;
                        star.pos.y = starL.pos.y;
                        candy.x = star.pos.x;
                        candy.y = star.pos.y;
                        CalculateTopLeft(candy);
                        Vector vector = VectSub(starL.pos, starL.prevPos);
                        Vector vector2 = VectSub(starR.pos, starR.prevPos);
                        Vector v2 = Vect((vector.x + vector2.x) / 2f, (vector.y + vector2.y) / 2f);
                        star.prevPos = VectSub(star.pos, v2);
                        int num10 = bungees.Count;
                        for (int m = 0; m < num10; m++)
                        {
                            Bungee rope2 = bungees.ObjectAtIndex(m).rope;
                            if (rope2 != null && rope2.cut != rope2.parts.Count - 3 && (rope2.tail == starL || rope2.tail == starR))
                            {
                                ConstraintedPoint constraintedPoint3 = rope2.parts[^2];
                                int num11 = (int)rope2.tail.RestLengthFor(constraintedPoint3);
                                star.AddConstraintwithRestLengthofType(constraintedPoint3, num11, Constraint.CONSTRAINT.DISTANCE);
                                rope2.tail = star;
                                rope2.parts[^1] = star;
                                rope2.initialCandleAngle = 0f;
                                rope2.chosenOne = false;
                            }
                        }
                        Animation animation = Animation.Animation_createWithResID(63);
                        animation.DoRestoreCutTransparency();
                        animation.x = candy.x;
                        animation.y = candy.y;
                        animation.anchor = 18;
                        int n = animation.AddAnimationDelayLoopFirstLast(0.05, Timeline.LoopType.TIMELINE_NO_LOOP, 21, 25);
                        animation.GetTimeline(n).delegateTimelineDelegate = aniPool;
                        animation.PlayTimeline(0);
                        _ = aniPool.AddChild(animation);
                    }
                    else
                    {
                        starL.ChangeRestLengthToFor(partsDist, starR);
                        starR.ChangeRestLengthToFor(partsDist, starL);
                    }
                }
                if (!noCandyL && !noCandyR && GameObject.ObjectsIntersect(candyL, candyR) && twoParts == 0)
                {
                    twoParts = 1;
                    partsDist = VectDistance(starL.pos, starR.pos);
                    starL.AddConstraintwithRestLengthofType(starR, partsDist, Constraint.CONSTRAINT.NOT_MORE_THAN);
                    starR.AddConstraintwithRestLengthofType(starL, partsDist, Constraint.CONSTRAINT.NOT_MORE_THAN);
                }
            }
            target.Update(delta);
            if (camera.type != CAMERATYPE.CAMERASPEEDPIXELS || !ignoreTouches)
            {
                foreach (object obj2 in stars)
                {
                    Star star = (Star)obj2;
                    star.Update(delta);
                    if (star.timeout > 0.0 && star.time == 0.0)
                    {
                        star.GetTimeline(1).delegateTimelineDelegate = aniPool;
                        _ = aniPool.AddChild(star);
                        stars.RemoveObject(star);
                        star.timedAnim.PlayTimeline(1);
                        star.PlayTimeline(1);
                        break;
                    }
                    if (twoParts == 2 ? GameObject.ObjectsIntersect(candy, star) && !noCandy : (GameObject.ObjectsIntersect(candyL, star) && !noCandyL) || (GameObject.ObjectsIntersect(candyR, star) && !noCandyR))
                    {
                        candyBlink.PlayTimeline(1);
                        starsCollected++;
                        hudStar[starsCollected - 1].PlayTimeline(0);
                        Animation animation2 = Animation.Animation_createWithResID(71);
                        animation2.DoRestoreCutTransparency();
                        animation2.x = star.x;
                        animation2.y = star.y;
                        animation2.anchor = 18;
                        int n2 = animation2.AddAnimationDelayLoopFirstLast(0.05, Timeline.LoopType.TIMELINE_NO_LOOP, 0, 12);
                        animation2.GetTimeline(n2).delegateTimelineDelegate = aniPool;
                        animation2.PlayTimeline(0);
                        _ = aniPool.AddChild(animation2);
                        stars.RemoveObject(star);
                        CTRSoundMgr.PlaySound(25 + starsCollected - 1);
                        if (target.GetCurrentTimelineIndex() == 0)
                        {
                            target.PlayAnimationtimeline(101, 3);
                            break;
                        }
                        break;
                    }
                }
            }
            foreach (object obj3 in bubbles)
            {
                Bubble bubble3 = (Bubble)obj3;
                bubble3.Update(delta);
                float num12 = 85f;
                if (twoParts != 2)
                {
                    if (!noCandyL && !bubble3.popped && PointInRect(candyL.x, candyL.y, bubble3.x - num12, bubble3.y - num12, num12 * 2f, num12 * 2f))
                    {
                        if (candyBubbleL != null)
                        {
                            PopBubbleAtXY(bubble3.x, bubble3.y);
                        }
                        candyBubbleL = bubble3;
                        candyBubbleAnimationL.visible = true;
                        CTRSoundMgr.PlaySound(13);
                        bubble3.popped = true;
                        bubble3.RemoveChildWithID(0);
                        break;
                    }
                    if (!noCandyR && !bubble3.popped && PointInRect(candyR.x, candyR.y, bubble3.x - num12, bubble3.y - num12, num12 * 2f, num12 * 2f))
                    {
                        if (candyBubbleR != null)
                        {
                            PopBubbleAtXY(bubble3.x, bubble3.y);
                        }
                        candyBubbleR = bubble3;
                        candyBubbleAnimationR.visible = true;
                        CTRSoundMgr.PlaySound(13);
                        bubble3.popped = true;
                        bubble3.RemoveChildWithID(0);
                        break;
                    }
                }
                else if (!noCandy && !bubble3.popped && PointInRect(candy.x, candy.y, bubble3.x - num12, bubble3.y - num12, num12 * 2f, num12 * 2f))
                {
                    if (candyBubble != null)
                    {
                        PopBubbleAtXY(bubble3.x, bubble3.y);
                    }
                    candyBubble = bubble3;
                    candyBubbleAnimation.visible = true;
                    CTRSoundMgr.PlaySound(13);
                    bubble3.popped = true;
                    bubble3.RemoveChildWithID(0);
                    break;
                }
                if (!bubble3.withoutShadow)
                {
                    foreach (object obj4 in rotatedCircles)
                    {
                        RotatedCircle rotatedCircle5 = (RotatedCircle)obj4;
                        if (VectDistance(Vect(bubble3.x, bubble3.y), Vect(rotatedCircle5.x, rotatedCircle5.y)) < rotatedCircle5.sizeInPixels)
                        {
                            bubble3.withoutShadow = true;
                        }
                    }
                }
            }
            foreach (object obj5 in tutorials)
            {
                ((Text)obj5).Update(delta);
            }
            foreach (object obj6 in tutorialImages)
            {
                ((GameObject)obj6).Update(delta);
            }
            foreach (object obj7 in pumps)
            {
                Pump pump = (Pump)obj7;
                pump.Update(delta);
                if (Mover.MoveVariableToTarget(ref pump.pumpTouchTimer, 0.0, 1.0, (double)delta))
                {
                    OperatePump(pump);
                }
            }
            RotatedCircle rotatedCircle6 = null;
            foreach (object obj8 in rotatedCircles)
            {
                RotatedCircle rotatedCircle7 = (RotatedCircle)obj8;
                foreach (object obj9 in bungees)
                {
                    Grab bungee4 = (Grab)obj9;
                    if (VectDistance(Vect(bungee4.x, bungee4.y), Vect(rotatedCircle7.x, rotatedCircle7.y)) <= rotatedCircle7.sizeInPixels + (RTPD(5.0) * 3f))
                    {
                        if (rotatedCircle7.containedObjects.GetObjectIndex(bungee4) == -1)
                        {
                            _ = rotatedCircle7.containedObjects.AddObject(bungee4);
                        }
                    }
                    else if (rotatedCircle7.containedObjects.GetObjectIndex(bungee4) != -1)
                    {
                        rotatedCircle7.containedObjects.RemoveObject(bungee4);
                    }
                }
                foreach (object obj10 in bubbles)
                {
                    Bubble bubble4 = (Bubble)obj10;
                    if (VectDistance(Vect(bubble4.x, bubble4.y), Vect(rotatedCircle7.x, rotatedCircle7.y)) <= rotatedCircle7.sizeInPixels + (RTPD(10.0) * 3f))
                    {
                        if (rotatedCircle7.containedObjects.GetObjectIndex(bubble4) == -1)
                        {
                            _ = rotatedCircle7.containedObjects.AddObject(bubble4);
                        }
                    }
                    else if (rotatedCircle7.containedObjects.GetObjectIndex(bubble4) != -1)
                    {
                        rotatedCircle7.containedObjects.RemoveObject(bubble4);
                    }
                }
                if (rotatedCircle7.removeOnNextUpdate)
                {
                    rotatedCircle6 = rotatedCircle7;
                }
                rotatedCircle7.Update(delta);
            }
            if (rotatedCircle6 != null)
            {
                rotatedCircles.RemoveObject(rotatedCircle6);
            }
            float num13 = RTPD(20.0);
            foreach (object obj11 in socks)
            {
                Sock sock3 = (Sock)obj11;
                sock3.Update(delta);
                if (Mover.MoveVariableToTarget(ref sock3.idleTimeout, 0.0, 1.0, (double)delta))
                {
                    sock3.state = Sock.SOCK_IDLE;
                }
                float num14 = sock3.rotation;
                sock3.rotation = 0f;
                sock3.UpdateRotation();
                Vector ptr = VectRotate(star.posDelta, (double)DEGREES_TO_RADIANS(0f - num14));
                sock3.rotation = num14;
                sock3.UpdateRotation();
                if (ptr.y >= 0.0 && (LineInRect(sock3.t1.x, sock3.t1.y, sock3.t2.x, sock3.t2.y, star.pos.x - num13, star.pos.y - num13, num13 * 2f, num13 * 2f) || LineInRect(sock3.b1.x, sock3.b1.y, sock3.b2.x, sock3.b2.y, star.pos.x - num13, star.pos.y - num13, num13 * 2f, num13 * 2f)))
                {
                    if (sock3.state != Sock.SOCK_IDLE)
                    {
                        continue;
                    }

                    foreach (Sock sock4 in socks)
                    {
                        if (sock4 != sock3 && sock4.group == sock3.group)
                        {
                            sock3.state = Sock.SOCK_RECEIVING;
                            sock4.state = Sock.SOCK_THROWING;
                            ReleaseAllRopes(false);
                            savedSockSpeed = 0.9f * VectLength(star.v);
                            savedSockSpeed *= 1.4f;
                            targetSock = sock4;
                            sock3.light.PlayTimeline(0);
                            sock3.light.visible = true;
                            CTRSoundMgr.PlaySound(45);
                            dd.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(Selector_teleport), null, 0.1);
                            break;
                        }
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
                razor.Update(delta);
                _ = CutWithRazorOrLine1Line2Immediate(razor, vectZero, vectZero, false);
            }
            foreach (object obj14 in spikes)
            {
                Spikes spike = (Spikes)obj14;
                spike.Update(delta);
                float num15 = 15f;
                if (!spike.electro || (spike.electro && spike.electroOn))
                {
                    bool flag5 = false;
                    bool flag6;
                    if (twoParts != 2)
                    {
                        flag6 = (LineInRect(spike.t1.x, spike.t1.y, spike.t2.x, spike.t2.y, starL.pos.x - num15, starL.pos.y - num15, num15 * 2f, num15 * 2f) || LineInRect(spike.b1.x, spike.b1.y, spike.b2.x, spike.b2.y, starL.pos.x - num15, starL.pos.y - num15, num15 * 2f, num15 * 2f)) && !noCandyL;
                        if (flag6)
                        {
                            flag5 = true;
                        }
                        else
                        {
                            flag6 = (LineInRect(spike.t1.x, spike.t1.y, spike.t2.x, spike.t2.y, starR.pos.x - num15, starR.pos.y - num15, num15 * 2f, num15 * 2f) || LineInRect(spike.b1.x, spike.b1.y, spike.b2.x, spike.b2.y, starR.pos.x - num15, starR.pos.y - num15, num15 * 2f, num15 * 2f)) && !noCandyR;
                        }
                    }
                    else
                    {
                        flag6 = (LineInRect(spike.t1.x, spike.t1.y, spike.t2.x, spike.t2.y, star.pos.x - num15, star.pos.y - num15, num15 * 2f, num15 * 2f) || LineInRect(spike.b1.x, spike.b1.y, spike.b2.x, spike.b2.y, star.pos.x - num15, star.pos.y - num15, num15 * 2f, num15 * 2f)) && !noCandy;
                    }
                    if (flag6)
                    {
                        if (twoParts != 2)
                        {
                            if (flag5)
                            {
                                if (candyBubbleL != null)
                                {
                                    PopCandyBubble(true);
                                }
                            }
                            else if (candyBubbleR != null)
                            {
                                PopCandyBubble(false);
                            }
                        }
                        else if (candyBubble != null)
                        {
                            PopCandyBubble(false);
                        }
                        Image image2 = Image.Image_createWithResID(63);
                        image2.DoRestoreCutTransparency();
                        CandyBreak candyBreak = (CandyBreak)new CandyBreak().InitWithTotalParticlesandImageGrid(5, image2);
                        if (gravityButton != null && !gravityNormal)
                        {
                            candyBreak.gravity.y = -500f;
                            candyBreak.angle = 90f;
                        }
                        candyBreak.particlesDelegate = new Particles.ParticlesFinished(aniPool.ParticlesFinished);
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
                        candyBreak.StartSystem(5);
                        _ = aniPool.AddChild(candyBreak);
                        CTRSoundMgr.PlaySound(14);
                        ReleaseAllRopes(flag5);
                        if (restartState != 0 && (twoParts == 2 || !noCandyL || !noCandyR))
                        {
                            dd.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(Selector_gameLost), null, 0.3);
                        }
                        return;
                    }
                }
            }
            foreach (object obj15 in bouncers)
            {
                Bouncer bouncer = (Bouncer)obj15;
                bouncer.Update(delta);
                float num16 = 40f;
                bool flag7 = false;
                bool flag8;
                if (twoParts != 2)
                {
                    flag8 = (LineInRect(bouncer.t1.x, bouncer.t1.y, bouncer.t2.x, bouncer.t2.y, starL.pos.x - num16, starL.pos.y - num16, num16 * 2f, num16 * 2f) || LineInRect(bouncer.b1.x, bouncer.b1.y, bouncer.b2.x, bouncer.b2.y, starL.pos.x - num16, starL.pos.y - num16, num16 * 2f, num16 * 2f)) && !noCandyL;
                    if (flag8)
                    {
                        flag7 = true;
                    }
                    else
                    {
                        flag8 = (LineInRect(bouncer.t1.x, bouncer.t1.y, bouncer.t2.x, bouncer.t2.y, starR.pos.x - num16, starR.pos.y - num16, num16 * 2f, num16 * 2f) || LineInRect(bouncer.b1.x, bouncer.b1.y, bouncer.b2.x, bouncer.b2.y, starR.pos.x - num16, starR.pos.y - num16, num16 * 2f, num16 * 2f)) && !noCandyR;
                    }
                }
                else
                {
                    flag8 = (LineInRect(bouncer.t1.x, bouncer.t1.y, bouncer.t2.x, bouncer.t2.y, star.pos.x - num16, star.pos.y - num16, num16 * 2f, num16 * 2f) || LineInRect(bouncer.b1.x, bouncer.b1.y, bouncer.b2.x, bouncer.b2.y, star.pos.x - num16, star.pos.y - num16, num16 * 2f, num16 * 2f)) && !noCandy;
                }
                if (flag8)
                {
                    if (twoParts != 2)
                    {
                        if (flag7)
                        {
                            HandleBouncePtDelta(bouncer, starL, delta);
                        }
                        else
                        {
                            HandleBouncePtDelta(bouncer, starR, delta);
                        }
                    }
                    else
                    {
                        HandleBouncePtDelta(bouncer, star, delta);
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
                        starL.ApplyImpulseDelta(Vect((0f - starL.v.x) / num18, ((0f - starL.v.y) / num18) - num17), delta);
                    }
                    else
                    {
                        starL.ApplyImpulseDelta(Vect((0f - starL.v.x) / num18, ((0f - starL.v.y) / num18) + num17), delta);
                    }
                }
                if (candyBubbleR != null)
                {
                    if (gravityButton != null && !gravityNormal)
                    {
                        starR.ApplyImpulseDelta(Vect((0f - starR.v.x) / num18, ((0f - starR.v.y) / num18) - num17), delta);
                    }
                    else
                    {
                        starR.ApplyImpulseDelta(Vect((0f - starR.v.x) / num18, ((0f - starR.v.y) / num18) + num17), delta);
                    }
                }
            }
            if (twoParts == 1)
            {
                if (candyBubbleR != null || candyBubbleL != null)
                {
                    if (gravityButton != null && !gravityNormal)
                    {
                        starL.ApplyImpulseDelta(Vect((0f - starL.v.x) / num18, ((0f - starL.v.y) / num18) - num17), delta);
                        starR.ApplyImpulseDelta(Vect((0f - starR.v.x) / num18, ((0f - starR.v.y) / num18) - num17), delta);
                    }
                    else
                    {
                        starL.ApplyImpulseDelta(Vect((0f - starL.v.x) / num18, ((0f - starL.v.y) / num18) + num17), delta);
                        starR.ApplyImpulseDelta(Vect((0f - starR.v.x) / num18, ((0f - starR.v.y) / num18) + num17), delta);
                    }
                }
            }
            else if (candyBubble != null)
            {
                if (gravityButton != null && !gravityNormal)
                {
                    star.ApplyImpulseDelta(Vect((0f - star.v.x) / num18, ((0f - star.v.y) / num18) - num17), delta);
                }
                else
                {
                    star.ApplyImpulseDelta(Vect((0f - star.v.x) / num18, ((0f - star.v.y) / num18) + num17), delta);
                }
            }
            if (!noCandy)
            {
                if (!mouthOpen)
                {
                    if (VectDistance(star.pos, Vect(target.x, target.y)) < 200f)
                    {
                        mouthOpen = true;
                        target.PlayTimeline(7);
                        CTRSoundMgr.PlaySound(17);
                        mouthCloseTimer = 1f;
                    }
                }
                else if (mouthCloseTimer > 0.0)
                {
                    _ = Mover.MoveVariableToTarget(ref mouthCloseTimer, 0.0, 1.0, (double)delta);
                    if (mouthCloseTimer <= 0.0)
                    {
                        if (VectDistance(star.pos, Vect(target.x, target.y)) > 200f)
                        {
                            mouthOpen = false;
                            target.PlayTimeline(8);
                            CTRSoundMgr.PlaySound(16);
                            tummyTeasers++;
                            if (tummyTeasers >= 10)
                            {
                                CTRRootController.PostAchievementName("1058281905", ACHIEVEMENT_STRING("\"Tummy Teaser\""));
                            }
                        }
                        else
                        {
                            mouthCloseTimer = 1f;
                        }
                    }
                }
                if (restartState != 0 && GameObject.ObjectsIntersect(candy, target))
                {
                    GameWon();
                    return;
                }
            }
            bool flag9 = twoParts == 2 && PointOutOfScreen(star) && !noCandy;
            bool flag10 = twoParts != 2 && PointOutOfScreen(starL) && !noCandyL;
            bool flag11 = twoParts != 2 && PointOutOfScreen(starR) && !noCandyR;
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
                    int num21 = Preferences.GetIntForKey("PREFS_CANDIES_LOST") + 1;
                    Preferences.SetIntForKey(num21, "PREFS_CANDIES_LOST", false);
                    if (num21 == 50)
                    {
                        CTRRootController.PostAchievementName("681497443", ACHIEVEMENT_STRING("\"Weight Loser\""));
                    }
                    if (num21 == 200)
                    {
                        CTRRootController.PostAchievementName("1058341297", ACHIEVEMENT_STRING("\"Calorie Minimizer\""));
                    }
                    if (twoParts == 2 || !noCandyL || !noCandyR)
                    {
                        GameLost();
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
                        tutorial2.PlayTimeline(0);
                    }
                }
                foreach (object obj17 in tutorialImages)
                {
                    GameObjectSpecial tutorialImage2 = (GameObjectSpecial)obj17;
                    if (tutorialImage2.special == 1)
                    {
                        tutorialImage2.PlayTimeline(0);
                    }
                }
            }
            if (clickToCut && !ignoreTouches)
            {
                ResetBungeeHighlight();
                bool flag12 = false;
                Vector p = VectAdd(slastTouch, camera.pos);
                if (gravityButton != null && ((Button)gravityButton.GetChild(gravityButton.On() ? 1 : 0)).IsInTouchZoneXYforTouchDown(p.x, p.y, true))
                {
                    flag12 = true;
                }
                if (candyBubble != null || (twoParts != 2 && (candyBubbleL != null || candyBubbleR != null)))
                {
                    foreach (object obj18 in bubbles)
                    {
                        Bubble bubble5 = (Bubble)obj18;
                        if (candyBubble != null && PointInRect(p.x, p.y, star.pos.x - 60f, star.pos.y - 60f, 120f, 120f))
                        {
                            flag12 = true;
                            break;
                        }
                        if (candyBubbleL != null && PointInRect(p.x, p.y, starL.pos.x - 60f, starL.pos.y - 60f, 120f, 120f))
                        {
                            flag12 = true;
                            break;
                        }
                        if (candyBubbleR != null && PointInRect(p.x, p.y, starR.pos.x - 60f, starR.pos.y - 60f, 120f, 120f))
                        {
                            flag12 = true;
                            break;
                        }
                    }
                }
                foreach (object obj19 in spikes)
                {
                    Spikes spike2 = (Spikes)obj19;
                    if (spike2.rotateButton != null && spike2.rotateButton.IsInTouchZoneXYforTouchDown(p.x, p.y, true))
                    {
                        flag12 = true;
                    }
                }
                foreach (object obj20 in pumps)
                {
                    Pump pump2 = (Pump)obj20;
                    if (GameObject.PointInObject(p, pump2))
                    {
                        flag12 = true;
                        break;
                    }
                }
                foreach (object obj21 in rotatedCircles)
                {
                    RotatedCircle rotatedCircle8 = (RotatedCircle)obj21;
                    if (rotatedCircle8.IsLeftControllerActive() || rotatedCircle8.IsRightControllerActive())
                    {
                        flag12 = true;
                        break;
                    }
                    if (VectDistance(Vect(p.x, p.y), Vect(rotatedCircle8.handle1.x, rotatedCircle8.handle1.y)) <= 90f || VectDistance(Vect(p.x, p.y), Vect(rotatedCircle8.handle2.x, rotatedCircle8.handle2.y)) <= 90f)
                    {
                        flag12 = true;
                        break;
                    }
                }
                foreach (object obj22 in bungees)
                {
                    Grab bungee5 = (Grab)obj22;
                    if (bungee5.wheel && PointInRect(p.x, p.y, bungee5.x - 110f, bungee5.y - 110f, 220f, 220f))
                    {
                        flag12 = true;
                        break;
                    }
                    if (bungee5.moveLength > 0.0 && (PointInRect(p.x, p.y, bungee5.x - 65f, bungee5.y - 65f, 130f, 130f) || bungee5.moverDragging != -1))
                    {
                        flag12 = true;
                        break;
                    }
                }
                if (!flag12)
                {
                    Vector s = default;
                    Grab grab2 = null;
                    Bungee nearestBungeeSegmentByBeziersPointsatXYgrab = GetNearestBungeeSegmentByBeziersPointsatXYgrab(ref s, slastTouch.x + camera.pos.x, slastTouch.y + camera.pos.y, ref grab2);
                    if (nearestBungeeSegmentByBeziersPointsatXYgrab != null)
                    {
                        nearestBungeeSegmentByBeziersPointsatXYgrab.highlighted = true;
                    }
                }
            }
            if (Mover.MoveVariableToTarget(ref dimTime, 0.0, 1.0, (double)delta))
            {
                if (restartState == 0)
                {
                    restartState = 1;
                    Hide();
                    Show();
                    dimTime = 0.15f;
                    return;
                }
                restartState = -1;
            }
        }
    }
}
