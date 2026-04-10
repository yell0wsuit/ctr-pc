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
        /// Handles pump physics interaction with a constrained point and game object
        /// </summary>
        /// <param name="p">Pump producing the flow impulse.</param>
        /// <param name="s">Constrained point to receive the impulse.</param>
        /// <param name="c">Game object tested against the pump flow area.</param>
        public static void HandlePumpFlowPtSkin(Pump p, ConstraintedPoint s, GameObject c)
        {
            float flowLength = Pump.FlowLength;
            if (GameObject.RectInObject(p.x - flowLength, p.y - flowLength, p.x + flowLength, p.y + flowLength, c))
            {
                Vector v = Vect(c.x, c.y);
                Vector vector = default;
                vector.X = p.x - (p.bb.w / 2f);
                Vector vector2 = default;
                vector2.X = p.x + (p.bb.w / 2f);
                vector.Y = vector2.Y = p.y;
                if (p.angle != 0)
                {
                    v = VectRotateAround(v, 0 - p.angle, p.x, p.y);
                }
                // Use pump's bbox dimensions for all objects (not the object's bbox)
                if (v.Y < vector.Y && RectInRect(v.X - (p.bb.w / 2), v.Y - (p.bb.h / 2), v.X + (p.bb.w / 2), v.Y + (p.bb.h / 2), vector.X, vector.Y - flowLength, vector2.X, vector2.Y))
                {
                    float verticalImpulse = flowLength * 2f * (flowLength - (vector.Y - v.Y)) / flowLength;
                    Vector v2 = Vect(0f, 0f - verticalImpulse);
                    v2 = VectRotate(v2, p.angle);
                    s.ApplyImpulseDelta(v2, 0.016f);
                }
            }
        }

        /// <summary>
        /// Handles bouncer physics interaction with a constrained point
        /// </summary>
        /// <param name="b">Bouncer source.</param>
        /// <param name="s">Constrained point affected by the bounce.</param>
        /// <param name="delta">Frame delta time used when applying impulse.</param>
        public static void HandleBouncePtDelta(Bouncer b, ConstraintedPoint s, float delta)
        {
            if (!b.skip)
            {
                // b.skip = true;
                Vector vector = VectSub(s.prevPos, s.pos);
                int directionSign = VectRotateAround(s.prevPos, 0f - b.angle, b.x, b.y).Y >= b.y ? 1 : -1;
                float s2 = MAX(VectLength(vector) * ActivePhysicsConstants.BouncerImpulseVelocityScale, ActivePhysicsConstants.BouncerMinImpulse) * directionSign;
                Vector impulse = VectMult(VectPerp(VectForAngle(b.angle)), s2);
                s.pos = VectRotateAround(s.pos, 0f - b.angle, b.x, b.y);
                s.prevPos = VectRotateAround(s.prevPos, 0f - b.angle, b.x, b.y);
                s.prevPos.Y = s.pos.Y;
                s.pos = VectRotateAround(s.pos, b.angle, b.x, b.y);
                s.prevPos = VectRotateAround(s.prevPos, b.angle, b.x, b.y);
                s.ApplyImpulseDelta(impulse, delta);
                b.PlayTimeline(0);
                CTRSoundMgr.PlaySound(Resources.Snd.Bouncer);
            }
        }

        /// <summary>
        /// Applies steam tube forces and interacts with candy pieces inside the flow area.
        /// Uses a dedicated mobile formula when mobile physics model is enabled.
        /// </summary>
        /// <param name="tube">Steam tube that emits the force field.</param>
        /// <param name="delta">Frame delta time used when applying impulses.</param>
        public void OperateSteamTube(SteamTube tube, float delta)
        {
            float tubeScale = tube.GetHeightScale();
            float damping = ActivePhysicsConstants.SteamTubeDamping;
            float angle = DEGREES_TO_RADIANS(tube.rotation);
            float tubeWidth = ActivePhysicsConstants.SteamTubeWidthScale * tubeScale;
            float currentHeight = tube.GetCurrentHeightModulated();
            float verticalOffset = ActivePhysicsConstants.SteamTubeVerticalOffsetScale * tubeScale;
            float collisionRadius = ActivePhysicsConstants.SteamTubeCollisionRadiusScale * tubeScale;
            bool useMobileFormula = ActivePhysicsConstants.UseMobilePhysicsModel;
            bool gravityInverted = gravityButton != null && !gravityNormal;

            float rectLeft = tube.x - (tubeWidth / 2f);
            float rectTop = tube.y - currentHeight - verticalOffset;
            float rectRight = tube.x + (tubeWidth / 2f);
            float rectBottom = tube.y - collisionRadius;

            bool ApplyImpulse(ConstraintedPoint pt)
            {
                Vector position = Vect(pt.pos.X, pt.pos.Y);
                Vector velocity = Vect(pt.v.X, pt.v.Y);
                position = VectRotateAround(position, 0 - angle, tube.x, tube.y);
                velocity = VectRotate(velocity, 0 - angle);

                bool insideTube = RectInRect(
                    position.X - collisionRadius, position.Y - (collisionRadius / 2f),
                    position.X + collisionRadius, position.Y + collisionRadius,
                    rectLeft, rectTop, rectRight, rectBottom);

                if (!insideTube)
                {
                    return false;
                }

                foreach (Bouncer bouncer in bouncers)
                {
                    bouncer.skip = false;
                }

                float horizontalImpulse = 0f;
                float localDamping = damping;
                float gravityCompensation;

                if (useMobileFormula)
                {
                    // Windows Phone-style centering applies only at 0 degrees.
                    if (tube.rotation == 0f)
                    {
                        float deltaX = tube.x - position.X;
                        horizontalImpulse = ABS(deltaX) > tubeWidth / 4f
                            ? ((0f - velocity.X) / damping) + (0.25f * deltaX)
                            : ABS(velocity.X) < 1f ? 0f - velocity.X : (0f - velocity.X) / damping;
                    }

                    // Windows Phone force, mapped to world scale (tubeScale is world/Windows Phone transform).
                    gravityCompensation = ActivePhysicsConstants.SteamTubeGravityCompensation * tubeScale / pt.weight;
                    if (tube.rotation != 0f)
                    {
                        localDamping *= ActivePhysicsConstants.SteamTubeNonAlignedDampingMultiplier;
                        if (tube.rotation == DEG_180)
                        {
                            gravityCompensation /= ActivePhysicsConstants.SteamTubeOppositeGravityDivisor;
                        }
                        else
                        {
                            gravityCompensation /= ActivePhysicsConstants.SteamTubeSideGravityDivisor;
                        }
                    }
                }
                else
                {
                    bool applyHorizontalCentering =
                        (tube.rotation == 0f && !gravityInverted) ||
                        (tube.rotation == DEG_180 && gravityInverted);
                    if (applyHorizontalCentering)
                    {
                        float deltaX = tube.x - position.X;
                        horizontalImpulse = ABS(deltaX) > tubeWidth / 4f
                            ? ((0f - velocity.X) / damping) + (0.25f * deltaX)
                            : ABS(velocity.X) < 1f ? 0f - velocity.X : (0f - velocity.X) / damping;
                    }

                    bool alignedWithGravity =
                        (tube.rotation == 0f && !gravityInverted) ||
                        (tube.rotation == DEG_180 && gravityInverted);
                    gravityCompensation = ActivePhysicsConstants.SteamTubeGravityCompensation / pt.weight * MathF.Sqrt(tubeScale);
                    if (!alignedWithGravity)
                    {
                        localDamping *= ActivePhysicsConstants.SteamTubeNonAlignedDampingMultiplier;
                        if (tube.rotation is DEG_90 or DEG_270)
                        {
                            gravityCompensation /= ActivePhysicsConstants.SteamTubeSideGravityDivisor;
                        }
                        else
                        {
                            gravityCompensation /= ActivePhysicsConstants.SteamTubeOppositeGravityDivisor;
                        }
                    }
                }

                Vector impulse = Vect(horizontalImpulse, ((0f - velocity.Y) / localDamping) + gravityCompensation);
                float distanceBelowValve = tube.y - position.Y;
                if (distanceBelowValve > currentHeight + collisionRadius)
                {
                    float attenuation = MathF.Exp(-2f * (distanceBelowValve - (currentHeight + collisionRadius)));
                    impulse = VectMult(impulse, attenuation);
                }
                impulse = VectRotate(impulse, angle);
                pt.ApplyImpulseDelta(impulse, delta);
                return true;
            }

            if (twoParts == 2)
            {
                if (!noCandy)
                {
                    _ = ApplyImpulse(star);
                }
            }
            else
            {
                if (!noCandyL)
                {
                    _ = ApplyImpulse(starL);
                }
                if (!noCandyR)
                {
                    _ = ApplyImpulse(starR);
                }
            }

            if (lightBulbs.Count > 0)
            {
                for (int i = 0; i < lightBulbs.Count; i++)
                {
                    LightBulb bulb = lightBulbs[i];
                    if (bulb == null || bulb.attachedSock != null)
                    {
                        continue;
                    }
                    _ = ApplyImpulse(bulb.constraint);
                }
            }
        }

        /// <summary>
        /// Operates a pump - creates particles and applies force
        /// </summary>
        /// <param name="p">Pump to animate and process.</param>
        public void OperatePump(Pump p)
        {
            p.PlayTimeline(0);
            CTRSoundMgr.PlayRandomSound(Resources.Snd.Pump1, Resources.Snd.Pump2, Resources.Snd.Pump3, Resources.Snd.Pump4);
            Image grid = Image.Image_createWithResID(Resources.Img.ObjPump);
            float flowLength = MathF.Max(0f, Pump.FlowLength - Pump.MouthOffset);
            PumpDirt pumpDirt = new PumpDirt().InitWithTotalParticlesAngleandImageGrid(5, RADIANS_TO_DEGREES(p.angle) - DEG_90, grid, flowLength);
            pumpDirt.particlesDelegate = new Particles.ParticlesFinished(aniPool.ParticlesFinished);
            Vector v = Vect(p.x + Pump.MouthOffset, p.y);
            v = VectRotateAround(v, p.angle - (MathF.PI / 2), p.x, p.y);
            pumpDirt.x = v.X;
            pumpDirt.y = v.Y;
            pumpDirt.StartSystem(5);
            _ = aniPool.AddChild(pumpDirt);
            if (!noCandy)
            {
                HandlePumpFlowPtSkin(p, star, candy);
            }
            if (twoParts != 2)
            {
                if (!noCandyL)
                {
                    HandlePumpFlowPtSkin(p, starL, candyL);
                }
                if (!noCandyR)
                {
                    HandlePumpFlowPtSkin(p, starR, candyR);
                }
            }
            if (lightBulbs.Count > 0)
            {
                for (int i = 0; i < lightBulbs.Count; i++)
                {
                    LightBulb bulb = lightBulbs[i];
                    if (bulb != null && bulb.attachedSock == null)
                    {
                        HandlePumpFlowPtSkin(p, bulb.constraint, bulb);
                    }
                }
            }
            foreach (object bungee in bungees)
            {
                Grab grab = (Grab)bungee;
                if (grab?.rope != null && grab.kickable && grab.kicked)
                {
                    HandlePumpFlowPtSkin(p, grab.rope.bungeeAnchor, grab);
                }
            }
        }

        /// <summary>
        /// Starts bamboo tube teleport sequence for the main candy.
        /// </summary>
        /// <param name="bambooTube">Bamboo tube that receives the candy.</param>
        public void OperateBambooTube(BambooTube bambooTube)
        {
            if (bambooTube == null || targetBambooTube != null || twoParts != PARTS_NONE || noCandy)
            {
                return;
            }

            ReleaseAllRopes(false);
            DetachActiveHands();
            targetBambooTube = bambooTube;
            dd.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(Selector_teleport), null, 0.15f);
            noCandy = true;
            star.disableGravity = true;
            candy.passTransformationsToChilds = true;
            candyMain.scaleX = candyMain.scaleY = 1f;
            candyTop.scaleX = candyTop.scaleY = 1f;

            if (candy.GetTimeline(1) != null)
            {
                candy.RemoveTimeline(1);
            }

            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline.AddKeyFrame(KeyFrame.MakePos((int)candy.x, (int)candy.y, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0f));
            float towardTubeX = candy.x + ((bambooTube.x - candy.x) * 0.3f);
            float towardTubeY = candy.y + ((bambooTube.y - candy.y) * 0.3f);
            timeline.AddKeyFrame(KeyFrame.MakePos((int)towardTubeX, (int)towardTubeY, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.1f));
            timeline.AddKeyFrame(KeyFrame.MakeScale(candy.scaleX, candy.scaleY, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0f));
            timeline.AddKeyFrame(KeyFrame.MakeScale(0f, 0f, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.1f));
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0f));
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.1f));
            candy.AddTimelinewithID(timeline, 1);
            candy.PlayTimeline(1);
            timeline.delegateTimelineDelegate = aniPool;
            _ = aniPool.AddChild(candy);
        }

        /// <summary>
        /// Cuts ropes with a razor or line. Returns number of ropes cut.
        /// </summary>
        /// <param name="r">Razor bounds source; when null, line intersection is used.</param>
        /// <param name="v1">Line start point when line cutting is used.</param>
        /// <param name="v2">Line end point when line cutting is used.</param>
        /// <param name="im">Whether to remove the rope part immediately after cutting.</param>
        /// <returns>The number of ropes cut during this call.</returns>
        public int CutWithRazorOrLine1Line2Immediate(Razor r, Vector v1, Vector v2, bool im)
        {
            int ropesCutCount = 0;
            for (int i = 0; i < bungees.Count; i++)
            {
                Grab grab = bungees[i];
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
                            flag = (!grab.wheel || !LineInRect(v1.X, v1.Y, v2.X, v2.Y, grab.x - 110f, grab.y - 110f, 220f, 220f)) &&
                                   (!grab.gun || !LineInRect(v1.X, v1.Y, v2.X, v2.Y, grab.x - Grab.GUN_CUT_RADIUS, grab.y - Grab.GUN_CUT_RADIUS, Grab.GUN_CUT_RADIUS * 2f, Grab.GUN_CUT_RADIUS * 2f)) &&
                                   LineInLine(v1.X, v1.Y, v2.X, v2.Y, constraintedPoint.pos.X, constraintedPoint.pos.Y, constraintedPoint2.pos.X, constraintedPoint2.pos.Y);
                        }
                        else if (constraintedPoint.prevPos.X != UNDEFINED_COORDINATE)
                        {
                            float minX = MinOf4(constraintedPoint.pos.X, constraintedPoint.prevPos.X, constraintedPoint2.pos.X, constraintedPoint2.prevPos.X);
                            float y1t = MinOf4(constraintedPoint.pos.Y, constraintedPoint.prevPos.Y, constraintedPoint2.pos.Y, constraintedPoint2.prevPos.Y);
                            float x1r = MaxOf4(constraintedPoint.pos.X, constraintedPoint.prevPos.X, constraintedPoint2.pos.X, constraintedPoint2.prevPos.X);
                            float y1b = MaxOf4(constraintedPoint.pos.Y, constraintedPoint.prevPos.Y, constraintedPoint2.pos.Y, constraintedPoint2.prevPos.Y);
                            flag = RectInRect(minX, y1t, x1r, y1b, r.drawX, r.drawY, r.drawX + r.width, r.drawY + r.height);
                        }
                        if (flag)
                        {
                            ropesCutCount++;
                            if (grab.hasSpider && grab.spiderActive)
                            {
                                SpiderBusted(grab);
                            }
                            string ropeSound = rope.relaxed switch
                            {
                                0 => Resources.Snd.RopeBleak1,
                                1 => Resources.Snd.RopeBleak2,
                                2 => Resources.Snd.RopeBleak3,
                                _ => Resources.Snd.RopeBleak4
                            };
                            CTRSoundMgr.PlaySound(ropeSound);
                            rope.SetCut(j);
                            if (im)
                            {
                                rope.cutTime = 0f;
                                rope.RemovePart(j);
                            }
                            if (grab.gun && grab.gunCup != null)
                            {
                                grab.gunCup.PlayTimeline(Grab.GUN_CUP_HIDE);
                            }
                            return ropesCutCount;
                        }
                    }
                }
            }
            return ropesCutCount;
        }

        /// <summary>
        /// Called when a spider is busted - handles animation and achievements
        /// </summary>
        /// <param name="g">Grab whose spider was busted.</param>
        public void SpiderBusted(Grab g)
        {
            int spidersBustedCount = Preferences.GetIntForKey("PREFS_SPIDERS_BUSTED") + 1;
            Preferences.SetIntForKey(spidersBustedCount, "PREFS_SPIDERS_BUSTED", false);
            if (spidersBustedCount == 40)
            {
                CTRRootController.PostAchievementName("681486608", ACHIEVEMENT_STRING("\"Spider Busted\""));
            }
            if (spidersBustedCount == 200)
            {
                CTRRootController.PostAchievementName("1058341284", ACHIEVEMENT_STRING("\"Spider Tammer\""));
            }
            CTRSoundMgr.PlaySound(Resources.Snd.SpiderFall);
            g.hasSpider = false;
            Image image = Image.Image_createWithResIDQuad(Resources.Img.ObjSpider, 11);
            image.DoRestoreCutTransparency();
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(3);
            if (gravityButton != null && !gravityNormal)
            {
                timeline.AddKeyFrame(KeyFrame.MakePos((int)g.spider.x, (int)g.spider.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0));
                timeline.AddKeyFrame(KeyFrame.MakePos((int)g.spider.x, (int)(g.spider.y + 50), KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.3f));
                timeline.AddKeyFrame(KeyFrame.MakePos((int)g.spider.x, (int)(g.spider.y - SCREEN_HEIGHT), KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 1));
            }
            else
            {
                timeline.AddKeyFrame(KeyFrame.MakePos((int)g.spider.x, (int)g.spider.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0));
                timeline.AddKeyFrame(KeyFrame.MakePos((int)g.spider.x, (int)(g.spider.y - 50), KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.3f));
                timeline.AddKeyFrame(KeyFrame.MakePos((int)g.spider.x, (int)(g.spider.y + SCREEN_HEIGHT), KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 1));
            }
            timeline.AddKeyFrame(KeyFrame.MakeRotation(0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline.AddKeyFrame(KeyFrame.MakeRotation(RND_RANGE(-120, 120), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1));
            image.AddTimelinewithID(timeline, 0);
            image.PlayTimeline(0);
            image.x = g.spider.x;
            image.y = g.spider.y;
            image.anchor = 18;
            timeline.delegateTimelineDelegate = aniPool;
            _ = aniPool.AddChild(image);
        }

        /// <summary>
        /// Called when a spider successfully captures the candy
        /// </summary>
        /// <param name="sg">Grab whose spider captured the candy.</param>
        public void SpiderWon(Grab sg)
        {
            CTRSoundMgr.PlaySound(Resources.Snd.SpiderWin);
            ConstraintedPoint capturedStar = sg.rope?.tail;
            int grabCount = bungees.Count;
            for (int i = 0; i < grabCount; i++)
            {
                Grab grab = bungees[i];
                Bungee rope = grab.rope;
                if (rope != null && rope.tail == capturedStar)
                {
                    if (rope.cut == -1)
                    {
                        rope.SetCut(rope.parts.Count - 2);
                        rope.forceWhite = false;
                    }
                    if (grab.hasSpider && grab.spiderActive && sg != grab)
                    {
                        SpiderBusted(grab);
                    }
                    if (grab.gun && grab.gunCup != null && RGBAColor.RGBAEqual(RGBAColor.solidOpaqueRGBA, grab.gunCup.color))
                    {
                        grab.gunCup.PlayTimeline(Grab.GUN_CUP_DROP_AND_HIDE);
                    }
                }
            }
            sg.hasSpider = false;
            // spiderTookCandy = true;
            GameObject capturedCandy;
            if (capturedStar == starL)
            {
                noCandyL = true;
                capturedCandy = candyL;
            }
            else if (capturedStar == starR)
            {
                noCandyR = true;
                capturedCandy = candyR;
            }
            else
            {
                noCandy = true;
                capturedCandy = candy;
            }
            Image image = Image.Image_createWithResIDQuad(Resources.Img.ObjSpider, 12);
            image.DoRestoreCutTransparency();
            capturedCandy.anchor = capturedCandy.parentAnchor = 18;
            capturedCandy.x = 0f;
            capturedCandy.y = -5f;
            _ = image.AddChild(capturedCandy);
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(3);
            if (gravityButton != null && !gravityNormal)
            {
                timeline.AddKeyFrame(KeyFrame.MakePos((int)sg.spider.x, (int)(sg.spider.y - 10), KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0));
                timeline.AddKeyFrame(KeyFrame.MakePos((int)sg.spider.x, (int)(sg.spider.y + 70), KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.3f));
                timeline.AddKeyFrame(KeyFrame.MakePos((int)sg.spider.x, (int)(sg.spider.y - SCREEN_HEIGHT), KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 1));
            }
            else
            {
                timeline.AddKeyFrame(KeyFrame.MakePos((int)sg.spider.x, (int)(sg.spider.y - 10), KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0));
                timeline.AddKeyFrame(KeyFrame.MakePos((int)sg.spider.x, (int)(sg.spider.y - 70), KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.3f));
                timeline.AddKeyFrame(KeyFrame.MakePos((int)sg.spider.x, (int)(sg.spider.y + SCREEN_HEIGHT), KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 1));
            }
            image.AddTimelinewithID(timeline, 0);
            image.PlayTimeline(0);
            image.x = sg.spider.x;
            image.y = sg.spider.y - 10f;
            image.anchor = 18;
            timeline.delegateTimelineDelegate = aniPool;
            _ = aniPool.AddChild(image);
            if (activeRocket != null)
            {
                activeRocket.state = Rocket.STATE_ROCKET_EXAUST;
                activeRocket.StopAnimation();
            }
            DetachActiveSnails();
            if (restartState != 0)
            {
                dd.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(Selector_gameLost), null, 2);
            }
        }

        /// <summary>
        /// Finds the nearest bungee segment using bezier drawing points
        /// </summary>
        /// <param name="s">Receives the nearest sampled point on the rope.</param>
        /// <param name="tx">Query X position.</param>
        /// <param name="ty">Query Y position.</param>
        /// <param name="grab">Receives the grab that owns the nearest segment.</param>
        /// <returns>The nearest bungee segment, or <see langword="null"/> when none is within range.</returns>
        public Bungee GetNearestBungeeSegmentByBeziersPointsatXYgrab(ref Vector s, float tx, float ty, ref Grab grab)
        {
            float maxDistance = 60f;
            Bungee result = null;
            float nearestDistance = maxDistance;
            Vector v = Vect(tx, ty);
            for (int i = 0; i < bungees.Count; i++)
            {
                Grab grab2 = bungees[i];
                Bungee rope = grab2.rope;
                if (rope != null)
                {
                    for (int j = 0; j < rope.drawPtsCount; j += 2)
                    {
                        Vector vector = Vect(rope.drawPts[j], rope.drawPts[j + 1]);
                        float distanceToPoint = VectDistance(vector, v);
                        if (distanceToPoint < maxDistance && distanceToPoint < nearestDistance)
                        {
                            nearestDistance = distanceToPoint;
                            result = rope;
                            s = vector;
                            grab = grab2;
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Finds the nearest bungee segment using constraint points
        /// </summary>
        /// <param name="s">Input query position and output nearest constraint position.</param>
        /// <param name="g">Grab whose rope is searched.</param>
        /// <returns>The nearest bungee segment, or <see langword="null"/> when no valid segment is found.</returns>
        public static Bungee GetNearestBungeeSegmentByConstraintsforGrab(ref Vector s, Grab g)
        {
            float initialDistance = UNDEFINED_COORDINATE;
            Bungee result = null;
            float closestDistance = initialDistance;
            Vector v = s;
            Bungee rope = g.rope;
            if (rope == null || rope.cut != -1)
            {
                return null;
            }
            for (int i = 0; i < rope.parts.Count - 1; i++)
            {
                ConstraintedPoint constraintedPoint = rope.parts[i];
                float distanceToConstraint = VectDistance(constraintedPoint.pos, v);
                if (distanceToConstraint < closestDistance && (!g.wheel || !PointInRect(constraintedPoint.pos.X, constraintedPoint.pos.Y, g.x - 110f, g.y - 110f, 220f, 220f)))
                {
                    closestDistance = distanceToConstraint;
                    result = rope;
                    s = constraintedPoint.pos;
                }
            }
            return result;
        }
    }
}
