using System;
using System.Collections.Generic;

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
        /// Every physical candy body the scene currently offers to its systems: one whole body per
        /// present candy and one per surviving half of a split candy. A candy that is removed or
        /// hidden in transport contributes nothing, so a system iterating this never has to ask
        /// whether the body it is holding still exists.
        /// </summary>
        /// <returns>The active bodies, in candy order and left-before-right within a split candy.</returns>
        internal IEnumerable<CandyBody> ActiveCandyBodies()
        {
            for (int i = 0; i < candies.Count; i++)
            {
                IReadOnlyList<CandyBody> bodies = candies[i].Lifecycle.ActiveBodies;
                for (int b = 0; b < bodies.Count; b++)
                {
                    yield return bodies[b];
                }
            }
        }

        /// <summary>
        /// Every active body that the specified system is allowed to act on, filtered by
        /// <see cref="CandyBodyEligibility"/>.
        /// </summary>
        /// <param name="interaction">The scene system asking for candidates.</param>
        /// <returns>The eligible active bodies, in <see cref="ActiveCandyBodies()"/> order.</returns>
        internal IEnumerable<CandyBody> ActiveCandyBodies(CandyInteraction interaction)
        {
            foreach (CandyBody body in ActiveCandyBodies())
            {
                if (body.Allows(interaction))
                {
                    yield return body;
                }
            }
        }

        /// <summary>
        /// Resolves the active body standing on a physics point. A rope tail, a snail's anchor, or a
        /// hand's constraint all identify a candy this way.
        /// </summary>
        /// <param name="point">The physics point to resolve.</param>
        /// <returns>
        /// The active body whose <see cref="CandyBody.Point"/> is <paramref name="point"/>, or
        /// <see langword="null"/> when no active body owns it — including the dormant whole body of a
        /// split candy and the body of a candy that was removed or hidden.
        /// </returns>
        internal CandyBody CandyBodyForPointOrNull(ConstraintedPoint point)
        {
            if (point == null)
            {
                return null;
            }

            foreach (CandyBody body in ActiveCandyBodies())
            {
                if (body.Point == point)
                {
                    return body;
                }
            }

            return null;
        }

        /// <summary>
        /// Resolves the active body a free bubble is currently carrying.
        /// </summary>
        /// <param name="bubbleObj">The bubble to resolve.</param>
        /// <returns>
        /// The body whose <see cref="CandyBody.Bubble"/> is <paramref name="bubbleObj"/>, or
        /// <see langword="null"/> when the bubble carries nothing.
        /// </returns>
        private CandyBody CandyBodyForBubbleOrNull(GameObject bubbleObj)
        {
            if (bubbleObj == null)
            {
                return null;
            }

            foreach (CandyBody body in ActiveCandyBodies())
            {
                if (body.Bubble == bubbleObj)
                {
                    return body;
                }
            }

            return null;
        }

        /// <summary>
        /// Whether a candy body or the merged-candy parking ticket currently owns a bubble.
        /// </summary>
        internal bool IsBubbleClaimedByCandy(GameObject bubbleObj)
        {
            return CandyBodyForBubbleOrNull(bubbleObj) != null
                || parkedGhostBubble?.Bubble == bubbleObj;
        }

        /// <summary>
        /// The point the camera follows: the primary candy's first active body, which is its left
        /// half while it is split and its whole body otherwise. Falls back to the whole body's point
        /// when the primary has no active body at all, so the camera never loses its target.
        /// </summary>
        /// <returns>The camera's focus point.</returns>
        private ConstraintedPoint CameraFocusPoint()
        {
            IReadOnlyList<CandyBody> primaryBodies = candies[0].Lifecycle.ActiveBodies;
            return primaryBodies.Count > 0 ? primaryBodies[0].Point : candies[0].WholeBody.Point;
        }

        private bool IsSpiderGrabbableCandyPoint(ConstraintedPoint point)
        {
            CandyBody body = CandyBodyForPointOrNull(point);
            return body != null && body.Owner.Capabilities.CanBeGrabbedBySpider;
        }

        private IEnumerable<CandyContext> LightEmitters()
        {
            for (int i = 0; i < candies.Count; i++)
            {
                CandyContext ctx = candies[i];
                if (ctx.emitsLight && !ctx.HasNoWholeBodyInPlay)
                {
                    yield return ctx;
                }
            }
        }

        private IEnumerable<LightBulb> LightEmitterVisuals()
        {
            foreach (CandyContext ctx in LightEmitters())
            {
                if (ctx.LightBulb != null)
                {
                    yield return ctx.LightBulb;
                }
            }
        }

        private CandyContext FindLightEmitterByNumber(string bulbNumber)
        {
            CandyContext fallback = null;
            for (int i = 0; i < candies.Count; i++)
            {
                CandyContext ctx = candies[i];
                if (!ctx.emitsLight || ctx.LightBulb == null)
                {
                    continue;
                }

                fallback = ctx;
                if (!string.IsNullOrEmpty(bulbNumber)
                    && string.Equals(ctx.lightBulbNumber, bulbNumber, StringComparison.OrdinalIgnoreCase))
                {
                    return ctx;
                }
            }

            return fallback;
        }

        /// <summary>
        /// Completes one pending candy teleport. The session is the dispatcher payload enqueued when
        /// the candy entered the transporter, and it identifies the transit as well as carrying it:
        /// a session that is no longer the candy's current one is a stale callback from a transit
        /// that already finished, and it is dropped without touching the candy.
        /// </summary>
        /// <param name="session">The transport session whose delayed completion is firing.</param>
        public void Teleport(CandyTransportSession session)
        {
            CandyContext ctx = session?.Candy;
            if (ctx == null || !ctx.Lifecycle.TryCompleteTransport(session))
            {
                return;
            }

            // Transport only ever hides and restores the whole body; halves never enter a
            // transporter, so the session has exactly one body to put back on the field.
            CandyBody body = ctx.WholeBody;

            if (session.Kind == CandyTransportKind.Bamboo)
            {
                RestoreCandyProperties(ctx);
                session.BambooTube.ThrowCandy(body.Point);
                session.BambooTube.ThrowParticlesOut(particlesAniPool);
                body.Visual.PlayTimeline(2);
                if (ctx.Lifecycle.Attachments.HasActiveRocket)
                {
                    ctx.Lifecycle.Attachments.Rocket.visible = true;
                    Vector holeOut = session.BambooTube.HoleOut;
                    Vector tubeCenter = Vect(session.BambooTube.x, session.BambooTube.y);
                    ctx.Lifecycle.Attachments.Rocket.rotation = RADIANS_TO_DEGREES(VectAngleNormalized(VectSub(tubeCenter, holeOut)));
                    ctx.Lifecycle.Attachments.Rocket.startRotation = ctx.Lifecycle.Attachments.Rocket.rotation;
                    ctx.Lifecycle.Attachments.Rocket.startCandyRotation = 0f;
                    GameObject rocketCandyVisual = body.Main ?? body.Visual;
                    rocketCandyVisual.rotation = 0f;
                    ctx.Lifecycle.Attachments.Rocket.additionalAngle = 0f;
                    ctx.Lifecycle.Attachments.Rocket.UpdateRotation();
                    ctx.Lifecycle.Attachments.Rocket.point.posDelta = vectZero;
                    ctx.Lifecycle.Attachments.Rocket.point.pos = body.Point.pos;
                    ctx.Lifecycle.Attachments.Rocket.point.prevPos = ctx.Lifecycle.Attachments.Rocket.point.pos;
                    ctx.Lifecycle.Attachments.Rocket.point.v = vectZero;
                }
                body.Point.disableGravity = IsCandyGravitySuppressed(ctx);

                return;
            }

            if (session.Sock != null)
            {
                session.Sock.light.PlayTimeline(0);
                session.Sock.light.visible = true;
                Vector v = Vect(0f, ActivePhysicsConstants.SockExitOffsetY);
                v = VectRotate(v, DEGREES_TO_RADIANS(session.Sock.rotation));
                body.Point.pos.X = session.Sock.x;
                body.Point.pos.Y = session.Sock.y;
                body.Point.pos = VectAdd(body.Point.pos, v);
                body.Point.prevPos.X = body.Point.pos.X;
                body.Point.prevPos.Y = body.Point.pos.Y;
                body.Point.v = VectMult(VectRotate(Vect(0f, -1f), DEGREES_TO_RADIANS(session.Sock.rotation)), session.SavedExitSpeed);
                body.Point.posDelta = VectDiv(body.Point.v, 60f);
                body.Point.prevPos = VectSub(body.Point.pos, body.Point.posDelta);

                if (ctx.Lifecycle.Attachments.HasActiveRocket)
                {
                    ctx.Lifecycle.Attachments.Rocket.visible = true;
                    ctx.Lifecycle.Attachments.Rocket.point.pos = body.Point.pos;
                    ctx.Lifecycle.Attachments.Rocket.point.prevPos = body.Point.prevPos;
                    ctx.Lifecycle.Attachments.Rocket.point.v = body.Point.v;
                    ctx.Lifecycle.Attachments.Rocket.point.posDelta = body.Point.posDelta;
                    ctx.Lifecycle.Attachments.Rocket.rotation = session.Sock.rotation + DEG_90;
                    ctx.Lifecycle.Attachments.Rocket.startRotation = session.Sock.rotation + DEG_90;
                    ctx.Lifecycle.Attachments.Rocket.startCandyRotation = body.Main.rotation;
                    ctx.Lifecycle.Attachments.Rocket.additionalAngle = 0f;
                    ctx.Lifecycle.Attachments.Rocket.UpdateRotation();
                }

                body.Point.disableGravity = IsCandyGravitySuppressed(ctx);
            }
        }

        /// <summary>
        /// Starts the level restart dimming animation.
        /// </summary>
        /// <remarks>
        /// Two callers: the loss timeline one second after <see cref="GameLost"/> (which converts
        /// <c>Losing</c> into <c>Lost</c>), and the reload path when a manual restart set
        /// <c>animateRestartDim</c>. The result is discarded because every rejection is a duplicate
        /// request - a dim already in flight, or an outcome that already claimed the restart. Both
        /// must be ignored rather than restarting the dim, which is what stops a pending loss
        /// dispatch from resetting a player-initiated dim back to full.
        /// </remarks>
        public void AnimateLevelRestart()
        {
            _ = gameplayFlow.TryBeginRestartDim();
        }

        /// <summary>
        /// Releases every rope holding one candy body. A split half also drops the ropes authored on
        /// its logical candy's whole point, which is where a <c>&lt;grab&gt;</c> without a
        /// <c>part</c> attribute lands.
        /// </summary>
        /// <param name="body">The body whose ropes are released.</param>
        public void ReleaseRopesForBody(CandyBody body)
        {
            ReleaseRopesForPoint(body.Point);
            if (body.Role != CandyBodyRole.Whole)
            {
                ReleaseRopesForPoint(body.Owner.WholeBody.Point);
            }
        }

        /// <summary>
        /// Adds a rope a ghost apparition just conjured to the scene's rope index, so the merged
        /// cut, axe and release sweeps see it like any other hook rope.
        /// </summary>
        /// <param name="rope">The rope that was created.</param>
        /// <param name="owner">The hook that owns it.</param>
        internal void RegisterRope(Bungee rope, Grab owner)
        {
            ropes.Register(rope, owner);
        }

        /// <summary>Drops a rope from the scene's rope index before it is disposed.</summary>
        /// <param name="rope">The rope being destroyed.</param>
        internal void UnregisterRope(Bungee rope)
        {
            ropes.Unregister(rope);
        }

        /// <summary>Cuts/hides all uncut ropes whose tail is the given candy point.</summary>
        public void ReleaseRopesForPoint(ConstraintedPoint candyPoint)
        {
            // One pass over every rope: RopeEntry knows which end a released candy sits on, which is
            // the connector's one asymmetry - a hook's rope only ever holds a candy at its tail.
            foreach (RopeEntry entry in ropes.All)
            {
                int? cutPart = entry.CutPartForCandy(candyPoint);
                if (cutPart == null)
                {
                    continue;
                }

                if (entry.Rope.cut == -1)
                {
                    entry.Rope.SetCut(cutPart.Value);
                }
                else
                {
                    entry.Rope.hideTailParts = true;
                }

                if (entry.Owner?.Spider?.ShouldBustOnRopeCut == true)
                {
                    SpiderBusted(entry.Owner);
                }

                entry.Owner?.OnRopeCut(RopeCutReason.CandyReleased);
            }
        }

        /// <summary>True when any candy is currently captured in the lantern group (group single-occupancy).</summary>
        private bool AnyCandyInLantern()
        {
            for (int i = 0; i < candies.Count; i++)
            {
                if (candies[i].Lifecycle.Attachments.InLantern)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>The candy currently flown by <paramref name="rocket"/>, or null if none.</summary>
        private CandyContext RocketBoundCandy(Rocket rocket)
        {
            for (int i = 0; i < candies.Count; i++)
            {
                if (candies[i].Lifecycle.Attachments.Rocket == rocket)
                {
                    return candies[i];
                }
            }
            return null;
        }

        /// <summary>The candy held by <paramref name="hand"/>, or null if the hand holds none.</summary>
        private CandyContext HandHeldCandy(MechanicalHand hand)
        {
            for (int i = 0; i < candies.Count; i++)
            {
                if (candies[i].Lifecycle.Attachments.Hand == hand)
                {
                    return candies[i];
                }
            }
            return null;
        }

        /// <summary>
        /// The nearest grabbable candy to <paramref name="hand"/> (not eaten, not in a lantern, not in a
        /// sock) and its distance. Returns null with <paramref name="distance"/> = float.MaxValue if none.
        /// </summary>
        private CandyContext NearestGrabbableCandy(MechanicalHand hand, out float distance)
        {
            CandyContext nearest = null;
            distance = float.MaxValue;
            for (int i = 0; i < candies.Count; i++)
            {
                CandyContext ctx = candies[i];
                if (!ctx.IsHandGrabbable || ctx.Lifecycle.Attachments.InLantern || ctx.Lifecycle.Transport?.Sock != null)
                {
                    continue;
                }
                float d = VectDistance(hand.cPoint.pos, ctx.WholeBody.Point.pos);
                if (d < distance)
                {
                    distance = d;
                    nearest = ctx;
                }
            }
            return nearest;
        }

        /// <summary>Exhausts the rocket bound to <paramref name="ctx"/> (one-time consume) and clears the binding.</summary>
        private static void ExhaustRocketForCandy(CandyContext ctx)
        {
            Rocket rocket = ctx?.Lifecycle.Attachments.Rocket;
            if (rocket == null)
            {
                return;
            }
            rocket.state = Rocket.STATE_ROCKET_EXAUST;
            rocket.StopAnimation();
            _ = ctx.Lifecycle.Attachments.TryReleaseRocket(rocket);
        }

        /// <summary>
        /// Nudges a flying rocket's <see cref="Rocket.additionalAngle"/> to fly perpendicular to
        /// <paramref name="rope"/>, picking whichever of the two perpendiculars is the smaller turn.
        /// Shared by the grab ropes and the candy connector (iOS steers off both the same way).
        /// </summary>
        private static void AlignRocketAngleToRope(Rocket rocket, Bungee rope, float delta)
        {
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

        /// <summary>Exhausts every candy's bound rocket (win/loss cleanup).</summary>
        private void ExhaustAllActiveRockets()
        {
            for (int i = 0; i < candies.Count; i++)
            {
                ExhaustRocketForCandy(candies[i]);
            }
        }

        /// <summary>
        /// Calculates time, star, and total score bonuses for the completed level.
        /// </summary>
        /// <returns>The immutable result calculated from the scene's current gameplay state.</returns>
        public LevelResult CalculateScore()
        {
            return LevelResultCalculator.Calculate(time, starsCollected);
        }

        /// <summary>
        /// Handles the level-won sequence, including candy consumption, scoring, cleanup, and delegate notification.
        /// </summary>
        public void GameWon()
        {
            if (!gameplayFlow.TryBeginWin())
            {
                return;
            }
            pendingLevelResult = CalculateScore();

            EndActiveFingerTraces();
            conveyors?.CancelAllDrags();
            dd.CancelAllDispatches();

            // Hide and reset sleep state for every Om Nom except one mid post-eat sleep: that
            // one keeps sleeping (and its zzz keeps looping) through the win transition, so it
            // is left untouched to avoid hiding/replaying its overlay.
            for (int ti = 0; ti < targets.Count; ti++)
            {
                TargetContext t = targets[ti];
                if (t.Feeding.IsAsleep)
                {
                    continue;
                }
                t.NightSleep.ClearPresentation();
                t.controller?.SetSleepOverlayVisible(false);
                if (t.targetObject != null)
                {
                    t.targetObject.scaleX = t.baseScaleX;
                    t.targetObject.scaleY = t.baseScaleY;
                    t.targetObject.rotationCenterX = 0f;
                    t.targetObject.rotationCenterY = 0f;
                }
            }

            PopCandyBubble(candies[0].WholeBody);
            // The primary is already Removed(Eaten) here: the single caller runs behind AllEaten,
            // which cannot pass while an eatable candy still has a body. So the win timeline below
            // owns the visual outright and no longer has to raise a gone-flag of its own first.
            candy.passTransformationsToChilds = true;
            candyMain.scaleX = candyMain.scaleY = 1f;
            candyTop.scaleX = candyTop.scaleY = 1f;
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline.AddKeyFrame(KeyFrame.MakePos((int)candy.x, (int)candy.y, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            float targetX = targetObject != null ? targetObject.x : candy.x;
            float targetY = targetObject != null ? targetObject.y : candy.y;
            timeline.AddKeyFrame(KeyFrame.MakePos((int)targetX, (int)(targetY + 10), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.1f));
            timeline.AddKeyFrame(KeyFrame.MakeScale(0.71f, 0.71f, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline.AddKeyFrame(KeyFrame.MakeScale(0, 0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.1f));
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.1f));
            candy.AddTimelinewithID(timeline, 0);
            candy.PlayTimeline(0);
            timeline.delegateTimelineDelegate = aniPool;
            _ = aniPool.AddChild(candy);
            dd.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(Selector_gameWon), null, 2);
            ReleaseRopesForBody(candies[0].WholeBody);
            ExhaustAllActiveRockets();
            DetachActiveSnails();
            DetachActiveHands();

            ShutDownMice();
        }

        /// <summary>
        /// Handles the level-lost sequence and schedules the restart animation.
        /// </summary>
        public void GameLost()
        {
            if (!gameplayFlow.TryBeginLoss())
            {
                return;
            }

            EndActiveFingerTraces();
            conveyors?.CancelAllDrags();
            dd.CancelAllDispatches();

            // Hide and reset sleep state for every Om Nom except one mid post-eat sleep: that
            // one keeps sleeping (and its zzz keeps looping) through the loss transition, so it
            // is left untouched to avoid hiding/replaying its overlay.
            for (int ti = 0; ti < targets.Count; ti++)
            {
                TargetContext t = targets[ti];
                if (t.Feeding.IsAsleep)
                {
                    continue;
                }
                t.NightSleep.ClearPresentation();
                t.controller?.SetSleepOverlayVisible(false);
                if (t.targetObject != null)
                {
                    t.targetObject.scaleX = t.baseScaleX;
                    t.targetObject.scaleY = t.baseScaleY;
                    t.targetObject.rotationCenterX = 0f;
                    t.targetObject.rotationCenterY = 0f;
                }
            }

            // Every Om Nom reacts sad on loss, except one that is already asleep after eating:
            // it stays asleep rather than waking to react. A still-chewing (pre-sleep) Om Nom
            // is not yet asleep, so it reacts sad normally.
            for (int ti = 0; ti < targets.Count; ti++)
            {
                TargetContext t = targets[ti];
                if (t.Feeding.IsAsleep)
                {
                    continue;
                }
                t.controller?.PlaySad();
                CTRSoundMgr.PlayOmNomSound(Resources.Snd.MonsterSad, t.controller?.SkinDefinition);
            }
            dd.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(Selector_animateLevelRestart), null, 1);
            gameSceneDelegate.GameLost();
            // Rockets are exhausted per-candy at each loss site (breakCandy in the C reference only
            // stops the lost candy's own rocket; gameLoseIm stops none). A surviving candy's rocket
            // keeps burning through the restart animation, matching the original.
            DetachActiveHands();

            ShutDownMice();
        }

        /// <summary>
        /// Ends mouse participation for a finished level: any candy still in a mouth goes back to
        /// the physics solver with gravity on, the mouse on screen retreats empty-handed, and the
        /// handoff is locked so no replacement pops out of the next hole.
        /// </summary>
        /// <remarks>
        /// The release has to come first and the lock has to follow. Releasing while the mouse is
        /// still active would only hand the candy back for a frame - it lands within grab radius of
        /// the very hole it came from, so the per-frame grab check would steal it straight back.
        /// Locking without releasing is what stranded it: the mouse leaves with the candy, the
        /// locked handoff refuses to pass it to the next hole, and the point stays pinned mid-air
        /// with gravity disabled and no mouse left to carry it.
        /// </remarks>
        private void ShutDownMice()
        {
            if (miceManager == null)
            {
                return;
            }

            ConstraintedPoint released = miceManager.ActiveMouseCarriedStar();
            miceManager.ReleaseAllCandy();
            if (CandyForPointOrNull(released) is CandyContext releasedCandy)
            {
                releasedCandy.WholeBody.Point.disableGravity = IsCandyGravitySuppressed(releasedCandy);
            }

            if (mice != null)
            {
                foreach (object obj in mice)
                {
                    if (obj is Mouse mouse && mouse.IsActive)
                    {
                        mouse.BeginRetreat();
                        break;
                    }
                }
            }

            miceManager.LockActiveMouse();
        }

        /// <summary>
        /// Pops the bubble carrying one candy body, releasing the bubble's ghost (if any) and
        /// clearing the body's bubble overlays. No-op when the body carries no bubble.
        /// </summary>
        /// <param name="body">The body whose bubble is popped.</param>
        public void PopCandyBubble(CandyBody body)
        {
            if (body?.Bubble != null)
            {
                PopCandyBubbleAt(body, Vect(body.Visual.x, body.Visual.y));
            }
        }

        /// <summary>
        /// Pops the bubble carrying one candy body and plays the pop effect at an explicit position,
        /// so a device that snatches the candy away (a mechanical hand's claw) can burst the bubble
        /// where it took it rather than where the body now sits.
        /// </summary>
        /// <param name="body">The body whose bubble is popped; ignored when it carries no bubble.</param>
        /// <param name="effectPosition">World position for the pop animation and sound.</param>
        public void PopCandyBubbleAt(CandyBody body, Vector effectPosition)
        {
            if (body?.Bubble == null)
            {
                return;
            }

            GameObject popped = body.Bubble;
            ReleaseGhostForBubble(popped);

            // A merge can fold both halves' ghost bubbles onto the merged candy, parking the second
            // one behind the first. Popping the survivor releases the ghost that was parked with it.
            ReleasePendingSecondGhostBubbleForBody(body);

            if (popped is Bubble bubble)
            {
                bubble.capturedByBulb = false;
            }

            body.Bubble = null;
            body.BubbleHasGhost = false;
            _ = (body.BubbleAnimation?.visible = false);
            _ = (body.GhostBubbleAnimation?.visible = false);
            PopBubbleAtXY(effectPosition.X, effectPosition.Y);
        }

        /// <summary>
        /// Plays bubble-pop effects at a world position.
        /// </summary>
        /// <param name="bx">World-space X position for the pop effect.</param>
        /// <param name="by">World-space Y position for the pop effect.</param>
        public void PopBubbleAtXY(float bx, float by)
        {
            CTRSoundMgr.PlaySound(Resources.Snd.BubbleBreak);
            Animation animation = Animation.Animation_createWithResID(Resources.Img.ObjBubble);
            animation.DoRestoreCutTransparency();
            animation.x = bx;
            animation.y = by;
            animation.anchor = 18;
            int i = animation.AddAnimationDelayLoopFirstLast(0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 18, 29);
            animation.GetTimeline(i).delegateTimelineDelegate = aniPool;
            animation.PlayTimeline(0);
            _ = aniPool.AddChild(animation);
        }

        /// <summary>
        /// Lazily-parsed chain-cut burst effect, shared across all spawns.
        /// </summary>
        private static FlashXmlOneShotEffect s_chainCutEffect;

        /// <summary>
        /// Calculates the chain break debris angle from the candy motion, matching the original
        /// <c>ChainBreak::initWith(atan2(current - previous))</c> call site.
        /// </summary>
        /// <param name="point">Candy physics point whose motion cut the chain.</param>
        /// <returns>Motion angle in degrees, or -90 when no previous position is available.</returns>
        internal static float GetChainCutSwingAngleDegrees(ConstraintedPoint point)
        {
            return point == null || point.prevPos.X == UNDEFINED_COORDINATE
                ? -90f
                : RADIANS_TO_DEGREES(MathF.Atan2(
                    point.pos.Y - point.prevPos.Y,
                    point.pos.X - point.prevPos.X));
        }

        /// <summary>
        /// Spawns the chain-cut burst animation at a world position and plays the chain-cut sound.
        /// Reusable hook: called wherever a breakable (chain) rope is cut, including the future axe path.
        /// </summary>
        /// <param name="x">World-space X for the burst.</param>
        /// <param name="y">World-space Y for the burst.</param>
        /// <param name="swingAngleDegrees">Direction the debris is flung (the cutter's swing direction in the original); defaults to upward.</param>
        public void SpawnChainCutEffectAtXY(float x, float y, float swingAngleDegrees = -90f)
        {
            s_chainCutEffect ??= new FlashXmlOneShotEffect("fx_cut_chain.xml", Resources.Img.FxCutChain);
            s_chainCutEffect.SpawnInto(aniPool, x, y, 0);
            SpawnChainCutDebris(x, y, swingAngleDegrees);
            SpawnChainFlashLight(x, y, swingAngleDegrees);
            CTRSoundMgr.PlaySound(Resources.Snd.ChainCut);
        }

        /// <summary>
        /// Breaks candy bodies that touch a Time Travel axe blade. Mirrors the spike pass: one
        /// sweep over every hazard-eligible body (whole candies and split halves alike), breaking
        /// the first one the blade reaches.
        /// </summary>
        /// <returns><see langword="true"/> when a body was broken and update should stop.</returns>
        private bool BreakCandyTouchedByAxes()
        {
            for (int ai = 0; ai < candies.Count; ai++)
            {
                CandyContext axeCtx = candies[ai];
                if (axeCtx.axe == null || axeCtx.HasNoWholeBodyInPlay)
                {
                    continue;
                }

                ConstraintedPoint bladePoint = axeCtx.WholeBody.Point;
                foreach (CandyBody body in ActiveCandyBodies(CandyInteraction.Hazard))
                {
                    CandyContext ctx = body.Owner;
                    if (!ctx.Capabilities.CanBeBrokenByHazards || ctx.Lifecycle.Attachments.InLantern)
                    {
                        continue;
                    }

                    if (VectDistance(body.Point.pos, bladePoint.pos) > AxeDefinition.HazardCollisionDistance)
                    {
                        continue;
                    }

                    BreakCandyBody(body);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Spawns the additive spark burst at a world position.
        /// </summary>
        /// <param name="x">World-space X for the sparks.</param>
        /// <param name="y">World-space Y for the sparks.</param>
        /// <param name="swingAngleDegrees">Direction the sparks are flung (the cutter's swing direction in the original).</param>
        private void SpawnChainFlashLight(float x, float y, float swingAngleDegrees)
        {
            Image grid = Image.Image_createWithResID(Resources.Img.FxCutChain);
            grid.DoRestoreCutTransparency();
            ChainFlashLight sparks = (ChainFlashLight)new ChainFlashLight().InitWithTotalParticlesandImageGrid(10, grid);
            sparks.angle = swingAngleDegrees;
            if (gravityState.IsInverted)
            {
                sparks.gravity.Y = -sparks.gravity.Y;
                sparks.angle = -swingAngleDegrees;
            }
            sparks.particlesDelegate = new Particles.ParticlesFinished(aniPool.ParticlesFinished);
            sparks.x = x;
            sparks.y = y;
            sparks.StartSystem(10);
            _ = aniPool.AddChild(sparks);
        }

        /// <summary>
        /// Spawns the two chain-link debris fragments at a world position.
        /// </summary>
        /// <param name="x">World-space X for the debris.</param>
        /// <param name="y">World-space Y for the debris.</param>
        /// <param name="swingAngleDegrees">Direction the fragments are flung (the cutter's swing direction in the original).</param>
        private void SpawnChainCutDebris(float x, float y, float swingAngleDegrees)
        {
            Image grid = Image.Image_createWithResID(Resources.Img.FxCutChain);
            grid.DoRestoreCutTransparency();
            ChainCutDebris debris = (ChainCutDebris)new ChainCutDebris().InitWithTotalParticlesandImageGrid(2, grid);
            debris.angle = swingAngleDegrees;
            if (gravityState.IsInverted)
            {
                debris.gravity.Y = -debris.gravity.Y;
                debris.angle = -swingAngleDegrees;
            }
            debris.particlesDelegate = new Particles.ParticlesFinished(aniPool.ParticlesFinished);
            debris.x = x;
            debris.y = y;
            debris.StartSystem(2);
            _ = aniPool.AddChild(debris);
        }

        /// <summary>
        /// Spawns the candy-break particle burst at a world position and plays the break sound.
        /// </summary>
        /// <param name="bx">World-space X for the burst.</param>
        /// <param name="by">World-space Y for the burst.</param>
        private void SpawnCandyBreakParticles(float bx, float by)
        {
            int selectedCandySkin = Preferences.GetIntForKey("PREFS_SELECTED_CANDY");
            string candyResource = CandySkinHelper.GetCandyResource(selectedCandySkin);
            Image image2 = Image.Image_createWithResID(candyResource);
            image2.DoRestoreCutTransparency();
            CandyBreak candyBreak = (CandyBreak)new CandyBreak().InitWithTotalParticlesandImageGrid(5, image2);
            if (gravityState.IsInverted)
            {
                candyBreak.gravity.Y = -ActivePhysicsConstants.CandyBreakGravityY;
                candyBreak.angle = 90f;
            }
            candyBreak.particlesDelegate = new Particles.ParticlesFinished(aniPool.ParticlesFinished);
            candyBreak.x = bx;
            candyBreak.y = by;
            candyBreak.StartSystem(5);
            _ = aniPool.AddChild(candyBreak);
            CTRSoundMgr.PlaySound(Resources.Snd.CandyBreak);
        }

        /// <summary>
        /// Schedules the loss sequence after a delay (e.g. while a candy-break animation plays) and
        /// immediately marks the outcome transition active. A destroyed candy is removed at once but
        /// defers <see cref="GameLost"/>; without marking the transition, another candy eaten during
        /// that window would satisfy the win check and trigger a false win in a multi-candy level.
        /// </summary>
        /// <param name="delay">Seconds to wait before running the loss sequence.</param>
        private void ScheduleGameLost(float delay)
        {
            if (!gameplayFlow.TryScheduleLoss())
            {
                return;
            }
            dd.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(Selector_gameLost), null, delay);
        }

        /// <summary>
        /// Destroys one candy body that touched a hazard (spike, axe, ...): pops its bubble, removes
        /// it as a hazard loss, releases its ropes, detaches its carriers, schedules the loss, and
        /// begins the authoritative loss transition. A split half loses only itself; its sibling
        /// keeps playing until the scheduled loss lands.
        /// </summary>
        /// <param name="body">The body being destroyed.</param>
        private void BreakCandyBody(CandyBody body)
        {
            Vector breakPosition = body.Point.pos;
            if (!TryRetireCandyBody(body, CandyRemovalReason.Hazard))
            {
                return;
            }

            body.Visual.x = breakPosition.X;
            body.Visual.y = breakPosition.Y;
            SpawnCandyBreakParticles(breakPosition.X, breakPosition.Y);
            if (gameplayFlow.CanTriggerOutcome)
            {
                ScheduleGameLost(0.3f);
            }
        }

        /// <summary>
        /// Cuts all ropes attached to the same candy as <paramref name="except"/>, sparing that grab's
        /// own rope. Matches iOS destroyRopesForCandy:except:.
        /// </summary>
        /// <remarks>
        /// Candies are identified by the rope's tail point rather than by a legacy candy index.
        /// The legacy numbering only distinguished whole candy (0) from the split halves (1/2), so the
        /// multi-candy loader hands every candy-bound grab the same number 0 — matching on it would cut
        /// ropes belonging to *other* candies. Tail identity is exact for all three cases.
        /// </remarks>
        /// <param name="except">Grab whose candy is targeted and whose own rope is preserved.</param>
        private void DestroyRopesForCandy(Grab except)
        {
            ConstraintedPoint candyPoint = except?.Rope?.tail;
            if (candyPoint == null)
            {
                return;
            }

            for (int i = 0; i < bungees.Count; i++)
            {
                Grab grab = bungees[i];
                bool ropeUncut = grab.Attachment.IsIntact;
                if (ConveyorRopeCut.ShouldCut(grab.Rope?.tail, candyPoint, grab == except, ropeUncut))
                {
                    grab.Rope.SetCut(grab.Rope.parts.Count - 2);
                }
            }
        }

        /// <summary>
        /// Clears the highlighted state from all uncut bungee ropes.
        /// </summary>
        public void ResetBungeeHighlight()
        {
            for (int i = 0; i < bungees.Count; i++)
            {
                Bungee rope = bungees[i].Rope;
                if (bungees[i].Attachment.IsIntact)
                {
                    rope.highlighted = false;
                }
            }
        }

        /// <summary>
        /// Detaches all active snails from the candy.
        /// </summary>
        public void DetachActiveSnails()
        {
            if (snailobjects == null || snailobjects.Count <= 0)
            {
                return;
            }

            for (int i = snailobjects.Count - 1; i >= 0; i--)
            {
                Snail snail = snailobjects[i];
                if (snail != null && snail.state == Snail.SNAIL_STATE_ACTIVE)
                {
                    snail.Detach();
                }
            }
        }

        /// <summary>
        /// Forces the active mouse to drop its candy only when that candy is <paramref name="point"/>.
        /// Capture devices (hand grab, sock, bamboo, lantern) strip the mouse per-candy; a mouse
        /// carrying a different candy keeps it.
        /// </summary>
        public void DropMouseCandyForPoint(ConstraintedPoint point)
        {
            if (MouseOwnership.CarriesCandy(miceManager?.ActiveMouseCarriedStar(), point))
            {
                miceManager.ForceDropCandy();
                CandyContext ctx = CandyForPointOrNull(point);
                if (ctx != null)
                {
                    point.disableGravity = IsCandyGravitySuppressed(ctx);
                }
            }
        }

        /// <summary>Gets whether the active mouse is the authoritative owner of this candy.</summary>
        private bool MouseCarries(CandyContext ctx)
        {
            return ctx != null && miceManager?.CarriesCandy(ctx.WholeBody.Point) == true;
        }

        /// <summary>Combines lifecycle-owned gravity suppression with derived mouse ownership.</summary>
        private bool IsCandyGravitySuppressed(CandyContext ctx)
        {
            return ctx?.Lifecycle.IsGravitySuppressed == true || MouseCarries(ctx);
        }

        /// <summary>
        /// Number of active snails currently riding the given candy point.
        /// </summary>
        /// <param name="point">Candy physics point to count attached snails for.</param>
        /// <returns>The count of snails in the active state whose attached point is <paramref name="point"/>; 0 if none or <paramref name="point"/> is null.</returns>
        public int ActiveSnailCountForPoint(ConstraintedPoint point)
        {
            if (snailobjects == null || snailobjects.Count <= 0 || point == null)
            {
                return 0;
            }

            int count = 0;
            foreach (Snail snail in snailobjects)
            {
                if (snail != null && snail.state == Snail.SNAIL_STATE_ACTIVE && snail.AttachedPoint() == point)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>Detaches active snails riding the given candy point (no-op if null).</summary>
        public void DetachSnailsForPoint(ConstraintedPoint point)
        {
            if (snailobjects == null || snailobjects.Count <= 0 || point == null)
            {
                return;
            }

            for (int i = snailobjects.Count - 1; i >= 0; i--)
            {
                Snail snail = snailobjects[i];
                if (snail != null && SnailDetachSelection.ShouldDetach(
                        snail.state == Snail.SNAIL_STATE_ACTIVE, snail.AttachedPoint(), point))
                {
                    snail.Detach();
                }
            }
        }

        /// <summary>
        /// Releases all mechanical hands currently holding a candy. Once a candy's
        /// <see cref="CandyAttachments.Hand"/> is cleared the ant conveyor is free to pick it up
        /// again, so no global conveyor unblock is needed.
        /// </summary>
        public void DetachActiveHands()
        {
            if (hands == null || hands.Count <= 0)
            {
                return;
            }

            foreach (MechanicalHand hand in hands)
            {
                if (hand != null && hand.State == MechanicalHandState.HoldingCandy)
                {
                    CandyContext held = HandHeldCandy(hand);
                    ConstraintedPoint heldPoint = held?.WholeBody.Point ?? star;
                    hand.cPoint.RemoveConstraint(heldPoint);
                    hand.ReleaseCandy();
                    hand.AnimateReleaseWithAnimationsPool(aniPool);
                    _ = held?.Lifecycle.Attachments.TryReleaseHand(hand);
                }
            }
        }

        /// <summary>
        /// Releases only the mechanical hand holding the candy at <paramref name="point"/> (no-op if null).
        /// </summary>
        /// <remarks>
        /// The drop sound plays here rather than being left to the release-to-idle transition, which is
        /// what a hand that lets go on its own uses. That transition needs the candy to travel past
        /// <see cref="MechanicalHand.MH_RELEASE_DISTANCE"/> from the claw, and a candy taken by a mouse
        /// stops at the hole it was stolen through - often inside that radius - so the hand would sit
        /// silently in the release state until the mouse eventually carried it away. Marking the sound
        /// as played keeps the transition from repeating it. This mirrors the player tapping the claw.
        /// </remarks>
        public void DetachHandsForPoint(ConstraintedPoint point)
        {
            if (hands == null || hands.Count <= 0 || point == null)
            {
                return;
            }

            foreach (MechanicalHand hand in hands)
            {
                if (hand != null && hand.State == MechanicalHandState.HoldingCandy)
                {
                    CandyContext held = HandHeldCandy(hand);
                    ConstraintedPoint heldPoint = held?.WholeBody.Point ?? star;
                    if (heldPoint != point)
                    {
                        continue;
                    }
                    hand.cPoint.RemoveConstraint(heldPoint);
                    hand.ReleaseCandyAfterDropSound();
                    hand.AnimateReleaseWithAnimationsPool(aniPool);
                    _ = held?.Lifecycle.Attachments.TryReleaseHand(hand);
                    CTRSoundMgr.PlaySound(Resources.Snd.ExpHandDrop);
                }
            }
        }

        /// <summary>
        /// Handles game-scene button actions such as toggling gravity.
        /// </summary>
        /// <param name="_">Game scene button identifier.</param>
        public void OnButtonPressed(GameSceneButtonId _)
        {
            gravityState.Toggle();
            CTRSoundMgr.PlaySound(gravityState.IsInverted
                ? Resources.Snd.GravityOn
                : Resources.Snd.GravityOff);
        }

        /// <inheritdoc />
        void IButtonDelegation.OnButtonPressed(ButtonId buttonId)
        {
            OnButtonPressed(GameSceneButtonId.FromButtonId(buttonId));
        }

        /// <summary>
        /// Rotates every spike object matching the supplied toggle ID.
        /// </summary>
        /// <param name="sid">Spike toggle identifier to match.</param>
        public void RotateAllSpikesWithID(int sid)
        {
            foreach (object obj in spikes)
            {
                Spikes spike = (Spikes)obj;
                if (spike.GetToggled() == sid)
                {
                    spike.RotateSpikes();
                }
            }
        }

        /// <summary>
        /// Returns the ghost that owns the specified apparition bubble.
        /// </summary>
        private Ghost GhostForBubble(GameObject bubbleObj)
        {
            if (bubbleObj is not Bubble bubble || ghosts == null)
            {
                return null;
            }
            foreach (object obj in ghosts)
            {
                Ghost ghost = (Ghost)obj;
                if (ghost?.OwnsBubble(bubble) == true)
                {
                    return ghost;
                }
            }
            return null;
        }

        /// <summary>
        /// Releases the ghost that owns a captured or parked apparition bubble.
        /// </summary>
        private void ReleaseGhostForBubble(GameObject bubbleObj)
        {
            if (bubbleObj is Bubble bubble)
            {
                _ = GhostForBubble(bubble)?.ReleaseBubble(bubble);
            }
        }

        /// <summary>Whether the specified bubble is the current apparition of a ghost.</summary>
        private bool IsGhostApparitionBubble(GameObject bubbleObj)
        {
            return GhostForBubble(bubbleObj) != null;
        }
    }
}
