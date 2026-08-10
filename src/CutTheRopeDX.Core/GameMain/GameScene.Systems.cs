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
            float flowLength = ActivePhysicsConstants.PumpFlowLength;
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
                // Desktop keeps the pump's bbox dimensions for all objects; mobile matches the
                // reference, which tests the target object's own bbox against the flow column.
                float flowBoxW = ActivePhysicsConstants.UseMobilePhysicsModel ? c.bb.w : p.bb.w;
                float flowBoxH = ActivePhysicsConstants.UseMobilePhysicsModel ? c.bb.h : p.bb.h;
                if (v.Y < vector.Y && RectInRect(v.X - (flowBoxW / 2), v.Y - (flowBoxH / 2), v.X + (flowBoxW / 2), v.Y + (flowBoxH / 2), vector.X, vector.Y - flowLength, vector2.X, vector2.Y))
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
            bool gravityInverted = gravityState.IsInverted;

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
                            : ABS(velocity.X) < ActivePhysicsConstants.SteamTubeVelocityDeadzone ? 0f - velocity.X : (0f - velocity.X) / damping;
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
                    float attenuation = MathF.Exp(ActivePhysicsConstants.SteamTubeFalloffExponent * (distanceBelowValve - (currentHeight + collisionRadius)));
                    impulse = VectMult(impulse, attenuation);
                }
                impulse = VectRotate(impulse, angle);
                pt.ApplyImpulseDelta(impulse, delta);
                return true;
            }

            // Lift every body in the steam column in one pass: whole candies and surviving split
            // halves alike, since the column pushes on physics points and knows nothing of topology.
            foreach (CandyBody body in ActiveCandyBodies(CandyInteraction.Steam))
            {
                _ = ApplyImpulse(body.Point);
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
            float flowLength = MathF.Max(0f, ActivePhysicsConstants.PumpFlowLength - Pump.MouthOffset);
            PumpDirt pumpDirt = new PumpDirt().InitWithTotalParticlesAngleandImageGrid(5, RADIANS_TO_DEGREES(p.angle) - DEG_90, grid, flowLength);
            pumpDirt.particlesDelegate = new Particles.ParticlesFinished(aniPool.ParticlesFinished);
            Vector v = Vect(p.x + Pump.MouthOffset, p.y);
            v = VectRotateAround(v, p.angle - (MathF.PI / 2), p.x, p.y);
            pumpDirt.x = v.X;
            pumpDirt.y = v.Y;
            pumpDirt.StartSystem(5);
            _ = aniPool.AddChild(pumpDirt);
            // Pump every body in the flow in one pass: whole candies and surviving split halves alike.
            foreach (CandyBody body in ActiveCandyBodies(CandyInteraction.Pump))
            {
                HandlePumpFlowPtSkin(p, body.Point, body.Visual);
            }
            foreach (object bungee in bungees)
            {
                Grab grab = (Grab)bungee;
                if (grab?.Rope != null && grab.Mount?.IsMounted == false)
                {
                    HandlePumpFlowPtSkin(p, grab.Rope.bungeeAnchor, grab);
                }
            }
        }

        /// <summary>
        /// Starts the bamboo teleport for one candy: hides its whole body inside a new
        /// <see cref="CandyTransportSession"/> and enqueues that exact session for the delayed exit.
        /// </summary>
        /// <param name="bambooTube">The tube swallowing the candy.</param>
        /// <param name="ctx">The candy entering the tube.</param>
        public void OperateBambooTube(BambooTube bambooTube, CandyContext ctx)
        {
            // A split candy has no whole body to swallow, so the lifecycle check below already
            // refuses it: HasNoWholeBodyInPlay covers removed, hidden, and split alike.
            if (bambooTube == null || ctx.HasNoWholeBodyInPlay)
            {
                return;
            }

            // TryHide is the entry gate as well as the transition: it refuses a candy that is already
            // hidden in another transit, removed, or split.
            CandyTransportSession session = CandyTransportSession.ForBamboo(ctx, bambooTube);
            if (!ctx.Lifecycle.TryHide(session, out CandyAttachmentSnapshot detached))
            {
                return;
            }

            // The tube swallows the whole body; the guard above already refused a split candy.
            CandyBody body = ctx.WholeBody;
            GameObject candyVisual = body.Visual;
            ReleaseRopesForPoint(body.Point);
            ReleaseTransportAttachments(detached, body.Point);
            // The Experiments reference's operateTube: sets the rocket invisible for the transit.
            if (ctx.Lifecycle.Attachments.HasActiveRocket)
            {
                ctx.Lifecycle.Attachments.Rocket.visible = false;
            }
            dd.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(Selector_teleport), session, 0.15f);
            body.Point.disableGravity = true;
            candyVisual.passTransformationsToChilds = true;
            foreach (BaseElement visual in ctx.TransformChildVisuals())
            {
                visual.scaleX = visual.scaleY = 1f;
            }

            if (candyVisual.GetTimeline(1) != null)
            {
                candyVisual.RemoveTimeline(1);
            }

            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline.AddKeyFrame(KeyFrame.MakePos((int)candyVisual.x, (int)candyVisual.y, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0f));
            float towardTubeX = candyVisual.x + ((bambooTube.x - candyVisual.x) * 0.3f);
            float towardTubeY = candyVisual.y + ((bambooTube.y - candyVisual.y) * 0.3f);
            timeline.AddKeyFrame(KeyFrame.MakePos((int)towardTubeX, (int)towardTubeY, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.1f));
            timeline.AddKeyFrame(KeyFrame.MakeScale(candyVisual.scaleX, candyVisual.scaleY, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0f));
            timeline.AddKeyFrame(KeyFrame.MakeScale(0f, 0f, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.1f));
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0f));
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.1f));
            candyVisual.AddTimelinewithID(timeline, 1);
            candyVisual.PlayTimeline(1);
            timeline.delegateTimelineDelegate = aniPool;
            _ = aniPool.AddChild(candyVisual);
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
            // One pass over every rope in the level: a hook's rope and the candy connector are cut
            // by the same test, they only differ in whether an owning hook reacts afterwards.
            foreach (RopeEntry entry in ropes.All)
            {
                Bungee rope = entry.Rope;
                if (rope.cut != -1 || rope.cutOnlyByAxe)
                {
                    // Chains are cut only by the axe (see CutAxeOnlyChainsWithAxes); the
                    // finger/razor never cuts them.
                    continue;
                }

                for (int j = 0; j < rope.parts.Count - 1; j++)
                {
                    ConstraintedPoint a = rope.parts[j];
                    ConstraintedPoint b = rope.parts[j + 1];
                    bool hit;
                    if (r == null)
                    {
                        CTRRectangle? exclusion = entry.Owner?.CutExclusionZone;
                        bool outsideExclusion = exclusion == null
                            || !LineInRect(v1.X, v1.Y, v2.X, v2.Y, exclusion.Value.x, exclusion.Value.y, exclusion.Value.w, exclusion.Value.h);
                        hit = outsideExclusion
                            && LineInLine(v1.X, v1.Y, v2.X, v2.Y, a.pos.X, a.pos.Y, b.pos.X, b.pos.Y);
                    }
                    else if (a.prevPos.X != UNDEFINED_COORDINATE)
                    {
                        float minX = MinOf4(a.pos.X, a.prevPos.X, b.pos.X, b.prevPos.X);
                        float minY = MinOf4(a.pos.Y, a.prevPos.Y, b.pos.Y, b.prevPos.Y);
                        float maxX = MaxOf4(a.pos.X, a.prevPos.X, b.pos.X, b.prevPos.X);
                        float maxY = MaxOf4(a.pos.Y, a.prevPos.Y, b.pos.Y, b.prevPos.Y);
                        hit = RectInRect(minX, minY, maxX, maxY, r.drawX, r.drawY, r.drawX + r.width, r.drawY + r.height);
                    }
                    else
                    {
                        hit = false;
                    }

                    if (!hit)
                    {
                        continue;
                    }

                    ropesCutCount++;
                    if (entry.Owner?.Spider?.ShouldBustOnRopeCut == true)
                    {
                        SpiderBusted(entry.Owner);
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

                    entry.Owner?.OnRopeCut(RopeCutReason.Severed);
                    return ropesCutCount;
                }
            }
            return ropesCutCount;
        }

        /// <summary>
        /// Cuts axe-only chains when an active Time Travel axe blade overlaps a chain point.
        /// Mirrors the original <c>cutTheUnbreakable</c> radius check.
        /// </summary>
        private void CutAxeOnlyChainsWithAxes()
        {
            for (int ci = 0; ci < candies.Count; ci++)
            {
                CandyContext axeCtx = candies[ci];
                if (axeCtx.axe == null || axeCtx.HasNoWholeBodyInPlay)
                {
                    continue;
                }

                // One pass over every chain in the level - hook chains and a chain connector alike.
                // At most one chain per axe swing, which is what the grab loop's break already did.
                foreach (RopeEntry entry in ropes.All)
                {
                    if (entry.Rope.cut != -1 || !entry.Rope.cutOnlyByAxe)
                    {
                        continue;
                    }

                    if (TryCutAxeOnlyChain(axeCtx, entry.Rope))
                    {
                        break;
                    }
                }
            }
        }

        private bool TryCutAxeOnlyChain(CandyContext axeCtx, Bungee rope)
        {
            ConstraintedPoint bladePoint = axeCtx.WholeBody.Point;
            for (int i = 0; i < rope.parts.Count; i++)
            {
                ConstraintedPoint part = rope.parts[i];
                if (part == null || ReferenceEquals(part, bladePoint))
                {
                    continue;
                }

                if (VectDistance(part.pos, bladePoint.pos) > AxeDefinition.ChainCutRadius)
                {
                    continue;
                }

                int cutPart = MIN(i, rope.parts.Count - 2);
                if (cutPart < 0)
                {
                    return false;
                }

                rope.SetCut(cutPart);
                SpawnChainCutEffectAtXY(part.pos.X, part.pos.Y, GetChainCutSwingAngleDegrees(bladePoint));
                return true;
            }

            return false;
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
            Image image = Image.Image_createWithResIDQuad(Resources.Img.ObjSpider, 11);
            image.DoRestoreCutTransparency();
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(3);
            if (gravityState.IsInverted)
            {
                timeline.AddKeyFrame(KeyFrame.MakePos((int)g.Spider.Animation.x, (int)g.Spider.Animation.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0));
                timeline.AddKeyFrame(KeyFrame.MakePos((int)g.Spider.Animation.x, (int)(g.Spider.Animation.y + 50), KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.3f));
                timeline.AddKeyFrame(KeyFrame.MakePos((int)g.Spider.Animation.x, (int)(g.Spider.Animation.y - SCREEN_HEIGHT), KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 1));
            }
            else
            {
                timeline.AddKeyFrame(KeyFrame.MakePos((int)g.Spider.Animation.x, (int)g.Spider.Animation.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0));
                timeline.AddKeyFrame(KeyFrame.MakePos((int)g.Spider.Animation.x, (int)(g.Spider.Animation.y - 50), KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.3f));
                timeline.AddKeyFrame(KeyFrame.MakePos((int)g.Spider.Animation.x, (int)(g.Spider.Animation.y + SCREEN_HEIGHT), KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 1));
            }
            timeline.AddKeyFrame(KeyFrame.MakeRotation(0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline.AddKeyFrame(KeyFrame.MakeRotation(RND_RANGE(-120, 120), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1));
            image.AddTimelinewithID(timeline, 0);
            image.PlayTimeline(0);
            image.x = g.Spider.Animation.x;
            image.y = g.Spider.Animation.y;
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
            ConstraintedPoint capturedStar = sg.Rope?.tail;
            // spiderTookCandy = true;
            // The spider takes whichever body its rope ends on - a whole candy or one split half. A
            // rope that ends on no live body has nothing to steal; it used to steal the primary candy.
            CandyBody capturedBody = CandyBodyForPointOrNull(capturedStar);
            if (capturedBody == null)
            {
                return;
            }

            // Generic rope retirement busts every rider whose rope is cut. Mark this rider first so
            // it is recognized as the winner while the other riders on this body are busted.
            sg.Spider.Win();
            if (!TryRetireCandyBody(capturedBody, CandyRemovalReason.Spider))
            {
                return;
            }

            CTRSoundMgr.PlaySound(Resources.Snd.SpiderWin);
            GameObject capturedCandy = capturedBody.Visual;
            Image image = Image.Image_createWithResIDQuad(Resources.Img.ObjSpider, 12);
            image.DoRestoreCutTransparency();
            capturedCandy.anchor = capturedCandy.parentAnchor = 18;
            capturedCandy.x = 0f;
            capturedCandy.y = -5f;
            _ = image.AddChild(capturedCandy);
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(3);
            if (gravityState.IsInverted)
            {
                timeline.AddKeyFrame(KeyFrame.MakePos((int)sg.Spider.Animation.x, (int)(sg.Spider.Animation.y - 10), KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0));
                timeline.AddKeyFrame(KeyFrame.MakePos((int)sg.Spider.Animation.x, (int)(sg.Spider.Animation.y + 70), KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.3f));
                timeline.AddKeyFrame(KeyFrame.MakePos((int)sg.Spider.Animation.x, (int)(sg.Spider.Animation.y - SCREEN_HEIGHT), KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 1));
            }
            else
            {
                timeline.AddKeyFrame(KeyFrame.MakePos((int)sg.Spider.Animation.x, (int)(sg.Spider.Animation.y - 10), KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0));
                timeline.AddKeyFrame(KeyFrame.MakePos((int)sg.Spider.Animation.x, (int)(sg.Spider.Animation.y - 70), KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.3f));
                timeline.AddKeyFrame(KeyFrame.MakePos((int)sg.Spider.Animation.x, (int)(sg.Spider.Animation.y + SCREEN_HEIGHT), KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 1));
            }
            image.AddTimelinewithID(timeline, 0);
            image.PlayTimeline(0);
            image.x = sg.Spider.Animation.x;
            image.y = sg.Spider.Animation.y - 10f;
            image.anchor = 18;
            timeline.delegateTimelineDelegate = aniPool;
            _ = aniPool.AddChild(image);
            if (gameplayFlow.CanTriggerOutcome)
            {
                ScheduleGameLost(2);
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
                Bungee rope = grab2.Rope;
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
            Bungee rope = g.Rope;
            if (rope == null || rope.cut != -1)
            {
                return null;
            }
            for (int i = 0; i < rope.parts.Count - 1; i++)
            {
                ConstraintedPoint constraintedPoint = rope.parts[i];
                float distanceToConstraint = VectDistance(constraintedPoint.pos, v);
                if (distanceToConstraint < closestDistance && (g.Wheel == null || !PointInRect(constraintedPoint.pos.X, constraintedPoint.pos.Y, g.x - WheelControl.TapHalfExtent, g.y - WheelControl.TapHalfExtent, WheelControl.TapHalfExtent * 2f, WheelControl.TapHalfExtent * 2f)))
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
