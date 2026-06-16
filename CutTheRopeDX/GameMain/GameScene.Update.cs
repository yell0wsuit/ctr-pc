using System;
using System.Collections.Generic;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    internal sealed partial class GameScene : BaseElement, ITimelineDelegate, IButtonDelegation
    {
        /// <inheritdoc />
        public override void Update(float delta)
        {
            delta = 0.016f;
            base.Update(delta);
            if (targetObject != null)
            {
                targetAnimationController?.UpdateAdditionalOverlays(delta);
                targetAnimationController?.SyncAdditionalOverlayPosition(targetObject.x, targetObject.y);
            }
            dd.Update(delta);
            pollenDrawer.Update(delta);
            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < fingerCuts[i].Count; j++)
                {
                    FingerCut fingerCut = fingerCuts[i][j];
                    float alpha = fingerCut.c.AlphaChannel;
                    if (Mover.MoveVariableToTarget(ref alpha, 0, 10, delta))
                    {
                        _ = fingerCuts[i].Remove(fingerCut);
                        j--;
                    }
                    else
                    {
                        fingerCut.c.AlphaChannel = alpha;
                    }
                }
                fingerTraces[i]?.Update(delta);
            }
            if (earthAnims != null)
            {
                foreach (object obj in earthAnims)
                {
                    ((Image)obj).Update(delta);
                }
            }
            decalsLayer?.Update(delta);
            if (waterLayer != null)
            {
                waterLayer.Update(delta);
                float waterSurfaceY = waterLayer.y;
                float waterLeftX = waterLayer.x;
                float waterRightX = waterLeftX + waterLayer.width;
                if (
                    GameObject.RectInObject(
                        waterLeftX,
                        waterSurfaceY - ActivePhysicsConstants.WaterSurfaceDetectionHeight,
                        waterRightX,
                        waterSurfaceY + ActivePhysicsConstants.WaterSurfaceDetectionHeight,
                        candy
                    )
                )
                {
                    if (!splashes)
                    {
                        waterLayer.AddWaterParticlesAtXY(candy.x, waterSurfaceY + ActivePhysicsConstants.WaterSplashParticleYOffset);
                        CTRSoundMgr.PlaySound(Resources.Snd.ExpWaterSplash);
                    }
                    splashes = true;
                }
                else
                {
                    splashes = false;
                }

                if (candy.y - (candy.texture.quadRects[0].h / 2f) > waterSurfaceY)
                {
                    if (!underwater)
                    {
                        int underwaterCount = Preferences.GetIntForKey("PREFS_UNDERWATER") + 1;
                        Preferences.SetIntForKey(underwaterCount, "PREFS_UNDERWATER", false);
                        if (underwaterCount >= 150)
                        {
                            CTRRootController.PostAchievementName("acDeepDiver");
                        }
                    }
                    underwater = true;
                }
                else
                {
                    underwater = false;
                }
            }
            _ = Mover.MoveVariableToTarget(ref ropeAtOnceTimer, 0, 1, delta);
            ConstraintedPoint constraintedPoint4 = twoParts != 2 ? starL : star;
            float targetCameraX = constraintedPoint4.pos.X - (SCREEN_WIDTH / 2f);
            float targetCameraY = constraintedPoint4.pos.Y - (SCREEN_HEIGHT / 2f);
            float boundedCameraX = FIT_TO_BOUNDARIES(targetCameraX, 0f, mapWidth - SCREEN_WIDTH);
            float boundedCameraY = FIT_TO_BOUNDARIES(targetCameraY, 0f, mapHeight - SCREEN_HEIGHT);
            camera.MoveToXYImmediate(boundedCameraX, boundedCameraY, false);
            if (!freezeCamera || camera.type != CAMERATYPE.CAMERASPEEDDELAY)
            {
                camera.Update(delta);
            }
            if (camera.type == CAMERATYPE.CAMERASPEEDPIXELS)
            {
                float touchEnableDistance = 100f;
                float cameraAcceleration = 800f;
                float cameraDeceleration = 400f;
                float maxCameraSpeed = 1000f;
                float minCameraSpeed = 300f;
                float cameraTargetDistance = VectDistance(camera.pos, Vect(boundedCameraX, boundedCameraY));
                if (cameraTargetDistance < touchEnableDistance)
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
                else if (cameraTargetDistance > initialCameraToStarDistance / 2)
                {
                    camera.speed += delta * cameraAcceleration;
                    camera.speed = MIN(maxCameraSpeed, camera.speed);
                }
                else
                {
                    camera.speed -= delta * cameraDeceleration;
                    camera.speed = MAX(minCameraSpeed, camera.speed);
                }
                if (MathF.Abs(camera.pos.X - boundedCameraX) < 1 && MathF.Abs(camera.pos.Y - boundedCameraY) < 1)
                {
                    camera.type = CAMERATYPE.CAMERASPEEDDELAY;
                    camera.speed = 14f;
                }
            }
            else
            {
                time += delta;
            }
            bool handHoldingCandy = false;
            if (hands != null)
            {
                foreach (MechanicalHand hand in hands)
                {
                    if (hand != null && hand.state == MechanicalHand.STATE_HAND_CANDY)
                    {
                        handHoldingCandy = true;
                        break;
                    }
                }
            }
            if (bungees.Count > 0)
            {
                bool flag = false;
                bool flag2 = false;
                bool flag3 = false;
                int grabCount = bungees.Count;
                int k = 0;
                while (k < grabCount)
                {
                    Grab grab = bungees[k];
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

                    // Process stickTimer for kickable grabs
                    if (rope != null && grab.stickTimer != -1f)
                    {
                        grab.stickTimer += delta;
                        if (grab.stickTimer > Grab.STICK_DELAY)
                        {
                            if (GameObject.RectInObject(mapOriginX, mapOriginY, mapOriginX + mapWidth, mapOriginY + mapHeight, grab))
                            {
                                rope.bungeeAnchor.pin = rope.bungeeAnchor.pos;
                                grab.kicked = false;
                                rope.bungeeAnchor.SetWeight(0.02f);
                                grab.UpdateKickState();
                                CTRSoundMgr.PlaySound(Resources.Snd.ExpSuckerLand);
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
                            grab.stickTimer = -1f;
                        }
                    }

                    if (grab.hasSpider && !grab.spiderActive)
                    {
                        grab.spider.x = grab.x;
                        grab.spider.y = grab.y;
                    }

                    bool shouldProcessGrabRadius = true;

                    if (rope != null)
                    {
                        if (rope.cut == -1 || rope.cutTime != 0)
                        {
                            UpdateRopeWithAntCarryOverride(rope, delta);
                            if (grab.hasSpider)
                            {
                                if (camera.type != CAMERATYPE.CAMERASPEEDPIXELS || !ignoreTouches)
                                {
                                    // Don't let spider activate if rope is not attached to candy
                                    if (grab.shouldActivate && !IsCandyPoint(rope.tail))
                                    {
                                        grab.shouldActivate = false;
                                    }
                                    grab.UpdateSpider(delta);
                                }
                                if (grab.spiderPos == -1f)
                                {
                                    // Only let spider win if rope is attached to candy
                                    if (IsCandyPoint(rope.tail))
                                    {
                                        SpiderWon(grab);
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            shouldProcessGrabRadius = false;
                        }
                    }

                    if (shouldProcessGrabRadius)
                    {
                        if (grab.radius != -1f && grab.rope == null)
                        {
                            if (twoParts != 2)
                            {
                                if (!noCandyL && VectDistance(Vect(grab.x, grab.y), starL.pos) <= grab.radius + ActivePhysicsConstants.CandyGrabPadding)
                                {
                                    Bungee bungee = new Bungee().InitWithHeadAtXYTailAtTXTYandLength(null, grab.x, grab.y, starL, starL.pos.X, starL.pos.Y, grab.radius + ActivePhysicsConstants.CandyGrabPadding);
                                    bungee.bungeeAnchor.pin = bungee.bungeeAnchor.pos;
                                    grab.hideRadius = true;
                                    grab.SetRope(bungee);

                                    // If mouse already has this candy, immediately cut the rope
                                    if (miceManager?.ActiveMouseHasCandy() ?? false)
                                    {
                                        bungee.SetCut(bungee.parts.Count - 2);
                                    }

                                    CTRSoundMgr.PlaySound(Resources.Snd.RopeGet);
                                    if (grab.mover != null)
                                    {
                                        CTRSoundMgr.PlaySound(Resources.Snd.Buzz);
                                    }
                                }
                                if (!noCandyR && grab.rope == null && VectDistance(Vect(grab.x, grab.y), starR.pos) <= grab.radius + ActivePhysicsConstants.CandyGrabPadding)
                                {
                                    Bungee bungee2 = new Bungee().InitWithHeadAtXYTailAtTXTYandLength(null, grab.x, grab.y, starR, starR.pos.X, starR.pos.Y, grab.radius + ActivePhysicsConstants.CandyGrabPadding);
                                    bungee2.bungeeAnchor.pin = bungee2.bungeeAnchor.pos;
                                    grab.hideRadius = true;
                                    grab.SetRope(bungee2);

                                    // If mouse already has this candy, immediately cut the rope
                                    if (miceManager?.ActiveMouseHasCandy() ?? false)
                                    {
                                        bungee2.SetCut(bungee2.parts.Count - 2);
                                    }

                                    CTRSoundMgr.PlaySound(Resources.Snd.RopeGet);
                                    if (grab.mover != null)
                                    {
                                        CTRSoundMgr.PlaySound(Resources.Snd.Buzz);
                                    }
                                }
                            }
                            else
                            {
                                for (int ci = 0; ci < candies.Count; ci++)
                                {
                                    CandyContext ctx = candies[ci];
                                    bool inRange = !ctx.noCandy
                                        && VectDistance(Vect(grab.x, grab.y), ctx.point.pos) <= grab.radius + ActivePhysicsConstants.CandyGrabPadding;
                                    if (!GrabHookAttach.ShouldAttach(grab.radius != -1f, grab.rope == null, !ctx.noCandy, inRange))
                                    {
                                        continue;
                                    }

                                    Bungee bungee3 = new Bungee().InitWithHeadAtXYTailAtTXTYandLength(null, grab.x, grab.y, ctx.point, ctx.point.pos.X, ctx.point.pos.Y, grab.radius + ActivePhysicsConstants.CandyGrabPadding);
                                    bungee3.bungeeAnchor.pin = bungee3.bungeeAnchor.pos;
                                    grab.hideRadius = true;
                                    grab.SetRope(bungee3);
                                    if (ctx.activeRocket != null)
                                    {
                                        ctx.activeRocket.anglePercent = 0f;
                                        ctx.activeRocket.perpSetted = false;
                                        ctx.activeRocket.startRotation += ctx.activeRocket.additionalAngle;
                                        ctx.activeRocket.additionalAngle = 0f;
                                    }

                                    // If mouse already has this candy, immediately cut the rope
                                    if (miceManager?.ActiveMouseHasCandy() ?? false)
                                    {
                                        bungee3.SetCut(bungee3.parts.Count - 2);
                                    }

                                    CTRSoundMgr.PlaySound(Resources.Snd.RopeGet);
                                    if (grab.mover != null)
                                    {
                                        CTRSoundMgr.PlaySound(Resources.Snd.Buzz);
                                    }
                                    break;
                                }
                            }
                            if (grab.rope == null && lightBulbs.Count > 0)
                            {
                                foreach (LightBulb bulb in lightBulbs)
                                {
                                    if (bulb == null || bulb.attachedSock != null)
                                    {
                                        continue;
                                    }
                                    if (VectDistance(Vect(grab.x, grab.y), bulb.constraint.pos) <= grab.radius + ActivePhysicsConstants.CandyGrabPadding)
                                    {
                                        Bungee bungeeBulb = new Bungee().InitWithHeadAtXYTailAtTXTYandLength(null, grab.x, grab.y, bulb.constraint, bulb.constraint.pos.X, bulb.constraint.pos.Y, grab.radius + ActivePhysicsConstants.CandyGrabPadding);
                                        bungeeBulb.bungeeAnchor.pin = bungeeBulb.bungeeAnchor.pos;
                                        grab.hideRadius = true;
                                        grab.SetRope(bungeeBulb);

                                        // Spider can't grab light bulbs - keep it idle
                                        if (grab.hasSpider)
                                        {
                                            grab.shouldActivate = false;
                                        }

                                        CTRSoundMgr.PlaySound(Resources.Snd.RopeGet);
                                        if (grab.mover != null)
                                        {
                                            CTRSoundMgr.PlaySound(Resources.Snd.Buzz);
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                        if (rope != null)
                        {
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
                                float ropeAngle = RADIANS_TO_DEGREES(VectAngleNormalized(v));
                                if (twoParts != 2)
                                {
                                    GameObject gameObject = constraintedPoint2 == starL ? candyL : candyR;
                                    if (!rope.chosenOne)
                                    {
                                        rope.initialCandleAngle = gameObject.rotation - ropeAngle;
                                    }
                                    if (constraintedPoint2 == starL)
                                    {
                                        lastCandyRotateDeltaL = ropeAngle + rope.initialCandleAngle - gameObject.rotation;
                                        flag2 = true;
                                    }
                                    else
                                    {
                                        lastCandyRotateDeltaR = ropeAngle + rope.initialCandleAngle - gameObject.rotation;
                                        flag3 = true;
                                    }
                                    gameObject.rotation = ropeAngle + rope.initialCandleAngle;
                                }
                                else if (!noCandy && constraintedPoint2 == star)
                                {
                                    if (!rope.chosenOne)
                                    {
                                        rope.initialCandleAngle = candyMain.rotation - ropeAngle;
                                    }
                                    lastCandyRotateDelta = ropeAngle + rope.initialCandleAngle - candyMain.rotation;
                                    candyMain.rotation = ropeAngle + rope.initialCandleAngle;
                                    flag = true;
                                }
                                rope.chosenOne = true;
                            }
                            else
                            {
                                rope.chosenOne = false;
                            }
                        }
                    }

                    k++;
                }
                if (twoParts != 2)
                {
                    if (!flag2 && !noCandyL)
                    {
                        candyL.rotation += MIN(5, lastCandyRotateDeltaL);
                        lastCandyRotateDeltaL *= 0.98f;
                    }
                    if (!flag3 && !noCandyR)
                    {
                        candyR.rotation += MIN(5, lastCandyRotateDeltaR);
                        lastCandyRotateDeltaR *= 0.98f;
                    }
                }
                else if (!flag && !noCandy && !handHoldingCandy)
                {
                    candyMain.rotation += MIN(5, lastCandyRotateDelta);
                    lastCandyRotateDelta *= 0.98f;
                }
            }
            // Step every candy point + visual in one pass. candies[0]'s "gone" flag is the
            // singleton `noCandy` (not synced to candies[0].noCandy - see plan); index 1+ use
            // their own noCandy. During split-candy, singleton noCandy is true, so candies[0] is
            // skipped here and its halves are stepped by the twoParts block below.
            for (int ci = 0; ci < candies.Count; ci++)
            {
                CandyContext ctx = candies[ci];
                bool gone = ci == 0 ? noCandy : ctx.noCandy;
                if (gone)
                {
                    continue;
                }
                ctx.point.Update(delta * ropePhysicsSpeed);
                ctx.candy.x = ctx.point.pos.X;
                ctx.candy.y = ctx.point.pos.Y;
                ctx.candy.Update(delta);
                CalculateTopLeft(ctx.candy);
            }
            // Candy-to-candy collision once all candy points are integrated (multi-candy only).
            ResolveCandyCollisions();
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
                if (partsDist > 0)
                {
                    // Abort merge if one half was destroyed to prevent
                    // reviving the broken half into a full candy
                    if (noCandyL || noCandyR)
                    {
                        partsDist = 0f;
                        twoParts = 0;
                    }
                    else if (Mover.MoveVariableToTarget(ref partsDist, 0, 200, delta))
                    {
                        CTRSoundMgr.PlaySound(Resources.Snd.CandyLink);
                        twoParts = 2;
                        noCandy = false;
                        noCandyL = true;
                        noCandyR = true;
                        int candiesUnitedCount = Preferences.GetIntForKey("PREFS_CANDIES_UNITED") + 1;
                        Preferences.SetIntForKey(candiesUnitedCount, "PREFS_CANDIES_UNITED", false);
                        if (candiesUnitedCount == 100)
                        {
                            CTRRootController.PostAchievementName("1432722351", ACHIEVEMENT_STRING("\"Romantic Soul\""));
                        }
                        if (candyBubbleL != null || candyBubbleR != null)
                        {
                            bool leftHasGhost = candyBubbleL != null && DisableGhostCycleForBubble(candyBubbleL);
                            bool rightHasGhost = candyBubbleR != null && DisableGhostCycleForBubble(candyBubbleR);
                            if (candyBubbleL != null && candyBubbleR != null && leftHasGhost && rightHasGhost)
                            {
                                candyBubble = candyBubbleL;
                                shouldRestoreSecondGhost = true;
                                candyBubbleAnimation.visible = false;
                                if (isCandyInGhostBubbleAnimationLoaded)
                                {
                                    candyGhostBubbleAnimation.visible = true;
                                }
                            }
                            else if (candyBubbleL != null && leftHasGhost)
                            {
                                candyBubble = candyBubbleL;
                                shouldRestoreSecondGhost = false;
                                candyBubbleAnimation.visible = false;
                                if (isCandyInGhostBubbleAnimationLoaded)
                                {
                                    candyGhostBubbleAnimation.visible = true;
                                }
                            }
                            else if (candyBubbleR != null && rightHasGhost)
                            {
                                candyBubble = candyBubbleR;
                                shouldRestoreSecondGhost = false;
                                candyBubbleAnimation.visible = false;
                                if (isCandyInGhostBubbleAnimationLoaded)
                                {
                                    candyGhostBubbleAnimation.visible = true;
                                }
                            }
                            else
                            {
                                candyBubble = candyBubbleL ?? candyBubbleR;
                                shouldRestoreSecondGhost = false;
                                candyBubbleAnimation.visible = true;
                                if (isCandyInGhostBubbleAnimationLoaded)
                                {
                                    candyGhostBubbleAnimation.visible = false;
                                }
                                EnableGhostCycleForBubble(candyBubbleL);
                                EnableGhostCycleForBubble(candyBubbleR);
                            }
                            candyBubbleAnimationL.visible = false;
                            candyBubbleAnimationR.visible = false;
                            if (isCandyInGhostBubbleAnimationLeftLoaded)
                            {
                                candyGhostBubbleAnimationL.visible = false;
                            }
                            if (isCandyInGhostBubbleAnimationRightLoaded)
                            {
                                candyGhostBubbleAnimationR.visible = false;
                            }
                        }
                        else
                        {
                            candyBubble = null;
                            shouldRestoreSecondGhost = false;
                            candyBubbleAnimation.visible = false;
                            if (isCandyInGhostBubbleAnimationLoaded)
                            {
                                candyGhostBubbleAnimation.visible = false;
                            }
                        }
                        lastCandyRotateDelta = 0f;
                        lastCandyRotateDeltaL = 0f;
                        lastCandyRotateDeltaR = 0f;
                        star.pos.X = starL.pos.X;
                        star.pos.Y = starL.pos.Y;
                        candy.x = star.pos.X;
                        candy.y = star.pos.Y;
                        CalculateTopLeft(candy);
                        Vector vector = VectSub(starL.pos, starL.prevPos);
                        Vector vector2 = VectSub(starR.pos, starR.prevPos);
                        Vector v2 = Vect((vector.X + vector2.X) / 2f, (vector.Y + vector2.Y) / 2f);
                        star.prevPos = VectSub(star.pos, v2);
                        int bungeeCount = bungees.Count;
                        for (int m = 0; m < bungeeCount; m++)
                        {
                            Bungee rope2 = bungees[m].rope;
                            if (rope2 != null && rope2.cut != rope2.parts.Count - 3 && (rope2.tail == starL || rope2.tail == starR))
                            {
                                ConstraintedPoint constraintedPoint3 = rope2.parts[^2];
                                int restLength = (int)rope2.tail.RestLengthFor(constraintedPoint3);
                                star.AddConstraintwithRestLengthofType(constraintedPoint3, restLength, Constraint.CONSTRAINT.DISTANCE);
                                rope2.tail = star;
                                rope2.parts[^1] = star;
                                rope2.initialCandleAngle = 0f;
                                rope2.chosenOne = false;
                            }
                        }
                        Animation animation = Animation.Animation_createWithResID(Resources.Img.ObjCandyFx);
                        animation.x = candy.x;
                        animation.y = candy.y;
                        animation.anchor = 18;
                        int n = animation.AddAnimationDelayLoopFirstLast(0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 11, 15);
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
            targetObject?.Update(delta);
            // Update additional Om Noms' animations (targets[0] handled above via targetObject).
            for (int ti = 1; ti < targets.Count; ti++)
            {
                targets[ti].targetObject?.Update(delta);
            }
            UpdateLightBulbPhysics(delta);
            UpdateNightLevel(delta);
            conveyors.Update(delta);

            UpdateAntConveyor(delta);

            if (camera.type != CAMERATYPE.CAMERASPEEDPIXELS || !ignoreTouches)
            {
                foreach (object obj2 in stars)
                {
                    Star star = (Star)obj2;
                    star.Update(delta);
                    if (star.timeout > 0 && star.time == 0)
                    {
                        star.GetTimeline(1).delegateTimelineDelegate = aniPool;
                        _ = aniPool.AddChild(star);
                        conveyors.Remove(star);
                        _ = stars.Remove(star);
                        star.timedAnim.PlayTimeline(1);
                        star.PlayTimeline(1);
                        break;
                    }
                    bool canCollect = !nightLevel || star.IsLit;
                    if (!canCollect)
                    {
                        continue;
                    }

                    // Which candy (if any) collects this star. candies[0] keeps its split-aware
                    // test (singleton `noCandy` / candyL,candyR halves); index 1+ are whole candies.
                    bool candyTouchesStar = false;
                    CandyContext collectingCandy = null;
                    for (int ci = 0; ci < candies.Count; ci++)
                    {
                        CandyContext ctx = candies[ci];
                        bool touches = ci == 0
                            ? (twoParts == 2
                                ? GameObject.ObjectsIntersect(candy, star) && !noCandy
                                : (GameObject.ObjectsIntersect(candyL, star) && !noCandyL) ||
                                  (GameObject.ObjectsIntersect(candyR, star) && !noCandyR))
                            : !ctx.noCandy && GameObject.ObjectsIntersect(ctx.candy, star);
                        if (touches)
                        {
                            candyTouchesStar = true;
                            collectingCandy = ctx;
                            break;
                        }
                    }

                    if (candyTouchesStar)
                    {
                        collectingCandy?.candyBlink?.PlayTimeline(1);
                        starsCollected++;
                        // Update RPC with new star count
                        Game1.RPC?.SetLevelPresence(cTRRootController.GetPack(), cTRRootController.GetLevel(), starsCollected, false);
                        if (starsCollected <= hudStar.Length)
                        {
                            hudStar[starsCollected - 1].PlayTimeline(0);
                        }
                        Animation animation2 = Animation.Animation_createWithResID(Resources.Img.ObjStarDisappear);
                        animation2.DoRestoreCutTransparency();
                        animation2.x = star.x;
                        animation2.y = star.y;
                        animation2.anchor = 18;
                        int n2 = animation2.AddAnimationDelayLoopFirstLast(0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 0, 12);
                        animation2.GetTimeline(n2).delegateTimelineDelegate = aniPool;
                        animation2.PlayTimeline(0);
                        _ = aniPool.AddChild(animation2);
                        conveyors.Remove(star);
                        _ = stars.Remove(star);
                        CTRSoundMgr.PlaySound(starsCollected switch
                        {
                            1 => Resources.Snd.Star1,
                            2 => Resources.Snd.Star2,
                            3 => Resources.Snd.Star3,
                            _ => Resources.Snd.Star1
                        });
                        if (targetAnimationController?.IsIdleLoopPlaying() == true)
                        {
                            targetAnimationController.PlayExcited();
                            CTRSoundMgr.PlayOmNomSound(Resources.Snd.MonsterExcited);
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
                float bubbleCaptureRadius = ActivePhysicsConstants.BubbleCaptureRadius;
                if (twoParts != 2)
                {
                    if (!noCandyL && !bubble3.popped && PointInRect(candyL.x, candyL.y, bubble3.x - bubbleCaptureRadius, bubble3.y - bubbleCaptureRadius, bubbleCaptureRadius * 2f, bubbleCaptureRadius * 2f))
                    {
                        if (candyBubbleL != null)
                        {
                            PopBubbleAtXY(bubble3.x, bubble3.y);
                            EnableGhostCycleForBubble(candyBubbleL);
                        }
                        candyBubbleL = bubble3;
                        bool leftHasGhost = DisableGhostCycleForBubble(bubble3);
                        if (leftHasGhost)
                        {
                            if (isCandyInGhostBubbleAnimationLeftLoaded)
                            {
                                candyGhostBubbleAnimationL.visible = true;
                            }
                            candyBubbleAnimationL.visible = false;
                        }
                        else
                        {
                            candyBubbleAnimationL.visible = true;
                            if (isCandyInGhostBubbleAnimationLeftLoaded)
                            {
                                candyGhostBubbleAnimationL.visible = false;
                            }
                        }
                        CTRSoundMgr.PlaySound(Resources.Snd.Bubble);
                        bubble3.popped = true;
                        bubble3.RemoveChildWithID(0);
                        conveyors.Remove(bubble3);
                        break;
                    }
                    if (!noCandyR && !bubble3.popped && PointInRect(candyR.x, candyR.y, bubble3.x - bubbleCaptureRadius, bubble3.y - bubbleCaptureRadius, bubbleCaptureRadius * 2f, bubbleCaptureRadius * 2f))
                    {
                        if (candyBubbleR != null)
                        {
                            PopBubbleAtXY(bubble3.x, bubble3.y);
                            EnableGhostCycleForBubble(candyBubbleR);
                        }
                        bool rightHasGhost = DisableGhostCycleForBubble(bubble3);
                        if (rightHasGhost)
                        {
                            if (isCandyInGhostBubbleAnimationRightLoaded)
                            {
                                candyGhostBubbleAnimationR.visible = true;
                            }
                            candyBubbleAnimationR.visible = false;
                        }
                        else
                        {
                            candyBubbleAnimationR.visible = true;
                            if (isCandyInGhostBubbleAnimationRightLoaded)
                            {
                                candyGhostBubbleAnimationR.visible = false;
                            }
                        }
                        candyBubbleR = bubble3;
                        CTRSoundMgr.PlaySound(Resources.Snd.Bubble);
                        bubble3.popped = true;
                        bubble3.RemoveChildWithID(0);
                        conveyors.Remove(bubble3);
                        break;
                    }
                }
                else if (!noCandy && !bubble3.popped && PointInRect(candy.x, candy.y, bubble3.x - bubbleCaptureRadius, bubble3.y - bubbleCaptureRadius, bubbleCaptureRadius * 2f, bubbleCaptureRadius * 2f))
                {
                    if (candyBubble != null)
                    {
                        PopBubbleAtXY(bubble3.x, bubble3.y);
                        EnableGhostCycleForBubble(candyBubble);
                        if (shouldRestoreSecondGhost)
                        {
                            EnableGhostCycleForBubble(candyBubbleR);
                            candyBubbleR = null;
                            shouldRestoreSecondGhost = false;
                        }
                    }
                    candyBubble = bubble3;
                    bool hasGhost = DisableGhostCycleForBubble(bubble3);
                    if (hasGhost)
                    {
                        candyBubbleAnimation.visible = false;
                        if (isCandyInGhostBubbleAnimationLoaded)
                        {
                            candyGhostBubbleAnimation.visible = true;
                        }
                    }
                    else
                    {
                        candyBubbleAnimation.visible = true;
                        if (isCandyInGhostBubbleAnimationLoaded)
                        {
                            candyGhostBubbleAnimation.visible = false;
                        }
                    }
                    CTRSoundMgr.PlaySound(Resources.Snd.Bubble);
                    bubble3.popped = true;
                    bubble3.RemoveChildWithID(0);
                    conveyors.Remove(bubble3);
                    break;
                }
                else
                {
                    // Additional independent candies (candies[1+]) capture their own bubble.
                    // candies[0] is handled by the singleton branch above; legacy single-candy
                    // levels have candies.Count == 1, so this loop is a no-op for them.
                    bool capturedExtra = false;
                    for (int ci = 1; ci < candies.Count; ci++)
                    {
                        CandyContext ctx = candies[ci];
                        if (ctx.noCandy || ctx.bubble != null || bubble3.popped)
                        {
                            continue;
                        }
                        if (BubbleCapture.Captures(Vect(ctx.candy.x, ctx.candy.y), Vect(bubble3.x, bubble3.y), bubbleCaptureRadius))
                        {
                            bool hasGhost = DisableGhostCycleForBubble(bubble3);
                            ctx.bubble = bubble3;
                            ctx.bubbleHasGhost = hasGhost;
                            BubbleVisualState visualState = BubbleVisualState.ForCapture(hasGhost, ctx.candyGhostBubbleAnimation != null);
                            ctx.candyBubbleAnimation.visible = visualState.ShowNormalBubble;
                            ctx.candyGhostBubbleAnimation.visible = visualState.ShowGhostBubble;
                            CTRSoundMgr.PlaySound(Resources.Snd.Bubble);
                            bubble3.popped = true;
                            bubble3.RemoveChildWithID(0);
                            conveyors.Remove(bubble3);
                            capturedExtra = true;
                            break;
                        }
                    }
                    if (capturedExtra)
                    {
                        break;
                    }
                }
                if (!bubble3.popped && lightBulbs.Count > 0)
                {
                    foreach (LightBulb bulb in lightBulbs)
                    {
                        if (bulb == null || bulb.attachedSock != null)
                        {
                            continue;
                        }
                        if (PointInRect(bulb.x, bulb.y, bubble3.x - BUBBLE_RADIUS, bubble3.y - BUBBLE_RADIUS, BUBBLE_RADIUS * 2f, BUBBLE_RADIUS * 2f))
                        {
                            if (bulb.capturingBubble != null && bulb.capturingBubble != bubble3)
                            {
                                PopLightBulbBubble(bulb);
                            }

                            bool isGhost = DisableGhostCycleForBubble(bubble3);
                            bulb.capturingBubble = bubble3;
                            bulb.capturingGhostBubble = isGhost;
                            bubble3.capturedByBulb = !isGhost;
                            bubble3.popped = true;
                            bubble3.RemoveChildWithID(0);
                            conveyors.Remove(bubble3);
                            CTRSoundMgr.PlaySound(Resources.Snd.Bubble);
                            break;
                        }
                    }
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
            if (ghosts != null)
            {
                foreach (object objGhost in ghosts)
                {
                    Ghost ghost = (Ghost)objGhost;
                    ghost?.Update(delta);
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
                if (Mover.MoveVariableToTarget(ref pump.pumpTouchTimer, 0, 1, delta))
                {
                    OperatePump(pump);
                }
            }

            foreach (BambooTube bambooTube in bambooTubes)
            {
                if (bambooTube == null)
                {
                    continue;
                }

                for (int ci = 0; ci < candies.Count; ci++)
                {
                    CandyContext ctx = candies[ci];
                    bool splitActive = ci == 0 && twoParts != PARTS_NONE;
                    bool inRange = !ctx.noCandy && bambooTube.TryCatchCandy(ctx.point);
                    if (TransportEntry.ShouldEnter(!ctx.noCandy, ctx.targetSock != null, ctx.targetBambooTube != null, ctx.inLantern, splitActive, inRange))
                    {
                        OperateBambooTube(bambooTube, ctx);
                        CTRSoundMgr.PlaySound(Resources.Snd.ExpBambooChute);
                    }
                }

                bambooTube.Update(delta);
            }

            UpdateHands(delta);

            foreach (SteamTube steamTube in tubes)
            {
                if (steamTube != null)
                {
                    steamTube.Update(delta);
                    if (steamTube.steamState != 3)
                    {
                        OperateSteamTube(steamTube, delta);
                    }
                }
            }
            List<Lantern> lanterns = Lantern.GetAllLanterns();
            foreach (Lantern lantern in lanterns)
            {
                lantern.Update(delta);

                bool lanternInactive = lantern.lanternState == Lantern.LanternStateInactive;
                bool groupOccupied = AnyCandyInLantern();
                for (int ci = 0; ci < candies.Count; ci++)
                {
                    CandyContext ctx = candies[ci];
                    bool inRange = VectDistance(ctx.point.pos, Vect(lantern.x, lantern.y)) < 82f;
                    if (!LanternCapture.ShouldCapture(lanternInactive, groupOccupied, !ctx.noCandy, ctx.inLantern, inRange))
                    {
                        continue;
                    }

                    ctx.inLantern = true;
                    if (ci == 0)
                    {
                        isCandyInLantern = true; // alias for not-yet-converted sock/rocket/hand guards
                    }
                    ExhaustRocketForCandy(ctx);
                    ctx.candy.passTransformationsToChilds = true;
                    ctx.candyMain.scaleX = ctx.candyMain.scaleY = 1f;
                    ctx.candyTop.scaleX = ctx.candyTop.scaleY = 1f;
                    Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
                    timeline.AddKeyFrame(KeyFrame.MakePos((int)ctx.candy.x, (int)ctx.candy.y, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                    timeline.AddKeyFrame(KeyFrame.MakePos((int)lantern.x, (int)lantern.y, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.1f));
                    timeline.AddKeyFrame(KeyFrame.MakeScale(0.71f, 0.71f, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                    timeline.AddKeyFrame(KeyFrame.MakeScale(0.3f, 0.3f, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.1f));
                    timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                    timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.1f));
                    ctx.candy.RemoveTimeline(0);
                    ctx.candy.AddTimelinewithID(timeline, 0);
                    ctx.candy.PlayTimeline(0);
                    ReleaseRopesForPoint(ctx.point);
                    DetachActiveHands();
                    if (ci == 0)
                    {
                        if (candyBubble != null)
                        {
                            PopCandyBubble(false);
                        }
                    }
                    else if (ctx.bubble != null)
                    {
                        PopCandyBubble(ctx);
                    }
                    dd.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(lantern.CaptureCandyFromDispatcher), ctx.point, 0.05f);

                    // Trigger special tutorial for lantern
                    TriggerSpecialTutorial(3);
                    break;
                }
            }
            RotatedCircle rotatedCircle6 = null;
            foreach (object obj8 in rotatedCircles)
            {
                RotatedCircle rotatedCircle7 = (RotatedCircle)obj8;
                foreach (object obj9 in bungees)
                {
                    Grab bungee4 = (Grab)obj9;
                    if (VectDistance(Vect(bungee4.x, bungee4.y), Vect(rotatedCircle7.x, rotatedCircle7.y)) <= rotatedCircle7.sizeInPixels + (RTPD(5) * 3f))
                    {
                        if (rotatedCircle7.containedObjects.IndexOf(bungee4) == -1)
                        {
                            rotatedCircle7.containedObjects.Add(bungee4);
                        }
                    }
                    else if (rotatedCircle7.containedObjects.IndexOf(bungee4) != -1)
                    {
                        _ = rotatedCircle7.containedObjects.Remove(bungee4);
                    }
                }
                foreach (object obj10 in bubbles)
                {
                    Bubble bubble4 = (Bubble)obj10;
                    if (VectDistance(Vect(bubble4.x, bubble4.y), Vect(rotatedCircle7.x, rotatedCircle7.y)) <= rotatedCircle7.sizeInPixels + (RTPD(10) * 3f))
                    {
                        if (rotatedCircle7.containedObjects.IndexOf(bubble4) == -1)
                        {
                            rotatedCircle7.containedObjects.Add(bubble4);
                        }
                    }
                    else if (rotatedCircle7.containedObjects.IndexOf(bubble4) != -1)
                    {
                        _ = rotatedCircle7.containedObjects.Remove(bubble4);
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
                _ = rotatedCircles.Remove(rotatedCircle6);
            }
            if (miceManager != null)
            {
                miceManager.Update(delta);

                if (twoParts == 2)
                {
                    // Non-split: the mouse grabs the first in-range grabbable candy (single-occupancy).
                    for (int ci = 0; ci < candies.Count; ci++)
                    {
                        CandyContext ctx = candies[ci];
                        if (ctx.noCandy || ctx.inLantern || ctx.targetSock != null || ctx.targetBambooTube != null)
                        {
                            continue;
                        }
                        if (MouseGrab.ShouldGrab(miceManager.ActiveMouseHasCandy(), !ctx.noCandy, miceManager.IsActiveMouseInRange(ctx.point)))
                        {
                            miceManager.GrabWithActiveMouse(ctx.point, ctx.candy, false);
                            ExhaustRocketForCandy(ctx);
                            TriggerSpecialTutorial(4);
                            break;
                        }
                    }
                }

                // Sync per-candy carried flag from the active mouse (covers grab + every drop path).
                ConstraintedPoint carried = miceManager.ActiveMouseCarriedStar();
                for (int ci = 0; ci < candies.Count; ci++)
                {
                    candies[ci].carriedByMouse = carried != null && candies[ci].point == carried;
                }
            }
            float collisionHalfSize = RTPD(20);
            foreach (object obj11 in socks)
            {
                Sock sock3 = (Sock)obj11;
                sock3.Update(delta);
                if (Mover.MoveVariableToTarget(ref sock3.idleTimeout, 0, 1, delta))
                {
                    sock3.state = Sock.SOCK_IDLE;
                }

                bool wasIdle = sock3.state == Sock.SOCK_IDLE;

                float originalSockRotation = sock3.rotation;
                sock3.rotation = 0f;
                sock3.UpdateRotation();
                float invRotation = DEGREES_TO_RADIANS(0f - originalSockRotation);
                sock3.rotation = originalSockRotation;
                sock3.UpdateRotation();

                float bbSize = collisionHalfSize * 2f;

                // Per-candy: each un-transiting candy can be caught by this idle sock independently.
                bool anyCandyHits = false;
                for (int ci = 0; ci < candies.Count; ci++)
                {
                    CandyContext ctx = candies[ci];
                    Vector ptr = VectRotate(ctx.point.posDelta, invRotation);
                    float bbX = ctx.point.pos.X - collisionHalfSize;
                    float bbY = ctx.point.pos.Y - collisionHalfSize;
                    bool candyHits = ptr.Y >= 0 &&
                        (LineInRect(sock3.t1.X, sock3.t1.Y, sock3.t2.X, sock3.t2.Y, bbX, bbY, bbSize, bbSize) ||
                         LineInRect(sock3.b1.X, sock3.b1.Y, sock3.b2.X, sock3.b2.Y, bbX, bbY, bbSize, bbSize));
                    anyCandyHits = anyCandyHits || candyHits;

                    bool splitActive = ci == 0 && twoParts != PARTS_NONE;
                    if (!wasIdle || !TransportEntry.ShouldEnter(!ctx.noCandy, ctx.targetSock != null, ctx.targetBambooTube != null, ctx.inLantern, splitActive, candyHits))
                    {
                        continue;
                    }

                    foreach (Sock sock4 in socks)
                    {
                        if (sock4 != sock3 && sock4.group == sock3.group)
                        {
                            sock4.state = Sock.SOCK_THROWING;
                            sock4.idleTimeout = 0.8f;
                            ReleaseRopesForPoint(ctx.point);
                            DetachActiveHands();
                            ctx.savedSockSpeed = ActivePhysicsConstants.SockSpeedKoeff * VectLength(ctx.point.v);
                            ctx.savedSockSpeed *= ActivePhysicsConstants.SockTeleportSpeedMultiplier;
                            ctx.targetSock = sock4;
                            if (ci == 0)
                            {
                                savedSockSpeed = ctx.savedSockSpeed; // keep singleton aliases in sync
                                targetSock = sock4;
                            }
                            sock3.light.PlayTimeline(0);
                            sock3.light.visible = true;

                            if (SpecialEvents.IsXmas)
                            {
                                CTRSoundMgr.PlaySound(Resources.Snd.TeleportXmas);
                            }
                            else
                            {
                                CTRSoundMgr.PlaySound(Resources.Snd.Teleport);
                            }

                            dd.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(Selector_teleport), ctx.point, 0.1f);
                            break;
                        }
                    }
                }

                bool bulbHits = false;
                if (!wasIdle && lightBulbs.Count > 0)
                {
                    foreach (LightBulb bulb in lightBulbs)
                    {
                        if (bulb == null || bulb.attachedSock != null)
                        {
                            continue;
                        }
                        Vector bulbDelta = VectRotate(bulb.constraint.posDelta, invRotation);
                        float bulbX = bulb.constraint.pos.X - collisionHalfSize;
                        float bulbY = bulb.constraint.pos.Y - collisionHalfSize;
                        bool bulbHit = bulbDelta.Y >= 0 &&
                            (LineInRect(sock3.t1.X, sock3.t1.Y, sock3.t2.X, sock3.t2.Y, bulbX, bulbY, bbSize, bbSize) ||
                             LineInRect(sock3.b1.X, sock3.b1.Y, sock3.b2.X, sock3.b2.Y, bulbX, bulbY, bbSize, bbSize));
                        if (bulbHit)
                        {
                            bulbHits = true;
                            break;
                        }
                    }
                }

                if (!wasIdle)
                {
                    if (!anyCandyHits && !bulbHits && sock3.idleTimeout == 0f)
                    {
                        sock3.idleTimeout = 0.8f;
                    }
                    continue;
                }

                if (lightBulbs.Count > 0)
                {
                    bool bulbTeleported = false;
                    foreach (LightBulb bulb in lightBulbs)
                    {
                        if (bulb == null || bulb.attachedSock != null)
                        {
                            continue;
                        }
                        Vector bulbDelta = VectRotate(bulb.constraint.posDelta, invRotation);
                        float bulbX = bulb.constraint.pos.X - collisionHalfSize;
                        float bulbY = bulb.constraint.pos.Y - collisionHalfSize;
                        bool bulbHit = bulbDelta.Y >= 0 &&
                            (LineInRect(sock3.t1.X, sock3.t1.Y, sock3.t2.X, sock3.t2.Y, bulbX, bulbY, bbSize, bbSize) ||
                             LineInRect(sock3.b1.X, sock3.b1.Y, sock3.b2.X, sock3.b2.Y, bulbX, bulbY, bbSize, bbSize));

                        if (!bulbHit)
                        {
                            continue;
                        }

                        foreach (Sock sock4 in socks)
                        {
                            if (sock4 != sock3 && sock4.group == sock3.group)
                            {
                                sock4.state = Sock.SOCK_THROWING;
                                sock4.idleTimeout = 0.8f;
                                ReleaseLightBulbRopes(bulb);
                                bulb.sockSpeed = ActivePhysicsConstants.SockSpeedKoeff * VectLength(bulb.constraint.v);
                                bulb.sockSpeed *= ActivePhysicsConstants.SockTeleportSpeedMultiplier;
                                bulb.attachedSock = sock4;
                                sock3.light.PlayTimeline(0);
                                sock3.light.visible = true;

                                if (SpecialEvents.IsXmas)
                                {
                                    CTRSoundMgr.PlaySound(Resources.Snd.TeleportXmas);
                                }
                                else
                                {
                                    CTRSoundMgr.PlaySound(Resources.Snd.Teleport);
                                }

                                dd.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(Selector_dropLightBulbFromSock), bulb, 0.1f);
                                bulbTeleported = true;
                                break;
                            }
                        }

                        if (bulbTeleported)
                        {
                            break;
                        }
                    }
                }
            }
            if (rockets != null)
            {
                foreach (Rocket rocket in rockets)
                {
                    if (rocket == null)
                    {
                        continue;
                    }
                    rocket.Update(delta);
                    rocket.UpdateRotation();
                    // The rocket flies exactly one candy; resolve it (null while idle/unbound).
                    CandyContext rocketCandy = RocketBoundCandy(rocket);
                    ConstraintedPoint rocketStar = rocketCandy?.point ?? star;
                    GameObject rocketCandyMain = rocketCandy?.candyMain ?? candyMain;
                    float dist = VectLength(VectSub(rocketStar.pos, rocket.point.pos));
                    if (rocket.state is Rocket.STATE_ROCKET_FLY or Rocket.STATE_ROCKET_DIST)
                    {
                        for (int i = 0; i < 30; i++)
                        {
                            ConstraintedPoint.SatisfyConstraints(rocketStar);
                            ConstraintedPoint.SatisfyConstraints(rocket.point);
                        }
                        rocket.rotation = AngleTo0_360(rocket.startRotation + rocketCandyMain.rotation - rocket.startCandyRotation);
                    }
                    if (rocket.state == Rocket.STATE_ROCKET_FLY)
                    {
                        lastCandyRotateDelta = 0f;
                        bool ropeRelaxed = false;
                        if (bungees != null)
                        {
                            foreach (Grab bungee in bungees)
                            {
                                if (bungee != null)
                                {
                                    Bungee rope = bungee.rope;
                                    if (rope != null && rope.tail == rocketStar && rope.cut == -1 && rope.relaxed > 0 && !handHoldingCandy)
                                    {
                                        ropeRelaxed = true;
                                        ConstraintedPoint anchor = rope.bungeeAnchor;
                                        ConstraintedPoint tail = rope.parts[^1];
                                        Vector ropeVector = VectSub(anchor.pos, tail.pos);
                                        Vector v1 = VectPerp(ropeVector);
                                        Vector v2 = VectRperp(ropeVector);
                                        float fa = RADIANS_TO_DEGREES(VectAngleNormalized(v1) - DEGREES_TO_RADIANS(rocket.rotation));
                                        float fb = RADIANS_TO_DEGREES(VectAngleNormalized(v2) - DEGREES_TO_RADIANS(rocket.rotation));
                                        rocket.additionalAngle = AngleTo0_360(rocket.additionalAngle);
                                        fa = NearestAngleTofrom(rocket.additionalAngle, fa);
                                        fb = NearestAngleTofrom(rocket.additionalAngle, fb);
                                        float da = MinAngleBetweenAandB(rocket.additionalAngle, fa);
                                        float db = MinAngleBetweenAandB(rocket.additionalAngle, fb);
                                        float target = da < db ? fa : fb;
                                        _ = Mover.MoveVariableToTarget(ref rocket.additionalAngle, target, 90f, delta);
                                    }
                                }
                            }
                        }
                        rocket.rotation += rocket.additionalAngle;
                        rocket.UpdateRotation();
                        float ang = rocket.angle;
                        Vector impulse = VectRotate(Vect(-1f, 0f), ang);
                        float rocketImpulse = rocket.impulse * ActivePhysicsConstants.RocketImpulseScale;
                        impulse = VectMult(impulse, rocketImpulse);
                        if (ropeRelaxed)
                        {
                            impulse = VectMult(impulse, rocket.impulseFactor);
                        }
                        rocketStar.ApplyImpulseDelta(impulse, delta);
                        rocketStar.gravity = vectZero;
                        rocket.point.pos.X = rocketStar.pos.X;
                        rocket.point.pos.Y = rocketStar.pos.Y;
                        if (rocket.time != -1f && Mover.MoveVariableToTarget(ref rocket.time, 0f, 1f, delta))
                        {
                            rocketStar.disableGravity = false;
                            ExhaustRocketForCandy(rocketCandy ?? candies[0]);
                        }
                    }
                    if (rocket.state == Rocket.STATE_ROCKET_DIST)
                    {
                        if (handHoldingCandy || Mover.MoveVariableToTarget(ref dist, 0f, 200f, delta))
                        {
                            rocket.state = Rocket.STATE_ROCKET_FLY;
                        }
                        else
                        {
                            rocket.point.ChangeRestLengthToFor(dist, rocketStar);
                        }
                    }
                    if (rocket.state == Rocket.STATE_ROCKET_IDLE)
                    {
                        for (int ci = 0; ci < candies.Count; ci++)
                        {
                            CandyContext ctx = candies[ci];
                            bool intersects = GameObject.ObjectsIntersectRotatedWithUnrotated(rocket, ctx.candy);
                            bool mouseHasCandy = miceManager?.ActiveMouseHasCandy() ?? false;
                            if (!RocketBind.ShouldBind(rocket.state == Rocket.STATE_ROCKET_IDLE, !ctx.noCandy, ctx.inLantern, mouseHasCandy, intersects))
                            {
                                continue;
                            }

                            rocket.mover?.Pause();
                            rocket.startRotation = rocket.rotation;
                            if (handHoldingCandy)
                            {
                                rocket.point.pos = ctx.point.pos;
                                rocket.point.AddConstraintwithRestLengthofType(ctx.point, 0f, Constraint.CONSTRAINT.NOT_MORE_THAN);
                                rocket.state = Rocket.STATE_ROCKET_FLY;
                            }
                            else
                            {
                                rocket.point.AddConstraintwithRestLengthofType(ctx.point, dist, Constraint.CONSTRAINT.NOT_MORE_THAN);
                                rocket.state = Rocket.STATE_ROCKET_DIST;
                            }
                            lastCandyRotateDelta = 0f;
                            Vector deltaPos = VectSub(ctx.point.pos, ctx.point.prevPos);
                            ctx.point.prevPos = VectAdd(ctx.point.prevPos, VectDiv(deltaPos, ctx.point.disableGravity ? 2f : 1.25f));
                            ctx.point.disableGravity = true;

                            // Exhaust any rocket already bound to this candy before re-binding (one-time-use safety).
                            if (ctx.activeRocket != null && ctx.activeRocket != rocket)
                            {
                                ExhaustRocketForCandy(ctx);
                            }

                            CTRSoundMgr.PlaySound(Resources.Snd.ExpRocketStart);
                            _ = CTRSoundMgr.PlaySoundLooped(Resources.Snd.ExpRocketFlyLooped);
                            ctx.activeRocket = rocket;
                            if (ci == 0)
                            {
                                activeRocket = rocket; // keep singleton alias in sync
                            }
                            rocket.isOperating = -1;
                            rocket.startCandyRotation = ctx.candyMain.rotation;

                            Image grid = Image.Image_createWithResID(Resources.Img.ObjRocket);
                            grid.DoRestoreCutTransparency();

                            if (new RocketSparks().InitWithTotalParticlesAngleandImageGrid(40, rocket.rotation, grid) is RocketSparks rocketSparks)
                            {
                                rocketSparks.particlesDelegate = new Particles.ParticlesFinished(particlesAniPool.ParticlesFinished);
                                rocketSparks.x = rocket.x;
                                rocketSparks.y = rocket.y;
                                rocketSparks.StartSystem(0);
                                _ = particlesAniPool.AddChild(rocketSparks);
                                rocket.particles = rocketSparks;
                            }

                            if (new RocketClouds().InitWithTotalParticlesAngleandImageGrid(20, rocket.rotation, grid) is RocketClouds rocketClouds)
                            {
                                rocketClouds.particlesDelegate = new Particles.ParticlesFinished(particlesAniPool.ParticlesFinished);
                                rocketClouds.x = rocket.x;
                                rocketClouds.y = rocket.y;
                                rocketClouds.StartSystem(0);
                                _ = particlesAniPool.AddChild(rocketClouds);
                                rocket.cloudParticles = rocketClouds;
                            }

                            rocket.StartAnimation();
                            int count = Preferences.GetIntForKey("PREFS_ROCKETS") + 1;
                            Preferences.SetIntForKey(count, "PREFS_ROCKETS", false);
                            if (count >= 100)
                            {
                                CTRRootController.PostAchievementName("acPartyAnimal", ACHIEVEMENT_STRING("\"Party Animal\""));
                            }
                            break;
                        }
                    }
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
                float spikeCollisionRadius = 15f;
                if (!isCandyInLantern && (!spike.electro || (spike.electro && spike.electroOn)))
                {
                    bool flag5 = false;
                    bool flag6;
                    if (twoParts != 2)
                    {
                        flag6 = (LineInRect(spike.t1.X, spike.t1.Y, spike.t2.X, spike.t2.Y, starL.pos.X - spikeCollisionRadius, starL.pos.Y - spikeCollisionRadius, spikeCollisionRadius * 2f, spikeCollisionRadius * 2f) || LineInRect(spike.b1.X, spike.b1.Y, spike.b2.X, spike.b2.Y, starL.pos.X - spikeCollisionRadius, starL.pos.Y - spikeCollisionRadius, spikeCollisionRadius * 2f, spikeCollisionRadius * 2f) || LineInLine(starL.prevPos.X, starL.prevPos.Y, starL.pos.X, starL.pos.Y, spike.t1.X, spike.t1.Y, spike.t2.X, spike.t2.Y) || LineInLine(starL.prevPos.X, starL.prevPos.Y, starL.pos.X, starL.pos.Y, spike.b1.X, spike.b1.Y, spike.b2.X, spike.b2.Y)) && !noCandyL;
                        if (flag6)
                        {
                            flag5 = true;
                        }
                        else
                        {
                            flag6 = (LineInRect(spike.t1.X, spike.t1.Y, spike.t2.X, spike.t2.Y, starR.pos.X - spikeCollisionRadius, starR.pos.Y - spikeCollisionRadius, spikeCollisionRadius * 2f, spikeCollisionRadius * 2f) || LineInRect(spike.b1.X, spike.b1.Y, spike.b2.X, spike.b2.Y, starR.pos.X - spikeCollisionRadius, starR.pos.Y - spikeCollisionRadius, spikeCollisionRadius * 2f, spikeCollisionRadius * 2f) || LineInLine(starR.prevPos.X, starR.prevPos.Y, starR.pos.X, starR.pos.Y, spike.t1.X, spike.t1.Y, spike.t2.X, spike.t2.Y) || LineInLine(starR.prevPos.X, starR.prevPos.Y, starR.pos.X, starR.pos.Y, spike.b1.X, spike.b1.Y, spike.b2.X, spike.b2.Y)) && !noCandyR;
                        }
                    }
                    else
                    {
                        flag6 = (LineInRect(spike.t1.X, spike.t1.Y, spike.t2.X, spike.t2.Y, star.pos.X - spikeCollisionRadius, star.pos.Y - spikeCollisionRadius, spikeCollisionRadius * 2f, spikeCollisionRadius * 2f) || LineInRect(spike.b1.X, spike.b1.Y, spike.b2.X, spike.b2.Y, star.pos.X - spikeCollisionRadius, star.pos.Y - spikeCollisionRadius, spikeCollisionRadius * 2f, spikeCollisionRadius * 2f) || LineInLine(star.prevPos.X, star.prevPos.Y, star.pos.X, star.pos.Y, spike.t1.X, spike.t1.Y, spike.t2.X, spike.t2.Y) || LineInLine(star.prevPos.X, star.prevPos.Y, star.pos.X, star.pos.Y, spike.b1.X, spike.b1.Y, spike.b2.X, spike.b2.Y)) && !noCandy;
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

                        float breakX, breakY;
                        if (twoParts != 2)
                        {
                            if (flag5)
                            {
                                breakX = candyL.x;
                                breakY = candyL.y;
                                noCandyL = true;
                            }
                            else
                            {
                                breakX = candyR.x;
                                breakY = candyR.y;
                                noCandyR = true;
                            }
                        }
                        else
                        {
                            breakX = candy.x;
                            breakY = candy.y;
                            noCandy = true;
                        }
                        ExhaustAllActiveRockets();
                        SpawnCandyBreakParticles(breakX, breakY);
                        ReleaseAllRopes(flag5);
                        DetachActiveHands();
                        DetachActiveSnails();
                        if (restartState != 0 && (twoParts == 2 || !noCandyL || !noCandyR))
                        {
                            dd.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(Selector_gameLost), null, 0.3f);
                        }
                        if (ghosts != null)
                        {
                            foreach (object objGhost in ghosts)
                            {
                                Ghost ghost = (Ghost)objGhost;
                                _ = (ghost?.candyBreak = true);
                            }
                        }
                        return;
                    }
                }
                // Additional candies (index 1+): any uneaten candy touching a spike breaks -> game loss.
                if (!spike.electro || (spike.electro && spike.electroOn))
                {
                    for (int ci = 1; ci < candies.Count; ci++)
                    {
                        CandyContext ctx = candies[ci];
                        if (ctx.noCandy || ctx.inLantern)
                        {
                            continue;
                        }
                        if (!BarrierCollision.Hits(
                            spike.t1.X, spike.t1.Y, spike.t2.X, spike.t2.Y,
                            spike.b1.X, spike.b1.Y, spike.b2.X, spike.b2.Y,
                            ctx.point.pos.X, ctx.point.pos.Y, ctx.point.prevPos.X, ctx.point.prevPos.Y,
                            spikeCollisionRadius))
                        {
                            continue;
                        }

                        PopCandyBubble(ctx);
                        ctx.candy.x = ctx.point.pos.X;
                        ctx.candy.y = ctx.point.pos.Y;
                        ctx.noCandy = true;
                        ExhaustAllActiveRockets();
                        SpawnCandyBreakParticles(ctx.candy.x, ctx.candy.y);
                        ReleaseRopesForPoint(ctx.point);
                        DetachActiveHands();
                        DetachActiveSnails();
                        if (restartState != 0)
                        {
                            dd.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(Selector_gameLost), null, 0.3f);
                        }
                        if (ghosts != null)
                        {
                            foreach (object objGhost in ghosts)
                            {
                                Ghost ghost = (Ghost)objGhost;
                                _ = (ghost?.candyBreak = true);
                            }
                        }
                        return;
                    }
                }
            }
            foreach (object obj15 in bouncers)
            {
                Bouncer bouncer = (Bouncer)obj15;
                bouncer.Update(delta);
                float bouncerCollisionRadius = ActivePhysicsConstants.BouncerCollisionRadius;
                bool anyCandyHit = false;
                for (int ci = 0; ci < candies.Count; ci++)
                {
                    CandyContext ctx = candies[ci];
                    if (ci == 0 && twoParts != 2)
                    {
                        // Split candy: bounce whichever half (left preferred) hits the bouncer.
                        bool flag7 = false;
                        bool flag8 = BarrierCollision.Hits(
                            bouncer.t1.X, bouncer.t1.Y, bouncer.t2.X, bouncer.t2.Y,
                            bouncer.b1.X, bouncer.b1.Y, bouncer.b2.X, bouncer.b2.Y,
                            starL.pos.X, starL.pos.Y, starL.prevPos.X, starL.prevPos.Y,
                            bouncerCollisionRadius) && !noCandyL;
                        if (flag8)
                        {
                            flag7 = true;
                        }
                        else
                        {
                            flag8 = BarrierCollision.Hits(
                                bouncer.t1.X, bouncer.t1.Y, bouncer.t2.X, bouncer.t2.Y,
                                bouncer.b1.X, bouncer.b1.Y, bouncer.b2.X, bouncer.b2.Y,
                                starR.pos.X, starR.pos.Y, starR.prevPos.X, starR.prevPos.Y,
                                bouncerCollisionRadius) && !noCandyR;
                        }
                        if (flag8)
                        {
                            anyCandyHit = true;
                            DetachActiveHands();
                            HandleBouncePtDelta(bouncer, flag7 ? starL : starR, delta);
                        }
                    }
                    else
                    {
                        if (ci == 0 ? noCandy : ctx.noCandy)
                        {
                            continue;
                        }
                        if (BarrierCollision.Hits(
                            bouncer.t1.X, bouncer.t1.Y, bouncer.t2.X, bouncer.t2.Y,
                            bouncer.b1.X, bouncer.b1.Y, bouncer.b2.X, bouncer.b2.Y,
                            ctx.point.pos.X, ctx.point.pos.Y, ctx.point.prevPos.X, ctx.point.prevPos.Y,
                            bouncerCollisionRadius))
                        {
                            anyCandyHit = true;
                            DetachActiveHands();
                            HandleBouncePtDelta(bouncer, ctx.point, delta);
                        }
                    }
                }
                bool bulbHit = false;
                if (lightBulbs.Count > 0)
                {
                    foreach (LightBulb bulb in lightBulbs)
                    {
                        if (bulb == null || bulb.attachedSock != null)
                        {
                            continue;
                        }
                        if (LineInRect(bouncer.t1.X, bouncer.t1.Y, bouncer.t2.X, bouncer.t2.Y, bulb.constraint.pos.X - bouncerCollisionRadius, bulb.constraint.pos.Y - bouncerCollisionRadius, bouncerCollisionRadius * 2f, bouncerCollisionRadius * 2f) || LineInRect(bouncer.b1.X, bouncer.b1.Y, bouncer.b2.X, bouncer.b2.Y, bulb.constraint.pos.X - bouncerCollisionRadius, bulb.constraint.pos.Y - bouncerCollisionRadius, bouncerCollisionRadius * 2f, bouncerCollisionRadius * 2f) || LineInLine(bulb.constraint.prevPos.X, bulb.constraint.prevPos.Y, bulb.constraint.pos.X, bulb.constraint.pos.Y, bouncer.t1.X, bouncer.t1.Y, bouncer.t2.X, bouncer.t2.Y) || LineInLine(bulb.constraint.prevPos.X, bulb.constraint.prevPos.Y, bulb.constraint.pos.X, bulb.constraint.pos.Y, bouncer.b1.X, bouncer.b1.Y, bouncer.b2.X, bouncer.b2.Y))
                        {
                            HandleBouncePtDelta(bouncer, bulb.constraint, delta);
                            bulbHit = true;
                        }
                    }
                }
                if (!anyCandyHit && !bulbHit)
                {
                    bouncer.skip = false;
                }
            }
            if (waterLayer != null && waterLevel > -SCREEN_HEIGHT && waterSpeed > 0f)
            {
                _ = Mover.MoveVariableToTarget(ref waterLevel, -SCREEN_HEIGHT, waterSpeed, delta);
                waterLayer.y = mapOriginY + mapHeight - waterLevel;
                waterLayer.height = waterLevel > 0f ? (int)waterLevel : 0;
            }
            float candyRadius = ActivePhysicsConstants.WaterCandyCollisionRadius;
            bool rocketInWater = false;
            float waterRocketDamping = ActivePhysicsConstants.WaterDamping * ActivePhysicsConstants.WaterRocketDampingMultiplier;
            if (waterLayer != null
                && waterLevel > 0f
                && star.pos.Y > waterLayer.y
                && star.pos.X + candyRadius >= waterLayer.x
                && star.pos.X - candyRadius <= waterLayer.x + waterLayer.width)
            {
                float damping = ActivePhysicsConstants.WaterDamping;
                float verticalWaterImpulse = ActivePhysicsConstants.WaterVerticalImpulseBase / star.weight;
                if (activeRocket != null)
                {
                    rocketInWater = true;
                    verticalWaterImpulse /= ActivePhysicsConstants.WaterRocketImpulseDivisor;
                    damping *= ActivePhysicsConstants.WaterRocketDampingMultiplier;
                    if (activeRocket.state == Rocket.STATE_ROCKET_FLY)
                    {
                        CTRSoundMgr.PlaySound(Resources.Snd.ExpRocketInWater);
                        ExhaustAllActiveRockets();
                    }
                }
                star.ApplyImpulseDelta(Vect(-star.v.X / damping, (-star.v.Y / damping) + verticalWaterImpulse), delta);
            }
            if (waterLayer != null && bungees != null)
            {
                foreach (Grab grab in bungees)
                {
                    if (grab != null && grab.kickable && grab.kicked && grab.y > waterLayer.y && grab.rope != null)
                    {
                        float damping = ActivePhysicsConstants.WaterDamping;
                        ConstraintedPoint anchor = grab.rope.bungeeAnchor;
                        anchor.ApplyImpulseDelta(Vect(-anchor.v.X / damping, (-anchor.v.Y / damping) + ActivePhysicsConstants.WaterRopeAnchorImpulse), delta);
                    }
                }
            }
            if (snailobjects != null && twoParts == 2 && snailobjects.Count > 0)
            {
                for (int i = snailobjects.Count - 1; i >= 0; i--)
                {
                    Snail snail = snailobjects[i];
                    if (snail == null)
                    {
                        snailobjects.RemoveAt(i);
                        continue;
                    }

                    snail.Update(delta);

                    if (snail.state == Snail.SNAIL_STATE_ACTIVE)
                    {
                        snail.rotation = candyMain.rotation - snail.startRotation;
                    }

                    if (snail.state == Snail.SNAIL_STATE_INACTIVE && !noCandy && GameObject.ObjectsIntersect(candy, snail))
                    {
                        DetachActiveSnails();
                        snail.startRotation += candyMain.rotation;
                        snail.AttachToPoint(star);
                        star.SetWeight(star.weight + 3f);
                    }

                    if (snail.state == Snail.SNAIL_STATE_VANISHED)
                    {
                        snailobjects.RemoveAt(i);
                    }
                }
            }
            float bubbleLift = ActivePhysicsConstants.BubbleImpulseY;
            float bubbleDamping = ActivePhysicsConstants.BubbleImpulseDamping;
            if (twoParts == 0)
            {
                if (candyBubbleL != null)
                {
                    if (gravityButton != null && !gravityNormal)
                    {
                        starL.ApplyImpulseDelta(Vect((0f - starL.v.X) / bubbleDamping, ((0f - starL.v.Y) / bubbleDamping) - bubbleLift), delta);
                    }
                    else
                    {
                        starL.ApplyImpulseDelta(Vect((0f - starL.v.X) / bubbleDamping, ((0f - starL.v.Y) / bubbleDamping) + bubbleLift), delta);
                    }
                }
                if (candyBubbleR != null)
                {
                    if (gravityButton != null && !gravityNormal)
                    {
                        starR.ApplyImpulseDelta(Vect((0f - starR.v.X) / bubbleDamping, ((0f - starR.v.Y) / bubbleDamping) - bubbleLift), delta);
                    }
                    else
                    {
                        starR.ApplyImpulseDelta(Vect((0f - starR.v.X) / bubbleDamping, ((0f - starR.v.Y) / bubbleDamping) + bubbleLift), delta);
                    }
                }
            }
            if (twoParts == 1)
            {
                if (candyBubbleR != null || candyBubbleL != null)
                {
                    if (gravityButton != null && !gravityNormal)
                    {
                        starL.ApplyImpulseDelta(Vect((0f - starL.v.X) / bubbleDamping, ((0f - starL.v.Y) / bubbleDamping) - bubbleLift), delta);
                        starR.ApplyImpulseDelta(Vect((0f - starR.v.X) / bubbleDamping, ((0f - starR.v.Y) / bubbleDamping) - bubbleLift), delta);
                    }
                    else
                    {
                        starL.ApplyImpulseDelta(Vect((0f - starL.v.X) / bubbleDamping, ((0f - starL.v.Y) / bubbleDamping) + bubbleLift), delta);
                        starR.ApplyImpulseDelta(Vect((0f - starR.v.X) / bubbleDamping, ((0f - starR.v.Y) / bubbleDamping) + bubbleLift), delta);
                    }
                }
            }
            else if (candyBubble != null)
            {
                if (gravityButton != null && !gravityNormal)
                {
                    star.ApplyImpulseDelta(Vect((0f - star.v.X) / bubbleDamping, ((0f - star.v.Y) / bubbleDamping) - bubbleLift), delta);
                }
                else
                {
                    star.ApplyImpulseDelta(Vect((0f - star.v.X) / bubbleDamping, ((0f - star.v.Y) / bubbleDamping) + bubbleLift), delta);
                }
            }
            // Per-candy bubble lift for additional candies. candies[0] handled above via `star`.
            for (int ci = 1; ci < candies.Count; ci++)
            {
                CandyContext ctx = candies[ci];
                if (ctx.bubble == null || ctx.noCandy)
                {
                    continue;
                }
                float lift = (gravityButton != null && !gravityNormal) ? -bubbleLift : bubbleLift;
                ctx.point.ApplyImpulseDelta(
                    Vect((0f - ctx.point.v.X) / bubbleDamping, ((0f - ctx.point.v.Y) / bubbleDamping) + lift),
                    delta);
            }
            if (activeRocket != null)
            {
                float rocketDamping = ActivePhysicsConstants.RocketActiveVelocityDamping;
                if (rocketInWater)
                {
                    rocketDamping = waterRocketDamping;
                }
                star.ApplyImpulseDelta(
                    Vect(
                        -star.v.X / rocketDamping,
                        -star.v.Y / rocketDamping
                    ),
                    delta
                );
            }
            if (lightBulbs.Count > 0)
            {
                foreach (LightBulb bulb in lightBulbs)
                {
                    if (bulb == null || bulb.attachedSock != null || bulb.capturingBubble == null)
                    {
                        continue;
                    }
                    if (gravityButton != null && !gravityNormal)
                    {
                        bulb.constraint.ApplyImpulseDelta(Vect((0f - bulb.constraint.v.X) / bubbleDamping, ((0f - bulb.constraint.v.Y) / bubbleDamping) - bubbleLift), delta);
                    }
                    else
                    {
                        bulb.constraint.ApplyImpulseDelta(Vect((0f - bulb.constraint.v.X) / bubbleDamping, ((0f - bulb.constraint.v.Y) / bubbleDamping) + bubbleLift), delta);
                    }
                }
            }

            ApplyAntCarryToCandyPosition();

            // Snapshot candies for pure decisions.
            List<CandyView> candyViews = new(candies.Count);
            for (int ci = 0; ci < candies.Count; ci++)
            {
                // A candy captured in a lantern is not a mouth-open candidate.
                if (candies[ci].inLantern)
                {
                    continue;
                }
                candyViews.Add(candies[ci].ToView());
            }

            bool canInteractWithTarget = !nightLevel || isNightTargetAwake == true;
            for (int ti = 0; ti < targets.Count; ti++)
            {
                TargetContext t = targets[ti];
                if (t.targetObject == null || t.asleep)
                {
                    continue;
                }
                Vector targetPos = Vect(t.targetObject.x, t.targetObject.y);

                if (!t.mouthOpen && canInteractWithTarget)
                {
                    if (CandyDecisions.ShouldOpenMouth(targetPos, candyViews, 200f))
                    {
                        t.mouthOpen = true;
                        t.controller?.PlayMouthOpening();
                        CTRSoundMgr.PlayOmNomSound(Resources.Snd.MonsterOpen);
                        t.mouthCloseTimer = 1f;
                    }
                }
                else if (t.mouthCloseTimer > 0 && canInteractWithTarget)
                {
                    float timer = t.mouthCloseTimer;
                    _ = Mover.MoveVariableToTarget(ref timer, 0, 1, delta);
                    t.mouthCloseTimer = timer;
                    if (t.mouthCloseTimer <= 0)
                    {
                        if (!CandyDecisions.ShouldOpenMouth(targetPos, candyViews, 200f))
                        {
                            t.mouthOpen = false;
                            t.controller?.PlayMouthClosing();
                            CTRSoundMgr.PlayOmNomSound(Resources.Snd.MonsterClose);
                            tummyTeasers++;
                            if (tummyTeasers >= 10)
                            {
                                CTRRootController.PostAchievementName("1058281905", ACHIEVEMENT_STRING("\"Tummy Teaser\""));
                            }
                        }
                        else
                        {
                            t.mouthCloseTimer = 1f;
                        }
                    }
                }
            }
            // Eat: an uneaten candy entering an open mouth is consumed; that Om Nom sleeps.
            if (restartState != 0 && canInteractWithTarget)
            {
                for (int ti = 0; ti < targets.Count; ti++)
                {
                    TargetContext t = targets[ti];
                    if (t.asleep || !t.mouthOpen || t.targetObject == null)
                    {
                        continue;
                    }
                    for (int ci = 0; ci < candies.Count; ci++)
                    {
                        CandyContext ctx = candies[ci];
                        if (ctx.noCandy)
                        {
                            continue;
                        }
                        ctx.candy.x = ctx.point.pos.X;
                        ctx.candy.y = ctx.point.pos.Y;
                        if (GameObject.ObjectsIntersect(ctx.candy, t.targetObject))
                        {
                            ctx.noCandy = true;
                            if (ci == 0)
                            {
                                noCandy = true;
                            }
                            ReleaseRopesForPoint(ctx.point);
                            ctx.candy.visible = false;
                            t.asleep = true;
                            t.mouthOpen = false;
                            t.controller?.PlayChewing();
                            CTRSoundMgr.PlayOmNomSound(Resources.Snd.MonsterChewing);
                            break;
                        }
                    }
                }

                // Win when every candy has been consumed, excluding candies hidden in transport.
                List<CandyView> allCandyViews = new(candies.Count);
                for (int ci = 0; ci < candies.Count; ci++)
                {
                    allCandyViews.Add(candies[ci].ToView());
                }
                if (CandyDecisions.AllConsumed(allCandyViews))
                {
                    GameWon();
                    return;
                }
            }
            // Lose if any uneaten candy leaves the screen. Mark each leaver consumed-as-lost.
            bool anyLeft = false;
            for (int ci = 0; ci < candies.Count; ci++)
            {
                CandyContext ctx = candies[ci];
                if (!ctx.noCandy && PointOutOfScreen(ctx.point))
                {
                    ctx.noCandy = true;
                    if (ci == 0)
                    {
                        noCandy = true;
                    }
                    anyLeft = true;
                }
            }
            if (twoParts != 2)
            {
                if (!noCandyL && PointOutOfScreen(starL))
                {
                    noCandyL = true;
                    anyLeft = true;
                }
                if (!noCandyR && PointOutOfScreen(starR))
                {
                    noCandyR = true;
                    anyLeft = true;
                }
            }
            if (anyLeft)
            {
                ExhaustAllActiveRockets();
                if (restartState != 0)
                {
                    int candiesLostCount = Preferences.GetIntForKey("PREFS_CANDIES_LOST") + 1;
                    Preferences.SetIntForKey(candiesLostCount, "PREFS_CANDIES_LOST", false);
                    if (candiesLostCount == 50)
                    {
                        CTRRootController.PostAchievementName("681497443", ACHIEVEMENT_STRING("\"Weight Loser\""));
                    }
                    if (candiesLostCount == 200)
                    {
                        CTRRootController.PostAchievementName("1058341297", ACHIEVEMENT_STRING("\"Calorie Minimizer\""));
                    }
                    GameLost();
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
                if (gravityButton != null && ((Button)gravityButton.GetChild(gravityButton.On() ? 1 : 0)).IsInTouchZoneXYforTouchDown(p.X, p.Y, true))
                {
                    flag12 = true;
                }
                if (candyBubble != null || (twoParts != 2 && (candyBubbleL != null || candyBubbleR != null)))
                {
                    foreach (object obj18 in bubbles)
                    {
                        Bubble bubble5 = (Bubble)obj18;
                        if (candyBubble != null && PointInRect(p.X, p.Y, star.pos.X - 60f, star.pos.Y - 60f, 120f, 120f))
                        {
                            flag12 = true;
                            break;
                        }
                        if (candyBubbleL != null && PointInRect(p.X, p.Y, starL.pos.X - 60f, starL.pos.Y - 60f, 120f, 120f))
                        {
                            flag12 = true;
                            break;
                        }
                        if (candyBubbleR != null && PointInRect(p.X, p.Y, starR.pos.X - 60f, starR.pos.Y - 60f, 120f, 120f))
                        {
                            flag12 = true;
                            break;
                        }
                    }
                }
                foreach (object obj19 in spikes)
                {
                    Spikes spike2 = (Spikes)obj19;
                    if (spike2.rotateButton != null && spike2.rotateButton.IsInTouchZoneXYforTouchDown(p.X, p.Y, true))
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
                    if (VectDistance(Vect(p.X, p.Y), Vect(rotatedCircle8.handle1.X, rotatedCircle8.handle1.Y)) <= 90f || VectDistance(Vect(p.X, p.Y), Vect(rotatedCircle8.handle2.X, rotatedCircle8.handle2.Y)) <= 90f)
                    {
                        flag12 = true;
                        break;
                    }
                }
                foreach (object obj22 in bungees)
                {
                    Grab bungee5 = (Grab)obj22;
                    if (bungee5.wheel && PointInRect(p.X, p.Y, bungee5.x - 110f, bungee5.y - 110f, 220f, 220f))
                    {
                        flag12 = true;
                        break;
                    }
                    if (bungee5.moveLength > 0 && (PointInRect(p.X, p.Y, bungee5.x - 65f, bungee5.y - 65f, 130f, 130f) || bungee5.moverDragging != -1))
                    {
                        flag12 = true;
                        break;
                    }
                }
                if (!flag12)
                {
                    Vector s = default;
                    Grab grab2 = null;
                    Bungee nearestBungeeSegmentByBeziersPointsatXYgrab = GetNearestBungeeSegmentByBeziersPointsatXYgrab(ref s, slastTouch.X + camera.pos.X, slastTouch.Y + camera.pos.Y, ref grab2);
                    _ = (nearestBungeeSegmentByBeziersPointsatXYgrab?.highlighted = true);
                }
            }
            if (Mover.MoveVariableToTarget(ref dimTime, 0, 1, delta))
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
                outcomeTransitionActive = false;
            }
        }

        /// <summary>
        /// Updates mechanical hand behavior, candy attachment, hand claps, and hand ordering.
        /// </summary>
        /// <param name="delta">Elapsed time in seconds since the last update.</param>
        private void UpdateHands(float delta)
        {
            if (hands == null || hands.Count <= 0)
            {
                return;
            }

            int selectedHandIndex = hands.Count - 1;
            bool reorderHands = false;

            foreach (MechanicalHand hand in hands)
            {
                if (hand == null)
                {
                    continue;
                }

                hand.Update(delta);
                CandyContext heldCandy = HandHeldCandy(hand);
                if (hand.state == MechanicalHand.STATE_HAND_CANDY && heldCandy != null)
                {
                    heldCandy.candy.drawX += hand.cPoint.pos.X - heldCandy.point.pos.X;
                    heldCandy.candy.drawY += hand.cPoint.pos.Y - heldCandy.point.pos.Y;
                    heldCandy.point.pos = hand.cPoint.pos;

                    if (hand.doRotateCandy)
                    {
                        if (hand.rotatingSegment != null)
                        {
                            heldCandy.candyMain.rotation += hand.rotatingSegment.RotationDelta();
                        }
                    }
                    else if (activeRocket != null)
                    {
                        _ = hand.IsRotating();
                        hand.doRotateCandy = true;
                    }
                }

                // Default distance for the grab test: nearest grabbable candy to this idle hand.
                CandyContext nearestCandy = NearestGrabbableCandy(hand, out float distance);
                foreach (MechanicalHand otherHand in hands)
                {
                    if (otherHand == null || otherHand == hand)
                    {
                        continue;
                    }

                    if (otherHand.state == MechanicalHand.STATE_HAND_CANDY)
                    {
                        distance = VectDistance(hand.cPoint.pos, otherHand.cPoint.pos);
                    }

                    if (hand.state == MechanicalHand.STATE_HAND_IDLE && otherHand.state == MechanicalHand.STATE_HAND_IDLE)
                    {
                        float handDistance = VectDistance(hand.cPoint.pos, otherHand.cPoint.pos);
                        if (handDistance < MechanicalHand.MH_CLAP_DISTANCE)
                        {
                            if ((hand.clapTimer <= 0f || otherHand.clapTimer <= 0f) && (hand.canPlayClap || otherHand.canPlayClap))
                            {
                                PlayMechanicalHandClapEffectAt(otherHand.ClawPosition());
                                hand.AnimateClap();
                                otherHand.AnimateClap();
                                CTRSoundMgr.PlaySound(Resources.Snd.ExpHandClap);
                            }

                            hand.clapTimer = MechanicalHand.MH_CLAP_COOLDOWN;
                            otherHand.clapTimer = MechanicalHand.MH_CLAP_COOLDOWN;
                        }
                    }
                }

                if (nearestCandy != null
                    && HandGrab.ShouldGrab(
                        hand.state == MechanicalHand.STATE_HAND_IDLE,
                        !nearestCandy.noCandy,
                        nearestCandy.inLantern,
                        nearestCandy.targetSock != null,
                        distance < MechanicalHand.MH_GRAB_DISTANCE))
                {
                    CandyContext ctx = nearestCandy;

                    // Hand-stealing: release any other hand currently holding this same candy.
                    if (hands.Count > 1)
                    {
                        foreach (MechanicalHand otherHand in hands)
                        {
                            if (otherHand != null && otherHand != hand
                                && otherHand.state == MechanicalHand.STATE_HAND_CANDY
                                && ctx.capturingHand == otherHand)
                            {
                                otherHand.cPoint.RemoveConstraint(ctx.point);
                                otherHand.state = MechanicalHand.STATE_HAND_RELEASE;
                                otherHand.releaseSoundPlayed = false;
                                reorderHands = true;
                                break;
                            }
                        }
                    }

                    hand.cPoint.AddConstraintwithRestLengthofType(ctx.point, 1f, Constraint.CONSTRAINT.NOT_MORE_THAN);
                    hand.state = MechanicalHand.STATE_HAND_CANDY;
                    hand.releaseSoundPlayed = false;
                    selectedHandIndex = hands.IndexOf(hand);
                    ctx.capturingHand = hand;

                    ResetConveyor();
                    BlockConveyor();

                    if (ctx == candies[0] && candyBubble != null)
                    {
                        candyBubble = null;
                        candyBubbleAnimation.visible = false;
                        Vector clawPosition = hand.ClawPosition();
                        PopBubbleAtXY(clawPosition.X, clawPosition.Y);
                    }
                    else if (ctx.bubble != null)
                    {
                        Vector clawPosition = hand.ClawPosition();
                        PopBubbleAtXY(clawPosition.X, clawPosition.Y);
                        PopCandyBubble(ctx);
                    }

                    if (activeRocket != null)
                    {
                        int count = Preferences.GetIntForKey("PREFS_GRAB_ROCKET") + 1;
                        Preferences.SetIntForKey(count, "PREFS_GRAB_ROCKET", false);
                        if (count >= 50)
                        {
                            CTRRootController.PostAchievementName("acRoboMaster", ACHIEVEMENT_STRING("\"Robo Master\""));
                        }
                    }

                    DetachActiveSnails();
                    miceManager?.ForceDropCandy();
                    RestoreCandyProperties(ctx);
                    hand.AnimateCatchWithCandyPartsandAnimationsPool([ctx.candy, ctx.candyMain, ctx.candyTop], aniPool);
                    CTRSoundMgr.PlaySound(Resources.Snd.ExpHandCatch);
                }

                if (hand.state == MechanicalHand.STATE_HAND_RELEASE && distance > MechanicalHand.MH_RELEASE_DISTANCE)
                {
                    hand.state = MechanicalHand.STATE_HAND_IDLE;
                    if (!hand.releaseSoundPlayed)
                    {
                        CTRSoundMgr.PlaySound(Resources.Snd.ExpHandDrop);
                    }
                    hand.releaseSoundPlayed = false;
                }
            }

            if (reorderHands && selectedHandIndex >= 0 && selectedHandIndex != hands.Count - 1)
            {
                MechanicalHand selectedHand = hands[selectedHandIndex];
                if (selectedHand != null)
                {
                    _ = hands.Remove(selectedHand);
                    hands.Add(selectedHand);
                }
            }
        }

        /// <summary>
        /// Spawns a short-lived clap effect for idle hand proximity claps.
        /// </summary>
        /// <param name="position">World position where the effect should appear.</param>
        private void PlayMechanicalHandClapEffectAt(Vector position)
        {
            Image clapEffect = Image.Image_createWithResIDQuad(Resources.Img.ObjRoboHand, 9);
            clapEffect.anchor = 18;
            clapEffect.x = position.X;
            clapEffect.y = position.Y;
            _ = aniPool.AddChild(clapEffect);

            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline.AddKeyFrame(KeyFrame.MakeScale(1f, 1f, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0f));
            timeline.AddKeyFrame(KeyFrame.MakeScale(1.2f, 1.2f, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.2f));
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0f));
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.2f));
            timeline.delegateTimelineDelegate = aniPool;

            _ = clapEffect.AddTimeline(timeline);
            clapEffect.PlayTimeline(0);
        }

        /// <summary>
        /// Plays the matching special tutorial and hides all other special tutorial prompts.
        /// </summary>
        /// <param name="tutorialId">Special tutorial identifier to trigger.</param>
        private void TriggerSpecialTutorial(int tutorialId)
        {
            if (special != tutorialId)
            {
                return;
            }

            special = 0;

            foreach (object tutorial in tutorials)
            {
                TutorialText tutorialText = (TutorialText)tutorial;
                if (tutorialText.special == tutorialId)
                {
                    tutorialText.PlayTimeline(0);
                }
                else
                {
                    Timeline currentTimeline = tutorialText.GetCurrentTimeline();
                    currentTimeline?.JumpToTrackKeyFrame(3, 2);
                    tutorialText.color = RGBAColor.transparentRGBA;
                    currentTimeline?.StopTimeline();
                }
            }

            foreach (object tutorialImageObj in tutorialImages)
            {
                GameObjectSpecial tutorialImage = (GameObjectSpecial)tutorialImageObj;
                if (tutorialImage.special == tutorialId)
                {
                    tutorialImage.PlayTimeline(0);
                }
                else
                {
                    Timeline currentTimeline = tutorialImage.GetCurrentTimeline();
                    currentTimeline?.JumpToTrackKeyFrame(3, 2);
                    tutorialImage.color = RGBAColor.transparentRGBA;
                    currentTimeline?.StopTimeline();
                }
            }
        }
    }
}
