using System;
using System.Collections.Generic;
using System.Linq;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.Framework.Platform;
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
            foreach (PauseSwitcher switcher in pauseSwitchers)
            {
                switcher?.Update(delta);
            }
            pauseSwitcherWaves?.Update(delta);
            for (int ti = 0; ti < targets.Count; ti++)
            {
                TargetContext t = targets[ti];
                if (t.targetObject != null)
                {
                    if (!timeFrozen)
                    {
                        t.controller?.UpdateAdditionalOverlays(delta);
                    }
                    t.controller?.SyncAdditionalOverlayPosition(t.targetObject.x, t.targetObject.y);
                }
            }
            dd.Update(delta);
            pollenDrawer.Update(delta);
            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
            UpdatePointerGestureVisuals(delta);
            gravityState.UpdateEarthAnimations(delta);
            decalsLayer?.Update(delta);
            if (waterLayer != null)
            {
                waterLayer.Update(delta);
                float waterSurfaceY = waterLayer.y;
                float waterLeftX = waterLayer.x;
                float waterRightX = waterLeftX + waterLayer.width;
                foreach (CandyBody body in ActiveCandyBodies(CandyInteraction.Water))
                {
                    // Bodies that don't interact with water (e.g. light bulbs) make no splash and
                    // don't count toward the "Deep Diver" underwater achievement.
                    if (!body.Owner.Capabilities.CanFloatInWater)
                    {
                        continue;
                    }
                    if (GameObject.RectInObject(
                            waterLeftX,
                            waterSurfaceY - ActivePhysicsConstants.WaterSurfaceDetectionHeight,
                            waterRightX,
                            waterSurfaceY + ActivePhysicsConstants.WaterSurfaceDetectionHeight,
                            body.Visual))
                    {
                        if (!body.Splashes)
                        {
                            waterLayer.AddWaterParticlesAtXY(body.Visual.x, waterSurfaceY + ActivePhysicsConstants.WaterSplashParticleYOffset);
                            CTRSoundMgr.PlaySound(Resources.Snd.ExpWaterSplash);
                        }
                        body.Splashes = true;
                    }
                    else
                    {
                        body.Splashes = false;
                    }

                    if (GameObject.BoundsTopY(body.Visual) > waterSurfaceY)
                    {
                        if (!body.Underwater)
                        {
                            int underwaterCount = Preferences.GetIntForKey("PREFS_UNDERWATER") + 1;
                            Preferences.SetIntForKey(underwaterCount, "PREFS_UNDERWATER", false);
                            if (underwaterCount >= 150)
                            {
                                CTRRootController.PostAchievementName("acDeepDiver");
                            }
                        }
                        body.Underwater = true;
                    }
                    else
                    {
                        body.Underwater = false;
                    }
                }
            }
            _ = Mover.MoveVariableToTarget(ref ropeAtOnceTimer, 0, 1, delta);

            ConstraintedPoint constraintedPoint4 = CameraFocusPoint();
            float targetCameraX = constraintedPoint4.pos.X - (SCREEN_WIDTH / 2f);
            float targetCameraY = constraintedPoint4.pos.Y - (SCREEN_HEIGHT / 2f);
            Vector boundedCamera = BoundedCameraPosition(targetCameraX, targetCameraY);
            float boundedCameraX = boundedCamera.X;
            float boundedCameraY = boundedCamera.Y;
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

            // Project where the tracking just left the camera onto the current viewport. Last,
            // because this reads the tracked position and writes only what gets drawn.
            ApplyCameraFit(ScreenPresentation.Instance.Snapshot);

            if (bungees.Count > 0)
            {
                // Bodies whose rotation a rope already drove this frame; one rope per body.
                HashSet<CandyBody> rotatedBodies = [];
                int grabCount = bungees.Count;
                int k = 0;
                while (k < grabCount)
                {
                    Grab grab = bungees[k];
                    grab.Update(delta);

                    if (grab.GunSource is GunSource gunSource)
                    {
                        gunSource.TrackAim(Vect(grab.x, grab.y), star.pos);
                        gunSource.TrackFiredCup(star.pos, candy.rotation);
                    }

                    Bungee rope = grab.Rope;
                    if (grab.mover != null)
                    {
                        grab.SyncRopeAnchor();
                        grab.ReCalcCircle();
                    }

                    // A detached suction cup that has been trying to stick for long enough re-sticks,
                    // but only where there is wall to stick to.
                    if (rope != null && grab.Mount is SuctionMount mount && mount.TickSticking(delta))
                    {
                        if (GameObject.RectInObject(mapOriginX, mapOriginY, mapOriginX + mapWidth, mapOriginY + mapHeight, grab))
                        {
                            mount.Remount(grab);
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
                    }

                    if (grab.Spider is SpiderRider idleRider && idleRider.IsAttached && !idleRider.IsWalking)
                    {
                        idleRider.Animation.x = grab.x;
                        idleRider.Animation.y = grab.y;
                    }

                    bool shouldProcessGrabRadius = true;

                    if (rope != null)
                    {
                        if (grab.Attachment.IsSimulated)
                        {
                            UpdateRopeWithAntCarryOverride(rope, delta);
                            if (grab.Spider is SpiderRider rider && rider.IsAttached)
                            {
                                if (camera.type != CAMERATYPE.CAMERASPEEDPIXELS || !ignoreTouches)
                                {
                                    // Don't let spider activate if rope is not attached to candy
                                    if (rider.State == SpiderRiderState.Arming && !IsSpiderGrabbableCandyPoint(rope.tail))
                                    {
                                        rider.Arm(ropeAttachedToCandy: false);
                                    }
                                    rider.Update(grab, delta);
                                }
                                // Only let spider win if rope is attached to candy
                                if (rider.HasReachedCandy && IsSpiderGrabbableCandyPoint(rope.tail))
                                {
                                    SpiderWon(grab);
                                    break;
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
                        if (grab.Source.CanAttach && grab.Attachment.State == RopeAttachmentState.Idle)
                        {
                            // One pass over every hookable body: whole candies and split halves alike
                            // attach to a radius hook the moment they come inside it.
                            foreach (CandyBody body in ActiveCandyBodies(CandyInteraction.Rope))
                            {
                                if (TryAutoAttachGrabToBody(grab, body))
                                {
                                    break;
                                }
                            }
                        }
                        if (rope != null)
                        {
                            MaterialPoint bungeeAnchor = rope.bungeeAnchor;
                            ConstraintedPoint constraintedPoint2 = rope.parts[^1];
                            Vector v = VectSub(bungeeAnchor.pos, constraintedPoint2.pos);
                            // The body this rope ends on, unless another rope already steered it
                            // this frame: one rope drives one body's rotation per frame.
                            CandyBody rotateBody = null;
                            foreach (CandyBody body in ActiveCandyBodies(CandyInteraction.Rope))
                            {
                                if (body.Point == constraintedPoint2 && !rotatedBodies.Contains(body))
                                {
                                    rotateBody = body;
                                    break;
                                }
                            }
                            if (rope.relaxed != 0 && rope.cut == -1 && rotateBody != null)
                            {
                                float ropeAngle = RADIANS_TO_DEGREES(VectAngleNormalized(v));
                                GameObject rotatedVisual = RotatedVisualOf(rotateBody);
                                if (rotateBody.Owner.Capabilities.CanRotateWithRopes)
                                {
                                    if (!rope.chosenOne)
                                    {
                                        rope.initialCandleAngle = rotatedVisual.rotation - ropeAngle;
                                    }
                                    rotateBody.ResidualRotation = ropeAngle + rope.initialCandleAngle - rotatedVisual.rotation;
                                    rotatedVisual.rotation = ropeAngle + rope.initialCandleAngle;
                                }
                                else
                                {
                                    rotateBody.ResidualRotation = 0f;
                                }
                                _ = rotatedBodies.Add(rotateBody);
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
                // Every body no rope steered this frame coasts on its own residual spin. Only a hand
                // holding THIS body's candy freezes that coast.
                foreach (CandyBody body in ActiveCandyBodies(CandyInteraction.Rope))
                {
                    if (!body.Owner.Capabilities.CanRotateWithRopes)
                    {
                        body.ResidualRotation = 0f;
                    }
                    else if (!rotatedBodies.Contains(body) && body.Owner.Lifecycle.Attachments.Hand == null)
                    {
                        RotatedVisualOf(body).rotation += MIN(5, body.ResidualRotation);
                        body.ResidualRotation *= 0.98f;
                    }
                }
            }
            // candiesConnected elastic: simulate alongside grab ropes (same timestep) so its
            // SatisfyConstraints pulls both candies. Update only while uncut or fading.
            if (candyConnector != null && (candyConnector.cut == -1 || candyConnector.cutTime != 0f))
            {
                candyConnector.Update(delta * ropePhysicsSpeed);
            }

            SplitCandyState primarySplit = candies[0].Lifecycle.Split;

            // Sample whether the halves touch before the loop below re-tops-left their visuals.
            bool halvesTouchedLastFrame = primarySplit != null
                && primarySplit.Left.IsPresent
                && primarySplit.Right.IsPresent
                && GameObject.ObjectsIntersect(primarySplit.Left.Body.Visual, primarySplit.Right.Body.Visual);

            // Step every active body's point and visual in one pass: whole candies and surviving
            // split halves alike. A removed, hidden, or split candy offers no body, so the old
            // presence guards are the enumerator's job.
            foreach (CandyBody body in ActiveCandyBodies(CandyInteraction.Physics))
            {
                if (ActivePhysicsConstants.UseMobilePhysicsModel)
                {
                    body.RocketCollisionDrawPosition = Vect(body.Visual.drawX, body.Visual.drawY);
                }
                if (!timeFrozen)
                {
                    body.Point.Update(delta * ropePhysicsSpeed);
                }
                if (ActivePhysicsConstants.RelaxCandyPointsAfterIntegration)
                {
                    // Time Travel relaxes each candy point the moment it has moved - and does so
                    // whether or not time is frozen, unlike the integration above.
                    ConstraintedPoint.SatisfyConstraints(body.Point);
                }
                body.Visual.x = body.Point.pos.X;
                body.Visual.y = body.Point.pos.Y;
                if (body.Visual is Axe axe)
                {
                    axe.Update(delta, timeFrozen);
                }
                else
                {
                    body.Visual.Update(delta);
                }
                CalculateTopLeft(body.Visual);
            }
            if (ActivePhysicsConstants.RelaxCandyPointsAfterIntegration && candyConnector != null)
            {
                // ...then corrects the connector's own ends, which the candy integration has just
                // pulled off their rest length. Unconditional in Time Travel - a cut connector too.
                ConstraintedPoint.SatisfyConstraints(candyConnector.bungeeAnchor);
                ConstraintedPoint.SatisfyConstraints(candyConnector.tail);
            }
            // Candy-to-candy collision once all candy points are integrated (multi-candy only).
            ResolveCandyCollisions(delta);
            if (primarySplit != null)
            {
                ConstraintedPoint leftPoint = primarySplit.Left.Body.Point;
                ConstraintedPoint rightPoint = primarySplit.Right.Body.Point;
                if (primarySplit.Phase == SplitPhase.Merging)
                {
                    for (int l = 0; l < 30; l++)
                    {
                        ConstraintedPoint.SatisfyConstraints(leftPoint);
                        ConstraintedPoint.SatisfyConstraints(rightPoint);
                    }
                }
                // A destroyed half already cancelled the merge on the split aggregate, which clears the
                // phase and the remaining distance together, so the old separate abort branch is gone.
                SplitCandyState merging = primarySplit;
                if (merging.MergeDistance > 0)
                {
                    if (merging.TryAdvanceMerge(ActivePhysicsConstants.CandyPartsMergeSpeed, delta))
                    {
                        CTRSoundMgr.PlaySound(Resources.Snd.CandyLink);
                        _ = candies[0].Lifecycle.TryCompleteMerge();
                        int candiesUnitedCount = Preferences.GetIntForKey("PREFS_CANDIES_UNITED") + 1;
                        Preferences.SetIntForKey(candiesUnitedCount, "PREFS_CANDIES_UNITED", false);
                        if (candiesUnitedCount == 100)
                        {
                            CTRRootController.PostAchievementName("1432722351", ACHIEVEMENT_STRING("\"Romantic Soul\""));
                        }
                        // The merged candy inherits its halves' bubbles: a ghost bubble wins over a
                        // plain one, and when both halves carried a ghost the second is parked until
                        // the merged candy's bubble pops.
                        CandyBody mergedBody = candies[0].WholeBody;
                        CandyBody leftBody = merging.Left.Body;
                        CandyBody rightBody = merging.Right.Body;
                        GameObject leftBubble = leftBody.Bubble;
                        GameObject rightBubble = rightBody.Bubble;
                        CancelParkedGhostBubble();
                        if (leftBubble != null || rightBubble != null)
                        {
                            bool leftHasGhost = leftBubble != null && IsGhostApparitionBubble(leftBubble);
                            bool rightHasGhost = rightBubble != null && IsGhostApparitionBubble(rightBubble);
                            if (leftHasGhost && rightHasGhost)
                            {
                                mergedBody.Bubble = leftBubble;
                                parkedGhostBubble = new ParkedGhostBubble(mergedBody, rightBubble);
                            }
                            else if (leftHasGhost)
                            {
                                mergedBody.Bubble = leftBubble;
                            }
                            else if (rightHasGhost)
                            {
                                mergedBody.Bubble = rightBubble;
                            }
                            else
                            {
                                mergedBody.Bubble = leftBubble ?? rightBubble;
                                ReleaseGhostForBubble(leftBubble);
                                ReleaseGhostForBubble(rightBubble);
                            }
                            mergedBody.BubbleHasGhost = leftHasGhost || rightHasGhost;
                            mergedBody.BubbleAnimation.visible = !mergedBody.BubbleHasGhost;
                            mergedBody.GhostBubbleAnimation.visible = mergedBody.BubbleHasGhost;
                            leftBody.Bubble = null;
                            rightBody.Bubble = null;
                            leftBody.BubbleAnimation.visible = false;
                            rightBody.BubbleAnimation.visible = false;
                            leftBody.GhostBubbleAnimation.visible = false;
                            rightBody.GhostBubbleAnimation.visible = false;
                        }
                        else
                        {
                            mergedBody.Bubble = null;
                            mergedBody.BubbleHasGhost = false;
                            mergedBody.BubbleAnimation.visible = false;
                            mergedBody.GhostBubbleAnimation.visible = false;
                        }
                        mergedBody.ResidualRotation = 0f;
                        leftBody.ResidualRotation = 0f;
                        rightBody.ResidualRotation = 0f;
                        // The merge already detached the split, so the halves are reached through the
                        // aggregate captured above rather than through the lifecycle, which the merge just cleared.
                        ConstraintedPoint mergedLeft = merging.Left.Body.Point;
                        ConstraintedPoint mergedRight = merging.Right.Body.Point;
                        star.pos.X = mergedLeft.pos.X;
                        star.pos.Y = mergedLeft.pos.Y;
                        candy.x = star.pos.X;
                        candy.y = star.pos.Y;
                        CalculateTopLeft(candy);
                        Vector vector = VectSub(mergedLeft.pos, mergedLeft.prevPos);
                        Vector vector2 = VectSub(mergedRight.pos, mergedRight.prevPos);
                        Vector v2 = Vect((vector.X + vector2.X) / 2f, (vector.Y + vector2.Y) / 2f);
                        star.prevPos = VectSub(star.pos, v2);
                        int bungeeCount = bungees.Count;
                        for (int m = 0; m < bungeeCount; m++)
                        {
                            Bungee rope2 = bungees[m].Rope;
                            if (rope2 != null && rope2.cut != rope2.parts.Count - 3 && (rope2.tail == mergedLeft || rope2.tail == mergedRight))
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
                        merging.Left.Body.Point.ChangeRestLengthToFor(merging.MergeDistance, merging.Right.Body.Point);
                        merging.Right.Body.Point.ChangeRestLengthToFor(merging.MergeDistance, merging.Left.Body.Point);
                    }
                }
                // The gap is measured from the live points even though the touch test above is a frame old.
                if (primarySplit.Left.IsPresent && primarySplit.Right.IsPresent
                    && primarySplit.Phase == SplitPhase.Separate
                    && halvesTouchedLastFrame)
                {
                    float gap = VectDistance(leftPoint.pos, rightPoint.pos);
                    _ = primarySplit.TryBeginMerge(gap);
                    leftPoint.AddConstraintwithRestLengthofType(rightPoint, gap, Constraint.CONSTRAINT.NOT_MORE_THAN);
                    rightPoint.AddConstraintwithRestLengthofType(leftPoint, gap, Constraint.CONSTRAINT.NOT_MORE_THAN);
                }
            }
            if (!timeFrozen)
            {
                targetObject?.Update(delta);
                // Update additional Om Noms' animations (targets[0] handled above via targetObject).
                for (int ti = 1; ti < targets.Count; ti++)
                {
                    targets[ti].targetObject?.Update(delta);
                }
                UpdateNightTargetPresentation(delta);
                UpdatePostEatSleep(delta);
            }
            UpdateLightEmitterPhysics();
            UpdateNightStarLighting();
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

                    // Which body (if any) collects this star: any whole candy or split half whose
                    // logical candy collects stars at all.
                    CandyBody collectingBody = null;
                    foreach (CandyBody body in ActiveCandyBodies(CandyInteraction.Star))
                    {
                        if (body.Owner.Capabilities.CanCollectStars && GameObject.ObjectsIntersect(body.Visual, star))
                        {
                            collectingBody = body;
                            break;
                        }
                    }

                    if (collectingBody != null)
                    {
                        collectingBody.BlinkAnimation?.PlayTimeline(1);
                        starsCollected++;
                        // Update RPC with new star count
                        PlatformServices.RichPresence?.SetLevelPresence(cTRRootController.GetPack(), cTRRootController.GetLevel(), starsCollected, false, levelName);
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
                        for (int ti = 0; ti < targets.Count; ti++)
                        {
                            TargetAnimationController controller = targets[ti].controller;
                            if (!timeFrozen && controller?.IsIdleLoopPlaying() == true)
                            {
                                controller.PlayExcited();
                                CTRSoundMgr.PlayOmNomSound(Resources.Snd.MonsterExcited, controller.SkinDefinition);
                            }
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
                // One capture pass over every body a bubble can lift: whole candies and split halves
                // alike. Each body owns its own bubble and overlays, so the swap is the same
                // wherever the bubble lands. At most one bubble is claimed per frame.
                bool captured = false;
                foreach (CandyBody body in ActiveCandyBodies(CandyInteraction.Bubble))
                {
                    if (bubble3.popped
                        || !BubbleCapture.Captures(Vect(body.Visual.x, body.Visual.y), Vect(bubble3.x, bubble3.y), bubbleCaptureRadius))
                    {
                        continue;
                    }

                    CandyContext ctx = body.Owner;

                    // Already carried by a different bubble: release the old one and swap to the new
                    // bubble. Without this, a bubbled body skips every new bubble (e.g. a bubbled
                    // bulb phasing through a ghost bubble).
                    if (body.Bubble != null && body.Bubble != bubble3)
                    {
                        PopBubbleAtXY(bubble3.x, bubble3.y);
                        ReleaseGhostForBubble(body.Bubble);
                        ReleasePendingSecondGhostBubbleForBody(body);
                    }

                    bool hasGhost = IsGhostApparitionBubble(bubble3);
                    body.Bubble = bubble3;
                    body.BubbleHasGhost = hasGhost;
                    if (ctx.LightBulb != null)
                    {
                        bubble3.capturedByBulb = !hasGhost;
                    }
                    else
                    {
                        BubbleVisualState visualState = BubbleVisualState.ForCapture(hasGhost, body.GhostBubbleAnimation != null);
                        body.BubbleAnimation.visible = visualState.ShowNormalBubble;
                        body.GhostBubbleAnimation.visible = visualState.ShowGhostBubble;
                    }
                    CTRSoundMgr.PlaySound(Resources.Snd.Bubble);
                    bubble3.popped = true;
                    bubble3.RemoveChildWithID(0);
                    conveyors.Remove(bubble3);
                    captured = true;
                    break;
                }

                if (captured)
                {
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

                // A bubble lying on a belt drops its ground shadow the same way, or the shadow is
                // stamped across the plates. An ordinary bubble gets that from being bound to the
                // belt; a ghost's bubble never binds (it stays with the ghost that conjured it) and
                // is conjured long after the one-off bind pass, so it needs the test here.
                if (!bubble3.withoutShadow && bubble3 is IGhostApparition && conveyors != null)
                {
                    foreach (ConveyorBelt belt in conveyors.Iterator())
                    {
                        if (belt.CollidesWithCircle(Vect(bubble3.x, bubble3.y), bubble3.CollisionRadius * 0.6f))
                        {
                            bubble3.withoutShadow = true;
                            break;
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

                // Only a whole body enters a tube; the body-role table keeps split halves out, so
                // there is no split carve-out left in the entry gate.
                foreach (CandyBody body in ActiveCandyBodies(CandyInteraction.Transport))
                {
                    CandyContext ctx = body.Owner;
                    if (!ctx.Capabilities.CanEnterTransport)
                    {
                        continue;
                    }
                    bool inRange = bambooTube.TryCatchCandy(body.Point);
                    if (ctx.Lifecycle.CanEnterTransport && inRange)
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
                foreach (CandyBody body in ActiveCandyBodies(CandyInteraction.Lantern))
                {
                    CandyContext ctx = body.Owner;
                    if (!ctx.Capabilities.CanEnterLantern)
                    {
                        continue;
                    }
                    bool inRange = VectDistance(body.Point.pos, Vect(lantern.x, lantern.y)) < ActivePhysicsConstants.LanternCaptureRadius;
                    if (!LanternCapture.ShouldCapture(lanternInactive, groupOccupied, candyPresent: true, ctx.Lifecycle.Attachments.InLantern, inRange))
                    {
                        continue;
                    }

                    CandyAttachmentSnapshot detached = ctx.Lifecycle.Attachments.CaptureInLantern();
                    ReleaseLanternCaptureAttachments(detached, body.Point);
                    body.Visual.passTransformationsToChilds = true;
                    body.Main.scaleX = body.Main.scaleY = 1f;
                    body.Top.scaleX = body.Top.scaleY = 1f;
                    Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
                    timeline.AddKeyFrame(KeyFrame.MakePos((int)body.Visual.x, (int)body.Visual.y, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                    timeline.AddKeyFrame(KeyFrame.MakePos((int)lantern.x, (int)lantern.y, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.1f));
                    timeline.AddKeyFrame(KeyFrame.MakeScale(0.71f, 0.71f, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                    timeline.AddKeyFrame(KeyFrame.MakeScale(0.3f, 0.3f, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.1f));
                    timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
                    timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.1f));
                    body.Visual.RemoveTimeline(0);
                    body.Visual.AddTimelinewithID(timeline, 0);
                    body.Visual.PlayTimeline(0);
                    ReleaseRopesForPoint(body.Point);
                    // Lantern capture is terminal for this candy's riders: snails hop off (giving
                    // their weight back) and ants stop carrying it, mirroring hand-grab.
                    int lanternDetachedSnails = ActiveSnailCountForPoint(body.Point);
                    DetachSnailsForPoint(body.Point);
                    if (lanternDetachedSnails > 0)
                    {
                        body.Point.SetWeight(SnailWeight.AfterForceDetach(body.Point.weight, lanternDetachedSnails));
                    }
                    PopCandyBubble(body);
                    pendingLanternCapture = new PendingLanternCapture(body.Point, lantern);
                    dd.CallObjectSelectorParamafterDelay(
                        new DelayedDispatcher.DispatchFunc(CompletePendingLanternCapture),
                        pendingLanternCapture,
                        0.05f);

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
                    // Self-moving grabs, player rails, and ghost apparitions never ride the disc.
                    bool discBindable = (bungee4.Mount?.FollowsPlatform ?? bungee4.Motion.FollowsPlatform)
                        && bungee4 is not IGhostApparition;
                    if (discBindable && VectDistance(Vect(bungee4.x, bungee4.y), Vect(rotatedCircle7.x, rotatedCircle7.y)) <= rotatedCircle7.sizeInPixels + (RTPD(5) * 3f))
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
                    if (bubble4 is not IGhostApparition
                        && VectDistance(Vect(bubble4.x, bubble4.y), Vect(rotatedCircle7.x, rotatedCircle7.y)) <= rotatedCircle7.sizeInPixels + (RTPD(10) * 3f))
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

                {
                    // The mouse grabs the first in-range grabbable body (single-occupancy). The
                    // body-role table keeps split halves out, so no split carve-out is needed.
                    foreach (CandyBody body in ActiveCandyBodies(CandyInteraction.Mouse))
                    {
                        CandyContext ctx = body.Owner;
                        if (ctx.Lifecycle.Attachments.InLantern || !ctx.Capabilities.CanBeGrabbedByMouse)
                        {
                            continue;
                        }
                        if (MouseGrab.ShouldGrab(miceManager.ActiveMouseHasCandy(), candyPresent: true, miceManager.IsActiveMouseInRange(body.Point)))
                        {
                            miceManager.GrabWithActiveMouse(body.Point, body.Visual);
                            TriggerSpecialTutorial(4);
                            break;
                        }
                    }
                }

            }
            float collisionHalfSize = ActivePhysicsConstants.SockCatchHalfSize;
            foreach (object obj11 in socks)
            {
                Sock sock3 = (Sock)obj11;
                sock3.Update(delta, timeFrozen);
                if (timeFrozen)
                {
                    continue;
                }
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
                foreach (CandyBody body in ActiveCandyBodies(CandyInteraction.Transport))
                {
                    CandyContext ctx = body.Owner;
                    if (!ctx.Capabilities.CanEnterTransport)
                    {
                        continue;
                    }
                    Vector ptr = VectRotate(body.Point.posDelta, invRotation);
                    float bbX = body.Point.pos.X - collisionHalfSize;
                    float bbY = body.Point.pos.Y - collisionHalfSize;
                    bool candyHits = ptr.Y >= 0 &&
                        (LineInRect(sock3.t1.X, sock3.t1.Y, sock3.t2.X, sock3.t2.Y, bbX, bbY, bbSize, bbSize) ||
                         LineInRect(sock3.b1.X, sock3.b1.Y, sock3.b2.X, sock3.b2.Y, bbX, bbY, bbSize, bbSize));
                    anyCandyHits = anyCandyHits || candyHits;

                    if (!wasIdle || !ctx.Lifecycle.CanEnterTransport || !candyHits)
                    {
                        continue;
                    }

                    foreach (Sock sock4 in socks)
                    {
                        if (sock4 != sock3 && sock4.group == sock3.group)
                        {
                            float exitSpeed = ActivePhysicsConstants.SockSpeedKoeff * VectLength(body.Point.v)
                                * ActivePhysicsConstants.SockTeleportSpeedMultiplier;
                            // The exit speed is read off the entry velocity, so the session has to be
                            // built before anything below disturbs the point.
                            CandyTransportSession session = CandyTransportSession.ForSock(ctx, sock4, exitSpeed);
                            if (!ctx.Lifecycle.TryHide(session, out CandyAttachmentSnapshot detached))
                            {
                                break;
                            }

                            sock4.state = Sock.SOCK_THROWING;
                            sock4.idleTimeout = 0.8f;
                            ReleaseRopesForPoint(body.Point);
                            ReleaseTransportAttachments(detached, body.Point);
                            // The rocket teleports with the candy; hide it for the transit like the
                            // reference (Gift catch sets visible = 0; Teleport re-shows it).
                            if (ctx.Lifecycle.Attachments.HasActiveRocket)
                            {
                                ctx.Lifecycle.Attachments.Rocket.visible = false;
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

                            dd.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(Selector_teleport), session, 0.1f);
                            break;
                        }
                    }
                }

                if (!wasIdle)
                {
                    if (!anyCandyHits && sock3.idleTimeout == 0f)
                    {
                        sock3.idleTimeout = 0.8f;
                    }
                    continue;
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
                    // The rocket flies exactly one candy; resolve it (null while idle/unbound).
                    // Resolved BEFORE rocket.Update so a parked rocket can pre-snap: the mice
                    // update (which teleports the candy across the level on a mouse handoff) runs
                    // earlier in the frame, and rocket.Update syncs the visual from point.pos — a
                    // post-Update snap would leave the rocket rendered at the old mouth for one frame.
                    CandyContext rocketCandy = RocketBoundCandy(rocket);
                    ConstraintedPoint rocketStar = rocketCandy?.WholeBody.Point;
                    GameObject rocketCandyMain = rocketCandy?.WholeBody.Main;
                    // Every branch below steers the bound candy, and a rocket only reaches DIST or
                    // FLY by binding one. An unresolved rocket now does nothing instead of falling
                    // back on candies[0]/star, which made a stray rocket thrust and de-spin whichever
                    // candy happened to be the primary.
                    bool carriesCandy = rocketCandy != null
                        && rocket.state is Rocket.STATE_ROCKET_FLY or Rocket.STATE_ROCKET_DIST;
                    // Park the rocket while the mouse carries its candy. The kinematic pin plus the
                    // per-frame rocket-point snap keep the rest-0 pair exactly coincident, and the
                    // solver's coincident fallback (DEFAULT_NON_ZERO_CONSTRAINT_DIRECTION, 30
                    // iterations/frame) makes the pair random-walk around the mouth — a visible
                    // wobble, frozen in as a position drift if the candy is dropped mid-jitter.
                    // Parked: no satisfaction, no thrust; position snap, rotation sync, fuse tick
                    // and gravity heal keep running, and full flight resumes on drop.
                    bool parkedOnMouse = MouseCarries(rocketCandy);
                    if (parkedOnMouse && carriesCandy)
                    {
                        // prevPos too: rocket.Update integrates the point next, and a bare pos
                        // teleport would replay the whole jump as one frame of velocity.
                        rocket.point.pos = rocketStar.pos;
                        rocket.point.prevPos = rocketStar.pos;
                    }
                    rocket.Update(delta, timeFrozen);
                    rocket.UpdateRotation();
                    if (timeFrozen)
                    {
                        if (carriesCandy && rocket.state == Rocket.STATE_ROCKET_FLY)
                        {
                            rocket.point.pos = rocketStar.pos;
                        }
                        continue;
                    }
                    // Rocket flight requires zero gravity on the candy point. Any drop path (e.g.
                    // Mouse.DropCandy re-enabling gravity when the mouse lets go of a rocket-bound
                    // candy) is healed here every frame while the rocket is bound — mirrors the
                    // reference's recurring `star->disableGravity = activeRocket != 0`.
                    if (carriesCandy)
                    {
                        rocketStar.disableGravity = true;
                    }
                    float dist = carriesCandy ? VectLength(VectSub(rocketStar.pos, rocket.point.pos)) : 0f;
                    if (carriesCandy)
                    {
                        // Time Travel relaxes the pair only through the reel-in; once the rocket is
                        // flying the rest length is fixed and it leaves the points alone.
                        if (!parkedOnMouse
                            && (rocket.state == Rocket.STATE_ROCKET_DIST
                                || ActivePhysicsConstants.RocketRelaxDuringFlight))
                        {
                            for (int i = 0; i < 30; i++)
                            {
                                ConstraintedPoint.SatisfyConstraints(rocketStar);
                                ConstraintedPoint.SatisfyConstraints(rocket.point);
                            }
                        }
                        rocket.rotation = AngleTo0_360(rocket.startRotation + rocketCandyMain.rotation - rocket.startCandyRotation);
                    }
                    if (carriesCandy && rocket.state == Rocket.STATE_ROCKET_FLY)
                    {
                        // Silence THIS candy's rope-spin coast: the rocket's heading tracks
                        // candyMain.rotation, so a leftover coast (e.g. after the rope is cut)
                        // curves the flight as if it were still steering along the rope.
                        rocketCandy.WholeBody.ResidualRotation = 0f;
                        bool ropeRelaxed = false;
                        if (bungees != null)
                        {
                            foreach (Grab bungee in bungees)
                            {
                                if (bungee != null)
                                {
                                    Bungee rope = bungee.Rope;
                                    bool candyIsFree = !ActivePhysicsConstants.RocketRopeAlignRequiresFreeCandy
                                        || rocketCandy?.Lifecycle.Attachments.Hand == null;
                                    if (rope != null && rope.tail == rocketStar && rope.cut == -1 && rope.relaxed > 0 && candyIsFree)
                                    {
                                        ropeRelaxed = true;
                                        AlignRocketAngleToRope(rocket, rope, delta);
                                    }
                                }
                            }
                        }
                        // iOS steers the rocket off the candy connector too. It lives outside the grab
                        // list and joins two candy points, so there is no rocketStar tail check and no
                        // hand gate. The connector counts as relaxed while it is nearly
                        // straight: |straight-line span - polyline length| < polyline length / 4.
                        if (candyConnector != null && candyConnector.cut == -1)
                        {
                            int connectorLength = candyConnector.GetLength();
                            int connectorSlack = (int)(VectDistance(candyConnector.bungeeAnchor.pos, candyConnector.parts[^1].pos) - connectorLength);
                            if (connectorSlack < 0)
                            {
                                connectorSlack = -connectorSlack;
                            }
                            if (connectorSlack < (connectorLength >> 2))
                            {
                                ropeRelaxed = true;
                                AlignRocketAngleToRope(rocket, candyConnector, delta);
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
                        if (!parkedOnMouse)
                        {
                            rocketStar.ApplyImpulseDelta(impulse, delta);
                        }
                        rocketStar.gravity = vectZero;
                        rocket.point.pos.X = rocketStar.pos.X;
                        rocket.point.pos.Y = rocketStar.pos.Y;
                        if (rocket.time != -1f && Mover.MoveVariableToTarget(ref rocket.time, 0f, 1f, delta))
                        {
                            ExhaustRocketForCandy(rocketCandy);
                            rocketStar.disableGravity = IsCandyGravitySuppressed(rocketCandy);
                        }
                    }
                    if (carriesCandy && rocket.state == Rocket.STATE_ROCKET_DIST)
                    {
                        // Per-candy: only a hand holding THIS rocket's candy skips the reel-in.
                        // Time Travel has no such shortcut - it reels in from whoever holds it.
                        bool heldSkipsReelIn = ActivePhysicsConstants.RocketBindsDirectlyToFlightWhenHeld
                            && rocketCandy?.Lifecycle.Attachments.Hand != null;
                        if (heldSkipsReelIn || Mover.MoveVariableToTarget(ref dist, 0f, ActivePhysicsConstants.RocketReelSpeed, delta))
                        {
                            rocket.state = Rocket.STATE_ROCKET_FLY;
                            if (ActivePhysicsConstants.RocketBindClearsCandyVelocity)
                            {
                                // Time Travel hands the thrust a candy at rest: whatever the reel-in
                                // built up is dropped as the flight starts.
                                rocketStar.v = vectZero;
                                rocketStar.a = vectZero;
                                rocketStar.gravity = vectZero;
                                rocketStar.prevPos = rocketStar.pos;
                            }
                        }
                        else
                        {
                            rocket.point.ChangeRestLengthToFor(dist, rocketStar);
                        }
                    }
                    if (rocket.state == Rocket.STATE_ROCKET_IDLE)
                    {
                        foreach (CandyBody body in ActiveCandyBodies(CandyInteraction.Rocket))
                        {
                            CandyContext ctx = body.Owner;
                            if (!ctx.Capabilities.CanBindRocket)
                            {
                                continue;
                            }
                            bool intersects = ActivePhysicsConstants.UseMobilePhysicsModel
                                ? GameObject.ObjectsIntersectRotatedWithUnrotatedAt(
                                    rocket,
                                    body.Visual,
                                    body.RocketCollisionDrawPosition.X,
                                    body.RocketCollisionDrawPosition.Y)
                                : GameObject.ObjectsIntersectRotatedWithUnrotated(rocket, body.Visual);
                            if (!RocketBind.ShouldBind(rocket.state == Rocket.STATE_ROCKET_IDLE, candyPresent: true, ctx.Lifecycle.Attachments.InLantern, intersects))
                            {
                                continue;
                            }

                            if (ActivePhysicsConstants.RocketBindPopsCandyBubble)
                            {
                                // Time Travel bursts the bubble before it takes the candy.
                                PopCandyBubble(body);
                            }
                            rocket.mover?.Pause();
                            rocket.startRotation = rocket.rotation;
                            // Per-candy: only a holder of THIS candy selects the direct-FLY bind.
                            // The rocket steals from nobody — it coexists with hand or mouse and
                            // launches when the holder releases.
                            if (ActivePhysicsConstants.RocketBindsDirectlyToFlightWhenHeld
                                && RocketBindPath.UsesDirectFlyPath(ctx.Lifecycle.Attachments.Hand != null, MouseCarries(ctx)))
                            {
                                rocket.point.pos = body.Point.pos;
                                rocket.point.AddConstraintwithRestLengthofType(body.Point, 0f, Constraint.CONSTRAINT.NOT_MORE_THAN);
                                rocket.state = Rocket.STATE_ROCKET_FLY;
                            }
                            else
                            {
                                float bindDist = VectLength(VectSub(body.Point.pos, rocket.point.pos));
                                rocket.point.AddConstraintwithRestLengthofType(body.Point, bindDist, Constraint.CONSTRAINT.NOT_MORE_THAN);
                                rocket.state = Rocket.STATE_ROCKET_DIST;
                            }
                            // Per-candy: zero the bound candy's rope-spin coast, not candy 0's.
                            body.ResidualRotation = 0f;
                            if (ActivePhysicsConstants.RocketBindClearsCandyVelocity)
                            {
                                // Time Travel kills the candy's velocity outright on capture, and
                                // additionally clears the accumulators when it was still falling.
                                if (!body.Point.disableGravity)
                                {
                                    body.Point.v = vectZero;
                                    body.Point.a = vectZero;
                                    body.Point.gravity = vectZero;
                                }
                                body.Point.prevPos = body.Point.pos;
                            }
                            else
                            {
                                Vector deltaPos = VectSub(body.Point.pos, body.Point.prevPos);
                                body.Point.prevPos = VectAdd(body.Point.prevPos, VectDiv(deltaPos, body.Point.disableGravity ? 2f : 1.25f));
                            }
                            body.Point.disableGravity = true;

                            // Exhaust any rocket already bound to this candy before re-binding (one-time-use safety).
                            if (ctx.Lifecycle.Attachments.HasActiveRocket && ctx.Lifecycle.Attachments.Rocket != rocket)
                            {
                                ExhaustRocketForCandy(ctx);
                            }

                            rocket.startSound = CTRSoundMgr.PlaySoundTracked(Resources.Snd.ExpRocketStart);
                            rocket.flyLoopSound = CTRSoundMgr.PlaySoundLooped(Resources.Snd.ExpRocketFlyLooped);
                            _ = ctx.Lifecycle.Attachments.BindRocket(rocket);
                            rocket.isOperating = -1;
                            rocket.startCandyRotation = body.Main.rotation;

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
            CutAxeOnlyChainsWithAxes();
            if (BreakCandyTouchedByAxes())
            {
                return;
            }
            foreach (object obj14 in spikes)
            {
                Spikes spike = (Spikes)obj14;
                spike.Update(delta, timeFrozen);
                float spikeCollisionRadius = 15f;
                // Break the first body that touches the spike, in one pass over whole candies and
                // split halves alike. Decision routed through BarrierCollision.Hits.
                if (!spike.electro || (spike.electro && spike.electroOn))
                {
                    foreach (CandyBody body in ActiveCandyBodies(CandyInteraction.Hazard))
                    {
                        CandyContext ctx = body.Owner;
                        if (!ctx.Capabilities.CanBeBrokenByHazards || ctx.Lifecycle.Attachments.InLantern)
                        {
                            continue;
                        }
                        if (!BarrierCollision.Hits(
                            spike.t1.X, spike.t1.Y, spike.t2.X, spike.t2.Y,
                            spike.b1.X, spike.b1.Y, spike.b2.X, spike.b2.Y,
                            body.Point.pos.X, body.Point.pos.Y, body.Point.prevPos.X, body.Point.prevPos.Y,
                            spikeCollisionRadius))
                        {
                            continue;
                        }

                        BreakCandyBody(body);
                        return;
                    }
                }
            }
            foreach (object obj15 in bouncers)
            {
                Bouncer bouncer = (Bouncer)obj15;
                bouncer.Update(delta, timeFrozen);
                float bouncerCollisionRadius = ActivePhysicsConstants.BouncerCollisionRadius;
                bool anyCandyHit = false;
                foreach (CandyBody body in ActiveCandyBodies(CandyInteraction.Bouncer))
                {
                    if (BarrierCollision.Hits(
                        bouncer.t1.X, bouncer.t1.Y, bouncer.t2.X, bouncer.t2.Y,
                        bouncer.b1.X, bouncer.b1.Y, bouncer.b2.X, bouncer.b2.Y,
                        body.Point.pos.X, body.Point.pos.Y, body.Point.prevPos.X, body.Point.prevPos.Y,
                        bouncerCollisionRadius,
                        includeSweep: !ActivePhysicsConstants.UseMobilePhysicsModel))
                    {
                        anyCandyHit = true;

                        if (timeFrozen)
                        {
                            continue;
                        }

                        // A hand that just caught this candy keeps it for a moment, otherwise the
                        // bouncer takes it straight back and the two fight over it every frame. The
                        // window belongs to the individual hold, so a candy held past its own grace
                        // still drops even while another hand is inside one.
                        MechanicalHand holder = body.Owner.Lifecycle.Attachments.Hand;
                        if (holder == null || holder.CanBeDetachedByBouncer)
                        {
                            DetachHandsForPoint(body.Point);
                        }
                        HandleBouncePtDelta(bouncer, body.Point, delta);
                    }
                }
                if (!anyCandyHit)
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
            float waterRocketDamping = ActivePhysicsConstants.WaterDamping * ActivePhysicsConstants.WaterRocketDampingMultiplier;
            if (waterLayer != null && waterLevel > 0f)
            {
                foreach (CandyBody body in ActiveCandyBodies(CandyInteraction.Water))
                {
                    CandyContext ctx = body.Owner;
                    if (!ctx.Capabilities.CanFloatInWater)
                    {
                        continue;
                    }
                    if (!WaterSubmersion.IsSubmerged(body.Point.pos.X, body.Point.pos.Y, waterLayer.x, waterLayer.y, waterLayer.width, candyRadius))
                    {
                        continue;
                    }
                    float damping = ActivePhysicsConstants.WaterDamping;
                    float verticalWaterImpulse = ActivePhysicsConstants.WaterVerticalImpulseBase / body.Point.weight;
                    if (ctx.Lifecycle.Attachments.HasActiveRocket)
                    {
                        verticalWaterImpulse /= ActivePhysicsConstants.WaterRocketImpulseDivisor;
                        damping *= ActivePhysicsConstants.WaterRocketDampingMultiplier;
                        if (ctx.Lifecycle.Attachments.Rocket.state == Rocket.STATE_ROCKET_FLY)
                        {
                            CTRSoundMgr.PlaySound(Resources.Snd.ExpRocketInWater);
                            ctx.Lifecycle.Attachments.Rocket.state = Rocket.STATE_ROCKET_EXAUST;
                            ctx.Lifecycle.Attachments.Rocket.StopAnimation();
                        }
                    }
                    body.Point.ApplyImpulseDelta(Vect(-body.Point.v.X / damping, (-body.Point.v.Y / damping) + verticalWaterImpulse), delta);
                }
            }
            if (waterLayer != null && bungees != null)
            {
                foreach (Grab grab in bungees)
                {
                    if (grab != null && grab.Mount?.IsMounted == false && grab.y > waterLayer.y && grab.Rope != null)
                    {
                        float damping = ActivePhysicsConstants.WaterDamping;
                        ConstraintedPoint anchor = grab.Rope.bungeeAnchor;
                        anchor.ApplyImpulseDelta(Vect(-anchor.v.X / damping, (-anchor.v.Y / damping) + ActivePhysicsConstants.WaterRopeAnchorImpulse), delta);
                    }
                }
            }
            if (snailobjects != null && snailobjects.Count > 0)
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

                    // A snail that is riding nothing resolvable steers nothing: it used to read the
                    // primary candy's rotation and pop the primary candy's bubble instead.
                    CandyContext ridden = CandyForPointOrNull(snail.AttachedPoint());
                    if (snail.state == Snail.SNAIL_STATE_ACTIVE && ridden != null)
                    {
                        snail.rotation = ridden.InteractionRotation - snail.startRotation;
                        // The snail wins over a bubble: pop the ridden candy's bubble
                        // (Experiments reference) so the pair never floats up together.
                        if (SnailBubblePop.ShouldPop(true, snail.AttachedPoint() != null, ridden.WholeBody.Bubble != null))
                        {
                            PopCandyBubble(ridden.WholeBody);
                        }
                    }

                    if (snail.state == Snail.SNAIL_STATE_INACTIVE)
                    {
                        foreach (CandyBody body in ActiveCandyBodies(CandyInteraction.Snail))
                        {
                            CandyContext ctx = body.Owner;
                            if (!SnailAttach.ShouldAttach(candyGone: false, ctx.Capabilities.CanBeDraggedBySnail, GameObject.ObjectsIntersect(body.Visual, snail)))
                            {
                                continue;
                            }

                            DetachSnailsForPoint(body.Point);
                            snail.startRotation += ctx.InteractionRotation;
                            snail.AttachToPoint(body.Point);
                            body.Point.SetWeight(body.Point.weight + 3f);
                            break;
                        }
                    }

                    if (snail.state == Snail.SNAIL_STATE_VANISHED)
                    {
                        snailobjects.RemoveAt(i);
                    }
                }
            }
            float bubbleLift = ActivePhysicsConstants.BubbleImpulseY;
            float bubbleDamping = ActivePhysicsConstants.BubbleImpulseDamping;
            // Per-body bubble lift: every body carrying a bubble floats, whole candy or split half.
            foreach (CandyBody body in ActiveCandyBodies(CandyInteraction.Bubble))
            {
                if (body.Bubble == null)
                {
                    continue;
                }
                float lift = gravityState.IsInverted ? -bubbleLift : bubbleLift;
                body.Point.ApplyImpulseDelta(
                    Vect((0f - body.Point.v.X) / bubbleDamping, ((0f - body.Point.v.Y) / bubbleDamping) + lift),
                    delta);
            }
            // Time Travel never damps a rocket-bound candy: it populates no force slot on any point
            // (setForcewithID has no call site in the binary), so thrust builds unopposed. Only the
            // Experiments rocket bleeds velocity off.
            for (int ci = 0; ActivePhysicsConstants.RocketDampsCandyVelocity && ci < candies.Count; ci++)
            {
                CandyContext ctx = candies[ci];
                ConstraintedPoint rocketPoint = ctx.WholeBody.Point;
                if (ctx.Lifecycle.Attachments.Rocket != null)
                {
                    // Experiments applies velocity damping as an impulse each frame instead.
                    bool inWater = waterLayer != null
                        && waterLevel > 0f
                        && WaterSubmersion.IsSubmerged(
                            rocketPoint.pos.X,
                            rocketPoint.pos.Y,
                            waterLayer.x,
                            waterLayer.y,
                            waterLayer.width,
                            candyRadius);
                    float damping = inWater
                        ? waterRocketDamping
                        : ActivePhysicsConstants.RocketActiveVelocityDamping;
                    rocketPoint.ApplyImpulseDelta(
                        Vect(-rocketPoint.v.X / damping, -rocketPoint.v.Y / damping),
                        delta);
                }
            }
            ApplyAntCarryToCandyPosition();

            // Snapshot the bodies an Om Nom can react to. Only a whole candy opens a mouth, and a
            // candy captured in a lantern is not a candidate.
            List<CandyView> candyViews = [];
            foreach (CandyBody body in ActiveCandyBodies(CandyInteraction.Eat))
            {
                if (!body.Owner.Lifecycle.Attachments.InLantern)
                {
                    candyViews.Add(body.ToView());
                }
            }

            if (!timeFrozen)
            {
                for (int ti = 0; ti < targets.Count; ti++)
                {
                    TargetContext t = targets[ti];
                    // No mouth opening/closing once a win/loss transition is active: a sad Om Nom must
                    // not react to a remaining candy during the loss reaction.
                    if (t.targetObject == null || !gameplayFlow.CanReactToCandy(t.Feeding.IsFed))
                    {
                        continue;
                    }
                    Vector targetPos = Vect(t.targetObject.x, t.targetObject.y);
                    bool canInteractWithTarget = !nightLevel || t.NightSleep.IsAwake;

                    if (t.Feeding.Phase == TargetFeedingPhase.Idle && canInteractWithTarget)
                    {
                        if (CandyDecisions.ShouldOpenMouth(targetPos, candyViews, ActivePhysicsConstants.MouthOpenDistance))
                        {
                            if (t.Feeding.TryOpenMouth(closeDelay: 1f))
                            {
                                t.controller?.PlayMouthOpening();
                                CTRSoundMgr.PlayOmNomSound(Resources.Snd.MonsterOpen, t.controller?.SkinDefinition);
                            }
                        }
                    }
                    else if (t.Feeding.Phase == TargetFeedingPhase.MouthOpen && canInteractWithTarget)
                    {
                        bool candyNearby = CandyDecisions.ShouldOpenMouth(
                            targetPos,
                            candyViews,
                            ActivePhysicsConstants.MouthOpenDistance);
                        if (t.Feeding.AdvanceMouthClose(delta, candyNearby, refreshDelay: 1f))
                        {
                            t.controller?.PlayMouthClosing();
                            CTRSoundMgr.PlayOmNomSound(Resources.Snd.MonsterClose, t.controller?.SkinDefinition);
                            tummyTeasers++;
                            if (tummyTeasers >= 10)
                            {
                                CTRRootController.PostAchievementName("1058281905", ACHIEVEMENT_STRING("\"Tummy Teaser\""));
                            }
                        }
                    }
                }
            }
            // Eat: an uneaten candy entering an open mouth is consumed; that Om Nom sleeps.
            // Once a win/loss transition is active, no further candy may be eaten so a sad Om Nom
            // does not consume a remaining candy during the loss transition.
            if (!timeFrozen && gameplayFlow.CanTriggerOutcome && gameplayFlow.CanReactToCandy())
            {
                for (int ti = 0; ti < targets.Count; ti++)
                {
                    TargetContext t = targets[ti];
                    bool canInteractWithTarget = !nightLevel || t.NightSleep.IsAwake;
                    if (!canInteractWithTarget
                        || !gameplayFlow.CanReactToCandy(t.Feeding.IsFed)
                        || t.Feeding.Phase != TargetFeedingPhase.MouthOpen
                        || t.targetObject == null)
                    {
                        continue;
                    }
                    foreach (CandyBody body in ActiveCandyBodies(CandyInteraction.Eat))
                    {
                        CandyContext ctx = body.Owner;
                        if (!ctx.Capabilities.CanBeEaten)
                        {
                            continue;
                        }
                        body.Visual.x = body.Point.pos.X;
                        body.Visual.y = body.Point.pos.Y;
                        if (GameObject.ObjectsIntersect(body.Visual, t.targetObject))
                        {
                            if (!TryRetireCandyBody(body, CandyRemovalReason.Eaten))
                            {
                                continue;
                            }

                            body.Visual.visible = false;
                            _ = t.Feeding.TryBeginChewing();
                            t.controller?.PlayChewing();
                            CTRSoundMgr.PlayOmNomSound(Resources.Snd.MonsterChewing, t.controller?.SkinDefinition);
                            SchedulePostEatSleep(t);
                            break;
                        }
                    }
                }

                // Win only when every edible candy reached Removed(Eaten). A candy still in play,
                // hidden in transport, split, or removed by a hazard never satisfies this.
                List<CandyOutcomeView> allCandyOutcomes = [];
                for (int ci = 0; ci < candies.Count; ci++)
                {
                    allCandyOutcomes.Add(candies[ci].ToOutcomeView());
                }
                if (CandyDecisions.AllEaten(allCandyOutcomes))
                {
                    GameWon();
                    return;
                }
            }
            // Lose if any uneaten candy leaves the screen. Mark each leaver consumed-as-lost.
            // Any body that leaves the play area is retired; a split half loses the level outright,
            // while a whole candy only does so when its capabilities say it should. The list is
            // materialized first because each removal changes what is active.
            bool anyLeft = false;
            foreach (CandyBody body in ActiveCandyBodies(CandyInteraction.OffScreen).ToList())
            {
                if (!PointOutOfScreen(body.Point))
                {
                    continue;
                }
                CandyContext ctx = body.Owner;
                if (!TryRetireCandyBody(body, CandyRemovalReason.OffScreen))
                {
                    continue;
                }

                anyLeft = anyLeft
                    || body.Role != CandyBodyRole.Whole
                    || ctx.Capabilities.CanLoseLevelWhenOffScreen;
            }
            if (anyLeft)
            {
                if (gameplayFlow.CanTriggerOutcome)
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
            if (special != 0 && special == 1 && !candies[0].HasNoWholeBodyInPlay && candies[0].WholeBody.Bubble != null && candy.y < 400f && candy.x > 1200f)
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
            if (clickToCut && !ignoreTouches && !AcceptsVisualOnlyPointerInput)
            {
                ResetBungeeHighlight();
                bool flag12 = false;
                Vector p = camera.ScreenToWorld(slastTouch.X, slastTouch.Y);
                if (gravityState.IsInToggleTouchZone(p.X, p.Y))
                {
                    flag12 = true;
                }
                // A tap inside a bubbled body's pop zone is a bubble pop, not a rope cut.
                foreach (CandyBody body in ActiveCandyBodies(CandyInteraction.Bubble))
                {
                    if (body.Bubble != null && PointInRect(p.X, p.Y, body.Point.pos.X - 60f, body.Point.pos.Y - 60f, 120f, 120f))
                    {
                        flag12 = true;
                        break;
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
                    if (bungee5.Wheel != null && PointInRect(p.X, p.Y, bungee5.x - WheelControl.TapHalfExtent, bungee5.y - WheelControl.TapHalfExtent, WheelControl.TapHalfExtent * 2f, WheelControl.TapHalfExtent * 2f))
                    {
                        flag12 = true;
                        break;
                    }
                    if (bungee5.Rail is RailMotion rail5 && (PointInRect(p.X, p.Y, bungee5.x - 65f, bungee5.y - 65f, 130f, 130f) || rail5.DraggingTouch != -1))
                    {
                        flag12 = true;
                        break;
                    }
                }
                if (!flag12)
                {
                    Vector s = default;
                    Grab grab2 = null;
                    Bungee nearestBungeeSegmentByBeziersPointsatXYgrab = GetNearestBungeeSegmentByBeziersPointsatXYgrab(ref s, camera.ScreenToWorldX(slastTouch.X), camera.ScreenToWorldY(slastTouch.Y), ref grab2);
                    _ = (nearestBungeeSegmentByBeziersPointsatXYgrab?.highlighted = true);
                }
            }
            if (timeFrozen)
            {
                HoldFrozenPoints();
            }
            switch (gameplayFlow.Advance(delta))
            {
                case RestartStep.SwapScene:
                    dd.CancelAllDispatches();
                    Hide();
                    Show();
                    return;
                case RestartStep.Completed:
                case RestartStep.None:
                default:
                    break;
            }
        }

        /// <summary>Advances pointer visuals even when outcome presentation freezes gameplay simulation.</summary>
        internal void UpdatePointerGestureVisuals(float delta)
        {
            foreach (PointerGestureState gesture in pointerGestures)
            {
                gesture.UpdateVisuals(delta);
            }
        }

        /// <summary>
        /// Attaches an auto-attaching radius grab to one candy body when that body is in range and
        /// the grab has not already created a rope. A body only exists while it is active, so
        /// presence is the enumerator's job and every whole candy and split half hooks identically.
        /// </summary>
        /// <param name="grab">The radius grab looking for a body.</param>
        /// <param name="body">The candidate body.</param>
        /// <returns><see langword="true"/> when a rope was created.</returns>
        private bool TryAutoAttachGrabToBody(Grab grab, CandyBody body)
        {
            AutoRadiusSource source = grab.RadiusSource;
            if (source == null || !source.CanAttach || !source.InRange(Vect(grab.x, grab.y), body.Point.pos))
            {
                return false;
            }

            Bungee bungee = new Bungee().InitWithHeadAtXYTailAtTXTYandLength(
                null, grab.x, grab.y, body.Point, body.Point.pos.X, body.Point.pos.Y,
                source.Radius + ActivePhysicsConstants.CandyGrabPadding);
            bungee.bungeeAnchor.pin = bungee.bungeeAnchor.pos;

            if (grab.IsChainAnchor)
            {
                bungee.SetCutOnlyByAxe();
            }

            source.BeginFade();
            grab.SetRope(bungee);
            ropes.Register(bungee, grab);

            CTRSoundMgr.PlaySound(Resources.Snd.RopeGet);
            if (grab.mover != null)
            {
                CTRSoundMgr.PlaySound(Resources.Snd.Buzz);
            }
            return true;
        }

        /// <summary>
        /// The visual a rope rotates for one body: a whole candy turns its main layer, while a split
        /// half has only the one sprite.
        /// </summary>
        /// <param name="body">The body being rotated.</param>
        /// <returns>The visual whose rotation the rope drives.</returns>
        private static GameObject RotatedVisualOf(CandyBody body)
        {
            return body.Main ?? body.Visual;
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
                if (hand.State == MechanicalHandState.HoldingCandy && heldCandy != null)
                {
                    CandyBody heldBody = heldCandy.WholeBody;
                    heldBody.Visual.drawX += hand.cPoint.pos.X - heldBody.Point.pos.X;
                    heldBody.Visual.drawY += hand.cPoint.pos.Y - heldBody.Point.pos.Y;
                    heldBody.Point.pos = hand.cPoint.pos;

                    // Pin prevPos to the claw as well. Otherwise prevPos keeps the candy's pre-grab
                    // physics position while pos is teleported to the claw, so Verlet reads the teleport
                    // gap (e.g. the rope still pulling the candy up) as a fake velocity. A bouncer sitting
                    // at the claw amplifies that phantom velocity into a huge impulse and launches the candy.
                    heldBody.Point.prevPos = heldBody.Point.pos;

                    if (hand.DoRotateCandy)
                    {
                        if (hand.rotatingSegment != null)
                        {
                            GameObject rotatingCandyVisual = heldBody.Main ?? heldBody.Visual;
                            rotatingCandyVisual.rotation += hand.rotatingSegment.RotationDelta();
                        }
                    }
                    else if (heldCandy.Lifecycle.Attachments.HasActiveRocket)
                    {
                        hand.BeginCandyRotation();
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

                    // Steal-proximity: only override the grab distance when the other hand
                    // holds *this* hand's target candy (single-candy legacy measured hand-to-hand
                    // because the holder sat on the only candy). With multiple candies a hand
                    // holding a different candy must not corrupt our distance to our own candy.
                    if (otherHand.State == MechanicalHandState.HoldingCandy && HandHeldCandy(otherHand) == nearestCandy)
                    {
                        distance = VectDistance(hand.cPoint.pos, otherHand.cPoint.pos);
                    }

                    if (hand.TryClapWith(otherHand))
                    {
                        PlayMechanicalHandClapEffectAt(otherHand.ClawPosition());
                        hand.AnimateClap();
                        otherHand.AnimateClap();
                        CTRSoundMgr.PlaySound(Resources.Snd.ExpHandClap);
                    }
                }

                if (nearestCandy != null
                    && HandGrab.ShouldGrab(
                        hand.State == MechanicalHandState.Idle,
                        !nearestCandy.HasNoWholeBodyInPlay,
                        nearestCandy.Lifecycle.Attachments.InLantern,
                        nearestCandy.Lifecycle.Transport?.Sock != null,
                        distance < MechanicalHand.MH_GRAB_DISTANCE))
                {
                    CandyContext ctx = nearestCandy;
                    // A hand only grabs a whole candy, so the claw constrains one body's point.
                    CandyBody grabbedBody = ctx.WholeBody;

                    // Hand-stealing: release any other hand currently holding this same candy.
                    if (hands.Count > 1)
                    {
                        foreach (MechanicalHand otherHand in hands)
                        {
                            if (otherHand != null && HandSteal.ShouldReleaseOtherHand(
                                    otherHand != hand,
                                    otherHand.State == MechanicalHandState.HoldingCandy,
                                    ctx.Lifecycle.Attachments.Hand == otherHand))
                            {
                                otherHand.cPoint.RemoveConstraint(grabbedBody.Point);
                                otherHand.ReleaseCandy();
                                reorderHands = true;
                                break;
                            }
                        }
                    }

                    hand.cPoint.AddConstraintwithRestLengthofType(grabbedBody.Point, 1f, Constraint.CONSTRAINT.NOT_MORE_THAN);
                    hand.GrabCandy();
                    selectedHandIndex = hands.IndexOf(hand);
                    _ = ctx.Lifecycle.Attachments.CaptureByHand(hand);

                    // Take this candy off the ants (if it was riding them). Other candies keep
                    // their conveyor; ants won't re-grab this one while the hand holds it.
                    DetachCandyFromConveyor(ctx);

                    // The claw bursts the bubble where it snatched the candy, not where the candy
                    // ends up.
                    PopCandyBubbleAt(grabbedBody, hand.ClawPosition());

                    if (ctx.Lifecycle.Attachments.HasActiveRocket)
                    {
                        int count = Preferences.GetIntForKey("PREFS_GRAB_ROCKET") + 1;
                        Preferences.SetIntForKey(count, "PREFS_GRAB_ROCKET", false);
                        if (count >= 50)
                        {
                            CTRRootController.PostAchievementName("acRoboMaster", ACHIEVEMENT_STRING("\"Robo Master\""));
                        }
                    }

                    // A snail riding this candy added weight to drag it down. Force-detaching the snail
                    // here must give that weight back, otherwise the released candy keeps falling as if the
                    // snail were still attached. Gate on real snail presence so a heavier rocket candy grabbed
                    // without a snail keeps its own weight.
                    int detachedSnails = ActiveSnailCountForPoint(grabbedBody.Point);
                    DetachSnailsForPoint(grabbedBody.Point);
                    if (detachedSnails > 0)
                    {
                        grabbedBody.Point.SetWeight(SnailWeight.AfterForceDetach(grabbedBody.Point.weight, detachedSnails));
                    }
                    DropMouseCandyForPoint(grabbedBody.Point);
                    RestoreCandyProperties(ctx);
                    hand.AnimateCatchWithCandyPartsandAnimationsPool(ctx.HandCatchVisuals(), ctx.HandCatchScale, aniPool);
                    CTRSoundMgr.PlaySound(Resources.Snd.ExpHandCatch);
                }

                if (hand.TrySettleToIdle(distance) == HandSettle.SettledOwingDropSound)
                {
                    CTRSoundMgr.PlaySound(Resources.Snd.ExpHandDrop);
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
