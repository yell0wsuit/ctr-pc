using System;

using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Sfe;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    internal sealed partial class GameScene
    {
        public bool HandleBubbleTouchXY(ConstraintedPoint s, float tx, float ty)
        {
            if (PointInRect(tx + camera.pos.X, ty + camera.pos.Y, s.pos.X - 60f, s.pos.Y - 60f, 120f, 120f))
            {
                PopCandyBubble(s == starL);
                int num = Preferences.GetIntForKey("PREFS_BUBBLES_POPPED") + 1;
                Preferences.SetIntForKey(num, "PREFS_BUBBLES_POPPED", false);
                if (num == 50)
                {
                    CTRRootController.PostAchievementName("681513183", ACHIEVEMENT_STRING("\"Bubble Popper\""));
                }
                if (num == 300)
                {
                    CTRRootController.PostAchievementName("1058345234", ACHIEVEMENT_STRING("\"Bubble Master\""));
                }
                return true;
            }
            return false;
        }

        private bool HandleLightBulbBubbleTouch(LightBulb bulb, float tx, float ty)
        {
            if (bulb?.capturingBubble == null)
            {
                return false;
            }
            if (PointInRect(tx + camera.pos.X, ty + camera.pos.Y, bulb.constraint.pos.X - BUBBLE_RADIUS, bulb.constraint.pos.Y - BUBBLE_RADIUS, BUBBLE_RADIUS * 2f, BUBBLE_RADIUS * 2f))
            {
                PopLightBulbBubble(bulb);
                return true;
            }
            return false;
        }

        public bool TouchDownXYIndex(float tx, float ty, int ti)
        {
            if (ignoreTouches)
            {
                if (camera.type == CAMERATYPE.CAMERASPEEDPIXELS)
                {
                    fastenCamera = true;
                }
                return true;
            }
            if (ti >= 5)
            {
                return true;
            }
            if (gravityButton != null && ((Button)gravityButton.GetChild(gravityButton.On() ? 1 : 0)).IsInTouchZoneXYforTouchDown(tx + camera.pos.X, ty + camera.pos.Y, true))
            {
                gravityTouchDown = ti;
            }
            float worldX = tx + camera.pos.X;
            float worldY = ty + camera.pos.Y;
            if (miceManager != null && miceManager.HandleClick(worldX, worldY))
            {
                return true;
            }
            Vector vector = Vect(tx, ty);
            if (candyBubble != null && HandleBubbleTouchXY(star, tx, ty))
            {
                return true;
            }
            if (twoParts != 2)
            {
                if (candyBubbleL != null && HandleBubbleTouchXY(starL, tx, ty))
                {
                    return true;
                }
                if (candyBubbleR != null && HandleBubbleTouchXY(starR, tx, ty))
                {
                    return true;
                }
            }
            if (lightBulbs.Count > 0)
            {
                foreach (LightBulb bulb in lightBulbs)
                {
                    if (HandleLightBulbBubbleTouch(bulb, tx, ty))
                    {
                        return true;
                    }
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
                if (spike.rotateButton != null && spike.touchIndex == -1 && spike.rotateButton.OnTouchDownXY(tx + camera.pos.X, ty + camera.pos.Y))
                {
                    spike.touchIndex = ti;
                    return true;
                }
            }
            int num = pumps.Count;
            for (int i = 0; i < num; i++)
            {
                Pump pump = pumps.ObjectAtIndex(i);
                if (GameObject.PointInObject(Vect(tx + camera.pos.X, ty + camera.pos.Y), pump))
                {
                    pump.pumpTouchTimer = 0.05f;
                    pump.pumpTouch = ti;
                    return true;
                }
            }
            // Handle gun tap
            if (!noCandy)
            {
                foreach (object obj in bungees)
                {
                    Grab grab = (Grab)obj;
                    if (grab.gun && !grab.gunFired && grab.rope == null)
                    {
                        float tapRadius = Grab.GUN_TAP_RADIUS;
                        if (PointInRect(tx + camera.pos.X, ty + camera.pos.Y, grab.x - tapRadius, grab.y - tapRadius, tapRadius * 2f, tapRadius * 2f))
                        {
                            // Calculate direction to candy
                            Vector gunToCandy = VectSub(Vect(grab.x, grab.y), star.pos);
                            grab.gunFired = true;
                            grab.gunInitialRotation = RADIANS_TO_DEGREES(VectAngleNormalized(gunToCandy)) + 90f;
                            grab.gunCandyInitialRotation = candyMain.rotation;
                            grab.gunCup.rotation = grab.gunInitialRotation;

                            // Change gunFront quad to fired state
                            grab.gunFront.SetDrawQuad(3);
                            grab.gunCup.PlayTimeline(Grab.GUN_CUP_SHOW);

                            // Fire the gun - create a rope to the candy
                            // BUNGEE_REST_LEN = 105
                            float gunToCandyDistance = VectDistance(Vect(grab.x, grab.y), star.pos) - 105f;
                            float ropeLength = MAX(gunToCandyDistance, 105f);
                            Bungee bungee = new Bungee().InitWithHeadAtXYTailAtTXTYandLength(null, grab.x, grab.y, star, star.pos.X, star.pos.Y, ropeLength);
                            bungee.bungeeAnchor.pin = bungee.bungeeAnchor.pos;
                            grab.SetRope(bungee);
                            CTRSoundMgr.PlaySound(Resources.Snd.ExpGun);

                            // Track achievement
                            int ropesShoot = Preferences.GetIntForKey("PREFS_ROPES_SHOOT") + 1;
                            Preferences.SetIntForKey(ropesShoot, "PREFS_ROPES_SHOOT", false);
                            if (ropesShoot >= 50)
                            {
                                CTRRootController.PostAchievementName("acRookieSniper", ACHIEVEMENT_STRING("\"Rookie Sniper\""));
                            }
                            if (ropesShoot >= 150)
                            {
                                CTRRootController.PostAchievementName("acSkilledSniper", ACHIEVEMENT_STRING("\"Skilled Sniper\""));
                            }
                            return true;
                        }
                    }
                }
            }
            foreach (SteamTube steamTube in tubes)
            {
                if (steamTube != null && steamTube.OnTouchDownXY(tx + camera.pos.X, ty + camera.pos.Y))
                {
                    return true;
                }
            }
            foreach (Lantern lantern in Lantern.GetAllLanterns())
            {
                if (lantern != null && lantern.OnTouchDown(tx + camera.pos.X, ty + camera.pos.Y))
                {
                    dd.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(Selector_revealCandyFromLantern), null, Lantern.LanternCandyRevealTime);
                    return true;
                }
            }
            RotatedCircle rotatedCircle = null;
            bool flag = false;
            bool flag2 = false;
            foreach (object obj2 in rotatedCircles)
            {
                RotatedCircle rotatedCircle2 = (RotatedCircle)obj2;
                float num2 = VectDistance(Vect(tx + camera.pos.X, ty + camera.pos.Y), rotatedCircle2.handle1);
                float num3 = VectDistance(Vect(tx + camera.pos.X, ty + camera.pos.Y), rotatedCircle2.handle2);
                if ((num2 < 90f && !rotatedCircle2.HasOneHandle()) || num3 < 90f)
                {
                    foreach (object obj3 in rotatedCircles)
                    {
                        RotatedCircle rotatedCircle3 = (RotatedCircle)obj3;
                        if (rotatedCircles.GetObjectIndex(rotatedCircle3) > rotatedCircles.GetObjectIndex(rotatedCircle2))
                        {
                            float num4 = VectDistance(Vect(rotatedCircle3.x, rotatedCircle3.y), Vect(rotatedCircle2.x, rotatedCircle2.y));
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
                    rotatedCircle2.lastTouch = Vect(tx + camera.pos.X, ty + camera.pos.Y);
                    rotatedCircle2.operating = ti;
                    if (num2 < 90f)
                    {
                        rotatedCircle2.SetIsLeftControllerActive(true);
                    }
                    if (num3 < 90f)
                    {
                        rotatedCircle2.SetIsRightControllerActive(true);
                    }
                    rotatedCircle = rotatedCircle2;
                    break;
                }
            }
            if (rotatedCircle != null && rotatedCircles.GetObjectIndex(rotatedCircle) != rotatedCircles.Count - 1 && flag2 && !flag)
            {
                Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
                timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.2));
                Timeline timeline2 = new Timeline().InitWithMaxKeyFramesOnTrack(1);
                timeline2.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.2));
                timeline2.delegateTimelineDelegate = this;
                RotatedCircle rotatedCircle4 = rotatedCircle.Copy();
                _ = rotatedCircle4.AddTimeline(timeline2);
                rotatedCircle4.PlayTimeline(0);
                _ = rotatedCircle.AddTimeline(timeline);
                rotatedCircle.PlayTimeline(0);
                rotatedCircles.SetObjectAt(rotatedCircle4, rotatedCircles.GetObjectIndex(rotatedCircle));
                _ = rotatedCircles.AddObject(rotatedCircle);
            }
            if (ghosts != null)
            {
                foreach (object objGhost in ghosts)
                {
                    Ghost ghost = (Ghost)objGhost;
                    if (ghost != null && ghost.OnTouchDownXY(tx + camera.pos.X, ty + camera.pos.Y))
                    {
                        return true;
                    }
                }
            }
            // Check if we touched a non-kicked kickable grab
            bool touchedNonKickedKickable = false;
            foreach (object obj4 in bungees)
            {
                Grab bungee = (Grab)obj4;
                float tapRadius = Grab.KICK_TAP_RADIUS;
                if (bungee.kickable && PointInRect(tx + camera.pos.X, ty + camera.pos.Y, bungee.x - tapRadius, bungee.y - tapRadius, tapRadius * 2f, tapRadius * 2f))
                {
                    if (!bungee.kicked)
                    {
                        touchedNonKickedKickable = true;
                        break;
                    }
                    bungee.kickActive = true;
                }
            }
            // Start stick timer for kicked kickable grabs if we didn't touch a non-kicked one
            foreach (object obj4 in bungees)
            {
                Grab bungee = (Grab)obj4;
                if (bungee.kickable && bungee.rope != null && !touchedNonKickedKickable && bungee.kicked)
                {
                    bungee.stickTimer = 0f;
                }
            }
            foreach (object obj4 in bungees)
            {
                Grab bungee = (Grab)obj4;
                if (bungee.wheel && PointInRect(tx + camera.pos.X, ty + camera.pos.Y, bungee.x - 110f, bungee.y - 110f, 220f, 220f))
                {
                    bungee.HandleWheelTouch(Vect(tx + camera.pos.X, ty + camera.pos.Y));
                    bungee.wheelOperating = ti;
                }
                if (bungee.moveLength > 0.0 && PointInRect(tx + camera.pos.X, ty + camera.pos.Y, bungee.x - 65f, bungee.y - 65f, 130f, 130f))
                {
                    bungee.moverDragging = ti;
                    return true;
                }
            }
            if (conveyors.OnPointerDown(worldX, worldY, ti))
            {
                dragging[ti] = false;
                return true;
            }
            if (clickToCut && !ignoreTouches)
            {
                Vector s = default;
                Grab grab2 = null;
                Bungee nearestBungeeSegmentByBeziersPointsatXYgrab = GetNearestBungeeSegmentByBeziersPointsatXYgrab(ref s, tx + camera.pos.X, ty + camera.pos.Y, ref grab2);
                if (nearestBungeeSegmentByBeziersPointsatXYgrab != null && nearestBungeeSegmentByBeziersPointsatXYgrab.highlighted && GetNearestBungeeSegmentByConstraintsforGrab(ref s, grab2) != null)
                {
                    _ = CutWithRazorOrLine1Line2Immediate(null, s, s, false);
                }
            }
            return true;
        }


        public bool TouchUpXYIndex(float tx, float ty, int ti)
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
                if (((Button)gravityButton.GetChild(gravityButton.On() ? 1 : 0)).IsInTouchZoneXYforTouchDown(tx + camera.pos.X, ty + camera.pos.Y, true))
                {
                    gravityButton.Toggle();
                    OnButtonPressed(0);
                }
                gravityTouchDown = -1;
            }
            foreach (object obj in spikes)
            {
                Spikes spike = (Spikes)obj;
                if (spike.rotateButton != null && spike.touchIndex == ti)
                {
                    spike.touchIndex = -1;
                    if (spike.rotateButton.OnTouchUpXY(tx + camera.pos.X, ty + camera.pos.Y))
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
                    rotatedCircle.SetIsLeftControllerActive(false);
                    rotatedCircle.SetIsRightControllerActive(false);
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
                if (bungee.kickable && bungee.rope != null)
                {
                    float tapRadius = Grab.KICK_TAP_RADIUS;
                    if (!bungee.kickActive && !bungee.kicked && bungee.rope.cut == -1 &&
                        PointInRect(tx + camera.pos.X, ty + camera.pos.Y, bungee.x - tapRadius, bungee.y - tapRadius, tapRadius * 2f, tapRadius * 2f))
                    {
                        if (bungee.stainCounter > 0)
                        {
                            Image stain = Image.Image_createWithResIDQuad(Resources.Img.ObjSticker, 0);
                            stain.DoRestoreCutTransparency();
                            stain.x = bungee.rope.bungeeAnchor.pos.X;
                            stain.y = bungee.rope.bungeeAnchor.pos.Y;
                            stain.anchor = 18;
                            stain.color.AlphaChannel = bungee.stainCounter / 10f;
                            _ = kickStainsPool.AddChild(stain);
                            bungee.stainCounter--;
                        }
                        bungee.rope.bungeeAnchor.pin = Vect(-1f, -1f);
                        bungee.rope.bungeeAnchor.SetWeight(0.1f);
                        bungee.kicked = true;
                        bungee.stickTimer = -1f;
                        bungee.UpdateKickState();
                        CTRSoundMgr.PlaySound(Resources.Snd.ExpSuckerDrop);
                        int wallClimberCount = Preferences.GetIntForKey("PREFS_WALL_CLIMBER") + 1;
                        Preferences.SetIntForKey(wallClimberCount, "PREFS_WALL_CLIMBER", false);
                        if (wallClimberCount >= 50)
                        {
                            CTRRootController.PostAchievementName("acRookieWallClimber", ACHIEVEMENT_STRING("\"Rookie Wall Climber\""));
                        }
                        if (wallClimberCount >= 400)
                        {
                            CTRRootController.PostAchievementName("acVeteranWallClimber", ACHIEVEMENT_STRING("\"Veteran Wall Climber\""));
                        }
                    }
                    bungee.kickActive = false;
                }
            }
            _ = conveyors.OnPointerUp(tx + camera.pos.X, ty + camera.pos.Y, ti);
            return true;
        }


        public bool TouchMoveXYIndex(float tx, float ty, int ti)
        {
            if (ignoreTouches)
            {
                return true;
            }
            Vector vector = Vect(tx, ty);
            if (ti >= 5)
            {
                return true;
            }
            foreach (object obj in pumps)
            {
                Pump pump3 = (Pump)obj;
                if (pump3.pumpTouch == ti && pump3.pumpTouchTimer != 0.0 && (double)VectDistance(startPos[ti], vector) > 10.0)
                {
                    pump3.pumpTouchTimer = 0f;
                }
            }
            if (rotatedCircles != null)
            {
                for (int i = 0; i < rotatedCircles.Count; i++)
                {
                    RotatedCircle rotatedCircle = rotatedCircles[i];
                    if (rotatedCircle != null && rotatedCircle.operating == ti)
                    {
                        Vector v = Vect(rotatedCircle.x, rotatedCircle.y);
                        Vector vector2 = Vect(tx + camera.pos.X, ty + camera.pos.Y);
                        Vector v2 = VectSub(rotatedCircle.lastTouch, v);
                        float num = VectAngleNormalized(VectSub(vector2, v)) - VectAngleNormalized(v2);
                        float initial_rotation = DEGREES_TO_RADIANS(rotatedCircle.rotation);
                        rotatedCircle.rotation += RADIANS_TO_DEGREES(num);
                        float a = DEGREES_TO_RADIANS(rotatedCircle.rotation);
                        a = FBOUND_PI(a);
                        rotatedCircle.handle1 = VectRotateAround(rotatedCircle.inithanlde1, (double)a, rotatedCircle.x, rotatedCircle.y);
                        rotatedCircle.handle2 = VectRotateAround(rotatedCircle.inithanlde2, (double)a, rotatedCircle.x, rotatedCircle.y);
                        int num2 = num > 0f ? 1 : 2;
                        if ((double)Math.Abs(num) < 0.07)
                        {
                            num2 = -1;
                        }
                        if (rotatedCircle.soundPlaying != num2 && num2 != -1)
                        {
                            CTRSoundMgr.PlaySound(num2 == 1 ? Resources.Snd.ScratchOut : Resources.Snd.ScratchIn);
                            rotatedCircle.soundPlaying = num2;
                        }
                        for (int j = 0; j < bungees.Count; j++)
                        {
                            Grab grab = bungees[j];
                            if (VectDistance(Vect(grab.x, grab.y), Vect(rotatedCircle.x, rotatedCircle.y)) <= rotatedCircle.sizeInPixels + 5f)
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
                                Vector vector3 = VectRotateAround(Vect(grab.initial_x, grab.initial_y), (double)a2, rotatedCircle.x, rotatedCircle.y);
                                grab.x = vector3.X;
                                grab.y = vector3.Y;
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
                        }
                        for (int k = 0; k < pumps.Count; k++)
                        {
                            Pump pump4 = pumps[k];
                            if (VectDistance(Vect(pump4.x, pump4.y), Vect(rotatedCircle.x, rotatedCircle.y)) <= rotatedCircle.sizeInPixels + 5f)
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
                                Vector vector4 = VectRotateAround(Vect(pump4.initial_x, pump4.initial_y), (double)a3, rotatedCircle.x, rotatedCircle.y);
                                pump4.x = vector4.X;
                                pump4.y = vector4.Y;
                                pump4.rotation += RADIANS_TO_DEGREES(num);
                                pump4.UpdateRotation();
                            }
                        }
                        for (int l = 0; l < bubbles.Count; l++)
                        {
                            Bubble bubble = bubbles[l];
                            if (VectDistance(Vect(bubble.x, bubble.y), Vect(rotatedCircle.x, rotatedCircle.y)) <= rotatedCircle.sizeInPixels + 10f && bubble != candyBubble && bubble != candyBubbleR && bubble != candyBubbleL)
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
                                Vector vector5 = VectRotateAround(Vect(bubble.initial_x, bubble.initial_y), (double)a4, rotatedCircle.x, rotatedCircle.y);
                                bubble.x = vector5.X;
                                bubble.y = vector5.Y;
                            }
                        }
                        if (PointInRect(target.x, target.y, rotatedCircle.x - rotatedCircle.size, rotatedCircle.y - rotatedCircle.size, 2f * rotatedCircle.size, 2f * rotatedCircle.size))
                        {
                            Vector vector6 = VectRotateAround(Vect(target.x, target.y), (double)num, rotatedCircle.x, rotatedCircle.y);
                            target.x = vector6.X;
                            target.y = vector6.Y;
                        }
                        rotatedCircle.lastTouch = vector2;
                        return true;
                    }
                }
            }
            int num3 = bungees.Count;
            for (int m = 0; m < num3; m++)
            {
                Grab grab2 = bungees.ObjectAtIndex(m);
                if (grab2 != null)
                {
                    if (grab2.wheel && grab2.wheelOperating == ti)
                    {
                        grab2.HandleWheelRotate(Vect(tx + camera.pos.X, ty + camera.pos.Y));
                        return true;
                    }
                    if (grab2.moveLength > 0.0 && grab2.moverDragging == ti)
                    {
                        if (grab2.moveVertical)
                        {
                            grab2.y = FIT_TO_BOUNDARIES(ty + camera.pos.Y, grab2.minMoveValue, grab2.maxMoveValue);
                        }
                        else
                        {
                            grab2.x = FIT_TO_BOUNDARIES(tx + camera.pos.X, grab2.minMoveValue, grab2.maxMoveValue);
                        }
                        if (grab2.rope != null)
                        {
                            grab2.rope.bungeeAnchor.pos = Vect(grab2.x, grab2.y);
                            grab2.rope.bungeeAnchor.pin = grab2.rope.bungeeAnchor.pos;
                        }
                        if (grab2.radius != -1f)
                        {
                            grab2.ReCalcCircle();
                        }
                        return true;
                    }
                    // Cancel stick timer if moved too much (kickable grabs)
                    if (grab2.kickable && grab2.kicked && grab2.rope != null &&
                        VectLength(VectSub(startPos[ti], vector)) > Grab.KICK_MOVE_LENGTH)
                    {
                        grab2.stickTimer = -1f;
                    }
                }
            }
            if (conveyors.OnPointerMove(tx + camera.pos.X, ty + camera.pos.Y, ti))
            {
                return true;
            }
            if (dragging[ti])
            {
                Vector start = VectAdd(startPos[ti], camera.pos);
                Vector end = VectAdd(Vect(tx, ty), camera.pos);
                FingerCut fingerCut = new()
                {
                    start = start,
                    end = end,
                    startSize = 5f,
                    endSize = 5f,
                    c = RGBAColor.whiteRGBA
                };
                _ = fingerCuts[ti].AddObject(fingerCut);
                int num4 = 0;
                foreach (object obj2 in fingerCuts[ti])
                {
                    FingerCut item = (FingerCut)obj2;
                    num4 += CutWithRazorOrLine1Line2Immediate(null, item.start, item.end, false);
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
                    int num5 = Preferences.GetIntForKey("PREFS_ROPES_CUT") + 1;
                    Preferences.SetIntForKey(num5, "PREFS_ROPES_CUT", false);
                    if (num5 == 100)
                    {
                        CTRRootController.PostAchievementName("681461850", ACHIEVEMENT_STRING("\"Rope Cutter\""));
                    }
                    if (ropesCutAtOnce is >= 3 and < 5)
                    {
                        CTRRootController.PostAchievementName("681464917", ACHIEVEMENT_STRING("\"Quick Finger\""));
                    }
                    if (ropesCutAtOnce >= 5)
                    {
                        CTRRootController.PostAchievementName("681508316", ACHIEVEMENT_STRING("\"Master Finger\""));
                    }
                    if (num5 == 800)
                    {
                        CTRRootController.PostAchievementName("681457931", ACHIEVEMENT_STRING("\"Rope Cutter Maniac\""));
                    }
                    if (num5 == 2000)
                    {
                        CTRRootController.PostAchievementName("1058248892", ACHIEVEMENT_STRING("\"Ultimate Rope Cutter\""));
                    }
                }
                prevStartPos[ti] = startPos[ti];
                startPos[ti] = vector;
            }
            return true;
        }


        public bool TouchDraggedXYIndex(float tx, float ty, int index)
        {
            if (index > 5)
            {
                return false;
            }
            slastTouch = Vect(tx, ty);
            return true;
        }

    }
}
