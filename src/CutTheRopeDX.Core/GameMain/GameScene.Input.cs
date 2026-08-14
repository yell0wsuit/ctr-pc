using System;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Handles tapping a candy bubble and pops it when the touch is inside the bubble touch area.
        /// </summary>
        /// <param name="body">Candy body whose bubble the touch may pop.</param>
        /// <param name="tx">Touch x-coordinate in screen space.</param>
        /// <param name="ty">Touch y-coordinate in screen space.</param>
        /// <returns><see langword="true"/> when the bubble was touched and popped; otherwise, <see langword="false"/>.</returns>
        public bool HandleBubbleTouchXY(CandyBody body, float tx, float ty)
        {
            if (PointInRect(camera.ScreenToWorldX(tx), camera.ScreenToWorldY(ty), body.Point.pos.X - 60f, body.Point.pos.Y - 60f, 120f, 120f))
            {
                PopCandyBubble(body);
                RegisterBubblePopped();
                return true;
            }
            return false;
        }

        /// <summary>Bumps the bubbles-popped counter and posts the related achievements.</summary>
        private static void RegisterBubblePopped()
        {
            int bubblesPoppedCount = Preferences.GetIntForKey("PREFS_BUBBLES_POPPED") + 1;
            Preferences.SetIntForKey(bubblesPoppedCount, "PREFS_BUBBLES_POPPED", false);
            if (bubblesPoppedCount == 50)
            {
                CTRRootController.PostAchievementName("681513183", ACHIEVEMENT_STRING("\"Bubble Popper\""));
            }
            if (bubblesPoppedCount == 300)
            {
                CTRRootController.PostAchievementName("1058345234", ACHIEVEMENT_STRING("\"Bubble Master\""));
            }
        }

        /// <summary>
        /// Ends any in-progress rope-cut finger traces so they fade out instead of lingering
        /// (used when a win/loss transition interrupts an active drag). Mirrors the touch-up cleanup.
        /// </summary>
        public void EndActiveFingerTraces()
        {
            foreach (PointerGestureState gesture in pointerGestures)
            {
                gesture?.Cancel();
            }
        }

        /// <summary>Resolves a supported pointer index before any gesture state is accessed.</summary>
        private bool TryGetPointerGesture(int pointerIndex, out PointerGestureState gesture)
        {
            if ((uint)pointerIndex < (uint)pointerGestures.Length)
            {
                gesture = pointerGestures[pointerIndex];
                return gesture != null;
            }

            gesture = null;
            return false;
        }

        /// <summary>
        /// Handles a touch-down event for gameplay objects and rope-cut gestures.
        /// </summary>
        /// <param name="tx">Touch x-coordinate in screen space.</param>
        /// <param name="ty">Touch y-coordinate in screen space.</param>
        /// <param name="ti">Touch index.</param>
        /// <returns><see langword="true"/> when the touch-down event was consumed; otherwise, <see langword="false"/>.</returns>
        public bool TouchDownXYIndex(float tx, float ty, int ti)
        {
            if (!TryGetPointerGesture(ti, out PointerGestureState gesture))
            {
                return true;
            }
            // Outcome input owns only the visual trace; it must not reach any gameplay object.
            if (AcceptsVisualOnlyPointerInput)
            {
                gesture.Begin(Vect(tx, ty), camera.ScreenToWorld(tx, ty));
                return true;
            }
            if (ignoreTouches)
            {
                if (camera.type == CAMERATYPE.CAMERASPEEDPIXELS)
                {
                    fastenCamera = true;
                }
                return true;
            }
            if (gravityState.IsInToggleTouchZone(camera.ScreenToWorldX(tx), camera.ScreenToWorldY(ty)))
            {
                gravityState.CaptureToggleTouch(ti);
            }
            Vector world = camera.ScreenToWorld(tx, ty);
            float worldX = world.X;
            float worldY = world.Y;
            waterLayer?.AddParticlesAtXY(worldX, worldY);
            if (miceManager != null && miceManager.HandleClick(worldX, worldY, out ConstraintedPoint droppedMouseCandy))
            {
                CandyContext droppedCandy = CandyForPointOrNull(droppedMouseCandy);
                if (droppedCandy != null)
                {
                    droppedMouseCandy.disableGravity = IsCandyGravitySuppressed(droppedCandy);
                }
                return true;
            }
            Vector vector = Vect(tx, ty);
            if (rockets != null)
            {
                foreach (Rocket rocket in rockets)
                {
                    if (rocket != null && rocket.state == Rocket.STATE_ROCKET_IDLE && rocket.isRotatable && rocket.isOperating == -1 &&
                        VectLength(VectSub(world, Vect(rocket.x, rocket.y))) < 90f)
                    {
                        rocket.HandleTouch(world);
                        rocket.isOperating = ti;
                        return true;
                    }
                }
            }
            if (snailobjects != null)
            {
                // Shaking a snail off only ever applies to a whole candy, which the body-role table
                // enforces for the snail interaction.
                foreach (CandyBody body in ActiveCandyBodies(CandyInteraction.Snail))
                {
                    ConstraintedPoint p = body.Point;
                    if (PointInRect(worldX, worldY, p.pos.X - 30f, p.pos.Y - 30f, 60f, 60f) && p.weight > 1f)
                    {
                        p.SetWeight(p.weight - 3f);
                        if (p.weight <= 1f)
                        {
                            p.SetWeight(1f);
                            DetachSnailsForPoint(p);
                        }
                        return true;
                    }
                }
            }
            // Tapping a bubbled body pops its bubble, whether that body is a whole candy or a half.
            foreach (CandyBody body in ActiveCandyBodies(CandyInteraction.Bubble))
            {
                if (body.Bubble != null && HandleBubbleTouchXY(body, tx, ty))
                {
                    return true;
                }
            }
            gesture.Begin(vector, world);
            foreach (object obj in spikes)
            {
                Spikes spike = (Spikes)obj;
                if (spike.rotateButton != null && spike.touchIndex == -1 && spike.rotateButton.OnTouchDownXY(camera.ScreenToWorldX(tx), camera.ScreenToWorldY(ty)))
                {
                    spike.touchIndex = ti;
                    return true;
                }
            }
            int pumpCount = pumps.Count;
            for (int i = 0; i < pumpCount; i++)
            {
                Pump pump = pumps[i];
                if (GameObject.PointInObject(camera.ScreenToWorld(tx, ty), pump))
                {
                    pump.pumpTouchTimer = 0.05f;
                    pump.pumpTouch = ti;
                    return true;
                }
            }
            // Handle gun tap
            bool primaryInPlay = !candies[0].HasNoWholeBodyInPlay;
            if (primaryInPlay)
            {
                foreach (object obj in bungees)
                {
                    Grab grab = (Grab)obj;
                    GunSource gun = grab.GunSource;
                    if (gun != null && gun.CanFire(candies[0].Lifecycle.Attachments.InLantern))
                    {
                        float mapLeftX = waterLayer?.x ?? 0f;
                        float mapRightX = waterLayer != null ? waterLayer.x + waterLayer.width : mapWidth;
                        bool candyInMapBounds = GameObject.RectInObject(mapLeftX, 0f, mapRightX, mapHeight, candy);
                        bool canFireFromWaterState = waterLayer == null || candyInMapBounds || waterLayer.y > star.pos.Y;
                        float tapRadius = Grab.GUN_TAP_RADIUS;
                        if (canFireFromWaterState && PointInRect(camera.ScreenToWorldX(tx), camera.ScreenToWorldY(ty), grab.x - tapRadius, grab.y - tapRadius, tapRadius * 2f, tapRadius * 2f))
                        {
                            gun.Fire(Vect(grab.x, grab.y), star.pos, candyMain.rotation);
                            gun.Cup.rotation = gun.InitialRotation;
                            gun.Front.SetDrawQuad(Grab.GunDisabledFrontQuad);
                            gun.Cup.PlayTimeline(Grab.GUN_CUP_SHOW);

                            // Fire the gun - create a rope to the candy
                            float gunToCandyDistance = VectDistance(Vect(grab.x, grab.y), star.pos) - ActivePhysicsConstants.BungeeRestLength;
                            float ropeLength = MAX(gunToCandyDistance, ActivePhysicsConstants.BungeeRestLength);
                            Bungee bungee = new Bungee().InitWithHeadAtXYTailAtTXTYandLength(null, grab.x, grab.y, star, star.pos.X, star.pos.Y, ropeLength);
                            bungee.bungeeAnchor.pin = bungee.bungeeAnchor.pos;
                            grab.SetRope(bungee);
                            ropes.Register(bungee, grab);
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
                if (steamTube != null && steamTube.OnTouchDownXY(camera.ScreenToWorldX(tx), camera.ScreenToWorldY(ty)))
                {
                    return true;
                }
            }

            BambooTube nearestBambooTube = null;
            float nearestBambooDistance = float.PositiveInfinity;
            foreach (BambooTube bambooTube in bambooTubes)
            {
                if (bambooTube == null)
                {
                    continue;
                }

                float distance = VectDistance(world, Vect(bambooTube.x, bambooTube.y));
                if (distance < nearestBambooDistance)
                {
                    nearestBambooDistance = distance;
                    nearestBambooTube = bambooTube;
                }
            }

            if (nearestBambooTube != null && nearestBambooTube.HandleBambooTouchWithIndex(world, ti))
            {
                return true;
            }

            bool handledHandInput = false;
            if (hands != null)
            {
                foreach (MechanicalHand hand in hands)
                {
                    if (hand == null)
                    {
                        continue;
                    }

                    if (hand.State == MechanicalHandState.HoldingCandy && VectDistance(world, hand.ClawPosition()) < MechanicalHand.MH_CLAW_TOUCH_RADIUS)
                    {
                        CandyContext held = HandHeldCandy(hand);
                        hand.cPoint.RemoveConstraint(held?.WholeBody.Point ?? star);
                        hand.ReleaseCandyAfterDropSound();
                        hand.AnimateReleaseWithAnimationsPool(aniPool);
                        _ = held?.Lifecycle.Attachments.TryReleaseHand(hand);
                        CTRSoundMgr.PlaySound(Resources.Snd.ExpHandDrop);
                        return true;
                    }

                    if (hand.segments == null)
                    {
                        continue;
                    }

                    for (int i = hand.segments.Count - 1; i >= 0; i--)
                    {
                        MechanicalHandSegment segment = hand.SegmentAtIndex(i);
                        if (segment?.button != null && segment.button.OnTouchDownXY(world.X, world.Y))
                        {
                            segment.Rotate();
                            hand.rotatingSegment = segment;
                            hand.ArmClap();
                            handledHandInput = true;
                            CTRSoundMgr.PlaySound(Resources.Snd.ExpHandRotate);
                            break;
                        }
                    }
                }
            }
            if (handledHandInput)
            {
                return true;
            }

            foreach (CandyBody body in ActiveCandyBodies())
            {
                if (body.Point != null && HandleConveyorTouchConstraintedPointXY(body.Point, tx, ty))
                {
                    return true;
                }
            }

            foreach (Lantern lantern in Lantern.GetAllLanterns())
            {
                if (lantern != null && lantern.OnTouchDown(camera.ScreenToWorldX(tx), camera.ScreenToWorldY(ty), out ConstraintedPoint releasedCandyPoint))
                {
                    dd.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(Selector_revealCandyFromLantern), releasedCandyPoint, Lantern.LanternCandyRevealTime);
                    return true;
                }
            }
            RotatedCircle rotatedCircle = null;
            bool flag = false;
            bool flag2 = false;
            foreach (object obj2 in rotatedCircles)
            {
                RotatedCircle rotatedCircle2 = (RotatedCircle)obj2;
                float distanceToLeftHandle = VectDistance(camera.ScreenToWorld(tx, ty), rotatedCircle2.handle1);
                float distanceToRightHandle = VectDistance(camera.ScreenToWorld(tx, ty), rotatedCircle2.handle2);
                if ((distanceToLeftHandle < 90f && !rotatedCircle2.HasOneHandle()) || distanceToRightHandle < 90f)
                {
                    foreach (object obj3 in rotatedCircles)
                    {
                        RotatedCircle rotatedCircle3 = (RotatedCircle)obj3;
                        if (rotatedCircles.IndexOf(rotatedCircle3) > rotatedCircles.IndexOf(rotatedCircle2))
                        {
                            float circleDistance = VectDistance(Vect(rotatedCircle3.x, rotatedCircle3.y), Vect(rotatedCircle2.x, rotatedCircle2.y));
                            if (circleDistance + rotatedCircle3.sizeInPixels <= rotatedCircle2.sizeInPixels)
                            {
                                flag = true;
                            }
                            if (circleDistance <= rotatedCircle2.sizeInPixels + rotatedCircle3.sizeInPixels)
                            {
                                flag2 = true;
                            }
                        }
                    }
                    rotatedCircle2.lastTouch = camera.ScreenToWorld(tx, ty);
                    rotatedCircle2.operating = ti;
                    if (distanceToLeftHandle < 90f)
                    {
                        rotatedCircle2.SetIsLeftControllerActive(true);
                    }
                    if (distanceToRightHandle < 90f)
                    {
                        rotatedCircle2.SetIsRightControllerActive(true);
                    }
                    rotatedCircle = rotatedCircle2;
                    break;
                }
            }
            if (rotatedCircle != null && rotatedCircles.IndexOf(rotatedCircle) != rotatedCircles.Count - 1 && flag2 && !flag)
            {
                Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
                timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.2f));
                Timeline timeline2 = new Timeline().InitWithMaxKeyFramesOnTrack(1);
                timeline2.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.2f));
                timeline2.delegateTimelineDelegate = this;
                RotatedCircle rotatedCircle4 = rotatedCircle.Copy();
                _ = rotatedCircle4.AddTimeline(timeline2);
                rotatedCircle4.PlayTimeline(0);
                _ = rotatedCircle.AddTimeline(timeline);
                rotatedCircle.PlayTimeline(0);
                rotatedCircles[rotatedCircles.IndexOf(rotatedCircle)] = rotatedCircle4;
                rotatedCircles.Add(rotatedCircle);
            }
            if (ghosts != null)
            {
                foreach (object objGhost in ghosts)
                {
                    Ghost ghost = (Ghost)objGhost;
                    if (ghost != null && ghost.OnTouchDownXY(camera.ScreenToWorldX(tx), camera.ScreenToWorldY(ty)))
                    {
                        return true;
                    }
                }
            }
            // A tap that lands on a stuck cup claims the touch; taps that land anywhere else start
            // every detached cup trying to re-stick.
            bool touchedMountedCup = false;
            foreach (object obj4 in bungees)
            {
                Grab bungee = (Grab)obj4;
                float tapRadius = Grab.KICK_TAP_RADIUS;
                if (bungee.Mount is SuctionMount tapped && tapped.IsMounted
                    && PointInRect(camera.ScreenToWorldX(tx), camera.ScreenToWorldY(ty), bungee.x - tapRadius, bungee.y - tapRadius, tapRadius * 2f, tapRadius * 2f))
                {
                    touchedMountedCup = true;
                    break;
                }
            }
            foreach (object obj4 in bungees)
            {
                Grab bungee = (Grab)obj4;
                if (bungee.Mount is SuctionMount mount && !mount.IsMounted && bungee.Rope != null && !touchedMountedCup)
                {
                    mount.BeginSticking();
                }
            }
            foreach (object obj4 in bungees)
            {
                Grab bungee = (Grab)obj4;
                if (bungee.Wheel?.TryBeginOperating(bungee, camera.ScreenToWorldX(tx), camera.ScreenToWorldY(ty), ti) == true)
                {
                    // A touch that lands on the wheel belongs to the wheel: without this, a wheel
                    // hook riding a manual belt let the same touch also start a belt drag.
                    return true;
                }
                if (bungee.Rail?.TryBeginDrag(bungee, camera.ScreenToWorldX(tx), camera.ScreenToWorldY(ty), ti) == true)
                {
                    return true;
                }
            }
            if (conveyors.OnPointerDown(worldX, worldY, ti))
            {
                gesture.Cancel();
                return true;
            }
            if (clickToCut && !ignoreTouches)
            {
                Vector s = default;
                Grab grab2 = null;
                Bungee nearestBungeeSegmentByBeziersPointsatXYgrab = GetNearestBungeeSegmentByBeziersPointsatXYgrab(ref s, camera.ScreenToWorldX(tx), camera.ScreenToWorldY(ty), ref grab2);
                if (nearestBungeeSegmentByBeziersPointsatXYgrab != null && nearestBungeeSegmentByBeziersPointsatXYgrab.highlighted && GetNearestBungeeSegmentByConstraintsforGrab(ref s, grab2) != null)
                {
                    _ = CutWithRazorOrLine1Line2Immediate(null, s, s, false);
                }
            }
            return true;
        }


        /// <summary>
        /// Handles a touch-up event for gameplay objects and active touch gestures.
        /// </summary>
        /// <param name="tx">Touch x-coordinate in screen space.</param>
        /// <param name="ty">Touch y-coordinate in screen space.</param>
        /// <param name="ti">Touch index.</param>
        /// <returns><see langword="true"/> when the touch-up event was consumed; otherwise, <see langword="false"/>.</returns>
        public bool TouchUpXYIndex(float tx, float ty, int ti)
        {
            if (!TryGetPointerGesture(ti, out PointerGestureState gesture))
            {
                return true;
            }
            // Outcome input ends the visual trace without reaching any gameplay object.
            if (AcceptsVisualOnlyPointerInput)
            {
                gesture.End();
                return true;
            }
            if (ignoreTouches)
            {
                return true;
            }
            gesture.End();
            if (rockets != null)
            {
                foreach (Rocket rocket in rockets)
                {
                    if (rocket == null || rocket.isOperating != ti)
                    {
                        continue;
                    }
                    if (!rocket.rotateHandled)
                    {
                        Timeline timeline = rocket.GetCurrentTimeline();
                        if (timeline != null && timeline.state == Timeline.TimelineState.TIMELINE_PLAYING)
                        {
                            timeline.JumpToTrackKeyFrame(2, 1);
                            timeline.StopTimeline();
                        }
                        rocket.PlayTimeline(0);
                        rocket.startRotation += DEG_45;
                    }
                    else
                    {
                        rocket.HandleRotateFinal();
                    }
                    rocket.rotateHandled = false;
                    rocket.isOperating = -1;
                    return true;
                }
            }
            if (gravityState.ReleaseToggleTouch(ti))
            {
                if (gravityState.IsInToggleTouchZone(camera.ScreenToWorldX(tx), camera.ScreenToWorldY(ty)))
                {
                    OnButtonPressed(0);
                }
            }

            foreach (BambooTube bambooTube in bambooTubes)
            {
                if (bambooTube != null && bambooTube.BambooTouchIndex == ti)
                {
                    bambooTube.HandleBambooCancel();
                    return true;
                }
            }

            if (hands != null)
            {
                foreach (MechanicalHand hand in hands)
                {
                    if (hand?.segments == null)
                    {
                        continue;
                    }

                    foreach (MechanicalHandSegment segment in hand.segments)
                    {
                        _ = (segment?.button?.OnTouchUpXY(camera.ScreenToWorldX(tx), camera.ScreenToWorldY(ty)));
                    }
                }
            }
            foreach (object obj in spikes)
            {
                Spikes spike = (Spikes)obj;
                if (spike.rotateButton != null && spike.touchIndex == ti)
                {
                    spike.touchIndex = -1;
                    if (spike.rotateButton.OnTouchUpXY(camera.ScreenToWorldX(tx), camera.ScreenToWorldY(ty)))
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
                bungee.Wheel?.EndOperating(ti);
                bungee.Rail?.EndDrag(ti);
                if (bungee.Mount is SuctionMount mount && bungee.Rope != null)
                {
                    float tapRadius = Grab.KICK_TAP_RADIUS;
                    if (mount.IsMounted && bungee.Rope.cut == -1 &&
                        PointInRect(camera.ScreenToWorldX(tx), camera.ScreenToWorldY(ty), bungee.x - tapRadius, bungee.y - tapRadius, tapRadius * 2f, tapRadius * 2f))
                    {
                        if (mount.TakeStain(out float stainAlpha))
                        {
                            Image stain = Image.Image_createWithResIDQuad(Resources.Img.ObjSticker, 0);
                            stain.DoRestoreCutTransparency();
                            stain.x = bungee.Rope.bungeeAnchor.pos.X;
                            stain.y = bungee.Rope.bungeeAnchor.pos.Y;
                            stain.anchor = 18;
                            stain.color.AlphaChannel = stainAlpha;
                            _ = decalsLayer.AddChild(stain);
                        }
                        mount.Kick(bungee);
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
                }
            }
            _ = conveyors.OnPointerUp(camera.ScreenToWorldX(tx), camera.ScreenToWorldY(ty), ti);
            return true;
        }


        /// <summary>
        /// Handles a touch-move event for active gameplay gestures, draggable objects, and rope cutting.
        /// </summary>
        /// <param name="tx">Touch x-coordinate in screen space.</param>
        /// <param name="ty">Touch y-coordinate in screen space.</param>
        /// <param name="ti">Touch index.</param>
        /// <returns><see langword="true"/> when the touch-move event was consumed; otherwise, <see langword="false"/>.</returns>
        public bool TouchMoveXYIndex(float tx, float ty, int ti)
        {
            if (!TryGetPointerGesture(ti, out PointerGestureState gesture))
            {
                return true;
            }
            Vector vector = Vect(tx, ty);
            Vector world = camera.ScreenToWorld(tx, ty);
            // Outcome input advances only the visual trace; no cuts or object handlers run.
            if (AcceptsVisualOnlyPointerInput)
            {
                _ = gesture.Move(vector, world, out _);
                return true;
            }
            if (ignoreTouches)
            {
                return true;
            }
            if (rockets != null)
            {
                foreach (Rocket rocket in rockets)
                {
                    if (rocket != null && rocket.isOperating == ti)
                    {
                        rocket.HandleRotate(world);
                        return true;
                    }
                }
            }
            foreach (object obj in pumps)
            {
                Pump pump3 = (Pump)obj;
                if (pump3.pumpTouch == ti && pump3.pumpTouchTimer != 0 && VectDistance(gesture.StartPosition, vector) > 10)
                {
                    pump3.pumpTouchTimer = 0f;
                }
            }

            foreach (BambooTube bambooTube in bambooTubes)
            {
                if (bambooTube != null && bambooTube.BambooTouchIndex == ti)
                {
                    bambooTube.HandleBambooRotate(world);
                }
            }

            if (hands != null)
            {
                foreach (MechanicalHand hand in hands)
                {
                    if (hand?.segments == null)
                    {
                        continue;
                    }

                    foreach (MechanicalHandSegment segment in hand.segments)
                    {
                        _ = (segment?.button?.OnTouchMoveXY(world.X, world.Y));
                    }
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
                        Vector vector2 = camera.ScreenToWorld(tx, ty);
                        Vector v2 = VectSub(rotatedCircle.lastTouch, v);
                        float rotationDelta = VectAngleNormalized(VectSub(vector2, v)) - VectAngleNormalized(v2);
                        float initial_rotation = DEGREES_TO_RADIANS(rotatedCircle.rotation);
                        rotatedCircle.rotation += RADIANS_TO_DEGREES(rotationDelta);
                        float a = DEGREES_TO_RADIANS(rotatedCircle.rotation);
                        a = FBOUND_PI(a);
                        rotatedCircle.handle1 = VectRotateAround(rotatedCircle.inithanlde1, a, rotatedCircle.x, rotatedCircle.y);
                        rotatedCircle.handle2 = VectRotateAround(rotatedCircle.inithanlde2, a, rotatedCircle.x, rotatedCircle.y);
                        int scratchSoundState = rotationDelta > 0f ? 1 : 2;
                        if (MathF.Abs(rotationDelta) < 0.07f)
                        {
                            scratchSoundState = -1;
                        }
                        if (rotatedCircle.soundPlaying != scratchSoundState && scratchSoundState != -1)
                        {
                            CTRSoundMgr.PlaySound(scratchSoundState == 1 ? Resources.Snd.ScratchOut : Resources.Snd.ScratchIn);
                            rotatedCircle.soundPlaying = scratchSoundState;
                        }
                        for (int j = 0; j < bungees.Count; j++)
                        {
                            Grab grab = bungees[j];
                            // A grab that carries its own movement is not the disc's to move: a path
                            // mover drives itself, and a drag rail belongs to the player. Scratching
                            // the disc would otherwise sweep such a hook off its rail, leaving the
                            // rail drawn where it was. Same rule the disc-capture test in the update
                            // loop applies, so both agree on what this disc owns.
                            if (!(grab.Mount?.FollowsPlatform ?? grab.Motion.FollowsPlatform)
                                || grab is IGhostApparition)
                            {
                                continue;
                            }
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
                                Vector vector3 = VectRotateAround(Vect(grab.initial_x, grab.initial_y), a2, rotatedCircle.x, rotatedCircle.y);
                                grab.x = vector3.X;
                                grab.y = vector3.Y;
                                grab.SyncRopeAnchor();
                                grab.ReCalcCircle();
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
                                Vector vector4 = VectRotateAround(Vect(pump4.initial_x, pump4.initial_y), a3, rotatedCircle.x, rotatedCircle.y);
                                pump4.x = vector4.X;
                                pump4.y = vector4.Y;
                                pump4.rotation += RADIANS_TO_DEGREES(rotationDelta);
                                pump4.UpdateRotation();
                            }
                        }
                        for (int l = 0; l < bubbles.Count; l++)
                        {
                            Bubble bubble = bubbles[l];
                            // A ghost's bubble belongs to the ghost, not the disc: its morph clouds
                            // stay at the ghost's spot, so rotating the bubble alone would strand them.
                            if (bubble is IGhostApparition)
                            {
                                continue;
                            }
                            if (VectDistance(Vect(bubble.x, bubble.y), Vect(rotatedCircle.x, rotatedCircle.y)) <= rotatedCircle.sizeInPixels + 10f && CandyBodyForBubbleOrNull(bubble) == null)
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
                                Vector vector5 = VectRotateAround(Vect(bubble.initial_x, bubble.initial_y), a4, rotatedCircle.x, rotatedCircle.y);
                                bubble.x = vector5.X;
                                bubble.y = vector5.Y;
                            }
                        }
                        for (int targetIndex = 0; targetIndex < targets.Count; targetIndex++)
                        {
                            GameObject to = targets[targetIndex].targetObject;
                            if (to != null && PointInRect(to.x, to.y, rotatedCircle.x - rotatedCircle.size, rotatedCircle.y - rotatedCircle.size, 2f * rotatedCircle.size, 2f * rotatedCircle.size))
                            {
                                Vector vector6 = VectRotateAround(Vect(to.x, to.y), rotationDelta, rotatedCircle.x, rotatedCircle.y);
                                to.x = vector6.X;
                                to.y = vector6.Y;
                            }
                        }
                        rotatedCircle.lastTouch = vector2;
                        return true;
                    }
                }
            }
            int grabCount = bungees.Count;
            for (int m = 0; m < grabCount; m++)
            {
                Grab grab2 = bungees[m];
                if (grab2 != null)
                {
                    if (grab2.Wheel is WheelControl wheel && wheel.OperatingTouch == ti)
                    {
                        wheel.HandleRotate(grab2, camera.ScreenToWorld(tx, ty));
                        return true;
                    }
                    if (grab2.Rail is RailMotion rail && rail.DraggingTouch == ti)
                    {
                        rail.DragTo(grab2, camera.ScreenToWorldX(tx), camera.ScreenToWorldY(ty));
                        grab2.SyncRopeAnchor();
                        grab2.ReCalcCircle();
                        return true;
                    }
                    // Cancel stick timer if moved too much (kickable grabs)
                    if (grab2.Mount is SuctionMount dragMount && !dragMount.IsMounted && grab2.Rope != null &&
                        VectLength(VectSub(gesture.StartPosition, vector)) > Grab.KICK_MOVE_LENGTH)
                    {
                        dragMount.CancelSticking();
                    }
                }
            }
            if (conveyors.OnPointerMove(camera.ScreenToWorldX(tx), camera.ScreenToWorldY(ty), ti))
            {
                return true;
            }
            if (gesture.Move(vector, world, out Vector segmentStart))
            {
                Vector start = camera.ScreenToWorld(segmentStart.X, segmentStart.Y);
                Vector end = camera.ScreenToWorld(tx, ty);
                FingerCut fingerCut = new()
                {
                    start = start,
                    end = end,
                    startSize = 5f,
                    endSize = 5f,
                    c = RGBAColor.whiteRGBA
                };
                gesture.Cuts.Add(fingerCut);
                int ropesCutThisFrame = 0;
                foreach (object obj2 in gesture.Cuts)
                {
                    FingerCut item = (FingerCut)obj2;
                    ropesCutThisFrame += CutWithRazorOrLine1Line2Immediate(null, item.start, item.end, false);
                }
                if (ropesCutThisFrame > 0)
                {
                    freezeCamera = false;
                    if (ropesCutAtOnce > 0 && ropeAtOnceTimer > 0)
                    {
                        ropesCutAtOnce += ropesCutThisFrame;
                    }
                    else
                    {
                        ropesCutAtOnce = ropesCutThisFrame;
                    }
                    ropeAtOnceTimer = 0.1f;
                    int ropesCutTotal = Preferences.GetIntForKey("PREFS_ROPES_CUT") + 1;
                    Preferences.SetIntForKey(ropesCutTotal, "PREFS_ROPES_CUT", false);
                    if (ropesCutTotal == 100)
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
                    if (ropesCutTotal == 800)
                    {
                        CTRRootController.PostAchievementName("681457931", ACHIEVEMENT_STRING("\"Rope Cutter Maniac\""));
                    }
                    if (ropesCutTotal == 2000)
                    {
                        CTRRootController.PostAchievementName("1058248892", ACHIEVEMENT_STRING("\"Ultimate Rope Cutter\""));
                    }
                }
            }
            return true;
        }


        /// <summary>
        /// Records the latest dragged touch location.
        /// </summary>
        /// <param name="tx">Touch x-coordinate in screen space.</param>
        /// <param name="ty">Touch y-coordinate in screen space.</param>
        /// <param name="index">Touch index.</param>
        /// <returns><see langword="true"/> when the drag was recorded; otherwise, <see langword="false"/>.</returns>
        public bool TouchDraggedXYIndex(float tx, float ty, int index)
        {
            if (!TryGetPointerGesture(index, out _))
            {
                return false;
            }
            // Hover state is gameplay-only; outcome drawing is handled by the gesture lifecycle.
            if (AcceptsVisualOnlyPointerInput)
            {
                return true;
            }
            slastTouch = Vect(tx, ty);
            return true;
        }

    }
}
